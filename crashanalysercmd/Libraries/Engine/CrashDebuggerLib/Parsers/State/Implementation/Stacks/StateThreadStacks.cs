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
using SymbianParserLib.BaseStructures;
using CrashDebuggerLib.Structures.CodeSeg;
using CrashDebuggerLib.Structures.Thread;
using CrashDebuggerLib.Structures.Process;
using CrashDebuggerLib.Structures.KernelObjects;
using CrashDebuggerLib.Threading;
using SymbianParserLib.Elements;
using SymbianParserLib.Enums;

namespace CrashDebuggerLib.Parsers.State.Implementation
{
    internal class StateThreadStacks : State
    {
        #region Constructors
        public StateThreadStacks( CrashDebuggerParser aParser )
            : base( aParser )
        {
        }
        #endregion

        #region API
        public override void Prepare()
        {
            // First, we need to know which thread we're dealing with
            // so that means we must look for the thread info
            ParserParagraph para = new ParserParagraph( "STACK_THREAD_INFO" );
            //
            ParserLine l1 = ParserLine.NewSymFormat( "STACK DATA for thread at %8x" );
            l1.SetTargetMethod( this, "SetThreadAddress" );
            para.Add( l1 );
            ParserEngine.Add( para );
        }

        public override void Finalise()
        {
            if ( iCurrentThread != null )
            {
                if ( iCurrentThread.OwningProcess != null )
                {
                    iCurrentThread.OwningProcess.PrepareDebugView();
                }
                //
                iCurrentThread.StackInfoUser.Data.BuildCallStackAsync();
                iCurrentThread.StackInfoSupervisor.Data.BuildCallStackAsync();
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private void SetThreadAddress( uint aAddress )
        {
            // Look for the thread which we're about to process...
            iCurrentThread = CrashDebugger.ThreadByAddress( aAddress );
            if ( iCurrentThread != null )
            {
                // Next we must start to process code segments for this
                // thread. These lines contain additional information about
                // the run address of the code segments, relative to the process.
                ParserParagraph para = new ParserParagraph( "STACK_THREAD_INFO_CODESEGS" );
                //
                ParserLine l1 = ParserLine.NewSymFormat( "CodeSeg[%03d/%03d] @ %08x - %08X-%08X %S" );
                l1.ElementComplete += new ParserElementBase.ElementCompleteHandler( CodeSegmentLineComplete );
                //
                para.Add( l1 );
                ParserEngine.Add( para );
            }
        }

        void CodeSegmentLineComplete( ParserElementBase aElement )
        {
            System.Diagnostics.Debug.Assert( iCurrentThread != null );
            ParserLine line = (ParserLine) aElement;
            //
            int index = line[ 0 ].AsInt;
            int count = line[ 1 ].AsInt;
            uint codeSegAddress = line[ 2 ].AsUint;
            uint startAddress = line[ 3 ].AsUint;
            uint endAddress = line[ 4 ].AsUint;
            string fileName = line[ 5 ].AsString;
            //
            DProcess process = iCurrentThread.OwningProcess;
            if ( process != null )
            {
                ProcessCodeSegCollection codeSegs = process.CodeSegments;
                ProcessCodeSeg codeSeg = codeSegs[ codeSegAddress ];
                //
                if ( codeSeg == null )
                {
                    // The code seg is not directly part of the process handle list
                    // but it is some how mapped into the process?
                    //
                    // Try looking up the underlying code seg entry details from
                    // the crash debugger data itself. It should be part of the code
                    // seg listing so this should always work.
                    codeSeg = new ProcessCodeSeg( CrashDebugger, codeSegAddress, 0 );
                    process.CodeSegments.Add( codeSeg );
                }
                //
                codeSeg.ProcessLocalRunAddress = startAddress;
                codeSeg.Size = ( endAddress - startAddress );
            }
            //
            int remaining = count - index;
            if ( remaining == 0 )
            {
                // Queue up stack data...
                iHelperStack.CreateStackParagraphs( ParserEngine, iCurrentThread );
            }
            else
            {
                // So that we capture the next line
                aElement.SetRepetitions( 1 );
            }
        }
        #endregion

        #region Data members
        private DThread iCurrentThread = null;
        private Helpers.HelperStack iHelperStack = new CrashDebuggerLib.Parsers.State.Implementation.Helpers.HelperStack();
        #endregion
    }
}
