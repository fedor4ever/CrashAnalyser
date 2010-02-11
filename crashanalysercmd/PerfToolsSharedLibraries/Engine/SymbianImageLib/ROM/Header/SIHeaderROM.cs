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
using SymbianUtils.Strings;
using SymbianUtils.Streams;
using SymbianImageLib.Common.Header;
using SymbianImageLib.Common.Image;
using SymbianStructuresLib.Compression.Common;
using SymbianImageLib.ROM.Structures;

namespace SymbianImageLib.ROM.Header
{
    internal class SIHeaderROM : SIHeader
    {
        #region Constructors
        public SIHeaderROM( SIImage aImage, Stream aStream )
            : base( aImage )
        {
            aStream.Seek( 0, SeekOrigin.Begin );
            //
            iHeaderData = new byte[ KMaximumRomHeaderSize ];
            int amountRead = aStream.Read( iHeaderData, 0, KMaximumRomHeaderSize );
            //
            string headerText = StringParsingUtils.BytesToString( iHeaderData, KEpocHeaderLength );
            base.Trace( "[SymbianImageHeaderROM] Ctor() - headerText: {0}, amountRead: {1}", headerText, amountRead );
            //
            if ( IsEpocHeader( headerText ) == false )
            {
                throw new NotSupportedException( string.Format( "ROM Image header is unsupported: {0}", headerText ) );
            }
            //
            ReadHeaderData( iHeaderData );
        }
        #endregion

        #region Constants
        public const int KMaximumRomHeaderSize = 0x400;
        public const int KPageSize = 0x1000;
        #endregion

        #region From SymbianImageHeader
        public override TSymbianCompressionType CompressionType
        {
            get
            {
                TSymbianCompressionType ret = TSymbianCompressionType.ENone;
                //
                if ( iRomHdr.CompressionType == (uint) TSymbianCompressionType.EBytePair )
                {
                    ret = TSymbianCompressionType.EBytePair;
                }
                else if ( iRomHdr.CompressionType == (uint) TSymbianCompressionType.EDeflate )
                {
                    ret = TSymbianCompressionType.EDeflate;
                }
                else
                {
                    // Check for byte pair...
                    if ( iRomHdr.RomPageIndex != 0 )
                    {
                        ret = TSymbianCompressionType.EBytePair;
                    }
                }
                //
                return ret;
            }
        }

        public override uint HeaderSize
        {
            get
            {
                uint ret = 0;
                //
                ret += HeaderSizeLoader;
                ret += iRomHdr.Size;
                //
                return ret;
            }
        }
        #endregion

        #region API
        public static bool IsROM( Stream aStream )
        {
            using ( SymbianStreamReaderLE reader = SymbianStreamReaderLE.New( aStream, SymbianStreamReaderLE.TCloseOperation.EResetPosition ) )
            {
                string signature = reader.ReadString( KEpocHeaderLength );
                bool ret = IsEpocHeader( signature );
                return ret;
            }
        }
        #endregion

        #region Properties
        public uint UncompressedRomSize
        {
            get { return iRomHdr.UncompressedSize; }
        }

        public uint HeaderSizeLoader
        {
            get { return iLoaderHdr.Size; }
        }

        public uint RomBaseAddress
        {
            get { return iRomHdr.RomBaseAddress; }
        }

        public int NumberOfPages
        {
            get
            {
                int pageAreaStart = iRomHdr.PageableRomStart;
                int pageRomSize = iRomHdr.PageableRomSize;
                int numPages = ( pageAreaStart + pageRomSize + KPageSize - 1 ) / KPageSize;
                //
                return numPages;
            }
        }

        public uint RomPageIndexOffset
        {
            get
            {
                uint ret = iLoaderHdr.Size;
                ret += iRomHdr.RomPageIndex;
                return ret;
            }
        }
        #endregion

        #region Internal constants
        // The signature for Core ROM images used to be EPOCARM4ROM, but now EPOCARM5ROM has
        // also been observed.
        private const int KEpocHeaderLength = 11;
        private static readonly Regex KEpocHeaderTextRegEx = new Regex(
              "EPOCARM\\dROM",
            RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );
        #endregion

        #region Internal methods
        private static bool IsEpocHeader( string aText )
        {
            Match m = KEpocHeaderTextRegEx.Match( aText );
            bool ret = m.Success;
            return ret;
        }

        private void ReadHeaderData( byte[] aBuffer )
        {
            using ( MemoryStream stream = new MemoryStream( aBuffer ) )
            {
                using ( BinaryReader reader = new BinaryReader( stream ) )
                {
                    iLoaderHdr.Read( reader );
                    iRomHdr.Read( reader );
                }
            }
        }
        #endregion

        #region Data members
        private TRomLoaderHeader iLoaderHdr = new TRomLoaderHeader();
        private TRomHeader iRomHdr = new TRomHeader();
        private readonly byte[] iHeaderData;
        #endregion
    }
}
