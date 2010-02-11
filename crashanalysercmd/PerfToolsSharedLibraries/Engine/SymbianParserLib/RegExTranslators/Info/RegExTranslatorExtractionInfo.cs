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
using SymbianParserLib.Enums;

namespace SymbianParserLib.RegExTranslators
{
    internal class RegExTranslatorExtractionInfo
    {
        #region Constructors
        public RegExTranslatorExtractionInfo( string aLine, int aStartPos )
        {
            iMatch = KRegEx.Match( aLine, aStartPos );
        }
        #endregion

        #region API
        #endregion

        #region Constants
        public const int KNoWidthSpecified = -1;
        #endregion

        #region Properties
        public bool Success
        {
            get { return Match.Success; }
        }

        public Match Match
        {
            get { return iMatch; }
        }

        public char ValueTypeChar
        {
            get
            {
                char ret = '\0';
                //
                if ( iMatch.Success )
                {
                    Group gpType = iMatch.Groups[ KGroupTypeChar ];
                    if ( gpType.Success )
                    {
                        string type = gpType.Value.ToUpper();
                        ret = type[ 0 ];
                    }
                }
                //
                return ret;
            }
        }

        public TParserValueType ValueType
        {
            get
            {
                TParserValueType ret = TParserValueType.EValueTypeUnknown;
                //
                if ( iMatch.Success )
                {
                    char typeChar = ValueTypeChar;
                    switch ( typeChar )
                    {
                    case 'U':
                        ret = TParserValueType.EValueTypeUint32;
                        break;
                    case 'S':
                        ret = TParserValueType.EValueTypeString;
                        break;
                    case 'D':
                        ret = TParserValueType.EValueTypeInt32;
                        break;
                    case 'X':
                        if ( IsLong )
                        {
                            ret = TParserValueType.EValueTypeUint64;
                        }
                        else
                        {
                            ret = TParserValueType.EValueTypeUint32;
                        }
                        break;
                    default:
                        break;
                    }
                }
                //
                return ret;
            }
        }

        public string ValuePrefix
        {
            get
            {
                string ret = string.Empty;
                //
                if ( iMatch.Success )
                {
                    Group gpValuePrefix = iMatch.Groups[ KGroupValuePrefix ];
                    if ( gpValuePrefix.Success )
                    {
                        ret = gpValuePrefix.Value;
                    }
                }
                //
                return ret;
            }
        }

        public bool IsValuePrefixHex
        {
            get
            {
                string prefix = ValuePrefix;
                bool ret = ( prefix == KValuePrefixHex1 || prefix == KValuePrefixHex2 );
                return ret;
            }
        }

        public int NumberBase
        {
            get
            {
                int ret = 0;
                //
                if ( iMatch.Success )
                {
                    if ( ValueType != TParserValueType.EValueTypeString )
                    {
                        char typeChar = ValueTypeChar;
                        switch ( typeChar )
                        {
                            case 'U':
                            case 'D':
                                ret = KNumberBaseDecimal;
                                break;
                            case 'X':
                                ret = KNumberBaseHexadecimal;
                                break;
                            default:
                                break;
                        }
                    }
                }
                //
                return ret;
            }
        }

        public bool IsLong
        {
            get
            {
                // The 'long' (L) specifier gets shoehorned into the width group
                bool ret = false;
                //
                if ( iMatch.Success )
                {
                    Group gpWidth = iMatch.Groups[ KGroupWidth ];
                    if ( gpWidth.Success )
                    {
                        string val = gpWidth.Value.Trim().ToUpper();
                        ret = ( val == "L" );
                    }
                }
                //
                return ret;
            }
        }

        public int Width
        {
            get
            {
                int ret = KNoWidthSpecified;
                //
                if ( iMatch.Success )
                {
                    Group gpWidth = iMatch.Groups[ KGroupWidth ];
                    if ( gpWidth.Success )
                    {
                        string val = gpWidth.Value.Trim();
                        if ( val.Length != 0 )
                        {
                            Match m = KNumericNumberRegex.Match( val );
                            if ( m.Success )
                            {
                                ret = System.Convert.ToInt32( m.Value );
                            }
                        }
                    }
                }
                //
                return ret;
            }
        }

        public int CapturePos
        {
            get
            {
                Group gpPercent = Match.Groups[ KGroupPercent ];
                int pos = gpPercent.Index;
                return pos;
            }
        }

        public int CaptureLength
        {
            get
            {
                // [StartPos] ..... [% Pos] .... [EndPos]
                //                  <------------------->       = what we return

                int startPos = Match.Index;
                int length = Match.Length;
                int endPos = startPos + length;
                int percentPos = CapturePos;
                int ret = ( endPos - percentPos );
                return ret;
            }
        }

        public string Name
        {
            get
            {
                string ret = string.Empty;
                //
                if ( Success )
                {
                    Group propertyName = Match.Groups[ KGroupPropertyName ];
                    ret = propertyName.Value.Trim();
                }
                //
                return ret;
            }
        }

        public string PadChar
        {
            get
            {
                string ret = string.Empty;
                //
                if ( Success )
                {
                    Group pad = Match.Groups[ KGroupPadChar ];
                    ret = pad.Value.Trim();
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        private const string KGroupValuePrefix = "ValuePrefix";
        private const string KGroupPropertyName = "PropertyName";
        private const string KGroupPadChar = "PadChar";
        private const string KGroupWidth = "Width";
        private const string KGroupTypeChar = "TypeChar";
        private const string KGroupPercent = "Percent";
        private const string KValuePrefixHex1 = "0x";
        private const string KValuePrefixHex2 = "x";
        private const int KNumberBaseDecimal = 10;
        private const int KNumberBaseHexadecimal = 16;
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        /// <summary>
        ///  Regular expression built for C# on: Tue, May 20, 2008, 12:54:34 PM
        ///  Using Expresso Version: 3.0.2766, http://www.ultrapico.com
        ///  
        ///  A description of the regular expression:
        ///  
        ///  [PropertyName]: A named capture group. [(?:\w+\s*)+?], zero or one repetitions
        ///      Match expression but don't capture it. [\w+\s*], one or more repetitions, as few as possible
        ///          \w+\s*
        ///              Alphanumeric, one or more repetitions
        ///              Whitespace, any number of repetitions
        ///  Comment: Property name, essentially captures words separated by whitespace
        ///  Match expression but don't capture it. [\s*]
        ///      Whitespace, any number of repetitions
        ///  Comment: More whitespace
        ///  Match expression but don't capture it. [-], zero or one repetitions
        ///      -
        ///  Comment: Ignore any leading minus char
        ///  Match expression but don't capture it. [\s*]
        ///      Whitespace, any number of repetitions
        ///  Comment: More whitespace
        ///  Comment: Next parse and discard any optional assignment prefix - such a ";" or "="
        ///  Match expression but don't capture it. [\s*]
        ///      Whitespace, any number of repetitions
        ///  Match expression but don't capture it. [=|\x3A], zero or one repetitions
        ///      Select from 2 alternatives
        ///          =
        ///          Hex 3A
        ///  Match expression but don't capture it. [\s*]
        ///      Whitespace, any number of repetitions
        ///  [ValuePrefix]: A named capture group. [0x|x], zero or one repetitions
        ///      Select from 2 alternatives
        ///          0x
        ///              0x
        ///          x
        ///  Match expression but don't capture it. [%{1}]
        ///      %, exactly 1 repetitions
        ///  [PadChar]: A named capture group. [(?:0| )?]
        ///      Match expression but don't capture it. [0| ], zero or one repetitions
        ///          Select from 2 alternatives
        ///              0
        ///              NULL
        ///  [Width]: A named capture group. [(?:[0-9]?|l)]
        ///      Match expression but don't capture it. [[0-9]?|l]
        ///          Select from 2 alternatives
        ///              Any character in this class: [0-9], zero or one repetitions
        ///              l
        ///  [TypeChar]: A named capture group. [d|D|x|X|u|U|s|S]
        ///      Select from 8 alternatives
        ///          dDxXuUsS
        ///  
        ///
        /// </summary>
        private static readonly Regex KRegEx = new Regex(
              "(?<PropertyName>(?:\\w+\\s*)+?)? # Property name, essentiall" +
              "y captures words separated by whitespace\r\n(?:\\s*) # More wh" +
              "itespace\r\n(?:-)? # Ignore any leading minus char\r\n(?:\\s*) #" +
              " More whitespace\r\n\r\n# Next parse and discard any optional as" +
              "signment prefix - such a \";\" or \"=\"\r\n(?:\\s*)(?:=|\\x3A)" +
              "?(?:\\s*) \r\n\r\n(?<ValuePrefix>0x|x)?(?<Percent>%{1})\r\n\r\n(?<PadChar>(?" +
              ":0| )?)(?<Width>(?:[0-9]?|l))(?<TypeChar>d|D|x|X|u|U|s|S|i|I)",
            RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );

        private static readonly Regex KNumericNumberRegex = new Regex( @"[0-9]", RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled );
        private readonly Match iMatch;
        #endregion
    }
}
