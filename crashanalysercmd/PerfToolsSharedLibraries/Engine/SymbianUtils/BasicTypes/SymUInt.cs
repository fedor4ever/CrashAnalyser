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
    public class SymUInt32 : SymUIntBase
    {
        #region Constructors
        public SymUInt32( uint aValue )
            : base( aValue, 32 )
        {
        }

        public SymUInt32( string aBinary )
            : base( aBinary, 32 )
        {
        }
        #endregion

        #region Operators
        public static SymUInt32 operator &( SymUInt32 aLeft, SymUInt32 aRight )
        {
            uint val = aLeft.RawValue & aRight.RawValue;
            return new SymUInt32( val );
        }

        public static implicit operator SymUInt32( uint aBasicType )
        {
            return new SymUInt32( aBasicType );
        }
        #endregion
    }

    public class SymUInt16 : SymUIntBase
    {
        #region Constructors
        public SymUInt16( ushort aValue )
            : base( aValue, 16 )
        {
        }

        public SymUInt16( string aBinary )
            : base( aBinary, 16 )
        {
        }
        #endregion

        #region Operators
        public static SymUInt16 operator &( SymUInt16 aLeft, SymUInt16 aRight )
        {
            ushort val = Convert.ToUInt16( aLeft.RawValue & aRight.RawValue );
            return new SymUInt16( val );
        }
        public static implicit operator SymUInt16( ushort aBasicType )
        {
            return new SymUInt16( aBasicType );
        }
        #endregion
    }

    public class SymUInt8 : SymUIntBase
    {
        #region Constructors
        public SymUInt8( byte aValue )
            : base( aValue, 8 )
        {
        }

        public SymUInt8( string aBinary )
            : base( aBinary, 8 )
        {
        }
        #endregion

        #region Operators
        public static SymUInt8 operator &( SymUInt8 aLeft, SymUInt8 aRight )
        {
            byte val = Convert.ToByte( aLeft.RawValue & aRight.RawValue );
            return new SymUInt8( val );
        }

        public static implicit operator SymUInt8( byte aBasicType )
        {
            return new SymUInt8( aBasicType );
        }
        #endregion
    }

    public abstract class SymUIntBase : IFormattable
    {
        #region Constructors
        protected SymUIntBase()
        {
        }

        protected SymUIntBase( string aBinary, int aNumberOfBits )
        {
            iValue = SymBitUtils.CreateMask( aBinary );
            iNumberOfBits = aNumberOfBits;
        }

        protected SymUIntBase( uint aValue, int aNumberOfBits )
        {
            iValue = aValue;
            iNumberOfBits = aNumberOfBits;
        }
        #endregion

        #region API
        public bool IsMatch( string aBinary )
        {
            uint value = 0;
            uint mask = SymBitUtils.CreateMask( aBinary, out value );
            bool ret = ( mask & iValue ) == value;
            return ret;
        }

        public uint RShift( int aBits )
        {
            uint v = iValue;
            v >>= aBits;
            this.RawValue = v;
            return v;
        }

        public uint LShift( int aBits )
        {
            uint v = (uint) ( iValue << aBits );
            this.RawValue = v;
            return v;
        }

        public SymUInt32 RotateRight( int aCount )
        {
            uint ret = iValue;
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
            return new SymUInt32( ret );
        }

        public uint ToUInt()
        {
            return iValue;
        }
        #endregion

        #region Properties
        public int NumberOfBits
        {
            get { return iNumberOfBits; }
            protected set
            {
                switch ( value )
                {
                default:
                    throw new ArgumentException( "Number of bits must be 8, 16 or 32" );
                case 8:
                case 16:
                case 32:
                    iNumberOfBits = value;
                    break;
                }
            }
        }

        public int NumberOfBytes
        {
            get { return NumberOfBits / 4; }
            set
            {
                this.NumberOfBits = value * 8;
            }
        }

        public uint MaxValue
        {
            get
            {
                uint ret = uint.MaxValue;
                if ( iNumberOfBits == 16 )
                {
                    ret = ushort.MaxValue;
                }
                else if ( iNumberOfBits == 8 )
                {
                    ret = byte.MaxValue;
                }
                return ret;
            }
        }

        public string Binary
        {
            get
            {
                string bits = SymBitUtils.BeautifyBits( iValue, iNumberOfBits );
                return bits;
            }
        }

        public string Hex
        {
            get
            {
                string hex = iValue.ToString( string.Format( "x{0}", this.NumberOfBytes * 2 ) );
                return hex;
            }
        }

        public SymBit this[ int aIndex ]
        {
            get
            {
                if ( aIndex < 0 || aIndex > iNumberOfBits )
                {
                    throw new ArgumentException( "Specified bit is out of bounds" );
                }

                uint mask = ( 1u << aIndex );
                SymBit ret = ( ( mask & iValue ) == mask ) ? SymBit.ESet : SymBit.EClear;
                return ret;
            }
        }

        public SymUInt32 this[ int aHighBitIndex, int aLowBitIndex ]
        {
            get
            {
                if ( aHighBitIndex < aLowBitIndex )
                {
                    throw new ArgumentException( "High bit index must be less than low bit index" );
                }

                // Build mask
                uint mask = 0;
                for ( int i = aLowBitIndex; i <= aHighBitIndex; i++ )
                {
                    mask |= ( 1u << i );
                }

                uint ret = iValue & mask;

                // Shift
                ret >>= aLowBitIndex;

                return new SymUInt32( ret );
            }
        }
        #endregion

        #region Operators
        public static implicit operator uint( SymUIntBase aObject )
        {
            return aObject.RawValue;
        }

        public static implicit operator ushort( SymUIntBase aObject )
        {
            ushort ret = Convert.ToUInt16( aObject.RawValue );
            return ret;
        }

        public static implicit operator byte( SymUIntBase aObject )
        {
            byte ret = Convert.ToByte( aObject.RawValue );
            return ret;
        }

        public static bool operator ==( SymUIntBase aLeft, SymUIntBase aRight )
        {
            // If both are null, or both are same instance, return true.
            if ( System.Object.ReferenceEquals( aLeft, aRight ) )
            {
                return true;
            }

            // If one is null, but not both, return false.
            if ( ( (object) aLeft == null ) || ( (object) aRight == null ) )
            {
                return false;
            }

            bool ret = aLeft.Equals( aRight );
            return ret;
        }

        public static bool operator !=( SymUIntBase aLeft, SymUIntBase aRight )
        {
            bool result = !( aLeft == aRight );
            return result;
        }

        public static SymUInt32 operator &( SymUIntBase aLeft, SymUIntBase aRight )
        {
            uint val = ( aLeft.RawValue & aRight.RawValue );
            return new SymUInt32( val );
        }
        #endregion

        #region IFormattable Members
        public string ToString( string aFormat, IFormatProvider aFormatProvider )
        {
            if ( aFormatProvider != null )
            {
                ICustomFormatter formatter = aFormatProvider.GetFormat( this.GetType() ) as ICustomFormatter;
                if ( formatter != null )
                {
                    return formatter.Format( aFormat, this, aFormatProvider );
                }
            }

            string ret = string.Empty;
            string format = aFormat != null ? aFormat : "full";
            //
            switch ( format )
            {
            case "full":
                ret = string.Format( "0x{0} [{1}]", Hex, Binary );
                break;
            default:
                ret = iValue.ToString( aFormat, aFormatProvider );
                break;
            }
            //
            return ret;
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return ToString( null );
        }

        public string ToString( string aFormat )
        {
            return ToString( aFormat, null );
        }

        public override bool Equals( object aObject )
        {
            bool ret = false;
            //
            if ( aObject != null && aObject is SymUIntBase )
            {
                SymUIntBase other = (SymUIntBase) aObject;
                //
                if ( other.NumberOfBits == this.NumberOfBits )
                {
                    ret = ( other.RawValue == this.RawValue );
                }
            }
            //
            return ret;
        }

        public override int GetHashCode()
        {
            return iValue.GetHashCode();
        }
        #endregion

        #region Internal properties & methods
        protected uint RawValue
        {
            get { return iValue; }
            set
            {
                if ( value > MaxValue )
                {
                    throw new ArgumentException( string.Format( "Specified value {0} exceeds bit range ({1})", value, this.NumberOfBits ) );
                }
                iValue = value;
            }
        }
        #endregion

        #region Internal constants
        private const uint KTopBit = 0x80000000;
        #endregion

        #region Data ember
        private uint iValue = 0;
        private int iNumberOfBits = 32;
        #endregion
    }
}
