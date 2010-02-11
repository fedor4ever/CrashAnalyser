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

namespace SymbianUtils
{
	public static class PrefixParser
	{
		public static long SkipPrefixAndReadLong( string aPrefix, ref string aLine )
		{
			long ret = 0;
			//
			if	( aLine.IndexOf( aPrefix ) >= 0 )
			{
				SkipPrefix( aPrefix, ref aLine );
				ret = ReadLong( ref aLine );
			}
			//
			return ret;
		}

		public static long SkipPrefixAndReadLongHex( string aPrefix, ref string aLine )
		{
			long ret = 0;
			//
			if	( aLine.IndexOf( aPrefix ) >= 0 )
			{
				SkipPrefix( aPrefix, ref aLine );
				ret = ReadLongHex( ref aLine );
			}
			//
			return ret;
		}

		public static int SkipPrefixAndReadInt( string aPrefix, ref string aLine )
		{
			int ret = 0;
			//
			if	( aLine.IndexOf( aPrefix ) >= 0 )
			{
				SkipPrefix( aPrefix, ref aLine );
				ret = (int) ReadLong( ref aLine );
			}
			//
			return ret;
		}

		public static uint SkipPrefixAndReadUint( string aPrefix, ref string aLine )
		{
			uint ret = 0;
			//
			if	( aLine.IndexOf( aPrefix ) >= 0 )
			{
				SkipPrefix( aPrefix, ref aLine );
				ret = (uint) ReadLong( ref aLine );
			}
			//
			return ret;
		}

		public static bool SkipPrefixAndReadBool( string aPrefix, ref string aLine )
		{
			bool ret = false;
			//
			if	( aLine.IndexOf( aPrefix ) >= 0 )
			{
				SkipPrefix( aPrefix, ref aLine );
				ret = ReadBool( ref aLine );
			}
			//
			return ret;
		}

		public static void SkipPrefix( string aPrefix, ref string aLine )
		{
			int index = aLine.IndexOf( aPrefix );
			if	( index >= 0 )
				aLine = aLine.Substring( index + aPrefix.Length );
			aLine = aLine.Trim();
		}

		public static uint ReadUint( ref string aLine )
		{
            uint retUint = 0;
			long retLong = 0;
			SymbianUtils.NumberBaseUtils.TNumberBase numberBase;
			string discardedText;
            bool convertedOkay = SymbianUtils.NumberBaseUtils.TryTextToDecimalNumber( ref aLine, out discardedText, out retLong, out numberBase );
            if ( convertedOkay )
            {
                try
                {
                    retUint = System.Convert.ToUInt32( retLong );
                }
                catch ( OverflowException )
                {
                }
            }
            //
            return retUint;
		}

		public static int ReadInt( ref string aLine )
		{
            int retInt = 0;
			long retLong = 0;
			SymbianUtils.NumberBaseUtils.TNumberBase numberBase;
			string discardedText;
            bool convertedOkay = SymbianUtils.NumberBaseUtils.TryTextToDecimalNumber( ref aLine, out discardedText, out retLong, out numberBase );
            if ( convertedOkay )
            {
                try
                {
                    retInt = System.Convert.ToInt32( retLong );
                }
                catch ( OverflowException )
                {
                }
            }
            //
            return retInt;
		}

		public static long ReadLong( ref string aLine )
		{
			long ret = -1;
			SymbianUtils.NumberBaseUtils.TNumberBase numberBase;
			string discardedText;
			bool convertedOkay = SymbianUtils.NumberBaseUtils.TryTextToDecimalNumber( ref aLine, out discardedText, out ret, out numberBase );
			return ret;
		}

		public static long ReadLongHex( ref string aLine )
		{
			long ret = 0;
			string discardedText;
			bool convertedOkay = SymbianUtils.NumberBaseUtils.TryTextToDecimalNumber( ref aLine, out discardedText, out ret, SymbianUtils.NumberBaseUtils.TNumberBase.EHex );
			return ret;
		}

		public static bool ReadBool( ref string aLine )
		{
            bool ret = false;
            
            // Trim the start of the line
			aLine = aLine.TrimStart();
            string upperLine = aLine.ToUpper();
            //
            if ( upperLine == "YES" || upperLine == "TRUE" || upperLine == "1" )
            {
                ret = true;
            }
            else if ( upperLine == "NO" || upperLine == "FALSE" || upperLine == "0" )
            {
                ret = false;
            }
            else
            {
                int endIndex = 0;
                bool continueSearchingForEndIndex = true;
                for ( int i = 0; i < aLine.Length && continueSearchingForEndIndex; i++ )
                {
                    char character = aLine[ i ];
                    switch ( character )
                    {
                    case '1':
                    case '0':
                        ++endIndex;
                        break;
                    default:
                        continueSearchingForEndIndex = false;
                        break;
                    }
                }

                string boolAsString = aLine.Substring( 0, endIndex );
                //
                try
                {
                    int boolAsInt = System.Convert.ToInt32( boolAsString );
                    ret = ( boolAsInt > 0 );
                    aLine = aLine.Substring( endIndex );
                }
                catch ( Exception )
                {
                }
            }
			//
			return ret;
		}	
	}
}
