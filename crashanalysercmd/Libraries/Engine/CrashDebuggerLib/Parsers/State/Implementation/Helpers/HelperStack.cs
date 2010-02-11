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
    internal class HelperStack
    {
        #region Constructors
        public HelperStack()
        {
        }
        #endregion

        #region API
        public void CreateStackParagraphs( ParserEngine aEngine, DThread aThread )
        {
            ParserParagraph p1 = PrepareUserStack( aThread.StackInfoUser );
            ParserParagraph p2 = PrepareSupervisorStack( aThread.StackInfoSupervisor );
            //
            aEngine.Add( p1, p2 );
        }
        #endregion

        #region Internal constants
        private const string KParagraphUser = "STACK_USER";
        #endregion

        #region Call-back methods
        void NoUserStackCallBack( ParserElementBase aElement )
        {
            // Called back when a thread has no user stack - in which case, dump the
            // user-stack items as they will prevent us gathering the supervisor info.
            ParserLine line = (ParserLine) aElement;
            ParserParagraph para = line.Paragraph;
            //
            foreach ( ParserLine l in para )
            {
                l.IsDisabled = true;
            }
        }

        void DisableUserStackParagraph( ParserElementBase aElement )
        {
            // Called back when the first supervisor stack item fires. We
            // must disable all items in the user-paragraph so that they don't
            // intercept things like the current stack pointer.
            ParserLine line = (ParserLine) aElement;
            ParserParagraph para = line.Paragraph;
            ParserEngine engine = (ParserEngine) para.Parent;
            //
            foreach ( ParserParagraph paragraph in engine )
            {
                if ( paragraph.Name == KParagraphUser )
                {
                    paragraph.IsDisabled = true;
                    break;
                }
            }
        }

        void SetFirstStackBytesStartingAddress( ParserField aField, uint aValue )
        {
            ThreadStackInfo stackInfo = (ThreadStackInfo) aField.Tag;
            ThreadStackData stackData = stackInfo.Data;
            stackData.SetStartingAddress( aValue );
        }
        #endregion

        #region Internal methods
        private ParserParagraph PrepareUserStack( ThreadStackInfo aStackInfo )
        {
            ParserParagraph para = new ParserParagraph( KParagraphUser );
            //
            ParserLine l0 = ParserLine.New( "No user-mode stack" );
            l0.ElementComplete += new ParserElementBase.ElementCompleteHandler( NoUserStackCallBack );
            //
            ParserLine l1 = ParserLine.NewSymFormat( "User stack base at %08x, size == %x\r\n" );
            l1.SetTargetProperties( aStackInfo, "BaseAddress", "Size" );
            //
            ParserLine l2 = ParserLine.NewSymFormat( "Stack pointer == %08x\r\n" );
            l2.SetTargetProperties( aStackInfo, "StackPointer" );
            //
            // Not needed - ParserLine l3 = ParserLine.NewSymFormat( "Stack mapped at %08x\r\n" );
            //l3.SetTargetProperties( aStackInfo.Data, "MappedAddress" );

            // Collect the raw stack bytes
            ParserLine l4 = ParserLine.NewSymFormat( "%08x: %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x" );
            l4.IsNeverEnding = true;
            l4.DisableWhenComplete = false;
            l4.SetTargetMethod( aStackInfo.Data, "Add" );

            // Record the starting address of the stack data
            l4[ 0 ].SetTargetMethod( this, "SetFirstStackBytesStartingAddress" );
            l4[ 0 ].Tag = aStackInfo;
            //
            para.Add( l0, l1, l2, l4 );
            return para;
        }

        private ParserParagraph PrepareSupervisorStack( ThreadStackInfo aStackInfo )
        {
            ParserParagraph para = new ParserParagraph( "STACK_SUPERVISOR" );
            //
            ParserLine l1 = ParserLine.NewSymFormat( "Supervisor stack base at %08x, size == %x\r\n" );
            l1.ElementComplete += new ParserElementBase.ElementCompleteHandler( DisableUserStackParagraph );
            l1.SetTargetProperties( aStackInfo, "BaseAddress", "Size" );
            //
            ParserLine l2 = ParserLine.NewSymFormat( "Stack pointer == %08x\r\n" );
            l2.SetTargetProperties( aStackInfo, "StackPointer" );
            
            // Collect the raw stack bytes
            ParserLine l3 = ParserLine.NewSymFormat( "%08x: %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x" );
            l3.IsNeverEnding = true;
            l3.DisableWhenComplete = false;
            l3.SetTargetMethod( aStackInfo.Data, "Add" );

            // Record the starting address of the stack data
            l3[ 0 ].SetTargetMethod( this, "SetFirstStackBytesStartingAddress" );
            l3[ 0 ].Tag = aStackInfo;
            //
            para.Add( l1, l2, l3 );
            return para;
        }
        #endregion

        #region Data members
        #endregion
    }
}
