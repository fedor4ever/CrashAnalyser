#define TRACE_INTERNING
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
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using SymbianUtils.Range;
using SymbianUtils.Strings;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.MemoryModel;
using SymbianStructuresLib.Debug.Symbols.Constants;
using SymbianStructuresLib.Debug.Common.Id;

namespace SymbianStructuresLib.Debug.Symbols
{
	public class Symbol : IFormattable
    {
        #region Static constructors
        public static Symbol New( SymbolCollection aCollection )
        {
            return new Symbol( aCollection );
        }

        public static Symbol NewDefault()
        {
            Symbol ret = new Symbol( null );
            //
            ret.IsDefault = true;
            ret.Size = 0;
            ret.OffsetAddress = SymbolConstants.KNullEntryAddress;
            ret.Object = SymbolConstants.KNonMatchingObjectName;
            ret.iName = InternedName.NewExplicit( SymbolConstants.KNonMatchingInternedName );
            //
            return ret;
        }

        internal static Symbol NewDefault( SymbolCollection aCollection )
        {
            Symbol ret = new Symbol( aCollection );
            //
            ret.IsDefault = true;
            ret.Size = 0;
            ret.OffsetAddress = SymbolConstants.KNullEntryAddress;
            ret.Object = SymbolConstants.KNonMatchingObjectName;
            ret.iName = InternedName.NewExplicit( SymbolConstants.KNonMatchingInternedName );
            //
            return ret;
        }

        internal static Symbol NewTemp( SymbolCollection aCollection, uint aAddress )
        {
            Symbol ret = new Symbol( aCollection );
            //
            ret.Size = 0;
            ret.OffsetAddress = aAddress - aCollection.BaseAddress;
            ret.iName = InternedName.NewExplicit( "TempInternal" );
            ret.Object = "TempInternal";
            //
            return ret;
        }

        internal static Symbol NewClone( SymbolCollection aCollection, Symbol aSymbol )
        {
            return new Symbol( aCollection, aSymbol );
        }
        #endregion

        #region Constructors
        private Symbol( SymbolCollection aCollection )
		{
            iCollection = aCollection;
            //
            if ( aCollection != null )
            {
                iId = aCollection.IdAllocator.AllocateId().Value;
            }
		}

        private Symbol( SymbolCollection aCollection, Symbol aSymbol )
            : this( aCollection )
        {
            iSize = aSymbol.iSize;
            iOffsetAddress = aSymbol.iOffsetAddress;
            iName = aSymbol.iName;
            iObject = aSymbol.iObject;
            iFlags = aSymbol.iFlags;
            iId = aCollection.IdAllocator.AllocateId().Value;
        }
		#endregion

		#region API
		public bool Contains( uint aAddress )
		{
            bool ret = ( aAddress >= Address && aAddress <= EndAddress );
            return ret;
		}

        public uint Offset( uint aInstructionAddress )
        {
            uint offset = aInstructionAddress - Address;
            return offset;
        }
        #endregion

        #region Properties
		public uint Address
		{
			get
            {
                uint address = BaseAddress + OffsetAddress;
                return address;
            }
		}

		public uint EndAddress
		{
			get
            {
                uint address = Address;
                uint size = Size;
                //
                if ( size > 0 )
                {
                    address += ( size - 1 );
                }
                //
                return address;
            }
		}

		public uint Size
		{
			get { return iSize; }
			set
			{
				iSize = value;
				System.Diagnostics.Debug.Assert( OffsetEndAddress >= OffsetAddress );
			}
		}

        public AddressRange AddressRange
        {
            get { return new AddressRange( Address, EndAddress ); }
        }

		public string Name
		{
			get
            {
                string ret = string.Empty;
                //
                if ( iName != null )
                {
                    ret = iName.ToString();
                }
                //
                return ret;
            }
			set
			{
                iName = InternedName.New( value );
               
                // Remove all property flags
                iFlags &= ~TFlags.EFlagsIsFunction;
                iFlags &= ~TFlags.EFlagsIsVTable;
                iFlags &= ~TFlags.EFlagsIsReadonly;
                iFlags &= ~TFlags.EFlagsIsSubObject;

                if ( InternedName.IsVTable( value ) )
                {
                    iFlags |= TFlags.EFlagsIsVTable;
                }
                else if ( InternedName.IsReadOnly( value ) )
                {
                    iFlags |= TFlags.EFlagsIsReadonly;
                }
                else if ( InternedName.IsSubObject( value ) )
                {
                    iFlags |= TFlags.EFlagsIsSubObject;
                }
                else if ( InternedName.IsFunction( value ) )
                {
                    iFlags |= TFlags.EFlagsIsFunction;
                }
            }
		}

        public string NameWithoutVTablePrefix
		{
			get
            {
                StringBuilder ret = new StringBuilder( Name );
                //
                ret = ret.Replace( "vtable for ", string.Empty );
                ret = ret.Replace( "typeinfo for ", string.Empty );
                ret = ret.Replace( "typeinfo name for ", string.Empty );
                //
                return ret.ToString();
            }
		}

		public string Object
		{
			get
            {
                string ret = string.Empty;
                //
                if ( iObject != null )
                {
                    ret = iObject;
                }
                //
                return ret;
            }
			set
            {
                iObject = string.Intern( value );
            }
		}

		public string ObjectWithoutSection
		{
			get
			{
				string ret = string.Empty;
                //
                if  ( Object != null )
                {
                    ret = Object;
                    //
                    int bracketPos = ret.IndexOf( "(" );
                    if	( bracketPos > 0 )
                    {
                        ret = ret.Substring( 0, bracketPos ).Trim();
                    }
                }
                //
				return ret;
			}
		}

        public SymbolCollection Collection
        {
            // NB: can be null
            get { return iCollection; }
        }

        public TArmInstructionSet InstructionSet
        {
            get
            {
                TArmInstructionSet ret = TArmInstructionSet.EARM;
                uint remainder = iOffsetAddress & 0x1;
                if ( remainder != 0 )
                {
                    ret = TArmInstructionSet.ETHUMB;
                }
                return ret;
            }
        }

        public PlatformId Id
        {
            get { return new PlatformId( iId ); }
        }

        [Browsable( false )]
        [EditorBrowsable( EditorBrowsableState.Never )]
        public uint OffsetAddress
        {
            get
            {
                // Don't include the top bit in any returned address
                uint ret = iOffsetAddress & 0xFFFFFFFE;
                return ret;
            }
            set
            {
                iOffsetAddress = value;
            }
        }

        [Browsable( false )]
        [EditorBrowsable( EditorBrowsableState.Never )]
        private uint OffsetEndAddress
        {
            get
            {
                uint ret = iOffsetAddress + iSize;
                if ( InstructionSet == TArmInstructionSet.ETHUMB && iSize > 0 )
                {
                    // For thumb, the end address is one too big, due to the MSB.
                    --ret;
                }
                return ret;
            }
        }
        #endregion

        #region Type query
        public bool IsFunction
        {
            get
            {
                bool ret = ( iFlags & TFlags.EFlagsIsFunction ) != 0;
                return ret;
            }
        }

        public bool IsVTable
        {
            get
            {
                bool ret = ( iFlags & TFlags.EFlagsIsVTable ) == TFlags.EFlagsIsVTable;
                return ret;
            }
        }

        public bool IsDefault
        {
            get
            {
                bool ret = ( iFlags & TFlags.EFlagsIsDefault ) == TFlags.EFlagsIsDefault;
                return ret;
            }
            set
            {
                if ( value )
                {
                    iFlags |= TFlags.EFlagsIsDefault;
                }
                else
                {
                    iFlags &= ~TFlags.EFlagsIsDefault;
                }
            }
        }

        public bool IsFromRAMLoadedCode
        {
            get
            {
                uint address = this.Address;
                TMemoryModelRegion region = MMUtilities.RegionByAddress( address );
                bool ret = ( region == TMemoryModelRegion.EMemoryModelRegionRAMLoadedCode );
                return ret;
            }
        }

		public TSymbolType Type
		{
			get
			{
                TSymbolType ret = TSymbolType.EUnknown;

                // First check against forced flags
                if ( ( iFlags & TFlags.EFlagsIsForcedSection ) == TFlags.EFlagsIsForcedSection )
                {
                    ret = TSymbolType.ESection;
                }
                else if ( ( iFlags & TFlags.EFlagsIsForcedCode ) == TFlags.EFlagsIsForcedCode )
                {
                    ret = TSymbolType.ECode;
                }
                else if ( ( iFlags & TFlags.EFlagsIsForcedData ) == TFlags.EFlagsIsForcedData )
                {
                    ret = TSymbolType.EData;
                }
                else if ( ( iFlags & TFlags.EFlagsIsForcedNumber ) == TFlags.EFlagsIsForcedNumber )
                {
                    ret = TSymbolType.ENumber;
                }
                 
                // If still unknown, work it out...
                if ( ret == TSymbolType.EUnknown )
                {
                    // First entries override type
                    if ( ( iFlags & TFlags.EFlagsIsReadonly ) != 0 )
                    {
                        ret = TSymbolType.EReadOnlySymbol;
                    }
                    else if ( ( iFlags & TFlags.EFlagsIsSubObject ) != 0 )
                    {
                        ret = TSymbolType.ESubObject;
                    }
                    else if ( Address >= 0xF8000000 && Address < 0xFFEFFFFF )
                    {
                        // ROM Symbol, Moving Memory Model
                        ret = TSymbolType.EROMSymbol;
                    }
                    else if ( Address >= 0xF4000000 && Address < 0xF7FFFFFF )
                    {
                        // RAM Symbol, Moving Memory Model
                        ret = TSymbolType.ERAMSymbol;
                    }
                    else if ( Address >= 0x64000000 && Address < 0x64FFFFFF )
                    {
                        // Kernel global, Moving Memory Model
                        ret = TSymbolType.EKernelGlobalVariable;
                    }
                    else if ( Address >= 0xc8000000 && Address < 0xC8FFFFFF )
                    {
                        // Kernel global, Multiple Memory Model
                        ret = TSymbolType.EKernelGlobalVariable;
                    }
                    else if ( Address >= 0x80000000 && Address < 0x8FFFFFFF )
                    {
                        // ROM Symbol, Multiple Memory Model
                        ret = TSymbolType.EROMSymbol;
                    }
                    else if ( Address >= 0x3C000000 && Address < 0x3DFFFFFF )
                    {
                        // RAM Symbol, Moving Memory Model [1gb]
                        ret = TSymbolType.ERAMSymbol;
                    }
                    else if ( Address >= 0x70000000 && Address < 0x7FFFFFFF )
                    {
                        // RAM Symbol, Moving Memory Model [2gb]
                        ret = TSymbolType.ERAMSymbol;
                    }
                    else if ( Address < 0x10000000 )
                    {
                        // A non-fixed up ROFS symbol entry
                        ret = TSymbolType.ERAMSymbol;
                    }
                    else if ( ret != TSymbolType.EKernelGlobalVariable )
                    {
                        bool isFunction = IsFunction;
                        if ( isFunction == false )
                        {
                            ret = TSymbolType.ELabel;
                        }
                    }
                }
                //
				return ret;
			}
            set
            {
                iFlags &= ~TFlags.EFlagsIsForcedCode;
                iFlags &= ~TFlags.EFlagsIsForcedData;
                iFlags &= ~TFlags.EFlagsIsForcedNumber;
                iFlags &= ~TFlags.EFlagsIsForcedSection;
                //
                switch ( value )
                {
                case TSymbolType.ESection:
                    iFlags |= TFlags.EFlagsIsForcedSection;
                    break;
                case TSymbolType.ECode:
                    iFlags |= TFlags.EFlagsIsForcedCode;
                    break;
                case TSymbolType.EData:
                    iFlags |= TFlags.EFlagsIsForcedData;
                    break;
                case TSymbolType.ENumber:
                    iFlags |= TFlags.EFlagsIsForcedNumber;
                    break;
                default:
                    throw new ArgumentException( "Specified type cannot be programatically set" );
                }
            }
        }

        public TSymbolSource Source
        {
            get
            {
                TSymbolSource ret = TSymbolSource.ESourceWasUnknown;
                //
                if ( ( iFlags & TFlags.EFlagsIsFromMapFile ) == TFlags.EFlagsIsFromMapFile )
                {
                    ret = TSymbolSource.ESourceWasMapFile;
                }
                else if ( ( iFlags & TFlags.EFlagsIsFromSymbolFile ) == TFlags.EFlagsIsFromSymbolFile )
                {
                    ret = TSymbolSource.ESourceWasSymbolFile;
                }
                //
                return ret;
            }
            set
            {
                iFlags &= ~TFlags.EFlagsIsFromMapFile;
                iFlags &= ~TFlags.EFlagsIsFromSymbolFile;
                //
                switch ( value )
                {
                case TSymbolSource.ESourceWasMapFile:
                    iFlags |= TFlags.EFlagsIsFromMapFile;
                    break;
                case TSymbolSource.ESourceWasSymbolFile:
                    iFlags |= TFlags.EFlagsIsFromSymbolFile;
                    break;
                default:
                case TSymbolSource.ESourceWasUnknown:
                    break;
                }
            }
        }
		#endregion
        
        #region Internal enumerations
        [Flags]
        private enum TFlags : ushort
        {
            EFlagsNone = 0,
            EFlagsIsDefault = 1,
            EFlagsIsVTable = 2,
            EFlagsIsFunction = 4,
            EFlagsIsReadonly = 8,
            EFlagsIsSubObject = 16,
            EFlagsIsForcedCode = 32,
            EFlagsIsForcedData = 64,
            EFlagsIsForcedNumber = 128,
            EFlagsIsForcedSection = 256,
            EFlagsIsFromMapFile = 512,
            EFlagsIsFromSymbolFile = 1024
        };
        #endregion

        #region Internal methods
        private uint BaseAddress
        {
            get
            {
                if ( iCollection != null )
                {
                    return iCollection.BaseAddress;
                }
                //
                return 0;
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
		{
            string ret = ToString( null, null );
			return ret;
		}

        public string ToStringOffset( uint aFrom )
        {
            uint baseAddressOffset = Offset( aFrom );
            string text = "[+ 0x" + baseAddressOffset.ToString( "x4" ) + "]";
            return text;
        }
	    #endregion

        #region IFormattable Members
        public string ToString( string aFormat, IFormatProvider aFormatProvider )
        {
            string ret = string.Empty;
            //
            if ( string.IsNullOrEmpty( aFormat ) )
            {
                ret = string.Format( "{0:x8}    {1:x4}    {2} [{3}]", Address, Size, Name, Object );
            }
            else
            {
                string format = aFormat.Trim().ToUpper();
                //
                if ( format == "STREAM" )
                {
                    ret = string.Format( "{0:x8}    {1:x4}    {2} [{3}]", Address, Size, Name.PadRight( 40, ' ' ), Object );
                }
                else
                {
                    throw new FormatException( String.Format( "Invalid format string: '{0}'.", aFormat ) );
                }
            }
            //
            return ret;
        }
        #endregion

		#region Data members
        private readonly SymbolCollection iCollection;
        private readonly uint iId; // This saves 8 bytes per instance over using PlatformId directly
        private uint iSize;
		private uint iOffsetAddress;
        private string iObject = null;
        private InternedName iName = null;
        private TFlags iFlags = TFlags.EFlagsNone;
		#endregion
    }
}