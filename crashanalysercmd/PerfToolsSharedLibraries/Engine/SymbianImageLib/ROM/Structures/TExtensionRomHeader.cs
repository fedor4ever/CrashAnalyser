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
using System.Runtime.InteropServices;
using SymbianStructuresLib.Version;

namespace SymbianImageLib.ROM.Structures
{
    internal class TExtensionRomHeader
    {
        #region Constructors
        #endregion

        #region API
        public void Read( BinaryReader aReader )
        {
            iVersion.Read( aReader );
            iRomBase = aReader.ReadUInt32();
            iRomSize = aReader.ReadUInt32();
            iRomRootDirectoryList = aReader.ReadUInt32();
            iTime = aReader.ReadInt64();
            iCheckSum = aReader.ReadUInt32();
            iKernelVersion.Read( aReader );
            iKernelTime = aReader.ReadInt64();
            iKernelCheckSum = aReader.ReadUInt32();
            iCompressionType = aReader.ReadUInt32();
            iCompressedSize = aReader.ReadUInt32();
            iUncompressedSize = aReader.ReadUInt32();
            iRomExceptionSearchTable = aReader.ReadUInt32();
            //
            int size = Marshal.SizeOf( iPad );
            byte[] pad = aReader.ReadBytes( size );
        }
        #endregion

        #region Properties
        #endregion

        #region Internal constants
        #endregion

        #region Data members
        private TVersion iVersion = new TVersion();
        private uint iRomBase;
        private uint iRomSize;
        private uint iRomRootDirectoryList;
        private long iTime;
        private uint iCheckSum;
        private TVersion iKernelVersion = new TVersion();
        private long iKernelTime;
        private uint iKernelCheckSum;
        private uint iCompressionType;	// compression type used
        private uint iCompressedSize;	// Size after compression
        private uint iUncompressedSize;	// Size before compression
        private uint iRomExceptionSearchTable;
        private uint[] iPad = new uint[ 32 - 15 ]; // sizeof(TExtensionRomHeader)=128
        #endregion
    }
}
