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
using SymbianStructuresLib.Version;

namespace SymbianImageLib.ROM.Structures
{
    internal class TRomHeader
    {
        #region Constructors
        #endregion

        #region API
        public void Read( BinaryReader aReader )
        {
            aReader.BaseStream.Read( iJump, 0, iJump.Length );
            iRestartVector = aReader.ReadUInt32();
            iTime = aReader.ReadInt64();
            iTimeHi = aReader.ReadUInt32();
            iRomBase = aReader.ReadUInt32();
            iRomSize = aReader.ReadUInt32();

            iRomRootDirectoryList = aReader.ReadUInt32();
            iKernDataAddress = aReader.ReadUInt32();
            iKernelLimit = aReader.ReadUInt32();
            iPrimaryFile = aReader.ReadUInt32();
            iSecondaryFile = aReader.ReadUInt32();
            iCheckSum = aReader.ReadUInt32();
            
            // Print the checksum to aid debugging
            System.Diagnostics.Debug.WriteLine( string.Format( "[SIHeaderROM] checksum: 0x{0:x8}", iCheckSum ) );
            
            iHardware = aReader.ReadUInt32();

            iLanguage = aReader.ReadInt64();

            iKernelConfigFlags = aReader.ReadUInt32();
            iRomExceptionSearchTable = aReader.ReadUInt32();
            iRomHeaderSize = aReader.ReadUInt32();
            iRomSectionHeader = aReader.ReadUInt32();

            iTotalSvDataSize = aReader.ReadInt32();

            iVariantFile = aReader.ReadUInt32();
            iExtensionFile = aReader.ReadUInt32();
            iRelocInfo = aReader.ReadUInt32();
            iOldTraceMask = aReader.ReadUInt32();
            iUserDataAddress = aReader.ReadUInt32();
            iTotalUserDataSize = aReader.ReadUInt32();
            iDebugPort = aReader.ReadUInt32();

            iVersion.Read( aReader );

            iCompressionType = aReader.ReadUInt32();
            iCompressedSize = aReader.ReadUInt32();
            iUncompressedSize = aReader.ReadUInt32();

            ReadUintArray( aReader, iDisabledCapabilities );
            ReadUintArray( aReader, iTraceMask );
            ReadUintArray( aReader, iInitialBTraceFilter );

            iInitialBTraceBuffer = aReader.ReadInt32();
            iInitialBTraceMode = aReader.ReadInt32();
            iPageableRomStart = aReader.ReadInt32();
            iPageableRomSize = aReader.ReadInt32();
            iRomPageIndex = aReader.ReadInt32();

            iDemandPagingConfig.Read( aReader );

            ReadUintArray( aReader, iSpare );
        }
        #endregion

        #region Properties
        public uint Size
        {
            get
            {
                return iRomHeaderSize;
            }
        }

        public uint RomBaseAddress
        {
            get { return iRomBase; }
        }

        public uint CompressedSize
        {
            get { return iCompressedSize; }
        }

        public uint UncompressedSize
        {
            get { return iUncompressedSize; }
        }

        public uint CompressionType
        {
            get { return iCompressionType; }
        }

        public int PageableRomStart
        {
            get { return iPageableRomStart; }
        }

        public int PageableRomSize
        {
            get { return iPageableRomSize; }
        }

        public uint RomPageIndex
        {
            get
            {
                return (uint) iRomPageIndex;
            }
        }
        #endregion

        #region Internal constants
        private const int KNumTraceMask = 8;
        #endregion

        #region Internal methods
        private void ReadUintArray( BinaryReader aReader, uint[] aArray )
        {
            for ( int i = 0; i < aArray.Length; i++ )
            {
                byte[] b = aReader.ReadBytes( 4 );
                uint val = 0;
                //
                val += (uint) ( b[ 0 ] );
                val += (uint) ( b[ 1 ] << 08 );
                val += (uint) ( b[ 2 ] << 16 );
                val += (uint) ( b[ 3 ] << 24 );
                //
                aArray[ i ] = val;
            }
        }
        #endregion

        #region Data members
        private byte[] iJump = new byte[ 124 ];
        private uint iRestartVector;
        private long iTime;
        private uint iTimeHi;

        // Default to the old moving memory model base address. 
        private uint iRomBase = 0xf8000000;
        private uint iRomSize;
        private uint iRomRootDirectoryList;
        private uint iKernDataAddress;
        private uint iKernelLimit;
        private uint iPrimaryFile;
        private uint iSecondaryFile;
        private uint iCheckSum;
        private uint iHardware;
        private long iLanguage;
        private uint iKernelConfigFlags;
        private uint iRomExceptionSearchTable;
        private uint iRomHeaderSize;
        private uint iRomSectionHeader;
        private int iTotalSvDataSize;
        private uint iVariantFile;
        private uint iExtensionFile;
        private uint iRelocInfo;
        private uint iOldTraceMask;
        private uint iUserDataAddress;
        private uint iTotalUserDataSize;
        private uint iDebugPort;
        private TVersion iVersion = new TVersion();
        private uint iCompressionType;
        private uint iCompressedSize;
        private uint iUncompressedSize;
        private uint[] iDisabledCapabilities = new uint[ 2 ];
        private uint[] iTraceMask = new uint[ KNumTraceMask ];
        private uint[] iInitialBTraceFilter = new uint[ 8 ];
        private int iInitialBTraceBuffer;
        private int iInitialBTraceMode;
        private int iPageableRomStart;
        private int iPageableRomSize;
        private int iRomPageIndex;
        private SDemandPagingConfig iDemandPagingConfig = new SDemandPagingConfig();
        private uint[] iSpare = new uint[ 40 ];
        #endregion
    }
}
