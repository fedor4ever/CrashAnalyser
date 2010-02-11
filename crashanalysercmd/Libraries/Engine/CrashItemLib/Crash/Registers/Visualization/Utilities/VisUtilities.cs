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
using System.Text;
using System.Collections.Generic;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Registers.Visualization.Bits;

namespace CrashItemLib.Crash.Registers.Visualization.Utilities
{
    public static class VisUtilities
    {
        public static IEnumerable<CIRegisterVisBit> ExtractBits( CIContainer aContainer, uint aValue, string aByte3Mask, string aByte2Mask, string aByte1Mask, string aByte0Mask )
        {
            return ExtractBits( aContainer, aValue, aByte3Mask + aByte2Mask + aByte1Mask + aByte0Mask );
        }

        public static IEnumerable<CIRegisterVisBit> ExtractBits( CIContainer aContainer, uint aValue, string aMask )
        {
            List<CIRegisterVisBit> ret = new List<CIRegisterVisBit>();
            //
            int shift = 0;
            int sigBitCount = 0;
            uint mask = MakeMask( aMask, out shift, out sigBitCount );
            uint maskedValue = ( aValue & mask ) >> shift;

            // Now make bits
            for ( int i = sigBitCount-1; i >= 0; i-- )
            {
                mask = (uint) ( 1u << i );
                uint value = ( maskedValue & mask ) >> i;

                CIRegisterVisBit bit = new CIRegisterVisBit( aContainer );
                if ( value != 0 )
                {
                    bit.Value = TBit.EBitSet;
                }
                ret.Add( bit );
            }

            return ret.ToArray();
        }

        public static TBit ExtractBit( uint aValue, string aByte1Mask, string aByte0Mask )
        {
            string b0 = EnsureByteMask( aByte0Mask );
            string b1 = EnsureByteMask( aByte1Mask );
            //
            int shift;
            uint mask = MakeMask( b1 + b0, out shift );
            //
            uint value = aValue & mask;
            value >>= shift;
            //
            if ( value == 1 )
            {
                return TBit.EBitSet;
            }

            return TBit.EBitClear;
        }

        public static TBit ExtractBit( uint aValue, string aByte3Mask, string aByte2Mask, string aByte1Mask, string aByte0Mask )
        {
            string b0 = EnsureByteMask( aByte0Mask );
            string b1 = EnsureByteMask( aByte1Mask );
            string b2 = EnsureByteMask( aByte2Mask );
            string b3 = EnsureByteMask( aByte3Mask );
            //
            int shift;
            uint mask = MakeMask( b3 + b2 + b1 + b0, out shift );
            //
            uint value = aValue & mask;
            value >>= shift;
            //
            if ( value == 1 )
            {
                return TBit.EBitSet;
            }

            return TBit.EBitClear;
        }

        // [ aByte0 ] 
        public static uint MakeMask( string aByte )
        {
            return MakeMask( string.Empty, string.Empty, string.Empty, aByte );
        }

        // [ aByte1 ] [ aByte0 ] 
        public static uint MakeMask( string aByte1, string aByte0 )
        {
            return MakeMask( string.Empty, string.Empty, aByte1, aByte0 );
        }

        // [ aByte3 ] [ aByte2 ] [ aByte1 ] [ aByte0 ] 
        public static uint MakeMask( string aByte3, string aByte2, string aByte1, string aByte0 )
        {
            int shift = 0;
            return MakeMask( aByte3, aByte2, aByte1, aByte0, out shift );
        }

        public static uint MakeMask( string aByte3, string aByte2, string aByte1, string aByte0, out int aShiftAmount )
        {
            string byteString3 = EnsureByteMask( aByte3 );
            string byteString2 = EnsureByteMask( aByte2 );
            string byteString1 = EnsureByteMask( aByte1 );
            string byteString0 = EnsureByteMask( aByte0 );
            string byteString = byteString3 + byteString2 + byteString1 + byteString0;
            //
            uint mask = MakeMask( byteString, out aShiftAmount );
            //
            return mask;
        }

        public static uint MakeMask( string aSpec, out int aShiftAmount )
        {
            int sigBitCount = 0;
            return MakeMask( aSpec, out aShiftAmount, out sigBitCount );
        }

        public static uint MakeMask( string aSpec, out int aShiftAmount, out int aSignificantBitCount )
        {
            bool setFirstBitIndex = false;
            aShiftAmount = 0;
            aSignificantBitCount = 0;
            uint mask = 0;

            // Loop through all characters in the mask, starting from the RHS, working
            // towards the left hand side.
            int count = aSpec.Length;
            for ( int bit = 0; bit < count; bit++ )
            {
                // Get a character from the string, starting at the RHS
                char c = aSpec[ count - bit - 1 ];
                //
                if ( c == KBitIsSet )
                {
                    mask |= (uint) ( 1u << bit );
                    if ( !setFirstBitIndex )
                    {
                        aShiftAmount = bit;
                        setFirstBitIndex = true;
                    }
                    ++aSignificantBitCount;
                }
                else if ( c == KBitIsClear )
                {
                    if ( !setFirstBitIndex )
                    {
                        aShiftAmount = bit;
                        setFirstBitIndex = true;
                    }
                    ++aSignificantBitCount;
                }
                else if ( c == KBitIsNotApplicable )
                {
                }
            }
            //
            return mask;
        }

        public static string EnsureByteMask( string aMask )
        {
            string ret = aMask.PadLeft( 8, '0' );
            return ret;
        }

        public static string ToBinary( uint aValue )
        {
            StringBuilder ret = new StringBuilder();
            //
            for ( int i = 31; i >= 0; i-- )
            {
                uint mask = (uint) ( 1u << i );
                uint value = ( aValue & mask ) >> i;
                if ( value != 0 )
                {
                    ret.Append( '1' );
                }
                else
                {
                    ret.Append( '0' );
                }
            }
            //
            return ret.ToString();
        }

        #region Constants
        public const char KBitIsSet = '1';
        public const char KBitIsClear = '0';
        public const char KBitIsNotApplicable = '#';
        #endregion
    }
}
