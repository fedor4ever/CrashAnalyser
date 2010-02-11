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

namespace SymbianImageLib.ROFS.Structures
{
    internal class TRofsHeader
    {
        #region Constructors
        #endregion

        #region API
        public void Read( BinaryReader aReader )
        {
            iIdentifier = SymbianUtils.Strings.StringParsingUtils.BytesToString( aReader.ReadBytes( 4 ) );
            iHeaderSize = aReader.ReadByte();
            iReserved = aReader.ReadByte();
            iRofsFormatVersion = aReader.ReadUInt16();
            iDirTreeOffset = aReader.ReadUInt32();
            iDirTreeSize = aReader.ReadUInt32();
            iDirFileEntriesOffset = aReader.ReadUInt32();
            iDirFileEntriesSize = aReader.ReadUInt32();
            iTime = aReader.ReadInt64();
            iImageVersion.Read( aReader );
            iImageSize = aReader.ReadUInt32();
            iCheckSum = aReader.ReadUInt32();
            iMaxImageSize = aReader.ReadUInt32();
        }
        #endregion

        #region Properties
        public string Identifier
        {
            get { return iIdentifier; }
        }

        public uint Size
        {
            get
            {
                return iHeaderSize;
            }
        }

        public uint RofsFormatVersion
        {
            get { return iRofsFormatVersion; }
        }

        public uint DirTreeOffset
        {
            get { return iDirTreeOffset; }
        }

        public uint DirTreeSize
        {
            get { return iDirTreeSize; }
        }

        public uint DirFileEntriesOffset
        {
            get { return iDirFileEntriesOffset; }
        }

        public uint DirFileEntriesSize
        {
            get { return iDirFileEntriesSize; }
        }

        public uint CheckSum
        {
            get { return iCheckSum; }
        }
        #endregion

        #region Data members
        private string iIdentifier = string.Empty;
        private byte iHeaderSize;
        private byte iReserved;
        private ushort iRofsFormatVersion;
        private uint iDirTreeOffset;	// offset to start of directory structure
        private uint iDirTreeSize;		// size in bytes of directory
        private uint iDirFileEntriesOffset;	// offset to start of file entries
        private uint iDirFileEntriesSize;	// size in bytes of file entry block
        private long iTime;
        private TVersion iImageVersion = new TVersion();		// licensee image version
        private uint iImageSize;
        private uint iCheckSum;
        private uint iMaxImageSize;
        #endregion
    }
}
