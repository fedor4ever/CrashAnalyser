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
    public class SymByte
    {
        #region Constructors
        public SymByte( byte aValue )
        {
            iValue = aValue;
        }

        public SymByte( string aBinary )
        {
            iValue = (byte) SymBitUtils.CreateMask( aBinary );
        }
        #endregion

        #region API
        public bool IsMatch( string aBinary )
        {
            SymByte value = 0;
            SymByte mask = SymBitUtils.CreateMask( aBinary, out value );
            bool ret = ( mask & iValue ) == value;
            return ret;
        }

        public SymByte LowestBits( int aCount )
        {
            // Make the mask
            byte mask = 0;
            for ( int i = 0; i < aCount; i++ )
            {
                mask |= (byte) ( 0x1 << i );
            }

            // Apply it
            byte ret = (byte) ( iValue & mask );
            return new SymByte( ret );
        }

        public SymByte HighestBitsShiftedRight( int aCount )
        {
            // Make the mask
            byte mask = 0;
            for ( int i = 7; i >=0; i-- )
            {
                mask |= (byte) ( 0x1 << i );
            }

            // Apply it
            byte ret = (byte) ( iValue & mask );

            // Shift
            ret >>= aCount;

            return new SymByte( ret );
        }

        public void RShift( int aBits )
        {
            iValue >>= aBits;
        }

        public uint LShift( int aBits )
        {
            uint ret = (uint) ( iValue << aBits );
            return ret;
        }
        #endregion

        #region Properties
        public byte Value
        {
            get { return iValue; }
            set { iValue = value; }
        }

        public string Binary
        {
            get
            {
                StringBuilder ret = new StringBuilder();
                ret.Append( Convert.ToString( Value, 2 ).PadLeft( 8, '0' ) );
                return ret.ToString();
            }
        }

        public bool this[ int aIndex ]
        {
            get
            {
                byte mask = (byte) ( 0x1 << aIndex );
                bool set = ( mask & Value ) == mask;
                return set;
            }
        }
        #endregion

        #region Operators
        public static implicit operator byte( SymByte aByte )
        {
            return aByte.Value;
        }

        public static implicit operator SymByte( byte aByte )
        {
            return new SymByte( aByte );
        }

        public static bool operator ==( SymByte aLeft, SymByte aRight )
        {
            bool result = aLeft.Value == aRight.Value;
            return result;
        }

        public static bool operator !=( SymByte aLeft, SymByte aRight )
        {
            bool result = !( aLeft == aRight );
            return result;
        }

        public static SymByte operator &( SymByte aLeft, SymByte aRight )
        {
            byte result = (byte) ( aLeft.Value & aRight.Value );
            return new SymByte( result );
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.AppendFormat( "{0:x2} [{1}]", iValue, System.Convert.ToString( iValue, 2 ).PadLeft( 8, '0' ) );
            return ret.ToString();
        }

        public override bool Equals( object aObject )
        {
            bool ret = false;
            //
            if ( aObject != null && aObject is SymByte )
            {
                SymByte other = (SymByte) aObject;
                ret = ( other.Value == this.Value );
            }
            //
            return ret;
        }

        public override int GetHashCode()
        {
            return iValue.GetHashCode();
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private byte iValue = 0;
        #endregion
    }
}
