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
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using SymbianUtils;
using SymbolLib.Engines;
using SymbolLib.CodeSegDef;

namespace SymbolLib.SearchAndReplace
{
	public class SymbolicSearchAndReplaceParser : AsyncTextFileReader
	{
		#region Constructors & destructor
		public SymbolicSearchAndReplaceParser( SymbolManager aSymbolManager, string aSourceFile, string aDestinationFile )
        :   this( aSymbolManager, aSourceFile, aDestinationFile, string.Empty )
		{
		}
        
        public SymbolicSearchAndReplaceParser( SymbolManager aSymbolManager, string aSourceFile, string aDestinationFile, string aPrefix )
            : base( aSourceFile, new AsyncTextReaderPrefix( aPrefix ), true )
        {
            iSymbolManager = aSymbolManager;
            iWriter = new StreamWriter( aDestinationFile, false );
        }
        #endregion

        #region API
        public void SearchAndReplace()
        {
            base.AsyncRead();
        }
        #endregion

        #region Properties
        public SymbolManager SymbolManager
		{
			get { return iSymbolManager; }
		}
		#endregion

		#region From AsyncTextReaderBase
        protected override void HandleFilteredLine( string aLine )
		{
			const string KHexChars = "abcdefABCDEF1234567890";

            int startPos = 0;
            StringBuilder line = new StringBuilder();

            CodeSegDefinition def = iSymbolManager.ROFSEngine.DefinitionParser.ParseAndResolveDefinition( aLine );
            if ( def != null )
            {
                SymbolManager.LoadDynamicCodeSegment( def, TSynchronicity.ESynchronous );
                line.Append( aLine );
            }
            else
            {
                // Look through the line looking for 0,1,2,3,4,5,6,7,8,9,a,b,c,d,e,f in runs of 8 characters
                MatchCollection collection = iAddressRegEx.Matches( aLine );
                if ( collection != null && collection.Count > 0 )
                {
                    foreach ( Match m in collection )
                    {
                        // Now get the stack address
                        CaptureCollection captures = m.Captures;
                        foreach ( Capture capture in captures )
                        {
                            string matchText = capture.Value.Trim();

                            // Take all the initial text
                            int capturePos = capture.Index;

                            // Check whether it is a discrete word
                            bool checkForSymbolMatch = true;
                            if ( capturePos > 0 )
                            {
                                // Check previous character wasn't a match from our group
                                char prevCharacter = aLine[ capturePos - 1 ];
                                checkForSymbolMatch = ( KHexChars.IndexOf( prevCharacter ) < 0 );
                            }
                            if ( checkForSymbolMatch && ( capturePos + matchText.Length < aLine.Length ) )
                            {
                                // Check next character too
                                char nextCharacter = aLine[ capturePos + matchText.Length ];
                                checkForSymbolMatch = ( KHexChars.IndexOf( nextCharacter ) < 0 );
                            }

                            // Take any preceeding text...
                            if ( capturePos > 0 )
                            {
                                int length = capturePos - startPos;
                                line.Append( aLine.Substring( startPos, length ) );
                                startPos = capturePos;
                            }

                            // Always store the original text
                            line.Append( matchText );

                            // Decide if we can try to find a symbol...
                            if ( checkForSymbolMatch )
                            {
                                // And now take the text as a symbol (if we have
                                // a match).
                                long address = SymbianUtils.NumberBaseUtils.TextToDecimalNumber( matchText, NumberBaseUtils.TNumberBase.EHex );
                                Generics.GenericSymbol symbol = iSymbolManager.EntryByAddress( address );

                                if ( symbol != null )
                                {
                                    line.Append( " [ " + symbol.Symbol + " ]" );
                                }
                                else if ( iSymbolManager.AddressInRange( address ) )
                                {
                                    line.Append( " [ #UNKNOWN# ]" );
                                }
                            }
                            else
                            {
                                // Not a match, just take the original match text and move on...
                            }

                            startPos += matchText.Length;
                        }
                    }
                }

                // Remember to add anything that is left at the end...
                string remainder = aLine.Substring( startPos );
                line.Append( remainder );
            }

            iWriter.WriteLine( line.ToString() );
        }

		protected override void HandleReadCompleted()
		{
			try
			{
                if ( iWriter != null )
                {
                    iWriter.Close();
                }
                iWriter = null;
			}
			finally
			{
				base.HandleReadCompleted();
			}
		}
		#endregion

        #region From DisposableObject
        protected override void CleanupManagedResources()
        {
            try
            {
                if ( iWriter != null )
                {
                    iWriter.Close();
                }
                iWriter = null;
            }
            finally
            {
                base.CleanupManagedResources();
            }
        }
        #endregion

        #region Data members
        private readonly SymbolManager iSymbolManager;
		private static Regex iAddressRegEx = new Regex( @"[a-fA-F0-9]{8}", RegexOptions.IgnoreCase );
        private StreamWriter iWriter;
		#endregion
	}
}
 
