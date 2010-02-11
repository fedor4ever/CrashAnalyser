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
using System.IO;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianUtils.Streams;
using SymbianStructuresLib.Uids;
using SymbianStructuresLib.Compression.Common;
using SymbianImageLib.Common.Content;
using SymbianImageLib.Common.Image;
using SymbianImageLib.Common.Header;
using SymbianImageLib.Common.Streams;
using SymbianImageLib.Common.Factory;
using SymbianImageLib.ROM.Image;
using SymbianImageLib.ROM.Header;
using SymbianImageLib.ROM.Structures;

namespace SymbianImageLib.ROM.Content
{
    internal class SIContentROM : SIContent
    {
        #region Constructors
        public SIContentROM( SIROM aImage )
            : base( aImage )
        {
            SIHeaderROM header = (SIHeaderROM) aImage.Header;
            base.RelocationAddress = header.RomBaseAddress;
        }
        #endregion

        #region From SymbianImageContentFile
        public override TSymbianCompressionType CompressionType
        {
            get
            {
                // Compression type comes from overall image header
                TSymbianCompressionType ret = base.ImageHeader.CompressionType;
                return ret;
            }
        }

        public override bool IsRelocationSupported
        {
            get { return false; }
        }

        public override string FileName
        {
            get { return Image.Name; }
        }

        public override uint FileSize
        {
            get { return (uint) base.Image.Stream.Length; }
        }

        public override uint ContentSize
        {
            get
            {
                // This is not 100% accurate, but hopefully good enough...
                uint size = (uint) base.ImageStream.Length;
                return size;
            }
        }

        public override byte[] GetAllData()
        {
            // The image ownes a memory stream (if the image has been decompressed)
            byte[] ret = new byte[ 0 ];
            //
            if ( base.ImageStream is SIMemoryStream )
            {
                SIMemoryStream memStream = (SIMemoryStream) base.ImageStream;
                ret = memStream.Data;
            }
            //
            return ret;
        }

        protected override uint DoProvideDataUInt32( uint aTranslatedAddress )
        {
            using ( SymbianStreamReaderLE reader = Image.Stream.CreateReader( SymbianStreamReaderLE.TCloseOperation.ENone ) )
            {
                reader.Seek( aTranslatedAddress );
                uint ret = reader.ReadUInt32();
                return ret;
            }
        }

        protected override ushort DoProvideDataUInt16( uint aTranslatedAddress )
        {
            using ( SymbianStreamReaderLE reader = Image.Stream.CreateReader( SymbianStreamReaderLE.TCloseOperation.ENone ) )
            {
                reader.Seek( aTranslatedAddress );
                ushort ret = reader.ReadUInt16();
                return ret;
            }
        }

        protected override void DoDecompress()
        {
            base.Trace( "[SymbianImageContentFileROM] DoDecompress() - START - compression type: {0}", CompressionType );
            //
            switch ( CompressionType )
            {
            case TSymbianCompressionType.ENone:
                base.Trace( "[SymbianImageContentFileROM] DoDecompress() - not compressed" );
                break;
            case TSymbianCompressionType.EBytePair:
                base.Trace( "[SymbianImageContentFileROM] DoDecompress() - byte pair" );
                DoDecompressBytePair();
                break;
            default:
            case TSymbianCompressionType.EDeflate:
                base.Trace( "[SymbianImageContentFileROM] DoDecompress() - unsuporrted compression type" );
                throw new NotSupportedException();
            }
            //
            base.Trace( "[SymbianImageROM] DoDecompress() - END" );
        }

        protected override bool GetIsContentPrepared()
        {
            return iContentIsPrepared;
        }
        #endregion

        #region Properties
        public new SIHeaderROM ImageHeader
        {
            get
            {
                SIHeaderROM header = (SIHeaderROM) base.ImageHeader;
                return header;
            }
        }
        #endregion

        #region Internal methods
        private void DoDecompressBytePair()
        {
            if ( iContentIsPrepared == false )
            {
                SIHeaderROM imageHeader = ImageHeader;
                base.Trace( "[SymbianImageROM] DoDecompressBytePair() - START - header uncompressed rom size: {0}", imageHeader.UncompressedRomSize );

                // Create new buffer and copy over rom image header
                SIMemoryStream resultantDataStream = new SIMemoryStream( imageHeader.UncompressedRomSize );

                int numPages = imageHeader.NumberOfPages;
                uint pageTableOffset = imageHeader.RomPageIndexOffset;
                uint romDataOffset = imageHeader.HeaderSizeLoader;
                base.Trace( "[SymbianImageROM] DoDecompressBytePair() - numPages: {0}, pageTableOffset: {1}, romDataOffset: {2}", numPages, pageTableOffset, romDataOffset );
                //
                SymbianDecompressor decompressor = SymbianDecompressor.NewByType( TSymbianCompressionType.EBytePair );
                //
                List<SRomPageInfo> pages = new List<SRomPageInfo>( numPages + 1 );
                for ( int i = 0; i < numPages; i++ )
                {
                    // Read a page table entry
                    long pageOffsetWithinFile = pageTableOffset + ( i * SRomPageInfo.Size );

                    //base.Trace( "[SymbianImageROM] DoDecompressBytePair() - page[{0:d5}] - pageOffsetWithinFile: {1}", i, pageOffsetWithinFile );
                    base.ImageStream.Seek( pageOffsetWithinFile, SeekOrigin.Begin );
                    SRomPageInfo pageInfo = SRomPageInfo.New( (Stream) base.ImageStream, romDataOffset );

                    // Process the entry based upon the compression type
                    //base.Trace( "[SymbianImageROM] DoDecompressBytePair() - page[{0:d5}] - pageInfo.DataSize: {1}, pageInfo.DataStart: {2}, pageInfo.CompressionType: {3}", i, pageInfo.DataSize, pageInfo.DataStart, pageInfo.CompressionType );
                    base.ImageStream.Seek( pageInfo.DataStart, SeekOrigin.Begin );

                    if ( pageInfo.CompressionType == SRomPageInfo.TSymbianCompressionType.ENoCompression )
                    {
                        // Read data - no decompression needed
                        //base.Trace( "[SymbianImageROM] DoDecompressBytePair() - page[{0:d5}] - PAGE NOT COMPRESSED", i );
                        resultantDataStream.Write( base.ImageStream, (int) pageInfo.DataSize );
                    }
                    else if ( pageInfo.CompressionType == SRomPageInfo.TSymbianCompressionType.EBytePair )
                    {
                        //base.Trace( "[SymbianImageROM] DoDecompressBytePair() - page[{0:d5}] - BYTE PAIR PAGE", i );

                        // Read data - need to decompress it
                        byte[] compressedData = new byte[ pageInfo.DataSize ];
                        base.ImageStream.Read( compressedData, 0, compressedData.Length );

                        // Make destination buffer - which is a page big
                        byte[] uncompressedData = new byte[ SRomPageInfo.KPageSize ];

                        // Decompress to buffer - we're handling the page management, so we want raw decompression
                        int error = decompressor.DecompressRaw( compressedData, uncompressedData );

                        // Save it
                        if ( error < 0 )
                        {
                            base.Trace( "[SymbianImageROM] DoDecompressBytePair() - page[{0:d5}] - Exception - bytepair decompression error", i );
                            throw new Exception( "BytePair decompression error: " + error.ToString() );
                        }
                        else if ( error != SRomPageInfo.KPageSize )
                        {
                            base.Trace( "[SymbianImageROM] DoDecompressBytePair() - page[{0:d5}] - Exception - bytepair underflow error", i );
                            throw new Exception( "Decompressor underflow - only created " + error.ToString() + " bytes" );
                        }
                        else
                        {
                            resultantDataStream.Write( uncompressedData );
                        }
                    }
                    else
                    {
                        base.Trace( "[SymbianImageROM] DoDecompressBytePair() - page[{0:d5}] - UNSUPPORTED COMPRESSION TYPE - Exception!", i );
                        throw new NotSupportedException( "Unsupported page compression type" );
                    }

                    // Report progress
                    base.ReportDecompressionEvent( TDecompressionEvent.EEventDecompressionProgress, ( (float) i / (float) numPages ) * 100 );
                }

                // Now we can replace the base class stream (which is just the raw compressed file data) with the new uncompressed version
                base.ImageStream = resultantDataStream;
                iContentIsPrepared = true;

                base.Trace( "[SymbianImageROM] DoDecompressBytePair() - END" );
            }
        }
        #endregion

        #region Data members
        private bool iContentIsPrepared = false;
        #endregion
    }
}
