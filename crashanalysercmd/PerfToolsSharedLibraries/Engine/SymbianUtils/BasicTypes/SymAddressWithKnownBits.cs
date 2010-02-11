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
    public class SymAddressWithKnownBits : SymAddress
    {
        #region Constructors
        public SymAddressWithKnownBits()
            : this( 0 )
        {
        }

        public SymAddressWithKnownBits( uint aAddress )
            : base( aAddress )
        {
        }
        #endregion

        #region API
        public void SetKnownAddressBits( uint aAddress, int aNumberOfValidBits )
        {
            // First build a bit mask of all the bits which we want to keep
            // from the currently maintained address.
            uint mask = 0;
            for ( int i = 31; i >= aNumberOfValidBits; i-- )
            {
                mask |= (uint) ( 1u << i );
            }

            // aNumberOfValidBits = 0xe
            // mask is 0xffffc000
            // base.Address is 0x802e20ac

            // 11111111 11111111 11000000 00000000 = mask
            // 10000000 00101110 01100110 10101100
            // 10000000 00101110 01000000 00000000

            // Next, we want to apply that mask to the current address
            // in order to prepare for the new values. 
            //
            // I.e. preserve any bits we know but aren't changing, and then
            // clear all other bits that we're about to set below.
            base.Address &= mask;

            // Now we can merge in the new bits
            base.Address |= aAddress;

            // Save how many valid bits of the address we currently have for debugging
            // purposes
            iKnownBits = Math.Max( iKnownBits, aNumberOfValidBits );
            System.Diagnostics.Debug.Assert( iKnownBits >= 0 && iKnownBits <= 32 );
        }
        #endregion

        #region Properties
        public int KnownBits
        {
            get { return iKnownBits; }
            set { iKnownBits = value; }
        }

        public bool IsKnown
        {
            get { return iKnownBits == 32; }
        }

        public bool IsPartial
        {
            get { return iKnownBits > 0; }
        }

        public bool IsUnknown
        {
            get { return iKnownBits == 0; }
        }

        public override string AddressBinary
        {
            get
            {
                string ret = SymBitUtils.BeautifyBits( base.Address, iKnownBits );
                return ret;
            }
        }

        public override string AddressHex
        {
            get
            {
                // Work out how many nibbles we have
                int validNibbles = 0;
                for ( ; validNibbles < iKnownBits; validNibbles += 4 )
                {
                }
                validNibbles /= 4; // Convert bits to nibbles
                int invalidNibbles = 8 - validNibbles;

                StringBuilder text = new StringBuilder();
                text.Append( string.Empty.PadLeft( invalidNibbles, '?' ) );
                text.Append( base.Address.ToString( string.Format( "x{0}", validNibbles ) ) );
                text.Length = Math.Min( 8, text.Length );
                return text.ToString();
            }
        }
        #endregion

        #region Operators
        public static implicit operator uint( SymAddressWithKnownBits aAddress )
        {
            return aAddress.Address;
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private int iKnownBits = 0;
        #endregion
    }
}
