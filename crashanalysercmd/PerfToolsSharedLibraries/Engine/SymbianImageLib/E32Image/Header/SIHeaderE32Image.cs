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
using SymbianUtils.Tracer;
using SymbianUtils.Streams;
using SymbianUtils.Strings;
using SymbianStructuresLib.Version;
using SymbianStructuresLib.Uids;
using SymbianStructuresLib.Security;
using SymbianStructuresLib.Compression.Common;
using SymbianImageLib.Common.Header;
using SymbianImageLib.Common.Image;
using SymbianImageLib.E32Image.Exceptions;

namespace SymbianImageLib.E32Image.Header
{
    public class SIHeaderE32Image : SIHeader
    {
        #region Constructors
        internal SIHeaderE32Image( SIImage aImage, SymbianStreamReaderLE aReader )
            : base( aImage )
        {
            long startPos = aReader.Position;
            //
            iUids = new TCheckedUid( aReader );
            iSignature = aReader.ReadUInt32();
            //
            if ( iSignature != KExpectedSignatureUInt32 ) // 'EPOC'
            {
                throw new E32ImageNotSupportedException( "Invalid signature" );
            }
            //
            iHeaderCrc = aReader.ReadUInt32();
            iModuleVersion = aReader.ReadUInt32();
            iCompressionType = SIHeaderE32Image.ReadCompressionType( aReader );
            iToolsVersion = new TVersion( aReader );
            iTimeLo = aReader.ReadUInt32();
            iTimeHi = aReader.ReadUInt32();
            iFlags = aReader.ReadUInt32();
            //
            iCodeSize = aReader.ReadInt32();
            iDataSize = aReader.ReadInt32();
            iHeapSizeMin = aReader.ReadInt32();
            iHeapSizeMax = aReader.ReadInt32();
            iStackSize = aReader.ReadInt32();
            iBssSize = aReader.ReadInt32();
            //
            iEntryPoint = aReader.ReadUInt32();
            iCodeBase = aReader.ReadUInt32();
            iDataBase = aReader.ReadUInt32();
            //
            iDllRefTableCount = aReader.ReadInt32();
            iExportDirOffset = aReader.ReadUInt32();
            iExportDirCount = aReader.ReadInt32();
            //
            iTextSize = aReader.ReadInt32();
            iCodeOffset = aReader.ReadUInt32();
            iDataOffset = aReader.ReadUInt32();
            iImportOffset = aReader.ReadUInt32();
            iCodeRelocOffset = aReader.ReadUInt32();
            iDataRelocOffset = aReader.ReadUInt32();
            //
            iProcessPriority = aReader.ReadInt16();
            //
            iCpuIdentifier = aReader.ReadUInt16();
            //
            iUncompressedSize = aReader.ReadUInt32();
            //
            iS = new SSecurityInfo( aReader );
            iExceptionDescriptor = aReader.ReadUInt32();
            iSpare2 = aReader.ReadUInt16();
            iExportDescSize = aReader.ReadUInt16();
            iExportDescType = aReader.ReadUInt8();
            //
            iHeaderSize = (uint) ( aReader.Position - startPos );
        }
        #endregion

        #region Constants
        public const int KMinimumSize = 16 + 4; // Enough to read UIDs + signature
        #endregion

        #region API
        public static bool IsSymbianImageHeader( byte[] aHeader )
        {
            // We expect to see 16 bytes (3 x UID, 1 x UID checksum) and then
            // the magic word EPOC
            bool ret = false;
            //
            if ( aHeader.Length >= KMinimumSize )
            {
                // We expect 16 bytes are the UID + checksum. Next should be the signature.
                string sig = StringParsingUtils.BytesToString( aHeader, 16, 20 );
                ret = ( sig == KExpectedSignature );
            }
            //
            return ret;
        }
        #endregion

        #region Constants
        public const uint KExpectedSignatureUInt32 = 0x434f5045;
        public const string KExpectedSignature = "EPOC";
        #endregion

        #region Properties
        public TCheckedUid Uid
        {
            get
            {
                return iUids;
            }
        }

        public int CodeSize
        {
            get { return iCodeSize; }
        }

        public int DataSize
        {
            get { return iDataSize; }
        }

        public uint CodeOffset
        {
            get { return iCodeOffset; }
        }

        public uint UncompressedSize
        {
            get { return iUncompressedSize; }
        }

        public uint TotalSize
        {
            // Just like in E32ImageHeader
            get { return CodeOffset; }
        }
        #endregion

        #region From SymbianImageHeader
        public override uint HeaderSize
        {
            get { return iHeaderSize; }
        }

        public override TSymbianCompressionType CompressionType
        {
            get { return iCompressionType; }
        }
        #endregion

        #region Internal methods
        private static TSymbianCompressionType ReadCompressionType( SymbianStreamReaderLE aReader )
        {
            TSymbianCompressionType ret = TSymbianCompressionType.ENone;
            //
            uint type = aReader.ReadUInt32();
            if ( type == (uint) TSymbianCompressionType.EBytePair )
            {
                ret = TSymbianCompressionType.EBytePair;
            }
            else if ( type == (uint) TSymbianCompressionType.EDeflate )
            {
                ret = TSymbianCompressionType.EDeflate;
            }
            else
            {
                throw new E32ImageNotSupportedException( "Unsupported compression type" );
            }
            //
            return ret;
        }
        #endregion

        #region Data members
        private readonly TCheckedUid iUids;
        //
        private readonly uint iSignature;			                            // Contains 'EPOC'.
        private readonly uint iHeaderCrc;			                            // CRC-32 of entire header. @see #KImageCrcInitialiser.
        private readonly uint iModuleVersion;		                            // Version number for this executable (used in link resolution).
        private readonly TSymbianCompressionType iCompressionType;	        // Type of compression used for file contents located after the header. (UID or 0 for none).
        //
        private readonly TVersion iToolsVersion;		                        // Version number of tools which generated this file.
        //
        private readonly uint iTimeLo;			                                // Least significant 32 bits of the time of image creation, in milliseconds since since midnight Jan 1st, 2000.
        private readonly uint iTimeHi;			                                // Most significant 32 bits of the time of image creation, in milliseconds since since midnight Jan 1st, 2000.
        private readonly uint iFlags;				                            // Contains various bit-fields of attributes for the image.
        //
        private readonly int iCodeSize;				                            // Size of executables code. Includes import address table, constant data and export directory.
        private readonly int iDataSize;				                            // Size of executables initialised data.
        private readonly int iHeapSizeMin;			                            // Minimum size for an EXEs runtime heap memory.
        private readonly int iHeapSizeMax;			                            // Maximum size for an EXEs runtime heap memory.
        private readonly int iStackSize;			                            // Size for stack required by an EXEs initial thread.
        private readonly int iBssSize;				                            // Size of executables uninitialised data.
        //
        private readonly uint iEntryPoint;			                            // Offset into code of the entry point.
        private readonly uint iCodeBase;			                            // Virtual address that the executables code is linked for.
        private readonly uint iDataBase;			                            // Virtual address that the executables data is linked for.
        //
        private readonly int iDllRefTableCount;		                            // Number of executable against which this executable is linked. The number of files mention in the import section at iImportOffset.
        private readonly uint iExportDirOffset;		                            // Byte offset into file of the export directory.
        private readonly int iExportDirCount;		                            // Number of entries in the export directory.
        //
        private readonly int iTextSize;				                            // Size of just the text section, also doubles as the offset for the Import Address Table w.r.t. the code section.
        private readonly uint iCodeOffset;			                            // Offset into file of the code section. Also doubles the as header size.
        private readonly uint iDataOffset;			                            // Offset into file of the data section.
        private readonly uint iImportOffset;		                            // Offset into file of the import section (E32ImportSection).
        private readonly uint iCodeRelocOffset;		                            // Offset into file of the code relocation section (E32RelocSection).
        private readonly uint iDataRelocOffset;		                            // Offset into file of the data relocation section (E32RelocSection).
        private readonly short iProcessPriority; 	                            // Initial runtime process priorty for an EXE. (Value from enum TProcessPriority.)
        private readonly ushort iCpuIdentifier;		                            // Value from enum TCpu which indicates the CPU architecture for which the image was created
        //
        private readonly uint iUncompressedSize;	                            // Uncompressed size of file data after the header, or zero if file not compressed.
        //
	    private readonly SSecurityInfo iS;				                        // Platform Security information of executable.
	    private readonly uint iExceptionDescriptor;                             // Offset in bytes from start of code section to Exception Descriptor, bit 0 set if valid.
	    private readonly ushort iSpare2;				                        // Reserved for future use. Set to zero.
	    private readonly ushort iExportDescSize;		                        // Size of export description stored in iExportDesc.
	    private readonly byte iExportDescType;		                            // Type of description of holes in export table

        // Not part of header
        private readonly uint iHeaderSize;
        #endregion
    }
}
