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
using CrashDebuggerLib.Structures.Thread;
using SymbianParserLib.Elements;
using SymbianParserLib.Enums;
using SymbianParserLib.BaseStructures;

namespace CrashDebuggerLib.Parsers.State.Implementation.Helpers
{
    internal class HelperDThread : HelperDObject
    {
        #region Constructors
        public HelperDThread()
        {
        }
        #endregion

        #region API
        public void CreateMonitorThread( ParserEngine aEngine, string aName, DThread aThread )
        {
            CreateMonitorThread( aEngine, aName, aThread, null );
        }

        public void CreateMonitorThread( ParserEngine aEngine, string aName, DThread aThread, ParserElementBase.ElementCompleteHandler aLastFieldHandler )
        {
            // Create DObject paragraph
            ParserParagraph para0 = base.CreateMonitorObjectParagraph( aName, aThread );
            aEngine.Add( para0 );

            // Create MState paragraphs
            ParserParagraph para1 = new ParserParagraph( aName + "_MSTATE" );
            para1.Tag = aThread;
            aEngine.Add( para1 );
            CreateThreadMState( para1, DThread.TThreadState.ECreated, "CREATED", false );
            CreateThreadMState( para1, DThread.TThreadState.EDead, "DEAD", false );
            CreateThreadMState( para1, DThread.TThreadState.EReady, "READY", false );
            CreateThreadMState( para1, DThread.TThreadState.EWaitSemaphore, "WAITSEM", true );
            CreateThreadMState( para1, DThread.TThreadState.EWaitSemaphoreSuspended, "WAITSEMS", true );
            CreateThreadMState( para1, DThread.TThreadState.EWaitMutex, "WAITMUTEX", true );
            CreateThreadMState( para1, DThread.TThreadState.EWaitMutexSuspended, "WAITMUTXS", true );
            CreateThreadMState( para1, DThread.TThreadState.EHoldMutexPending, "HOLDMUTXP", true );
            CreateThreadMState( para1, DThread.TThreadState.EWaitCondVar, "WAITCONDVAR", true );
            CreateThreadMState( para1, DThread.TThreadState.EWaitCondVarSuspended, "WAITCONDVRS", true );
            CreateThreadMState( para1, DThread.TThreadState.EUnknown, "??", true );

            // Create common thread paragraph
            ParserParagraph para2 = CreateThreadCommon( aName, aThread );
            aEngine.Add( para2 );

            // Create NThread paragraphs
            iHelper.CreateMonitorNThread( aEngine, aName + "_NTHREAD", aThread.NThread, aLastFieldHandler );

            // TODO: add support for older memory models?
        }
        #endregion

        #region Call-back methods
        void ThreadMState_ElementComplete( ParserElementBase aElement )
        {
            ParserLine line = (ParserLine) aElement;
            ParserParagraph paragraph = line.Paragraph;
            System.Diagnostics.Debug.Assert( paragraph.Tag is DThread );
            DThread thread = (DThread) paragraph.Tag;
            DThread.TThreadState state = (DThread.TThreadState) line.Tag;
            thread.MState = state;
        }

        public void SetThreadMStateWaitObject( ParserParagraph aParagraph, ParserLine aLine, ParserFieldName aParameterName, uint aWaitObjectAddress )
        {
            System.Diagnostics.Debug.Assert( aParagraph.Tag is DThread );
            DThread thread = (DThread) aParagraph.Tag;
            thread.WaitObj = aWaitObjectAddress;
        }
        #endregion

        #region Internal methods
        private void CreateThreadMState( ParserParagraph aParagraph, DThread.TThreadState aState, string aMStateName, bool aCapturesWaitObject )
        {
            StringBuilder format = new StringBuilder( "Thread MState" );
            format.Append( " " + aMStateName );
            //
            if ( aCapturesWaitObject )
            {
                format.Append( " %8x" );
            }
            //
            string finalFormat = format.ToString();
            ParserLine l1 = null;
            //
            if ( aCapturesWaitObject )
            {
                l1 = ParserLine.NewSymFormat( finalFormat );
            }
            else
            {
                l1 = ParserLine.New( finalFormat );
            }
            
            l1.Tag = aState;
            l1.ElementComplete += new ParserElementBase.ElementCompleteHandler( ThreadMState_ElementComplete );
            //
            if ( aCapturesWaitObject )
            {
                l1[ 0 ].SetTargetMethod( this, "SetThreadMStateWaitObject" );
            }
            //
            aParagraph.Add( l1 );
        }

        private ParserParagraph CreateThreadCommon( string aName, DThread aThread )
        {
            ParserParagraph para = new ParserParagraph( aName );
            //
            ParserLine l1 = ParserLine.NewSymFormat( "Default priority %d WaitLink Priority %d\r\n" );
            l1.SetTargetProperties( aThread.Priorities, "Default", "WaitLink" );
            //
            ParserLine l2 = ParserLine.NewSymFormat( "ExitInfo %d,%d,%lS\r\n" );
            l2.SetTargetProperties( aThread.ExitInfo, "Type", "Reason", "Category" );
            //
            ParserLine l3 = ParserLine.NewSymFormat( "Flags %08x, Handles %08x\r\n" );
            l3.SetTargetProperties( aThread, "Flags", "Handles" );
            //
            ParserLine l4 = ParserLine.NewSymFormat( "Supervisor stack base %08x size %x\r\n" );
            l4.SetTargetProperties( aThread.StackInfoSupervisor, "BaseAddress", "Size" );
            //
            ParserLine l5 = ParserLine.NewSymFormat( "User stack base %08x size %x\r\n" );
            l5.SetTargetProperties( aThread.StackInfoUser, "BaseAddress", "Size" );
            //
            ParserLine l6 = ParserLine.NewSymFormat( "Id=%d, Alctr=%08x, Created alctr=%08x, Frame=%08x\r\n" );
            l6.SetTargetProperties( new object[] { aThread, aThread.AllocatorInfo, aThread.AllocatorInfo, aThread }, "Id", "Allocator", "CreatedAllocator", "Frame" );
            //
            ParserLine l7 = ParserLine.NewSymFormat( "Trap handler=%08x, ActiveScheduler=%08x, Exception handler=%08x\r\n" );
            l7.SetTargetProperties( aThread.Handlers, "TrapHandler", "ActiveScheduler", "ExceptionHandler" );
            //
            ParserLine l8 = ParserLine.NewSymFormat( "TempObj=%08x TempAlloc=%08x IpcCount=%08x\r\n" );
            l8.SetTargetProperties( new object[] { aThread.Temporaries, aThread.Temporaries, aThread }, "TempObj", "TempAlloc", "IpcCount" );
            //
            para.Add( l1, l2, l3, l4, l5, l6, l7, l8 );
            return para;
        }
        #endregion

        #region Data members
        private HelperNThread iHelper = new HelperNThread();
        #endregion
    }
}
