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

namespace SymbianUtils.RawItems
{
	public class RawItem
	{
		#region Constructors
        public RawItem()
            : this( 0, 0, 0, "...." )
        {
        }

        public RawItem( uint aAddress, uint aReversedData, uint aOriginalData, string aCharacterisedData )
		{
			iAddress = aAddress;
			iData = aReversedData;
			iOriginalData = aOriginalData;
			iCharacterisedData = aCharacterisedData;
		}

        public RawItem( uint aAddress, uint aData )
        {
            iAddress = aAddress;
            iData = aData;
            
            // Reverse bytes to get the original data
            uint[] bytes = AsBytes( aData );
			iOriginalData = SwapEndianness( bytes );

            // Get chars
            iCharacterisedData = ConvertToCharacters( bytes );
        }
		#endregion

        #region API
        public int PrintableCharacterCount( bool aUnicode, ref int aDotCount )
        {
            int count = 0;
            //
            int start = ( aUnicode ) ? 1 : 0;
            int delta = ( aUnicode ) ? 2 : 1;
            int length = iCharacterisedData.Length;
            //
            for ( int i = start; i < length; i += delta )
            {
                char c = iCharacterisedData[ i ];
                int charAsInt = System.Convert.ToInt32( c );

                if ( c >= 32 && c < 128 || c == 0x0D || c == 0x0A || c == 0x09 )
                {
                    ++count;
                }
                if ( c == '.' )
                {
                    ++aDotCount;
                }
            }
            return count;
        }
        #endregion

        #region Properties
        public uint Address
		{
			get { return iAddress; }
			set { iAddress = value; }
		}

		public uint Data
		{
			get { return iData; }
			set { iData = value; }
		}

        public byte[] DataArray
        {
            get
            {
                byte[] ret = new byte[ 4 ];
                //
                ret[ 0 ] = (byte) ( ( iData >> 0 ) & 0xFF );
                ret[ 1 ] = (byte) ( ( iData >> 8 ) & 0xFF );
                ret[ 2 ] = (byte) ( ( iData >> 16 ) & 0xFF );
                ret[ 3 ] = (byte) ( ( iData >> 24 ) & 0xFF );
                //
                return ret;
            }
        }

        public ushort[] DataArrayWords
        {
            get
            {
                ushort[] ret = new ushort[ 2 ];
                //
                ret[ 0 ] = (ushort) ( ( iData )       & 0x0000FFFF );
                ret[ 1 ] = (ushort) ( ( iData >> 16 ) & 0x0000FFFF );
                //
                return ret;
            }
        }

		public uint OriginalData
		{
			get { return iOriginalData; }
			set { iOriginalData = value; }
		}

		public string CharacterisedData
		{
			get { return iCharacterisedData; }
			set { iCharacterisedData = value; }
		}

		public string OriginalCharacterisedData
		{
			get 
			{
				char[] reversedCharacterisedData = new char[ iCharacterisedData.Length ];
				for (int i = 0; i < iCharacterisedData.Length; i+=4)
				{
					string bytes = iCharacterisedData.Substring(i, 4);
					reversedCharacterisedData[i]   = bytes[3];
					reversedCharacterisedData[i+1] = bytes[2];
					reversedCharacterisedData[i+2] = bytes[1];
					reversedCharacterisedData[i+3] = bytes[0];
				}
				//
				string characterisedData = new string(reversedCharacterisedData);
				return characterisedData;
			}
		}

		public string OriginalCharacterisedDataAsUnicode
		{
			get 
			{
                StringBuilder text = new StringBuilder( OriginalCharacterisedData );
				for( int i = text.Length-1; i >= 0; i-=2 )
				{
                    text.Remove( i, 1 );
				}
                string ret = text.ToString();
                return ret;
			}
		}

        public object Tag
		{
			get { return iTag; }
			set { iTag = value; }
		}

        public static uint RoundToNearestDWord( uint aValue )
        {
            uint dwords = aValue / KSizeOfOneRawItemInBytes;
            uint ret = dwords * KSizeOfOneRawItemInBytes;
            uint remainder = aValue % KSizeOfOneRawItemInBytes;
            //
            if ( remainder > 0 )
            {
                ret += KSizeOfOneRawItemInBytes;
            }
            //
            return ret;
        }
		#endregion

        #region Constants
        public const uint KSizeOfOneRawItemInBytes = 4;
        #endregion

        #region Internal methods
        private static uint[] AsBytes( uint aValue )
        {
            uint[] ret = new uint[4];
            //
            ret[ 0 ] = ( aValue ) & 0xFF;
            ret[ 1 ] = ( aValue >> 8 ) & 0xFF;
            ret[ 2 ] = ( aValue >> 16 ) & 0xFF;
            ret[ 3 ] = ( aValue >> 24 ) & 0xFF;
            //
            return ret;
        }

        private static uint SwapEndianness( uint aValue )
        {
            uint[] bytes = AsBytes( aValue );
            return SwapEndianness( bytes );
        }

        private static uint SwapEndianness( uint[] aBytes )
        {
            System.Diagnostics.Debug.Assert( aBytes.Length == KSizeOfOneRawItemInBytes );
            uint ret = 0;
            //
            ret += ( aBytes[ 3 ] <<  0 );
            ret += ( aBytes[ 2 ] <<  8 );
            ret += ( aBytes[ 1 ] << 16 );
            ret += ( aBytes[ 0 ] << 24 );
            //
            return ret;
        }

        private static string ConvertToCharacters( uint[] aBytes )
        {
            StringBuilder ret = new StringBuilder();
            //
            foreach ( uint val in aBytes )
            {
                char c = System.Convert.ToChar( val );
                //
    			if ( c <= 0x20 || c >= 0x7f || c == '%' )
                {
                    c = '.';
                }
                //
                ret.Insert( 0, c );
            }
            //
            return ret.ToString();
        }
        #endregion

        #region Data members
        private object iTag = null;
		private uint iAddress;
		private uint iData;
		private uint iOriginalData;
		private string iCharacterisedData = string.Empty;
		#endregion
	}
}
