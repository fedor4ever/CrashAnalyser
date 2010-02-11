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
using SymbianParserLib.Elements;
using SymbianParserLib.Enums;
using SymbianParserLib.BaseStructures;

namespace CrashDebuggerLib.Parsers.State.Implementation.Helpers
{
    internal class HelperDProcess : HelperDObject
    {
        #region Constructors
        public HelperDProcess()
        {
        }
        #endregion

        #region API
        public void CreateMonitorProcess( ParserEngine aEngine, string aName, DProcess aProcess )
        {
            CreateMonitorProcess( aEngine, aName, aProcess, null );
        }

        public void CreateMonitorProcess( ParserEngine aEngine, string aName, DProcess aProcess, ParserElementBase.ElementCompleteHandler aLastFieldHandler )
        {
            ParserParagraph para0 = base.CreateMonitorObjectParagraph( aName, aProcess );
            aEngine.Add( para0 );
            ParserParagraph para1 = CreateMonitorProcessCommon( aName, aProcess );
            aEngine.Add( para1 );
            ParserParagraph para2 = CreateMonitorProcessCodeSegs( aName + "_CodeSegs", aProcess );
            aEngine.Add( para2 );
            ParserParagraph para3 = CreateMonitorProcessMemModelMultiple( aName + "_MemModel_Multiple", aProcess, aLastFieldHandler );
            aEngine.Add( para3 );

            // TODO: add support for older memory models?
        }
        #endregion

        #region Call-back methods
        public void AddCodeSegToProcess( ParserParagraph aParagraph, ParserFieldName aParameterName, uint aParameterValue )
        {
            System.Diagnostics.Debug.Assert( aParagraph.Tag is DProcess );
            DProcess process = (DProcess) aParagraph.Tag;
            ProcessCodeSegCollection codeSegs = process.CodeSegments;
            //
            if ( aParameterName == "lib" )
            {
                int count = codeSegs.Count;
                if ( count > 0 )
                {
                    ProcessCodeSeg lastEntry = codeSegs[ count - 1 ];
                    lastEntry.LibraryAddress = aParameterValue;
                }
            }
            else if ( aParameterName == "seg" )
            {
                ProcessCodeSeg entry = new ProcessCodeSeg( process.CrashDebugger );
                entry.CodeSegAddress = aParameterValue;
                codeSegs.Add( entry );
            }
        }

        public void AddChunkToProcess( ParserParagraph aParagraph, ParserFieldName aParameterName, uint aParameterValue )
        {
            System.Diagnostics.Debug.Assert( aParagraph.Tag is DProcess );
            DProcess process = (DProcess) aParagraph.Tag;
            ProcessChunkCollection chunks = process.Chunks;
            //
            ProcessChunk chunk = new ProcessChunk( process.CrashDebugger, aParameterValue, 0 );
            chunks.Add( chunk );
        }

        public void SetChunkAccessCount( ParserParagraph aParagraph, ParserFieldName aParameterName, int aParameterValue )
        {
            System.Diagnostics.Debug.Assert( aParagraph.Tag is DProcess );
            DProcess process = (DProcess) aParagraph.Tag;
            ProcessChunkCollection chunks = process.Chunks;
            //
            int count = chunks.Count;
            if ( count > 0 )
            {
                ProcessChunk lastEntry = chunks[ count - 1 ];
                lastEntry.AccessCount = aParameterValue;
            }
        }
        #endregion

        #region Internal methods
        private ParserParagraph CreateMonitorProcessCommon( string aName, DProcess aProcess )
        {
            ParserParagraph para = new ParserParagraph( aName );
            //
            ParserLine l1 = ParserLine.NewSymFormat( "ExitInfo %d,%d,%lS\r\n" );
            l1.SetTargetProperties( aProcess.ExitInfo, "Type", "Reason", "Category" );
            //
            ParserLine l2 = ParserLine.NewSymFormat( "Flags %08x, Handles %08x, Attributes %08x\r\n" );
            l2.SetTargetProperties( aProcess, "Flags", "Handles", "Attributes" );
            //
            ParserLine l3 = ParserLine.NewSymFormat( "DataBssChunk %08x, CodeSeg %08x\r\n" );
            l3.SetTargetProperties( aProcess, "DataBssStackChunkAddress", "CodeSegAddress" );
            //
            ParserLine l4 = ParserLine.NewSymFormat( "DllLock %08x, Process Lock %08x SID %08x\r\n" );
            l4.SetTargetProperties( new object[] { aProcess.LockInfo, aProcess.LockInfo, aProcess }, "DllMutexAddress", "ProcessMutexAddress", "SID" );
            //
            ParserLine l5 = ParserLine.NewSymFormat( "TempCodeSeg %08x CodeSeg %08x Capability %08x %08x\r\n" );
            l5.SetTargetProperties( new object[] { aProcess, aProcess, aProcess.Capabilities, aProcess.Capabilities }, "TempCodeSegAddress", "CodeSegAddress", "HighDWord", "LowDWord" );
            //
            ParserLine l6 = ParserLine.NewSymFormat( "Id=%d" );
            l6.SetTargetProperties( aProcess, "Id" );

            para.Add( l1, l2, l3, l4, l5, l6 );
            return para;
        }

        private ParserParagraph CreateMonitorProcessCodeSegs( string aName, DProcess aProcess )
        {
            ParserParagraph para = new ParserParagraph( aName );
            para.Tag = aProcess;

            // Loop body - construct this first as we use it for the header line
            ParserLine l2 = ParserLine.NewSymFormat( "%2d: seg=%08x lib=%08x\r\n" );
            l2[ 0 ].SetTargetObject();
            l2.SetTargetMethod( this, "AddCodeSegToProcess" );

            // Loop header
            ParserLine l1 = ParserLine.NewSymFormat( "CodeSegs: Count=%d\r\n" );
            l1.SetTargetMethod( l2, "SetRepetitions" );

            para.Add( l1, l2 );
            return para;
        }

        private ParserParagraph CreateMonitorProcessMemModelMultiple( string aName, DProcess aProcess, ParserElementBase.ElementCompleteHandler aLastFieldHandler )
        {
            ParserParagraph para = new ParserParagraph( aName );
            para.Tag = aProcess;
            if ( aLastFieldHandler != null )
            {
                para.ElementComplete += new ParserElementBase.ElementCompleteHandler( aLastFieldHandler );
            }

            // Misc
            ParserLine l0 = ParserLine.NewSymFormat( "OS ASID=%d, LPD=%08x, GPD=%08x\r\n" );
            l0.SetTargetProperties( aProcess, "OSASID", "LPD", "GPD" );

            // Loop body - construct this first as we use it for the header line
            ParserLine l2 = ParserLine.NewSymFormat( "%d: Chunk %08x, access count %d\r\n" );
            l2[ 0 ].SetTargetObject();
            l2[ 1 ].SetTargetMethod( this, "AddChunkToProcess" );
            l2[ 2 ].SetTargetMethod( this, "SetChunkAccessCount" );

            // Loop header
            ParserLine l1 = ParserLine.NewSymFormat( "ChunkCount=%d ChunkAlloc=%d\r\n" );
            l1[ 0 ].SetTargetMethod( l2, "SetRepetitions" );

            para.Add( l0, l1, l2 );
            return para;
        }
        #endregion
    }
}
