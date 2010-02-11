/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using CrashDebuggerLib.Structures.KernelObjects;
using CrashDebuggerLib.Structures.Common;
using CrashDebuggerLib.Structures.Process;

namespace CrashDebuggerLib.Structures.Chunk
{
    public class DChunk : DObject
    {
        #region Enumerations
        [System.ComponentModel.TypeConverter( typeof( SymbianParserLib.TypeConverters.SymbianEnumConverter ) )]
        public enum TChunkType
        {
            EUnknown = -1,

            // these never change or move or anything
            EKernelData = 0,	// Supervisor,Rw,Cacheable
            EKernelStack = 1,	// Supervisor,Rw,Cacheable
            EKernelCode = 2,	// Supervisor,Rw,Cacheable
            EDll = 3,			// User,Ro,Cacheable
            EUserCode = 4,

            // This changes on a PDE basis when the file server runs
            ERamDrive = 5,		// Supervisor,Rw,Cacheable

            // these change on PDE basis when run
            EUserData = 6,
            EDllData = 7,
            EUserSelfModCode = 8,

            ESharedKernelSingle = 9,		// Must be same value as TChunkCreateInfo::ESharedKernelSingle
            ESharedKernelMultiple = 10,		// Must be same value as TChunkCreateInfo::ESharedKernelMultiple

            ESharedIo = 11,
            ESharedKernelMirror = 12,
        }

        [Flags, System.ComponentModel.TypeConverter( typeof( SymbianParserLib.TypeConverters.SymbianEnumConverter ) )]
        public enum TChunkAttributes : uint
        {
            // Basic range
            //ENormal             = 0x00000000, // Commented out because it conflicts with EAddressLocal
            EDoubleEnded        = 0x00000001,
            EDisconnected       = 0x00000002,
            EConstructed        = 0x00000004,
		    EMemoryNotOwned     = 0x00000008,

            // From Multiple Memory Model
            EPrivate			= 0x80000000,
		    ECode				= 0x40000000,
		    EAddressAllocDown	= 0x20000000,

		    EAddressLocal		= 0x00000000,
		    EAddressShared		= 0x01000000,
		    EAddressUserGlobal	= 0x02000000,
		    EAddressKernel		= 0x03000000,
		    EAddressFixed		= 0x04000000,

		    EMapTypeMask		= 0x00c00000,
		    EMapTypeLocal		= 0x00000000,
		    EMapTypeGlobal		= 0x00400000,
		    EMapTypeShared		= 0x00800000,
        }
        #endregion

        #region Constructors
        public DChunk( CrashDebuggerInfo aCrashDebugger )
            : base( aCrashDebugger, TObjectType.EChunk )
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint OwningProcessAddress
        {
            get { return iOwningProcessAddress; }
            set { iOwningProcessAddress = value; }
        }

        public DProcess OwningProcess
        {
            get { return CrashDebugger.ProcessByAddress( OwningProcessAddress ); }
        }

        public uint OSAsids
        {
            get { return iOsAsids; }
            set { iOsAsids = value; }
        }

        public uint Size
        {
            get { return iSize; }
            set { iSize = value; }
        }

        public uint MaxSize
        {
            get { return iMaxSize; }
            set { iMaxSize = value; }
        }

        public uint Base
        {
            get { return iBase; }
            set { iBase = value; }
        }

        public TChunkAttributes Attributes
        {
            get { return iAttributes; }
            set { iAttributes = value; }
        }

        public uint StartPos
        {
            get { return iStartPos; }
            set { iStartPos = value; }
        }

        public TChunkType ChunkType
        {
            get { return iChunkType; }
            set { iChunkType = value; }
        }

        public ChunkPermissions Permissions
        {
            get { return iPermissions; }
        }

        public uint PageTables
        {
            get { return iPageTables; }
            set { iPageTables = value; }
        }

        public uint PageBitMap
        {
            get { return iPageBitMap; }
            set { iPageBitMap = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        private uint iOwningProcessAddress = 0;
        private uint iOsAsids = 0;
        private uint iSize = 0;
        private uint iMaxSize = 0;
        private uint iBase = 0;
        private TChunkAttributes iAttributes = TChunkAttributes.EAddressLocal;
        private uint iStartPos = 0;
        private TChunkType iChunkType = TChunkType.EUnknown;
        private ChunkPermissions iPermissions = new ChunkPermissions();
        private uint iPageTables = 0;
        private uint iPageBitMap = 0;
        #endregion
    }
}
