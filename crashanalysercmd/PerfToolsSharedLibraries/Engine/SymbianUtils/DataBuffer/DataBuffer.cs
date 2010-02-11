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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SymbianUtils.DataBuffer.Entry;
using SymbianUtils.Range;

namespace SymbianUtils.DataBuffer
{
    public class DataBuffer : IEnumerable<byte>
    {
        #region Constructors
        public DataBuffer()
		{
		}
		#endregion

		#region API
        public void Add( byte aByte )
        {
            uint address = 0;
            //
            if ( Count > 0 )
            {
                address = ( Last.Address - AddressOffset ) + 1;
            }
            //
            DataBufferByte entry = new DataBufferByte( aByte, address );
            Add( entry );
        }

        public void Add( IEnumerable<byte> aBytes )
        {
            foreach ( byte b in aBytes )
            {
                Add( b );
            }
        }
        
        public void Add( uint aDWord )
        {
            // Make 4 bytes
            byte b0 = (byte) (( aDWord & 0x000000FF ) >> 00);
            byte b1 = (byte) (( aDWord & 0x0000FF00 ) >> 08);
            byte b2 = (byte) (( aDWord & 0x00FF0000 ) >> 16);
            byte b3 = (byte) (( aDWord & 0xFF000000 ) >> 24);
            //
            Add( b0 );
            Add( b1 );
            Add( b2 );
            Add( b3 );
        }

        public void Add( DataBufferByte aEntry )
        {
            aEntry.Buffer = this;
            iData.Add( aEntry );
        }

        public void Set( DataBuffer aCopyFrom )
        {
            iAddressOffset = aCopyFrom.AddressOffset;
            iData = aCopyFrom.iData;
        }

        public void Save( Stream aStream )
        {
            byte[] bytes = this;
            aStream.Write( bytes, 0, bytes.Length );
        }

        public void Read( Stream aStream )
        {
        }

        public void Read( Stream aStream, int aOffset, int aLength )
        {
            byte[] bytes = new byte[ aLength ];
            aStream.Seek( aOffset, SeekOrigin.Begin );
            aStream.Read( bytes, 0, bytes.Length );
            Add( bytes );
        }

        public void Clear()
        {
            iData.Clear();
            iAddressOffset = 0;
        }

        public byte[] ToArray()
        {
            List<byte> ret = new List<byte>( iData.Count + 1 );
            //
            int count = iData.Count;
            for ( int i = 0; i < count; i++ )
            {
                ret.Add( iData[ i ].Byte );
            }
            //
            return ret.ToArray();
        }
        #endregion

		#region Properties
        public int Count
        {
            get { return iData.Count; }
        }

        public uint AddressOffset
        {
            get { return iAddressOffset; }
            set { iAddressOffset = value; }
        }

        public DataBufferByte First
        {
            get
            {
                DataBufferByte ret = new DataBufferByte( 0, 0 );
                //
                if ( Count > 0 )
                {
                    ret = iData[ 0 ];
                }
                //
                return ret;
            }
        }

        public DataBufferByte Last
        {
            get
            {
                DataBufferByte ret = new DataBufferByte( 0, 0 );
                //
                if ( Count > 0 )
                {
                    ret = iData[ Count - 1 ];
                }
                //
                return ret;
            }
        }

        public AddressRange Range
        {
            get
            {
                AddressRange ret = new AddressRange();
                if ( First != null )
                {
                    ret.Min = First.Address;
                }
                if ( Last != null )
                {
                    ret.Max = Last.Address;
                }
                return ret;
            }
        }

        public DataBufferUint this[ uint aAddress ]
        {
            get
            {
                DataBufferUint ret = new DataBufferUint( 0, aAddress );
                //
                foreach ( DataBufferUint uintEntry in GetUintEnumerator() )
                {
                    if ( uintEntry.Address == aAddress )
                    {
                        ret = uintEntry;
                        break;
                    }
                }
                //
                return ret;
            }
        }
		#endregion

        #region Enumerator API
        public IEnumerable<DataBufferUint> GetUintEnumerator()
        {
            // This iterator works from the bottom of the stack
            // upwards, just like Symbian OS/ARM stack allocation
            //
            //
            // Count = 12
            //
            // [0123][4567][89AB]
            //
            int count = iData.Count;
            //
            for ( int i = count - 4; i >= 0; i -= 4 )
            {
                DataBufferByte e0 = iData[ i + 0 ];
                DataBufferByte e1 = iData[ i + 1 ];
                DataBufferByte e2 = iData[ i + 2 ];
                DataBufferByte e3 = iData[ i + 3 ];
                //
                uint value = Combine( e0, e1, e2, e3 );
                DataBufferUint ret = new DataBufferUint( value, e0.Address );
                yield return ret;
            }
        }

        public IEnumerable<DataBufferByte> GetByteEnumerator()
        {
            int count = iData.Count;
            for ( int i = count - 1; i >= 0; i-- )
            {
                DataBufferByte entry = iData[ i ];
                yield return entry;
            }
        }
        #endregion

        #region From IEnumerable<byte>
        public IEnumerator<byte> GetEnumerator()
        {
            foreach ( DataBufferByte b in iData )
            {
                yield return b.Byte;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( DataBufferByte b in iData )
            {
                yield return b.Byte;
            }
        }
        #endregion

        #region Operators
        public static implicit operator byte[]( DataBuffer aBuffer )
        {
            byte[] ret = aBuffer.ToArray();
            return ret;
        }
        #endregion

        #region Internal methods
        private List<byte> GetRawBytes()
        {
            List<byte> ret = new List<byte>( iData.Count + 1 );
            //
            foreach ( DataBufferByte b in iData )
            {
                ret.Add( b.Byte );
            }
            //
            return ret;
        }

        private uint Combine( params DataBufferByte[] aItems )
        {
            if ( aItems.Length != 4 )
            {
                throw new ArgumentException( "Expected 4 items" );
            }
            //
            uint ret =
                ( (uint) aItems[ 0 ].Byte )       +
                ( (uint) aItems[ 1 ].Byte <<  8 ) +
                ( (uint) aItems[ 2 ].Byte << 16 ) +
                ( (uint) aItems[ 3 ].Byte << 24 )
                ;
            return ret;
        }
		#endregion

        #region From System.Object
        public override string ToString()
        {
            string ret = string.Empty;
            //
            List<byte> rawBytes = GetRawBytes();
            if ( rawBytes.Count > 0 )
            {
                DataBufferByte firstByte = First;
                uint startingAddress = firstByte.Address;
                //
                ret = SymbianUtils.Utilities.RawByteUtility.ConvertDataToText( rawBytes, true, ref startingAddress );
            }
            //
            return ret;
        }
        #endregion

        #region Data members
        private uint iAddressOffset = 0;
        private List<DataBufferByte> iData = new List<DataBufferByte>();
        #endregion
    }
}
