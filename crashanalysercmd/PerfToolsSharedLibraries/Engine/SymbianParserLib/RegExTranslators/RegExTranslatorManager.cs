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
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using SymbianParserLib.Utilities;
using SymbianParserLib.Elements;
using SymbianParserLib.RegExTranslators.Types;

namespace SymbianParserLib.RegExTranslators
{
    public static class TheTickCounter
    {
        public static long TickCount;
    }

    internal class RegExTranslatorManager
    {
        #region Constructors
        public RegExTranslatorManager()
        {
            iTranslators.AddRange( KDefaultList );
        }
        #endregion

        #region API
        public static ParserLine PreCachedCompiledEntry( string aKey )
        {
            ParserLine ret = TheTranslator.iCache.CreateClone( aKey );
            return ret;
        }

        public static void CompileToRegularExpression( ParserLine aLine )
        {
            TheTranslator.DoCompileToRegularExpression( aLine );
        }
        #endregion

        #region Properties
        public static RegExTranslatorManager TheTranslator
        {
            get { return iTheTranslator; }
        }
        #endregion

        #region Internal methods
        private static string FixupOriginalValue( string aLine )
        {
            StringBuilder ret = new StringBuilder( ParserUtils.RemoveLineEndings( aLine ) );
            
            // Escape double percents
            ret = ret.Replace( KEscapedPercent, KReplacementPercent );

            // Escape [ and ]
            ret = EscapeCharacter( ".", ret );
            ret = EscapeCharacter( "$", ret );
            ret = EscapeCharacter( "+", ret );
            ret = EscapeCharacter( "?", ret );
            ret = EscapeCharacter( "^", ret );
            ret = EscapeCharacter( "*", ret );
            ret = EscapeCharacter( "\\", ret );
            ret = EscapeCharacter( "[", ret );
            ret = EscapeCharacter( "]", ret );
            ret = EscapeCharacter( "(", ret );
            ret = EscapeCharacter( ")", ret );
            ret = EscapeCharacter( "{", ret );
            ret = EscapeCharacter( "}", ret );

            return ret.ToString();
        }

        private static StringBuilder EscapeCharacter( string aCharacter, StringBuilder aObject )
        {
            string replaceWith = "\\" + aCharacter;
            return aObject.Replace( aCharacter, replaceWith );
        }

        private void DoCompileToRegularExpression( ParserLine aLine )
        {
            System.DateTime starTime = DateTime.Now;
            string input = aLine.OriginalValue;

            // First phase is to replace all excaped percents with a single percent
            aLine.OriginalValue = FixupOriginalValue( aLine.OriginalValue );

            // Next phase is to identify all format specifiers
            MatchCollection matches = KRegExFormatSpecifier.Matches( aLine.OriginalValue );
            if ( matches.Count != 0 )
            {
                // Get combined list of all captures spanning all matches. There must be a better way to do this?
                List<Capture> captureList = new List<Capture>();
                foreach ( Match m in matches )
                {
                    if ( m.Success )
                    {
                        foreach ( Capture cap in m.Captures )
                        {
                            captureList.Add( cap );
                        }
                    }
                }

                // Convert captures into format specifiers
                FixupFormatSpecifiers( captureList, aLine );

                // Cache entry
                iCache.Add( input, aLine );
            }
            else
            {
            }

            System.DateTime endTime = DateTime.Now;
            long tickDuration = ( ( endTime.Ticks - starTime.Ticks ) / 100 );
            TheTickCounter.TickCount += tickDuration;
        }

        private void FixupFormatSpecifiers( List<Capture> aCaptures, ParserLine aLine )
        {
            int lastCapturePos = 0;

            // Pull out all the format specifiers and build regular expressions
            int count = aCaptures.Count;
            for ( int i = 0; i < count; i++ )
            {
                Capture capture = aCaptures[ i ];

                // Process the capture, starting at the last capture pos
                lastCapturePos = TryToProcessCapture( capture, lastCapturePos, aLine );
            }

            aLine.Finalise();
        }

        private int TryToProcessCapture( Capture aCapture, int aStartAt, ParserLine aLine )
        {
            // Return the end position of the current capture.
            int ret = -1;
            //
            foreach ( RegExTranslatorBase translator in iTranslators )
            {
                ParserField field = translator.Process( aCapture, aStartAt, aLine );
                if ( field != null )
                {
                    aLine.Add( field );
                    ret = field.FormatSpecifier.OriginalLocation + field.FormatSpecifier.OriginalLength;
                    break;
                }
            }
            //
            return ret;
        }
        #endregion

        #region Internal constants
        private static readonly RegExTranslatorBase[] KDefaultList = new RegExTranslatorBase[]
            {
                new RegExTranslatorDecimal(),
                new RegExTranslatorHex(),
                new RegExTranslatorString()
            };

        private static readonly Regex KRegExFormatSpecifier = new Regex(
              "%{1}",
              RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );

        private const string KEscapedPercent = "%%";
        private const string KReplacementPercent = "%";
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        private static RegExTranslatorManager iTheTranslator = new RegExTranslatorManager();
        private List<RegExTranslatorBase> iTranslators = new List<RegExTranslatorBase>();
        private Cache.RegExTranslatorCache iCache = new SymbianParserLib.RegExTranslators.Cache.RegExTranslatorCache();
        #endregion
    }
}
