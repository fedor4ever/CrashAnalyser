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
using SymbianUtils.BasicTypes;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Exceptions;
using SymbianETMLib.Common.Types;
using SymbianETMLib.Common.Config;
using SymbianETMLib.Common.State;

namespace SymbianETMLib.Common.BranchDecoder
{
    internal class ETMBranchDecoderAlternative : ETMBranchDecoderOriginal
    {
        #region Constructors
        public ETMBranchDecoderAlternative( ETMStateData aManager )
            : base( aManager )
        {
        }
        #endregion

        #region From ETBBranchDecoder
        protected override void DecodePartial()
        {
            int count = base.Count;
            System.Diagnostics.Debug.Assert( count > 0 && count < 5 );

            // According to Table 7-13, "Missing components of exception Branch address packets"
            // then after a partial branch, we're always in "no exception" state
            base.ExceptionType = TArmExceptionType.ENone;

            if ( count == 1 )
            {
                // The single-byte packet is exactly the same as in the original encoding
                base.DecodePartial();
            }
            else
            {
                // We have full 5 bytes of address info.
                SymMask maskFirst = new SymMask( 0x7E, SymMask.TShiftDirection.ERight, 1 );
                SymMask maskMiddle = new SymMask( 0x7F );
                SymMask maskLast = new SymMask( 0x3F );

                SymByte b;
                SymAddressWithKnownBits address = new SymAddressWithKnownBits();
                uint masked = 0;

                // First byte
                b = base[ 0 ];
                address.KnownBits += 6;
                address.Address |= maskFirst.Apply( b );

                // Middle bytes, but not the last
                for ( int i = 1; i < count-1; i++ )
                {
                    b = base[ i ];
                    b = (byte) maskMiddle.Apply( b );
                    masked = (uint) ( b << address.KnownBits );
                    address.KnownBits += 7;
                    address.Address |= masked;
                }

                // Last byte
                b = base.LastByte;
                address.KnownBits += 6;
                address.Address |= maskLast.Apply( b );

                // Shift entire address by shift count based upon current instruction set.
                int isShift = ETMBranchDecoder.CompressionLeftShiftCount( base.InstructionSet );
                address.KnownBits += isShift;
                address.Address <<= isShift;

                // Save address
                base.BranchAddress = address;
            }
        }

        public override bool IsPacketComplete
        {
            get
            {
                bool ret = false;
                //
                SymByte lastByte = base.LastByte;
                bool bit7 = lastByte[ 7 ];
                //
                if ( bit7 )
                {
                    // If the 7th bit is set, then the interpretation is the same as in the original
                    // compression scheme, in which case we can use the base class call.
                    ret = base.IsPacketComplete;
                }
                else
                {
                    if ( base.Count == 1 )
                    {
                        // If the 7th bit is clear and this is the first byte, then this is also
                        // the only byte - no exception continuation byte follows.
                        ret = true;
                    }
                    else
                    {
                        // If the 7th bit is clear and the 6th bit is also clear, then this is
                        // the last byte.
                        bool bit6 = lastByte[ 6 ];
                        ret = ( bit6 == false );
                    }
                }
                //
                return ret;
            }
        }

        public override bool IsBranchAddressAvailable
        {
            get
            {
                bool ret = base.IsBranchAddressAvailable;
                return ret;
            }
        }
        #endregion

        #region From ETBBranchDecoderOriginal
        /*
        protected override bool ContainsInlineException
        {
            get
            {
                // Not supported in alternative encoding
                return false;
            }
        }*/
        #endregion

        #region Properties
        #endregion

        #region Internal constants
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
