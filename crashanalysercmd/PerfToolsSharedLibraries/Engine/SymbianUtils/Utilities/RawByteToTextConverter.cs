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
using System.Collections;
using System.Collections.Generic;

namespace SymbianUtils.Utilities
{
	public class RawByteUtility
	{
		public static void ConvertToCharacter( byte aByte, StringBuilder aBuilder )
		{
			if	( aByte <= 32 || aByte > 126 )
			{
				aBuilder.Append( '.' );
			}
			else
			{
				aBuilder.Append( System.Convert.ToChar( aByte ) );
			}
		}

        public static string ConvertDataToText( IEnumerable<byte> aBytes, bool aFlushEntireList, ref uint aStartingAddress )
        {
            Queue<byte> queue = new Queue<byte>();
            foreach ( byte b in aBytes )
            {
                queue.Enqueue( b );
            }
            return ConvertDataToText( queue, aFlushEntireList, ref aStartingAddress );
        }

        public static string ConvertDataToText( Queue<byte> aByteQueue, bool aFlushEntireQueue, ref uint aStartingAddress )
        {
            RawByteConverter converter = new RawByteConverter();
            string ret = converter.Convert( aByteQueue, aFlushEntireQueue, ref aStartingAddress );
            return ret;
        }

		public static uint CombineBytes( uint aByte0, uint aByte1, uint aByte2, uint aByte3 )
		{
			aByte0 = ( aByte0 << 24 );
			aByte1 = ( aByte1 << 16 );
			aByte2 = ( aByte2 <<  8 );
			//
			uint ret = ( aByte0 + aByte1 + aByte2 + aByte3 );
			//
			return ret;
		}

        public static string CreateHexData( byte aByte0, byte aByte1, byte aByte2, byte aByte3 )
		{
			StringBuilder charData = new StringBuilder();
			//
			charData.Append( aByte0.ToString("x2") + " " );
			charData.Append( aByte1.ToString("x2") + " " );
			charData.Append( aByte2.ToString("x2") + " " );
			charData.Append( aByte3.ToString("x2") + " " );
			//
			string charDataString = charData.ToString();
			return charDataString;
		}

        public static string CreateCharacterisedData( uint aDWord )
        {
            byte b3 = (byte) (   aDWord & 0x000000FF );
            byte b2 = (byte) ( ( aDWord & 0x0000FF00 ) >>  8 );
            byte b1 = (byte) ( ( aDWord & 0x00FF0000 ) >> 16 );
            byte b0 = (byte) ( ( aDWord & 0xFF000000 ) >> 24 );
            //
            return CreateCharacterisedData( b0, b1, b2, b3 );
        }

        public static string CreateCharacterisedData( byte[] aBytes )
        {
            if ( aBytes.Length != 4 )
            {
                throw new ArgumentException( "Expected 4 byte array" );
            }

            return CreateCharacterisedData( aBytes[ 0 ], aBytes[ 1 ], aBytes[ 2 ], aBytes[ 3 ] );
        }

		public static string CreateCharacterisedData( byte aByte0, byte aByte1, byte aByte2, byte aByte3 )
		{
			StringBuilder charData = new StringBuilder();
			//
			ConvertToCharacter( aByte3, charData );
			ConvertToCharacter( aByte2, charData );
			ConvertToCharacter( aByte1, charData );
			ConvertToCharacter( aByte0, charData );
			//
			string charDataString = charData.ToString();
			return charDataString;
		}
	}

    public class RawByteConverter
    {
        #region Delegates & events
        public delegate void HandleLine( string aLine );
        public event HandleLine iLineHandler;
        #endregion

        #region Constructors
        public RawByteConverter()
        {
        }
        #endregion

        #region API
        public string Convert( Queue<byte> aByteQueue, bool aFlushEntireQueue, ref uint aStartingAddress )
        {
            const int KNumberOfBytesPerLine = 16;

            StringBuilder ret = new StringBuilder();

            // First try to build entire lines of 16 bytes
            while ( aByteQueue.Count >= KNumberOfBytesPerLine )
            {
                StringBuilder byteVals = new StringBuilder();
                byteVals.Append( aStartingAddress.ToString( "x8" ) + ": " );
                int bytesProcessedForThisLine = 0;
                //
                StringBuilder byteChars = new StringBuilder();
                while ( bytesProcessedForThisLine != KNumberOfBytesPerLine )
                {
                    // Extract at most 4 bytes of data to process
                    byte b0 = aByteQueue.Dequeue();
                    byte b1 = aByteQueue.Dequeue();
                    byte b2 = aByteQueue.Dequeue();
                    byte b3 = aByteQueue.Dequeue();

                    // Create double-char hex representation of each character
                    byteVals.Append( RawByteUtility.CreateHexData( b0, b1, b2, b3 ) );

                    // Character representation of data...
                    byteChars.Append( RawByteUtility.CreateCharacterisedData( b0, b1, b2, b3 ) );

                    // Handle new line scenario
                    bytesProcessedForThisLine += 4;
                }

                byteVals.Append( byteChars.ToString() );
                byteVals.Append( System.Environment.NewLine );
                //
                if ( iLineHandler != null )
                {
                    iLineHandler( byteVals.ToString() );
                }
                //
                ret.Append( byteVals.ToString() );
                aStartingAddress += KNumberOfBytesPerLine;
            }

            // Extract remaining data only if the client specified that the entire queue should
            // be emptied
            int numberLeft = aByteQueue.Count;
            if ( aFlushEntireQueue && numberLeft > 0 && numberLeft < 16 )
            {
                StringBuilder byteVals = new StringBuilder();
                byteVals.Append( aStartingAddress.ToString( "x8" ) + ": " );
                //
                StringBuilder byteChars = new StringBuilder();
                while ( aByteQueue.Count > 0 )
                {
                    byte b0 = aByteQueue.Dequeue();

                    // Create double-char hex representation of each character
                    byteVals.Append( b0.ToString( "x2" ) + " " );

                    // Character representation of data...
                    RawByteUtility.ConvertToCharacter( b0, byteChars );
                }

                // We may need to pad some bytes
                int padCount = KNumberOfBytesPerLine - numberLeft;
                while ( padCount > 0 )
                {
                    byteVals.Append( "  " + " " );
                    byteChars.Append( "." );
                    --padCount;
                }

                // Combine data
                byteVals.Append( byteChars.ToString() );
                byteVals.Append( System.Environment.NewLine );
                //
                if ( iLineHandler != null )
                {
                    iLineHandler( byteVals.ToString() );
                }
                //
                ret.Append( byteVals.ToString() );
                aStartingAddress += (uint) numberLeft;
            }

            return ret.ToString();
        }
        #endregion

        #region Data members
        #endregion
    }
}
