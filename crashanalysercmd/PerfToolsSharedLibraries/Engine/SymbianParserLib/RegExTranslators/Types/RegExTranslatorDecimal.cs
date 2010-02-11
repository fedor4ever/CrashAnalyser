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
    internal class RegExTranslatorDecimal : RegExTranslatorBase
    {
        #region Constructors
        public RegExTranslatorDecimal()
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
            if ( m.Success && ( m.ValueTypeChar == 'D' || m.ValueTypeChar == 'I' || m.ValueTypeChar == 'U' ) )
            {
                bool requiresNumberedCaptureGroup = true;

                // Build the regular expression
                StringBuilder regex = new StringBuilder( "[0-9]" );
                if ( m.Width != RegExTranslatorExtractionInfo.KNoWidthSpecified )
                {
                    // (?(\b[ 0-9+-]{4}\b:)(?:[ ]{0,3}([+-]?[0-9]{1,4}):))
                    int width = m.Width;
                    string padChar = m.PadChar;

                    // This becomes very complex now. We must match the pad character
                    regex = new StringBuilder();

                    // Start a conditional group
                    regex.Append( "(?" );

                    // Start the if-statement match. The idea here is to perform a first pass
                    // that matches any numbers, signs (-/+) and pad characters without any
                    // care about the ordering. The length, however, is critical.
                    regex.AppendFormat( "([{0}0-9+-]{{1}})", padChar, width );

                    // If the match above is true, then we'll execute this 'yes' case. The
                    // idea now is to pull out the specific value, ignoring the padding.

                    // We start a non-capturing group. This is the beginning of the value we
                    // need, but we don't want to capture the padding (although we validate it
                    // exists).
                    regex.AppendFormat( "(?:" );
                    if ( padChar != string.Empty )
                    {
                        // We must allow for width-1 pad characters.
                        regex.AppendFormat( "[{0}]", padChar );
                        regex.Append( "{0," );
                        regex.Append( width );
                        regex.Append( "}" );
                    }

                    // We must now allow for a plus/minus sign. We do this within
                    // a numbered group, since this is the bit we're eventually interested in.
                    regex.Append( "(" );
                    regex.Append( "[+-]?[0-9]" );
                    regex.Append( "{1," + width + "}" );
                    regex.Append( ")" );

                    // End non-capturing group
                    regex.Append( ")" );

                    // End conditional group
                    regex.Append( ")" );

                    // We explicitly managed this ourselves
                    requiresNumberedCaptureGroup = false;
                }
                else
                {
                    // Add "one or more" suffix
                    regex.Append( "+" );
                }

                ret = CreateField( regex.ToString(), m.Name, m.ValueType, m.CapturePos, m.CaptureLength, requiresNumberedCaptureGroup );
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
        #endregion
    }
}
