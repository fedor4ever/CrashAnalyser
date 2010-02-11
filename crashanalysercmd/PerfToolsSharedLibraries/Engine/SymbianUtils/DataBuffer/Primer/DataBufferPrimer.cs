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
using System.Text.RegularExpressions;
using System.IO;
using SymbianUtils.DataBuffer.Entry;

namespace SymbianUtils.DataBuffer.Primer
{
    public class DataBufferPrimer
    {
        #region Delegates & Events
        public delegate void DataBufferPrimerUnhandledLine( DataBufferPrimer aPrimer, DataBuffer aBuffer, string aLine );
        public event DataBufferPrimerUnhandledLine LineNotHandled;

        public delegate void DataBufferPrimerCompleteHandler( DataBufferPrimer aPrimer, DataBuffer aBuffer, uint aFirstByteAddress, uint aLastByteAddress );
        public event DataBufferPrimerCompleteHandler PrimerComplete;
        #endregion

        #region Constructors
        public DataBufferPrimer( DataBuffer aBuffer )
        {
            iDataBuffer = aBuffer;
        }
        #endregion

        #region API
        public void Prime( IEnumerable<string> aLines )
        {
            iDataBuffer.Clear();
            //
            foreach ( string line in aLines )
            {
                PrimeLine( line );
            }

            Primed = true;
        }

        public void PrimeLine( string aLine )
        {
            Match m = iRawDataRegEx.Match( aLine );
            //
            if ( m.Success )
            {
                uint startOfLineAddress = ExtractDataSourceEntryFromMatch( m );
                
                // If the data buffer has never had an address applied to it, then set it now
                if ( iHaveSetFirstAddress == false )
                {
                    iDataBuffer.AddressOffset = startOfLineAddress;
                    iHaveSetFirstAddress = true;
                }
            }
            else if ( LineNotHandled != null )
            {
                LineNotHandled( this, iDataBuffer, aLine );
            }
        }

        public void Prime( string aBinaryFileName )
        {
            Prime( aBinaryFileName, 0 );
        }

        public void Prime( string aBinaryFileName, uint aAddressOfFirstByte )
        {
            FileInfo info = new FileInfo( aBinaryFileName );
            if ( info.Exists )
            {
                int length = (int) info.Length;
                //
                byte[] bytes = new byte[ length ];
                using ( FileStream stream = new FileStream( aBinaryFileName, FileMode.Open, FileAccess.Read, FileShare.Read ) )
                {
                    stream.Read( bytes, 0, length );
                }
                //
                Prime( bytes, aAddressOfFirstByte );
            }
        }

        public void Prime( IEnumerable<byte> aBytes, uint aAddressOfFirstByte )
        {
            iDataBuffer.Clear();

            // Set the starting address
            iDataBuffer.AddressOffset = aAddressOfFirstByte;

            // Read bytes
            uint offset = 0;
            foreach ( byte b in aBytes )
            {
                DataBufferByte entry = new DataBufferByte( b, offset++ );
                iDataBuffer.Add( entry );
            }

            Primed = true;
        }

        public void Prime( DataBuffer aBuffer )
        {
            iDataBuffer.Clear();
            iDataBuffer.Set( aBuffer );
            Primed = true;
        }
        #endregion

        #region Properties
        public bool Primed
        {
            get { return iPrimed; }
            private set
            {
                iPrimed = value;
                if ( Primed )
                {
                    uint firstByte = ( iDataBuffer.Count > 0 ) ? iDataBuffer.First.Address : 0;
                    uint lastByte = ( iDataBuffer.Count > 0 ) ? iDataBuffer.Last.Address : 0;
                    //
                    if ( PrimerComplete != null )
                    {
                        PrimerComplete( this, iDataBuffer, firstByte, lastByte );
                    }
                }
            }
        }
        #endregion

        #region Internal methods
        private uint ExtractDataSourceEntryFromMatch( Match aMatch )
        {
            System.Diagnostics.Debug.Assert( aMatch.Success );

            uint address = 0;
            uint nextExpectedAddress = 0;
            if ( iDataBuffer.Count > 0 )
            {
                nextExpectedAddress = iDataBuffer.Last.Address + 1;
            }
            //
            GroupCollection groups = aMatch.Groups;
            CaptureCollection data = groups[ "Data" ].Captures;

            if ( data.Count > 0 )
            {
                address = System.Convert.ToUInt32( groups[ "Address" ].Value, 16 );

                // Validate the address
                if ( nextExpectedAddress != 0 && address != nextExpectedAddress )
                {
                    throw new Exception( string.Format( "Data is corrupt - expected: 0x{0:x8}, actual: 0x{1:x8}", nextExpectedAddress, address ) );
                }
                else
                {
                    foreach ( Capture capture in data )
                    {
                        string val = capture.Value.Trim();
                        byte b = System.Convert.ToByte( val, 16 );
                        DataBufferByte entry = new DataBufferByte( b, (uint) iDataBuffer.Count );
                        iDataBuffer.Add( entry );
                    }
                }
            }

            return address;
        }
        #endregion

        #region Data members
        private static readonly Regex iRawDataRegEx = new Regex( "(?:.*)\r\n(?<Address>[a-fA-F0-9]{8})\r\n\\:\\s{1}\r\n(?<Data>(?:[a-fA-F0-9]{2})\\s{1}){1,16}\r\n(?:.*)", RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled );
        private readonly DataBuffer iDataBuffer;
        private bool iPrimed = false;
        private bool iHaveSetFirstAddress = false;
        #endregion
    }
}
