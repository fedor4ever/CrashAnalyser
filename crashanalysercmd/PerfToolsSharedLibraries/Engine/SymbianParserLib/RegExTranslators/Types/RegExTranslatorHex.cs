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
using SymbianParserLib.Elements;
using SymbianParserLib.Enums;

namespace SymbianParserLib.RegExTranslators.Types
{
    internal class RegExTranslatorHex : RegExTranslatorBase
    {
        #region Constructors
        public RegExTranslatorHex()
        {
        }
        #endregion

        #region API
        public override ParserField Process( Capture aCapture, int aStartAt, ParserLine aLine )
        {
            ParserField ret = null;
            //
            RegExTranslatorExtractionInfo m = new RegExTranslatorExtractionInfo( aLine.OriginalValue, aStartAt );
            //
            if ( m.Success && m.ValueTypeChar == 'X' )
            {
                // Build the regular expression
                StringBuilder regex = new StringBuilder( "[A-Fa-f0-9]" );
                if ( m.Width != RegExTranslatorExtractionInfo.KNoWidthSpecified )
                {
                    // Add specific length suffix
                    regex.Append( "{" );
                    regex.Append( m.Width.ToString() );
                    regex.Append( "}" );
                }
                else
                {
                    // Add "one or more" suffix
                    regex.Append( "+" );
                }

                ret = CreateField( regex.ToString(), m.Name, m.ValueType, m.CapturePos, m.CaptureLength, true );
                ret.FormatSpecifier.NumberBase = m.NumberBase;
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        /// <summary>
        ///  Regular expression built for C# on: Thu, May 15, 2008, 11:33:33 AM
        ///  Using Expresso Version: 3.0.2766, http://www.ultrapico.com
        ///  
        ///  A description of the regular expression:
        ///  
        ///  %
        ///  [PadChar]: A named capture group. [(?:0| )?]
        ///      Match expression but don't capture it. [0| ], zero or one repetitions
        ///          Select from 2 alternatives
        ///              0
        ///              Space
        ///  [Width]: A named capture group. [[0-9]?]
        ///      Any character in this class: [0-9], zero or one repetitions
        ///  Match expression but don't capture it. [x|X]
        ///      Select from 2 alternatives
        ///          xX
        ///  
        ///
        /// </summary>
        private static Regex KRegEx = new Regex(
              "%(?<PadChar>(?:0| )?)(?<Width>[0-9]?)(?:x|X)",
            RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.Compiled
            );
        #endregion
    }
}
