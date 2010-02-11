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
using System.IO;
using System.Text;
using System.Collections.Generic;
using SymbianUtils;
using SymbianUtils.Range;
using SymbianUtils.Streams;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Common.Id;
using SymbianStructuresLib.Debug.Common.FileName;
using SymbianStructuresLib.Debug.Common.Interfaces;
using SymbianStructuresLib.Debug.Code.Interfaces;

namespace SymbianStructuresLib.Debug.Code
{
    public sealed class CodeCollection : DisposableObject, IComparable<CodeCollection>, IArmInstructionProvider
    {
        #region Static constructors
        public static CodeCollection New( IPlatformIdAllocator aIdAllocator, string aFileNameInHost, string aFileNameInDevice )
        {
            CodeCollection ret = new CodeCollection( aIdAllocator, aFileNameInHost, aFileNameInDevice );
            return ret;
        }

        public static CodeCollection NewCopy( IPlatformIdAllocator aIdAllocator, CodeCollection aCollection )
        {
            CodeCollection ret = new CodeCollection( aIdAllocator, aCollection );
            return ret;
        }

        public static CodeCollection NewByHostFileName( IPlatformIdAllocator aIdAllocator, string aFileName )
        {
            CodeCollection ret = new CodeCollection( aIdAllocator, aFileName );
            return ret;
        }
        #endregion

        #region Delegates & events
        public delegate void RelocationStatusChangeHandler( CodeCollection aCollection );
        public event RelocationStatusChangeHandler RelocationStatusChanged;
        #endregion

        #region Constructors
        private CodeCollection( IPlatformIdAllocator aIdAllocator, string aFileNameInHost )
		{
            iId = aIdAllocator.AllocateId();
            iFileName = PlatformFileName.NewByHostName( aFileNameInHost );
		}

        private CodeCollection( IPlatformIdAllocator aIdAllocator, string aFileNameInHost, string aFileNameInDevice )
            : this( aIdAllocator, aFileNameInHost )
        {
            iFileName.FileNameInDevice = aFileNameInDevice;
        }

        private CodeCollection( IPlatformIdAllocator aIdAllocator, CodeCollection aCopy )
        {
            iId = aIdAllocator.AllocateId();
            //
            iCode = aCopy.iCode;
            iTag = aCopy.iTag;
            iFlags = aCopy.iFlags;
            iTagged = aCopy.iTagged;
            iBaseAddress = aCopy.iBaseAddress;
            iCodeSegmentResolver = aCopy.iCodeSegmentResolver;
            iRelocationHandler = aCopy.iRelocationHandler;
            iFileName = PlatformFileName.New( aCopy.FileName );
            iInstructionConverter = aCopy.IfaceInstructionConverter;
            iCodeSegmentResolver = aCopy.IfaceCodeSegmentResolver;
            iRelocationHandler = aCopy.IfaceRelocationHandler;
        }            
		#endregion

        #region API
        public bool Contains( uint aAddress )
        {
            bool ret = IsInstructionAddressValid( aAddress );
            return ret;
        }

        public bool IsMatchingCodeSegment( CodeSegDefinition aCodeSegment )
        {
            bool ret = false;
            //
            if ( iCodeSegmentResolver != null )
            {
                ret = iCodeSegmentResolver.IsMatchingCodeSegment( this, aCodeSegment );
            }
            else
            {
                PlatformFileName codeSegName = PlatformFileName.NewByDeviceName( aCodeSegment.FileName );
                ret = FileName.Equals( codeSegName );
            }
            //
            return ret;
        }

        public void Relocate( uint aTo )
        {
            if ( aTo != iBaseAddress )
            {
                uint old = iBaseAddress;
                iBaseAddress = aTo;
                //
                if ( iRelocationHandler != null )
                {
                    iRelocationHandler.PrepareForRelocation( this, old, BaseAddress );
                }
            }
        }
        #endregion

		#region Properties
		public uint Size
		{
            get
            {
                uint ret = 0;
                if ( iCode != null )
                {
                    ret = (uint) iCode.Length;
                }
                return ret;
            }
		}

        public bool Tagged
        {
            get { return iTagged; }
            set
            { 
                iTagged = value; 
            }
        }

        public bool IsRelocatable
        {
            get { return ( iFlags & TFlags.EFlagsIsRelocatable ) == TFlags.EFlagsIsRelocatable; }
            set
            {
                lock ( iFlagLock )
                {
                    bool wasSet = ( iFlags & TFlags.EFlagsIsRelocatable ) == TFlags.EFlagsIsRelocatable;
                    if ( wasSet != value )
                    {
                        if ( value )
                        {
                            iFlags |= TFlags.EFlagsIsRelocatable;
                        }
                        else
                        {
                            iFlags &= ~TFlags.EFlagsIsRelocatable;
                        }

                        // Report event if needed
                        if ( RelocationStatusChanged != null )
                        {
                            RelocationStatusChanged( this );
                        }
                    }
                }
            }
        }

        public bool IsFixed
        {
            get { return !IsRelocatable; }
            set { IsRelocatable = !value; }
        }

        public object Tag
        {
            get { return iTag; }
            set
            {
                iTag = value;
            }
        }

        public uint BaseAddress
        {
            get
            {
                uint ret = iBaseAddress;
                return ret;
            }
        }

        public bool IsCodeAvailable
        {
            get { return iCode != null; }
        }

        public byte[] Code
        {
            get { return iCode; }
            set
            {
                iCode = value;
            }
        }

        public PlatformId Id
        {
            get { return iId; }
        }

        public PlatformFileName FileName
		{
			get { return iFileName; }
		}

        public CodeCollectionList ParentList
        {
            get { return iParentList; }
            internal set { iParentList = value; }
        }
		#endregion

        #region Properties - interfaces
        public ICodeCollectionCodeSegmentResolver IfaceCodeSegmentResolver
        {
            get { return iCodeSegmentResolver; }
            set { iCodeSegmentResolver = value; }
        }

        public ICodeCollectionRelocationHandler IfaceRelocationHandler
        {
            get { return iRelocationHandler; }
            set { iRelocationHandler = value; }
        }

        public ICodeCollectionInstructionConverter IfaceInstructionConverter
        {
            get { return iInstructionConverter; }
            set { iInstructionConverter = value; }
        }
        #endregion

        #region Internal enumerations
        [Flags]
        private enum TFlags : byte
        {
            EFlagsNone = 0,
            EFlagsIsRelocatable = 1
        };
        #endregion

        #region Internal constants
        #endregion

        #region Internal methods
        #endregion

        #region From IArmInstructionProvider
        public bool IsInstructionAddressValid( uint aAddress )
        {
            bool valid = false;
            //
            if ( IsCodeAvailable )
            {
                AddressRange range = new AddressRange( this.BaseAddress, 0 );
                range.UpdateMax( range.Min + iCode.Length );
                //
                valid = range.Contains( aAddress );
            }
            //
            return valid;
        }

        public uint GetDataUInt32( uint aAddress )
        {
            uint ret = 0;
            IArmInstruction[] inst = null;
            //
            bool available = GetInstructions( aAddress, TArmInstructionSet.EARM, 1, out inst );
            if ( available && inst.Length >= 1 )
            {
                ret = inst[ 0 ].AIRawValue;
            }
            //
            return ret;
        }

        public ushort GetDataUInt16( uint aAddress )
        {
            ushort ret = 0;
            IArmInstruction[] inst = null;
            //
            bool available = GetInstructions( aAddress, TArmInstructionSet.ETHUMB, 1, out inst );
            if ( available && inst.Length >= 1 )
            {
                ret = inst[ 0 ].AIRawValue;
            }
            //
            return ret;
        }

        public bool GetInstructions( uint aAddress, TArmInstructionSet aInstructionSet, int aCount, out IArmInstruction[] aInstructions )
        {
            bool valid = false;
            aInstructions = new IArmInstruction[ 0 ];
      
            // We need the code and the instruction converter
            if ( IsCodeAvailable && IfaceInstructionConverter != null )
            {
                // Check range is valid
                AddressRange range = new AddressRange( iBaseAddress, 0 );
                range.UpdateMax( range.Min + iCode.Length );
                uint extent = aAddress + ( (uint) aCount * (uint) aInstructionSet );
                //
                valid = range.Contains( aAddress ) && range.Contains( extent );
                if ( valid )
                {
                    List<uint> rawInstructions = new List<uint>();
                    //
                    using ( SymbianStreamReaderLE reader = SymbianStreamReaderLE.New( new MemoryStream( iCode ) ) )
                    {
                        uint address = aAddress - iBaseAddress;
                        reader.Seek( address );
                        //
                        for ( int i = 0; i < aCount; i++ )
                        {
                            uint value = 0;
                            //
                            switch ( aInstructionSet )
                            {
                            case TArmInstructionSet.ETHUMB:
                                value = reader.ReadUInt16();
                                break;
                            case TArmInstructionSet.EARM:
                                value = reader.ReadUInt32();
                                break;
                            default:
                            case TArmInstructionSet.EJAZELLE:
                                throw new NotSupportedException( "Jazelle is not supported" );
                            }
                            //
                            rawInstructions.Add( value );
                            address += (uint) aInstructionSet;
                        }
                    }
                    //
                    aInstructions = iInstructionConverter.ConvertRawValuesToInstructions( aInstructionSet, rawInstructions.ToArray(), aAddress );
                }
            }

            // Return empty array if not valid
            return valid;
        }
        #endregion

        #region From IComparable<CodeCollection>
        public int CompareTo( CodeCollection aCollection )
		{
            int ret = ( aCollection.FileName == this.FileName ) ? 0 : -1;
			//
            if ( ret == 0 )
            {
                if ( BaseAddress == aCollection.BaseAddress )
                {
                    ret = 0;
                }
                else if ( BaseAddress > aCollection.BaseAddress )
                {
                    ret = 1;
                }
                else
                {
                    ret = -1;
                }
            }
			//
			return ret;
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
                iTag = null;
                iParentList = null;
                iInstructionConverter = null;
                iCodeSegmentResolver = null;
                iRelocationHandler = null;
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return iFileName.ToString();
        }

        public override bool Equals( object aObject )
        {
            if ( aObject != null && aObject is CodeCollection )
            {
                CodeCollection col = (CodeCollection) aObject;
                bool ret = ( col.FileName == this.FileName );
                return ret;
            }
            //
            return base.Equals( aObject );
        }

        public override int GetHashCode()
        {
            return iFileName.GetHashCode();
        }
        #endregion

        #region Data members
        private readonly PlatformId iId;
        private readonly PlatformFileName iFileName;
        private object iTag = null;
        private object iFlagLock = new object();
        private byte[] iCode = null;
        private uint iBaseAddress = 0;
        private bool iTagged = false;
        private TFlags iFlags = TFlags.EFlagsNone;
        private CodeCollectionList iParentList = null;
        private ICodeCollectionCodeSegmentResolver iCodeSegmentResolver = null;
        private ICodeCollectionRelocationHandler iRelocationHandler = null;
        private ICodeCollectionInstructionConverter iInstructionConverter = null;
		#endregion
    }
}
