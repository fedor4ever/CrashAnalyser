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
    public static class StringUtils
    {
        public static string MakeRandomString()
        {
            return MakeRandomString( 20 );
        }

        public static string MakeRandomString( int aLength )
        {
            StringBuilder ret = new StringBuilder();

            Random random = new Random( DateTime.UtcNow.Millisecond );
            for ( int i = 0; i < aLength; i++ )
            {
                char c = Convert.ToChar( Convert.ToInt32( Math.Floor( 26 * random.NextDouble() + 65 ) ) );
                ret.Append( c );
            }

            return ret.ToString();
        }

        public static bool StartsWithAny( string[] aPrefixes, string aText )
        {
            bool ret = false;
            //
            foreach ( string p in aPrefixes )
            {
                if ( aText.StartsWith( p ) )
                {
                    ret = true;
                    break;
                }
            }
            //
            return ret;
        }
    }
}
