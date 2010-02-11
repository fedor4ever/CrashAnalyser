/*
* Copyright (c) 2009 Nokia Corporation and/or its subsidiary(-ies). 
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
using SymbianUtils;
using SymbianParserLib.Engine;
using SymbianParserLib.Elements;
using SymbianStructuresLib.CodeSegments;
using SymbianStackLib.Engine;

namespace SymbianStackLib.Prefixes
{
	public class StackPrefixManager
	{
		#region Constructors
        internal StackPrefixManager( StackEngine aEngine )
		{
            iEngine = aEngine;
            //
            PreparePrefixes_CodeSegment();
            PreparePrefixes_Pointer();
            //
            iParserEntries.Add( iPrefixEngine_Pointer );
            iParserEntries.Add( iPrefixEngine_CodeSegment );
        }
		#endregion

		#region API
        public void SetCustomPointer( string aPrefix )
        {
            // Find any existing custom entry and remove it
            int paraCount = iPrefixEngine_Pointer.Count;
            System.Diagnostics.Debug.Assert( paraCount >= 1 );
            if ( paraCount > 1 )
            {
                iPrefixEngine_Pointer.RemoveRange( 1 );
            }

            string prefixText = aPrefix.Trim();
            if ( prefixText != string.Empty )
            {
                ParserParagraph para = new ParserParagraph( "Dynamic_Pointer" );
                //
                prefixText += "%08x";
                //
                ParserLine l1 = ParserLine.NewSymFormat( prefixText );
                l1.SetTargetProperty( iEngine.AddressInfo, "Pointer" );
                para.Add( l1 );
                iPrefixEngine_Pointer.Add( para );
            }
        }

        public void SetCustomCodeSegment( string aPrefix )
        {
            // Find any existing custom entry and remove it
            int paraCount = iPrefixEngine_CodeSegment.Count;
            System.Diagnostics.Debug.Assert( paraCount >= 1 );
            if ( paraCount > 1 )
            {
                iPrefixEngine_CodeSegment.RemoveRange( 1 );
            }

            string prefixText = aPrefix.Trim();
            if ( prefixText != string.Empty )
            {
                ParserParagraph para = new ParserParagraph( "Dynamic_CodeSegment" );
                //
                prefixText += "%08x-%08x %S";
                //
                ParserLine l1 = ParserLine.NewSymFormat( prefixText );
                l1.SetTargetMethod( this, "TryToParseCodeSegment" );
                l1.DisableWhenComplete = false;
                para.Add( l1 );
                para.DisableWhenComplete = false;
                iPrefixEngine_CodeSegment.Add( para );
            }
        }
        #endregion

        #region Properties
        public string CodeSegment
        {
            get
            {
                string ret = string.Empty;
                //
                int count = iPrefixEngine_CodeSegment.Count;
                if ( count > 0 )
                {
                    ParserParagraph para = iPrefixEngine_CodeSegment[ count - 1 ];
                    count = para.Count;
                    if ( count > 0 )
                    {
                        ParserLine line = para[ count - 1 ];
                        //
                        if ( line.LineType == ParserLine.TLineType.ELineTypeSimpleStringMatch )
                        {
                            ret = line.OriginalValue;
                        }
                    }
                }
                //
                return ret;
            }
        }

        public string Pointer
        {
            get
            {
                string ret = "CurrentSP - ";
                //
                int count = iPrefixEngine_Pointer.Count;
                if ( count > 0 )
                {
                    ParserParagraph para = iPrefixEngine_Pointer[ count - 1 ];
                    count = para.Count;
                    if ( count > 0 )
                    {
                        ParserLine line = para[ count - 1 ];
                        //
                        if ( line.LineType == ParserLine.TLineType.ELineTypeSimpleStringMatch )
                        {
                            ret = line.OriginalValue;
                        }
                    }
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        internal void TryAgainstPrefixes( string aLine )
        {
            string line = aLine;
            foreach ( ParserEngine engine in iParserEntries )
            {
                if ( engine.OfferLine( ref line ) )
                {
                    break;
                }
            }
        }

        private void TryToParseCodeSegment( ParserLine aLine )
        {
            string line = aLine.GetCurrentLine();
            CodeSegDefinition codeSegDef = CodeSegDefinitionParser.ParseDefinition( line );
            if ( codeSegDef != null )
            {
                iEngine.CodeSegments.Add( codeSegDef );
            }
        }

        private void PreparePrefixes_Pointer()
        {
            ParserParagraph para = new ParserParagraph( "Fixed_Pointer" );
            //
            ParserLine l1 = ParserLine.NewSymFormat( "CurrentSP - 0x%08x" );
            l1.SetTargetProperty( iEngine.AddressInfo, "Pointer" );
            //
            ParserLine l2 = ParserLine.NewSymFormat( "CurrentSP - %08x" );
            l2.SetTargetProperty( iEngine.AddressInfo, "Pointer" );
            //
            ParserLine l3 = ParserLine.NewSymFormat( "CurrentSP: 0x%08x" );
            l3.SetTargetProperty( iEngine.AddressInfo, "Pointer" );
            //
            ParserLine l4 = ParserLine.NewSymFormat( "CurrentSP: %08x" );
            l4.SetTargetProperty( iEngine.AddressInfo, "Pointer" );
            //
            ParserLine l5 = ParserLine.NewSymFormat( "r12=%08x %08x %08x %08x" );
            l5[1].SetTargetProperty( iEngine.AddressInfo, "Pointer" );
            //
            ParserLine l6 = ParserLine.NewSymFormat( "Current stack pointer: 0x%08x" );
            l6.SetTargetProperty( iEngine.AddressInfo, "Pointer" );
            //
            ParserLine l7 = ParserLine.NewSymFormat( "sp: 0x%08x" );
            l7.SetTargetProperty( iEngine.AddressInfo, "Pointer" );
            //
            ParserLine l8 = ParserLine.NewSymFormat( "sp: %08x" );
            l8.SetTargetProperty( iEngine.AddressInfo, "Pointer" );
            //
            para.Add( l1, l2, l3, l4, l5, l6, l7, l8 );
            //
            iPrefixEngine_Pointer.Add( para );
        }

        private void PreparePrefixes_CodeSegment()
        {
            ParserParagraph para = new ParserParagraph( "Fixed_CodeSegment" );
            //
            ParserLine l1 = ParserLine.NewSymFormat( "%08x-%08x %S" );
            l1.SetTargetMethod( this, "TryToParseCodeSegment" );
            l1.DisableWhenComplete = false;
            //
            para.Add( l1 );
            para.DisableWhenComplete = false;
            //
            iPrefixEngine_CodeSegment.Add( para );
        }
        #endregion

        #region Constants
		#endregion

		#region Data members
        private readonly StackEngine iEngine;
        private ParserEngine iPrefixEngine_Pointer = new ParserEngine();
        private ParserEngine iPrefixEngine_CodeSegment = new ParserEngine();
        private List<ParserEngine> iParserEntries = new List<ParserEngine>();
		#endregion
    }
}
