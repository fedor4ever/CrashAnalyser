/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
* All rights reserved.
* This component and the accompanying materials are made available
* under the terms of "Eclipse Public License v1.0"
* which accompanies this distribution, and is available
* at the URL "http://www.eclipse.org/legal/epl-v10.html".
*
* Initial Contributors:
* Nokia Corporation - initial contribution.
*
* Contributors:
*
* Description: 
*
*/

using System;
using System.Collections.Generic;
using System.Text;
using SymbianParserLib.Engine;
using CrashDebuggerLib.Structures.KernelObjects;
using CrashDebuggerLib.Structures.Process;
using CrashDebuggerLib.Structures.CodeSeg;
using CrashDebuggerLib.Structures.Register;
using CrashDebuggerLib.Structures.Thread;
using CrashDebuggerLib.Structures.NThread;
using SymbianParserLib.Elements;
using SymbianParserLib.Enums;
using SymbianParserLib.BaseStructures;

namespace CrashDebuggerLib.Parsers.State.Implementation.Helpers
{
    internal class HelperNThread
    {
        #region Constructors
        public HelperNThread()
        {
        }
        #endregion

        #region API
        public void CreateMonitorNThread( ParserEngine aEngine, string aName, NThread aThread )
        {
            CreateMonitorNThread( aEngine, aName, aThread, null );
        }

        public void CreateMonitorNThread( ParserEngine aEngine, string aName, NThread aThread, ParserElementBase.ElementCompleteHandler aLastFieldHandler )
        {
            // Create MState lines
            ParserParagraph para0 = new ParserParagraph( aName + "_NSTATE" );
            para0.Tag = aThread;
            CreateThreadNState( para0, aThread, "READY", false );
            CreateThreadNState( para0, aThread, "SUSPENDED", false );
            CreateThreadNState( para0, aThread, "WAITFSEM", true );
            CreateThreadNState( para0, aThread, "SLEEP", false );
            CreateThreadNState( para0, aThread, "BLOCKED", false );
            CreateThreadNState( para0, aThread, "DEAD", false );
            CreateThreadNState( para0, aThread, "WAITDFC", false );
            CreateThreadNState( para0, aThread, "??", true );
            aEngine.Add( para0 );

            ParserParagraph para1 = CreateThreadCommon( aName, aThread );
            aEngine.Add( para1 );

            CreateRegisterParagraphs( aEngine, aThread, aLastFieldHandler );

            // TODO: add support for older memory models?
        }
        #endregion

        #region Call-back methods
        public void SetThreadNStateWaitObject( ParserParagraph aParagraph, ParserFieldName aParameterName, uint aWaitObjectAddress )
        {
            System.Diagnostics.Debug.Assert( aParagraph.Tag is NThread );
            NThread thread = (NThread) aParagraph.Tag;
            thread.WaitObj = aWaitObjectAddress;
        }
        #endregion

        #region Event handlers
        void NThreadState_ElementComplete( ParserElementBase aElement )
        {
            ParserLine line = (ParserLine) aElement;
            System.Diagnostics.Debug.Assert( line.Tag is NThread.TNThreadState );
            NThread.TNThreadState state = (NThread.TNThreadState) line.Tag;
            ParserParagraph paragraph = line.Paragraph;
            System.Diagnostics.Debug.Assert( paragraph.Tag is NThread );
            NThread thread = (NThread) paragraph.Tag;
            thread.NState = state;
        }
        #endregion

        #region Internal methods
        private void CreateThreadNState( ParserParagraph aParagraph, NThread aThread, string aNStateName, bool aCapturesWaitObject )
        {
            StringBuilder format = new StringBuilder( "NThread @ %8x Pri %d NState " + aNStateName );
            //
            if ( aCapturesWaitObject )
            {
                format.Append( " %8x" );
            }
            format.Append( "\r\n" );
            //
            //
            string finalFormat = format.ToString();
            NThread.TNThreadState state = NThread.NStateFromString( aNStateName );
            ParserLine l1 = ParserLine.NewSymFormat( finalFormat );
            l1.Tag = state;
            //
            l1[ 0 ].SetTargetProperty( aThread, "Address" );
            l1[ 1 ].SetTargetProperty( aThread, "Priority" );
            //
            if ( aCapturesWaitObject )
            {
                l1[ 2 ].SetTargetMethod( this, "SetThreadNStateWaitObject" );
            }
            //
            l1.ElementComplete += new ParserElementBase.ElementCompleteHandler( NThreadState_ElementComplete );
            aParagraph.Add( l1 );
        }

        private ParserParagraph CreateThreadCommon( string aName, NThread aThread )
        {
            ParserParagraph para = new ParserParagraph( aName );
            //
            ParserLine l1 = ParserLine.NewSymFormat( "Next=%08x Prev=%08x Att=%02x\r\n" );
            l1.SetTargetProperties( new object[] { aThread.LinkedListInfo, aThread.LinkedListInfo, aThread }, "Next", "Previous", "Attributes" );
            //
            ParserLine l2 = ParserLine.NewSymFormat( "HeldFM=%08x WaitFM=%08x AddrSp=%08x\r\n" );
            l2.SetTargetProperties( new object[] { aThread.MutexInfo, aThread.MutexInfo, aThread }, "HeldAddress", "WaitAddress", "AddressSpace" );
            //
            ParserLine l3 = ParserLine.NewSymFormat( "Time=%d Timeslice=%d ReqCount=%d\r\n" );
            l3.SetTargetProperties( new object[] { aThread.TimeInfo, aThread.TimeInfo, aThread.CountInfo }, "Time", "Timeslice", "RequestSemaphoreCount" );
            //
            ParserLine l4 = ParserLine.NewSymFormat( "LastStartTime=%08x TotalCpuTime=%lx Tag=%08x\r\n" );
            l4.SetTargetProperties( aThread.TimeInfo, "LastStartTime", "TotalCpuTime", "Tag" );
            //
            ParserLine l5 = ParserLine.NewSymFormat( "ReturnValue=%d, UCT=%d\r\n" );
            l5.SetTargetProperties( aThread, "ReturnValue", "UserContextType" );
            //
            ParserLine l6 = ParserLine.NewSymFormat( "SuspendCount=%d CsCount=%d CsFunction=%08x\r\n" );
            l6.SetTargetProperties( aThread.CountInfo, "SuspendCount", "CsCount", "CsFunctionRaw" );
            //
            ParserLine l7 = ParserLine.NewSymFormat( "SavedSP=%08x ExtraContext=%08x ExtraContextSize=%08x\r\n" );
            l7.SetTargetProperties( new object[] { aThread, aThread.ExtraContextInfo, aThread.ExtraContextInfo }, "SavedSP", "ExtraContext", "ExtraContextSizeRaw" );
            //
            para.Add( l1, l2, l3, l4, l5, l6, l7 );
            return para;
        }

        private void CreateRegisterParagraphs( ParserEngine aEngine, NThread aThread, ParserElementBase.ElementCompleteHandler aLastFieldHandler )
        {
            {
            ParserParagraph para = new ParserParagraph( "NTHREAD_REGS1" );
            para.SetTargetMethod( aThread.Registers, "Add" );
            ParserLine l1 = ParserLine.NewSymFormat( "FPEXC %08x\r\n" );
            ParserLine l2 = ParserLine.NewSymFormat( "CAR %08x\r\n" );
            ParserLine l3 = ParserLine.NewSymFormat( "DACR %08x\r\n" );
            para.Add( l1, l2, l3 );
            aEngine.Add( para );
            }

            {
            ParserParagraph para = new ParserParagraph( "NTHREAD_REGS2" );
            para.SetTargetMethod( aThread.Registers, "Add" );
            ParserLine l1 = ParserLine.NewSymFormat( "R13_USR %08x R14_USR %08x SPSR_SVC %08x\r\n" );
            ParserLine l2 = ParserLine.NewSymFormat( " R4 %08x  R5 %08x  R6 %08x  R7 %08x\r\n" );
            ParserLine l3 = ParserLine.NewSymFormat( " R8 %08x  R9 %08x R10 %08x R11 %08x\r\n" );
            ParserLine l4 = ParserLine.NewSymFormat( " PC %08x\r\n" );
            //
            if ( aLastFieldHandler != null )
            {
                l4[ 0 ].ElementComplete += new ParserElementBase.ElementCompleteHandler( aLastFieldHandler );
            }
            //
            para.Add( l1, l2, l3, l4 );
            aEngine.Add( para );
            }
        }
        #endregion
    }
}
