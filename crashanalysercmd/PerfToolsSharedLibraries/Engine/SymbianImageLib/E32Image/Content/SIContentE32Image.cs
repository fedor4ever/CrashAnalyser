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
using SymbianImageLib.Common.Streams;
using SymbianImageLib.E32Image.Image;
using SymbianImageLib.E32Image.Header;

namespace SymbianImageLib.E32Image.Content
{
    public class SIContentE32Image : SIContent
    {
        #region Constructors
        internal SIContentE32Image( SymbianImageE32 aImage, string aName, uint aSize, long aImageContentOffset )
            :  base( aImage )
        {
            iName = aName;
            iFileSize = aSize;
            iFileSize = aSize;
        }
        #endregion

        #region From SymbianImageContentFile
        public override TSymbianCompressionType CompressionType
        {
            get { return base.ImageHeader.CompressionType; }
        }

        public override string FileName
        {
            get { return iName; }
        }

        public override uint FileSize
        {
            get { return iFileSize; }
        }

        public override uint ContentSize
        {
            get
            {
                uint ret = 0;
                //
                lock ( iCodeSyncRoot )
                {
                    if ( iCode != null )
                    {
                        ret = (uint) iCode.Length;
                    }
                }
                //
                return ret;
            }
        }

        public override TCheckedUid Uid
        {
            get { return ImageHeader.Uid; }
        }

        public override byte[] GetAllData()
        {
            lock ( iCodeSyncRoot )
            {
                return iCode;
            }
        }

        protected override uint DoProvideDataUInt32( uint aTranslatedAddress )
        {
            uint ret = 0;
            //
            if ( iStream != null )
            {
                using ( SymbianStreamReaderLE reader = SymbianStreamReaderLE.New( iStream, SymbianStreamReaderLE.TCloseOperation.ENone ) )
                {
                    reader.Seek( aTranslatedAddress );
                    ret = reader.ReadUInt32();
                }
            }
            //
            return ret;
        }

        protected override ushort DoProvideDataUInt16( uint aTranslatedAddress )
        {
            ushort ret = 0;
            //
            if ( iStream != null )
            {
                using ( SymbianStreamReaderLE reader = SymbianStreamReaderLE.New( iStream, SymbianStreamReaderLE.TCloseOperation.ENone ) )
                {
                    reader.Seek( aTranslatedAddress );
                    ret = reader.ReadUInt16();
                }
            }
            //
            return ret;
        }

        protected override void DoDecompress()
        {
            lock ( iCodeSyncRoot )
            {
                if ( iCode == null )
                {
                    TSymbianCompressionType type = this.CompressionType;
                    switch ( type )
                    {
                    default:
                    case TSymbianCompressionType.ENone:
                        // NB: This has not yet been observed in reality
                        DecompressNone();
                        break;
                    case TSymbianCompressionType.EDeflate:
                    case TSymbianCompressionType.EBytePair:
                        {
                            using ( SymbianDecompressor decompressor = SymbianDecompressor.NewByType( type ) )
                            {
                                //
                                switch ( type )
                                {
                                case TSymbianCompressionType.EBytePair:
                                    DecompressBytePair( decompressor );
                                    break;
                                case TSymbianCompressionType.EDeflate:
                                    DecompressDeflate( decompressor );
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    if ( iCode != null )
                    {
                        if ( iStream != null )
                        {
                            iStream.Dispose();
                            iStream = null;
                        }
                        //
                        iStream = new MemoryStream( iCode );
                    }
                }
            }
        }

        protected override bool GetIsContentPrepared()
        {
            lock ( iCodeSyncRoot )
            {
                return ( iCode != null );
            }
        }
        #endregion

        #region Properties
        public new SIHeaderE32Image ImageHeader
        {
            get { return (SIHeaderE32Image) base.ImageHeader; }
        }

        public new SymbianImageE32 Image
        {
            get { return (SymbianImageE32) base.Image; }
        }
        #endregion

        #region Internal methods
        private byte[] DecompressCommon( SymbianDecompressor aDecompressor, int aSeekOffset, int aAmountToRead, int aMaximumOutputSize, out int aNumberOfInputBytesRead )
        {
            System.Diagnostics.Debug.WriteLine( "[SIContentE32Image] DecompressCommon - START - " + base.Image.Name + " # " + System.Threading.Thread.CurrentThread.Name );
            //
            uint imageContentSize = iFileSize;
            uint orighdrsz = ImageHeader.TotalSize;
            uint remainder = imageContentSize - orighdrsz;
            //
            using ( SymbianStreamReaderLE reader = base.ImageStream.CreateReader( SymbianStreamReaderLE.TCloseOperation.EResetPosition ) )
            {
                long codePos = Image.ContentOffsetWithinDataStream + orighdrsz + aSeekOffset;
                reader.Seek( codePos );
                //
                byte[] input = reader.ReadBytes( aAmountToRead );
                byte[] output = new byte[ aMaximumOutputSize ];

                // The decompressor tells us how many bytes of output it really created.
                int numberOfBytesCreated = aDecompressor.DecompressImage( input, output, out aNumberOfInputBytesRead );

                // We can then return that to the callee.
                byte[] ret = new byte[ numberOfBytesCreated > 0 ? numberOfBytesCreated : 0 ];
                if ( numberOfBytesCreated > 0 )
                {
                    Array.Copy( output, ret, numberOfBytesCreated );
                }
                //
                System.Diagnostics.Debug.WriteLine( "[SIContentE32Image] DecompressCommon - END - " + base.Image.Name + " # " + System.Threading.Thread.CurrentThread.Name );
                //
                return ret;
            }
        }

        private void DecompressNone()
        {
            uint imageContentSize = iFileSize;
            uint orighdrsz = ImageHeader.TotalSize;
            uint uncompressedSize = ImageHeader.UncompressedSize;
        
            using ( SymbianStreamReaderLE reader = base.ImageStream.CreateReader( SymbianStreamReaderLE.TCloseOperation.EResetPosition ) )
            {
                long codePos = Image.ContentOffsetWithinDataStream + orighdrsz;
                reader.Seek( codePos );
                //
                lock ( iCodeSyncRoot )
                {
                    iCode = reader.ReadBytes( (int) uncompressedSize );
                }
            }
        }

        private void DecompressBytePair( SymbianDecompressor aDecompressor )
        {
            int inputBytesRead = 0;
            //
            uint imageContentSize = iFileSize;
            uint orighdrsz = ImageHeader.TotalSize;
            uint uncompressedSize = ImageHeader.UncompressedSize;

            // First decompress the code
            byte[] code = DecompressCommon( aDecompressor, 0, (int) ( imageContentSize - orighdrsz ), (int) uncompressedSize, out inputBytesRead );
            if ( code.Length < ImageHeader.CodeSize )
            {
                throw new Exception( "E32Image bytepair decompression did not provide enough code" );
            }

            // Now get the data
            int remainder = (int) ( uncompressedSize - inputBytesRead );
            byte[] data = DecompressCommon( aDecompressor, inputBytesRead, remainder, (int) uncompressedSize, out inputBytesRead );

            // We should have read all the decompressed data
            int totalAmountOfDecompressedDataSupplied = data.Length + code.Length;
            if ( totalAmountOfDecompressedDataSupplied != uncompressedSize )
            {
                throw new Exception( "E32Image bytepair decompression did not supply enough decompressed output" );
            }

            lock ( iCodeSyncRoot )
            {
                iCode = new byte[ uncompressedSize ];
                Array.Copy( code, iCode, code.Length );
                Array.Copy( data, 0, iCode, code.Length, data.Length );
            }
        }

        private void DecompressDeflate( SymbianDecompressor aDecompressor )
        {
            int inputBytesRead = 0;
            //
            uint imageContentSize = iFileSize;
            uint orighdrsz = ImageHeader.TotalSize;
            uint uncompressedSize = ImageHeader.UncompressedSize;

            byte[] combinedDataAndCode = DecompressCommon( aDecompressor, 0, (int) ( imageContentSize - orighdrsz ), (int) uncompressedSize, out inputBytesRead );
            if ( combinedDataAndCode.Length != uncompressedSize )
            {
                throw new Exception( "E32Image inflate decompression did not supply enough decompressed output" );
            }

            lock ( iCodeSyncRoot )
            {
                iCode = combinedDataAndCode;
            }
        }
        #endregion

        #region From DisposableObject
        protected override void CleanupManagedResources()
        {
            try
            {
                base.CleanupManagedResources();
            }
            finally
            {
                if ( iStream != null )
                {
                    iStream.Dispose();
                    iStream = null;
                }
            }
        }
        #endregion

        #region Data members
        private readonly string iName;
        private readonly uint iFileSize;
        private byte[] iCode = null;
        private object iCodeSyncRoot = new object();
        private MemoryStream iStream = null;
        #endregion
    }
}
