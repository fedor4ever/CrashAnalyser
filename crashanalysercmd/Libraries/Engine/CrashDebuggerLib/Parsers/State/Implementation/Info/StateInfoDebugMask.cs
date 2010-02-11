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
using CrashDebuggerLib.Structures.DebugMask;
using SymbianParserLib.Elements;

namespace CrashDebuggerLib.Parsers.State.Implementation
{
    internal class StateInfoDebugMask : State
    {
        #region Constructors
        public StateInfoDebugMask( CrashDebuggerParser aParser )
            : base( aParser )
        {
        }
        #endregion

        #region API
        public override void Prepare()
        {
            PrepareMandatoryParagraph();
        }

        public override void Finalise()
        {
            System.Diagnostics.Debug.Assert( ParserEngine.Count == 1 ); // paragraphs
            ParserParagraph para = ParserEngine[ 0 ];
            System.Diagnostics.Debug.Assert( para.Count == 8 ); // lines

            // Go through each line and pull out the debug mask.
            for ( int i = 0; i < 4; i++ )
            {
                int baseIndex = ( i * 2 );
                
                // Get lines and check that each has one field
                ParserLine line1 = para[ baseIndex ];
                System.Diagnostics.Debug.Assert( line1.Count == 1 );
                System.Diagnostics.Debug.Assert( line1[ 0 ].IsUint );
                ParserLine line2 = para[ baseIndex + 1 ];
                System.Diagnostics.Debug.Assert( line2.Count == 1 );
                System.Diagnostics.Debug.Assert( line2[ 0 ].IsUint );
                //
                uint val1 = line1[ 0 ].AsUint;
                uint val2 = line2[ 0 ].AsUint;
                ulong combined = val1 + ( val2 << 32 );
                //
                CrashDebugger.InfoDebugMask.SetValueByWordIndex( combined, baseIndex );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private void PrepareMandatoryParagraph()
        {
            ParserParagraph para = new ParserParagraph( "Debug_Mask_Info" );
            //
            ParserLine l1 = ParserLine.NewSymFormat( "DebugMask[0] = %08x\r\n" );
            ParserLine l2 = ParserLine.NewSymFormat( "DebugMask[1] = %08x\r\n" );
            ParserLine l3 = ParserLine.NewSymFormat( "DebugMask[2] = %08x\r\n" );
            ParserLine l4 = ParserLine.NewSymFormat( "DebugMask[3] = %08x\r\n" );
            ParserLine l5 = ParserLine.NewSymFormat( "DebugMask[4] = %08x\r\n" );
            ParserLine l6 = ParserLine.NewSymFormat( "DebugMask[5] = %08x\r\n" );
            ParserLine l7 = ParserLine.NewSymFormat( "DebugMask[6] = %08x\r\n" );
            ParserLine l8 = ParserLine.NewSymFormat( "DebugMask[7] = %08x\r\n" );
            para.Add( l1, l2, l3, l4, l5, l6, l7, l8 );
            ParserEngine.Add( para );
        }
        #endregion

        #region Data members
        #endregion
    }
}
