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
using SymbianUtils.SourceParser.Objects;
using SymbianUtils.SourceParser.Parsers;

namespace SymbianUtils.SourceParser.Parsers
{
    public class ParserSrcMethod
    {
        #region Constructors
        public ParserSrcMethod()
        {
        }
        #endregion

        #region API
        public SrcMethod Parse( ref string aText )
        {
            SrcMethod ret = null;
            //
            if ( aText.Length > 0 )
            {
                string parameters = string.Empty;
                //
                if ( ContainsParameters( aText ) )
                {
                    // This leaves modifiers intact
                    parameters = ExtractParameters( ref aText );
                }

                // Look for the class separator. If we find that, then everything after
                // it is the method name.
                //
                // If no class separator exists, then we treat the whole thing as the method
                // name.
                int pos = aText.IndexOf( SrcClass.KClassSeparator );

                // By default, treat the whole text as the class name
                string methodText = aText;
                if ( pos >= 0 )
                {
                    methodText = aText.Substring( pos + SrcClass.KClassSeparator.Length );
                    aText = aText.Substring( 0, pos + SrcClass.KClassSeparator.Length );
                }
                else
                {
                    // Everything was consumed...
                    aText = string.Empty;
                }

                // Make a new method. Work out if the method text
                // actually has any parameters
                ret = new SrcMethod();

                // Try to parse the modifiers. We extract that first
                // to leave us with just the method name and the parameters.
                bool hasModifier = ContainsModifier( methodText );
                if ( hasModifier )
                {
                    ParserSrcMethodModifier parser = new ParserSrcMethodModifier();
                    SrcMethodModifier modifier = parser.Parse( ref methodText );
                    if ( modifier != null )
                    {
                        ret.Modifier = modifier;
                    }
                }

                // Try to parse the parameters. We can also use this
                // to calculate the exact method name.
                if ( parameters.Length > 0 )
                {
                    ParserSrcMethodParameter parser = new ParserSrcMethodParameter();
                    parser.Parse( ref parameters, ret );
                }

                // What's left should be the method name followed by "()" if the
                // 'method' wasn't a label.
                if ( ContainsParameters( methodText ) )
                {
                    // Discard "()";
                    pos = methodText.LastIndexOf( "(" );
                    methodText = methodText.Substring( 0, pos );
                }

                ret.Name = methodText;
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private static bool ContainsParameters( string aText )
        {
            // Search initiall for '(' - if that is found, then
            // we should also find a closing bracket.
            bool parameters = false;
            int openingBracketPos = aText.IndexOf( "(" );
            //
            if ( openingBracketPos > 0 )
            {
                // Should also be a closing bracket and it should
                // appear after the opening bracket
                int closingBracketPos = aText.LastIndexOf( ")" );
                parameters = ( closingBracketPos > openingBracketPos );
            }
            //
            return parameters;
        }

        private static bool ContainsModifier( string aText )
        {
            bool modifiers = false;

            int openingBracketPos = aText.IndexOf( "(" );
            if ( openingBracketPos >= 0 )
            {
                int closingBracketPos = openingBracketPos;
                SymbianUtils.Strings.StringParsingUtils.SkipToEndOfSection( ref aText, ref closingBracketPos, '(', ')' );

                if ( closingBracketPos > openingBracketPos )
                {
                    // everything here on is the modifier text;
                    string modifierText = aText.Substring( closingBracketPos + 1 );
                    modifiers = ( modifierText.Trim().Length > 0 );
                }
            }
            //
            return modifiers;
        }

        private static string ExtractParameters( ref string aText )
        {
            const string KOperatorChevronText = "operator <<";

            // DoAppendFormatList<TDes16, (int)2>(T1&, const T3&, std::__va_list, T2*)
            // DoAppendFormatList<TDes16, (int)2>(T1&, TBuf<(int)256>, std::__va_list, T2*)
            // Method<TDes16>::Wibble( something )
            // Method::Wibble( RPointerArray<HBufC> )
            // RTest::operator ()(int, int, const unsigned short*)
            // TDesC16::Left(int) const
            // CObjectCon::AtL(int) const
            // User::Panic(const TDesC16&, int)
            // operator <<(RWriteStream&, const unsigned char&)

            // Handle special case of "operator <<" confusing matters
            string workingText = aText;
            int operatorOpeningChevronPos = aText.IndexOf( KOperatorChevronText );
            if ( operatorOpeningChevronPos >= 0 )
            {
                aText = aText.Substring( 0, operatorOpeningChevronPos + KOperatorChevronText.Length );
                workingText = workingText.Substring( operatorOpeningChevronPos + KOperatorChevronText.Length );
            }
            else
            {
                aText = string.Empty;
            }

            string ret = string.Empty;
            //
            int closingPos = 0;
            int openingPos = 0;
            int templatePos = 0;
            //
            while ( openingPos >= 0 )
            {
                if ( templatePos >= 0 )
                    templatePos = workingText.IndexOf( "<", templatePos );
                openingPos = workingText.IndexOf( "(", openingPos );

                if ( templatePos >= 0 && templatePos < openingPos )
                {
                    // Template region appears before the next bracket. Skip
                    // over all characters until we hit the end of the template
                    // section
                    int endingPos = templatePos;
                    SymbianUtils.Strings.StringParsingUtils.SkipToEndOfSection( ref workingText, ref endingPos, '<', '>' );

                    if ( endingPos < 0 )
                    {
                        // Matching closing brace was never found - dealing with operator << ?
                        templatePos = -1;
                    }
                    else
                    {
                        // Something like DoAppendFormatList<TDes16, (int)2>(T1&, const T3&, std::__va_list, T2*) ???
                        templatePos = endingPos;
                        openingPos = endingPos;
                    }
                }
                else if ( openingPos >= 0 )
                {
                    // Skipped over any template nonsense. Work backward from the end 
                    // in order to locate start of parameters.
                    closingPos = workingText.LastIndexOf( ')' );
                    openingPos = closingPos;
                    SymbianUtils.Strings.StringParsingUtils.SkipToBeginningOfSection( ref workingText, ref openingPos, '(', ')' );

                    string parameters = workingText.Substring( openingPos + 1, ( closingPos - openingPos ) - 1 ).Trim();
                    ret = parameters;
                    workingText = workingText.Substring( 0, openingPos + 1 ) + workingText.Substring( closingPos );
                    aText = aText + workingText;
                    break;
                }
            }
            //
            return ret;
        }
        #endregion

        #region Data members
        #endregion
    }
}
