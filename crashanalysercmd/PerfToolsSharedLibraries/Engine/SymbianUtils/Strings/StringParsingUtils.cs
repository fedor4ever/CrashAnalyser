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

namespace SymbianUtils.Strings
{
    public static class StringParsingUtils
    {
        public static void SkipToEndOfSection( ref string aText, ref int aStartPos, char aOpeningChar, char aClosingChar )
        {
            bool found = false;
            int charCount = 1;
            int pos = aStartPos;
            //
            while ( charCount > 0 && ++pos < aText.Length )
            {
                System.Diagnostics.Debug.Assert( pos >= 0 && pos < aText.Length );

                char character = aText[ pos ];
                if ( character == aOpeningChar )
                    ++charCount;
                else if ( character == aClosingChar )
                    --charCount;
                if ( charCount == 0 )
                {
                    found = true;
                    break;
                }
            }
            //
            aStartPos = -1;
            if ( found == true )
                aStartPos = pos;
        }

        public static void SkipToBeginningOfSection( ref string aText, ref int aStartPos, char aOpeningChar, char aClosingChar )
        {
            bool found = false;
            int charCount = 1;
            int pos = aStartPos;
            //
            while ( charCount > 0 && --pos < aText.Length )
            {
                System.Diagnostics.Debug.Assert( pos >= 0 && pos < aText.Length );

                char character = aText[ pos ];
                if ( character == aClosingChar )
                    ++charCount;
                else if ( character == aOpeningChar )
                    --charCount;
                if ( charCount == 0 )
                {
                    found = true;
                    break;
                }
            }
            //
            aStartPos = -1;
            if ( found == true )
                aStartPos = pos;
        }

        public static bool IsNumeric( string aText, bool aAllowHex )
        {
            bool ret = true;
            //
            foreach ( char c in aText )
            {
                if ( char.IsDigit( c ) )
                {
                }
                else if ( char.IsLetter( c ) && aAllowHex )
                {
                    char upper = Char.ToUpper( c );
                    const string KHexChars = "ABCDEF";
                    if ( KHexChars.IndexOf( upper ) < 0 )
                    {
                        ret = false;
                        break;
                    }
                }
                else
                {
                    ret = false;
                    break;
                }
            }
            //
            return ret;
        }

        public static string BytesToString( byte[] aBytes )
        {
            return BytesToString( aBytes, aBytes.Length );
        }

        public static string BytesToString( byte[] aBytes, int aLength )
        {
            return BytesToString( aBytes, 0, aLength );
        }

        public static string BytesToString( byte[] aBytes, int aStart, int aEnd )
        {
            StringBuilder ret = new StringBuilder();
            for ( int i = aStart; i < aEnd; i++ )
            {
                byte b = aBytes[ i ];
                ret.Append( System.Convert.ToChar( b ) );
            }
            return ret.ToString();
        }
    }
}
