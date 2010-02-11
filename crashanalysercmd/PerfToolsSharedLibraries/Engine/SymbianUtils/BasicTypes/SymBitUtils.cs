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

namespace SymbianUtils.BasicTypes
{
    public static class SymBitUtils
    {
        #region API
        public static uint CreateMask( string aBinary )
        {
            uint value;
            return CreateMask( aBinary, out value );
        }

        public static uint CreateMask( string aBinary, out uint aExpectedValue )
        {
            uint mask = 0;
            aExpectedValue = 0;

            int bit = 0;
            int count = aBinary.Length;
            for ( int charIndex = count - 1; charIndex >= 0; charIndex-- )
            {
                // Get a character from the string
                char c = Char.ToLower( aBinary[ charIndex ] );
                //
                if ( c == SymBitUtils.KBitIsSet )
                {
                    mask |= (uint) ( 1 << bit );
                    aExpectedValue |= (uint) ( 1 << bit );
                }
                else if ( c == SymBitUtils.KBitIsClear )
                {
                    mask |= (uint) ( 1 << bit );
                }
                else if ( c == SymBitUtils.KBitIsNotApplicable1 || c == SymBitUtils.KBitIsNotApplicable2 )
                {
                }
                //
                if ( c != SymBitUtils.KBitIsReadabilitySpacer )
                {
                    ++bit;
                }
            }
            //
            return mask;
        }

        public static byte CreateMask( string aBinary, out byte aExpectedValue )
        {
            uint value;
            uint ret = CreateMask( aBinary, out value );
            //
            if ( ret > 0xFF || value > 0xFF )
            {
                throw new ArgumentException( "Binary sequence is too large to fit byte" );
            }
            //
            aExpectedValue = (byte) value;
            return (byte) ret;
        }

        public static SymByte CreateMask( string aBinary, out SymByte aExpectedValue )
        {
            byte value = 0;
            SymByte ret = CreateMask( aBinary, out value );
            aExpectedValue = value;
            return ret;
        }

        public static string GetBits( byte aByte )
        {
            string ret = System.Convert.ToString( aByte, 2 ).PadLeft( 8, KBitIsClear );
            return ret;
        }

        public static uint StringToUint( string aText )
        {
            uint ret = 0;
            //
            int bit = 0;
            int count = aText.Length;
            for ( int charIndex = count - 1; charIndex >= 0; charIndex--, ++bit )
            {
                char c = char.ToLower( aText[ charIndex ] );
                //
                if ( c == KBitIsSet )
                {
                    uint mergeIn = (uint) ( 1 << bit );
                    ret |= mergeIn;
                }
            }
            //
            return ret;
        }

        public static string BeautifyBits( uint aAddress, int aNumberOfKnownBits )
        {
            string binAddress = System.Convert.ToString( aAddress, 2 );
            //
            binAddress = binAddress.PadLeft( 32, '0' );
            binAddress = binAddress.Substring( 32 - aNumberOfKnownBits, aNumberOfKnownBits );
            binAddress = binAddress.PadLeft( 32, 'x' );
            binAddress = BeautifyBits( binAddress );
            //
            return binAddress;
        }

        public static string BeautifyBits( string aBits )
        {
            StringBuilder ret = new StringBuilder( aBits.PadLeft( 32, 'x' ) );
            ret.Insert( 24, ' ' );
            ret.Insert( 16, ' ' );
            ret.Insert( 08, ' ' );
            return ret.ToString();
        }
        
        public static uint RotateRight( uint aValue, int aCount )
        {
            uint ret = aValue;
            //
            for ( int i = 0; i < aCount; i++ )
            {
                bool bitZeroSet = ( ( ret & 0x1 ) == 0x1 );
                ret >>= 1;
                //
                if ( bitZeroSet )
                {
                    // Wrap the bit around by appling it at the top end
                    ret |= KTopBit;
                }
            }
            //
            return ret;
        }

        public static SymBit GetBit( uint aValue, int aBitNumber )
        {
            uint mask = 1u << aBitNumber;
            uint value = aValue & mask;
            SymBit ret = ( value != 0 ) ? SymBit.ESet : SymBit.EClear;
            return ret;
        }
        #endregion

        #region Internal constants
        internal const char KBitIsSet = '1';
        internal const char KBitIsClear = '0';
        internal const char KBitIsNotApplicable1 = '#';
        internal const char KBitIsNotApplicable2 = 'x';
        internal const char KBitIsReadabilitySpacer = ' ';
        private const uint KTopBit = 0x80000000;
        #endregion
    }
}
