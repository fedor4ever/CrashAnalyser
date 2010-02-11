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
using CrashDebuggerLib.Structures.Fault;
using CrashDebuggerLib.Structures.CodeSeg;
using SymbianParserLib.Elements;
using SymbianParserLib.Enums;

namespace CrashDebuggerLib.Parsers.State.Implementation
{
    internal class StateContainerCodeSegs : State
    {
        #region Constructors
        public StateContainerCodeSegs( CrashDebuggerParser aParser )
            : base( aParser )
        {
        }
        #endregion

        #region API
        public override void Prepare()
        {
            PrepareEntryParser();
        }

        public override void Finalise()
        {
            
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private void PrepareEntryParser()
        {
            ParserParagraph para = new ParserParagraph( "CODE SEGS" );
            //
            ParserLine l1 = ParserLine.NewSymFormat( "\r\nCodeSeg at %08x:\r\n" );
            ParserLine l2 = ParserLine.NewSymFormat( "   FileName: %S\r\n" );
            ParserLine l3 = ParserLine.NewSymFormat( "   RunAddress: %08x\r\n" );

            para.ElementComplete += new SymbianParserLib.BaseStructures.ParserElementBase.ElementCompleteHandler( ParagraphComplete );
            //
            para.Add( l1, l2, l3 );
            ParserEngine.Add( para );
        }
        #endregion

        #region Event handlers
        void ParagraphComplete( SymbianParserLib.BaseStructures.ParserElementBase aElement )
        {
            ParserParagraph para = (ParserParagraph) aElement;
            //
            ParserField fAddress = para[ 0 ][ 0 ];
            ParserField fFileName = para[ 1 ][ 0 ];
            ParserField fRunAddress = para[ 2 ][ 0 ];
            //
            uint address = fAddress.AsUint;
            string fName = fFileName.AsString;
            uint runAddress = fRunAddress.AsUint;
            //
            CodeSegEntry entry = new CodeSegEntry( CrashDebugger, address, fName );
            entry.RunAddress = runAddress;
            CrashDebugger.CodeSegs.Add( entry );

            // Remove completed entry, add a new one
            ParserEngine.Remove( para );
            PrepareEntryParser();
        }
        #endregion

        #region Data members
        #endregion
    }
}
