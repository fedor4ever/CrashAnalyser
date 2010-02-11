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
using SymbianUtils.Range;
using SymbianUtils;

namespace SymbolLib.Generics
{
    #region Exception
	public class GenericSymbolicCreationException : Exception
	{
	}
    #endregion

	public abstract class GenericSymbol
	{
		#region Enumerations
		public enum TAddressType
		{
            // Do not change the order - these are priority based with
            // the most important symbol type appearing with a larger
            // value
			EAddressTypeUnknown = -1,
            EAddressTypeReadOnlySymbol = 0,
			EAddressTypeKernelGlobalVariable,
            EAddressTypeSubObject,
            EAddressTypeLabel,
			EAddressTypeRAMSymbol,
            EAddressTypeROMSymbol,
		}

        public enum TSourceType
        {
            ESourceTypeFileSymbol = 0,
            ESourceTypeFileMap
        }

        public enum TInstructionSet
        {
            EInstructionSetARM = 0,
            EInstructionSetTHUMB
        }
		#endregion

        #region Public constants
        public const int KNullEntryAddress = 0;
        public const string KNonMatchingObjectName = "Unknown Object";
        public const string KNonMatchingSymbolName = "Unknown Symbol";
        #endregion

        #region Constructors
        protected GenericSymbol( GenericSymbolCollection aCollection )
		{
            iCollection = aCollection;
		}
		#endregion

		#region Abstract API
        public abstract TSourceType SourceType { get; }

		public abstract bool Parse( string aLine );
		#endregion

		#region Framework API
		public virtual bool FallsWithinDomain(long aAddress)
		{
			return (aAddress >= Address && aAddress <= EndAddress);
		}

		public virtual bool IsSubObject
		{
			get
			{
				bool ret = ( AddressType == TAddressType.EAddressTypeSubObject );
				return ret;
			}
		}

		public virtual bool IsLabel
		{
			get
			{
				bool ret = ( AddressType == TAddressType.EAddressTypeLabel );
				return ret;
			}
		}

		public virtual bool IsSymbol
		{
			get
			{
				bool ret = ( Collection.BaseAddress == 0 || AddressType == TAddressType.EAddressTypeRAMSymbol || AddressType == TAddressType.EAddressTypeROMSymbol );
				return ret;
			}
		}

		public virtual MemoryModel.TMemoryModelType MemoryModelType // Guess
		{
			get
			{
				return iStaticMemoryModelType;
			}
		}

		public virtual TAddressType AddressType
		{
			get
			{
				TAddressType ret = TAddressType.EAddressTypeUnknown;

				if	( Address >= 0xF8000000 && Address < 0xFFEFFFFF )
				{
					// ROM Symbol, Moving Memory Model
					ret = TAddressType.EAddressTypeROMSymbol;
				}
				else if ( Address >= 0xF4000000 && Address < 0xF7FFFFFF )
				{
					// RAM Symbol, Moving Memory Model
					ret = TAddressType.EAddressTypeRAMSymbol;
				}
				else if ( Address >= 0x64000000 && Address < 0x64FFFFFF )
				{
					// Kernel global, Moving Memory Model
					ret = TAddressType.EAddressTypeKernelGlobalVariable;
				}
				else if ( Address >= 0xc8000000 && Address < 0xC8FFFFFF)
				{
					// Kernel global, Multiple Memory Model
					ret = TAddressType.EAddressTypeKernelGlobalVariable;
				}
				else if ( Address >= 0x80000000 && Address < 0x8FFFFFFF )
				{
					// ROM Symbol, Multiple Memory Model
					ret = TAddressType.EAddressTypeROMSymbol;
				}
				else if ( Address >= 0x3C000000 && Address < 0x3DFFFFFF )
				{
					// RAM Symbol, Moving Memory Model [1gb]
					ret = TAddressType.EAddressTypeRAMSymbol;
				}
				else if ( Address >= 0x70000000 && Address < 0x7FFFFFFF )
				{
					// RAM Symbol, Moving Memory Model [2gb]
					ret = TAddressType.EAddressTypeRAMSymbol;
				}
                else if ( Address < 0x10000000 )
                {
                    // A non-fixed up ROFS symbol entry
                    ret = TAddressType.EAddressTypeRAMSymbol;
                }
				
				// These can over-ride the previous items...
				if	( iSymbol.IndexOf("Image$$ER_RO$$") >= 0 )
				{
					ret = TAddressType.EAddressTypeReadOnlySymbol;
				}
				else if ( iSymbol.IndexOf("__sub_object(") > 0 )
				{
					ret = TAddressType.EAddressTypeSubObject;
				}
				else if ( ret != TAddressType.EAddressTypeKernelGlobalVariable )
				{
                    bool containsBrackets = iSymbol.Contains( "(" ) && iSymbol.Contains( ")" );
                    if ( !containsBrackets )
                    {
                        ret = TAddressType.EAddressTypeLabel;
                    }
				}

				return ret;
			}
		}
		#endregion

        #region API
        public static string UnknownOffset()
        {
            string text = "[+ ??????]";
            return text;
        }

        public uint Offset( uint aInstructionAddress )
        {
            uint baseAddressOffset = aInstructionAddress - System.Convert.ToUInt32( Address );
            return baseAddressOffset;
        }

        public string OffsetAsString( uint aInstructionAddress )
        {
            uint baseAddressOffset = Offset( aInstructionAddress );
            string text = "[+ 0x" + baseAddressOffset.ToString( "x4" ) + "]";
            return text;
        }
        #endregion

        #region Properties
        public TInstructionSet InstructionSet
        {
            get
            {
                TInstructionSet ret = TInstructionSet.EInstructionSetARM;
                //
                if ( ( iFlags & TFlags.EFlagsIsThumb ) == TFlags.EFlagsIsThumb )
                {
                    ret = TInstructionSet.EInstructionSetTHUMB;
                }
                //
                return ret;
            }
        }

        public long BaseAddress
        {
            get { return iCollection.BaseAddress; }
        }

		public long Address
		{
			get
            {
                long address = BaseAddress + iOffsetAddress;
                return address;
            }
		}

		public long EndAddress
		{
			get
            {
                long ret = BaseAddress + iOffsetEndAddress;
                return ret;
            }
		}

		public long Size
		{
			get { return iSize; }
			set
			{
				System.Diagnostics.Debug.Assert( value >= 0 );
				iSize = value;
                
                // We need to update the end address
                if ( iSize > 0 )
                {
                    long address = OffsetAddress;
                    OffsetEndAddress = address + iSize - 1;
                }
                else
                {
                    OffsetEndAddress = OffsetAddress;
                }

                System.Diagnostics.Debug.Assert( iOffsetEndAddress >= OffsetAddress );
			}
		}

        public AddressRange AddressRange
        {
            get { return new AddressRange( Address, EndAddress ); }
        }

		public string Symbol
		{
			get { return iSymbol; }
			set
			{
				iSymbol = value;
                if ( StartsWithAny( KVTableOrTypeInfoPrefixes, value ) )
                {
                    iFlags |= TFlags.EFlagsVTable;
                }
			}
		}

		public string SymbolNameWithoutVTablePrefix
		{
			get
            {
                StringBuilder ret = new StringBuilder( Symbol );
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
			get { return iObject; }
			set { iObject = value; }
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

                // Casing looks odd and it can also confuse
                // hash table look ups in dependent code.
                //ret = SymbianUtils.Strings.StringCaser.PrettyCase( ret );
				return ret;
			}
		}

        public GenericSymbolCollection Collection
        {
            get { return iCollection; }
        }
  
        public bool IsUnknownSymbol
        {
            get
            {
                bool ret = Symbol.StartsWith( KNonMatchingSymbolName ) &&
                           ( OffsetAddress == GenericSymbol.KNullEntryAddress );
                return ret;
            }
        }

        public bool IsVTable
        {
            get
            {
                bool ret = ( iFlags & TFlags.EFlagsVTable ) == TFlags.EFlagsVTable;
                return ret;
            }
        }
        #endregion
        
        #region Internal enumerations
        [Flags]
        private enum TFlags : byte
        {
            EFlagsNone = 0,
            EFlagsVTable = 1,
            EFlagsIsThumb = 2,
        };
        #endregion

        #region Internal properties
        internal long OffsetAddress
        {
            get { return iOffsetAddress; }
            set
            {
                long remainder = value & 0x1;
                if ( remainder != 0 )
                {
                    iFlags |= TFlags.EFlagsIsThumb;
                }
                else
                {
                    iFlags &= ~TFlags.EFlagsIsThumb;
                }

                iOffsetAddress = value & 0xFFFFFFFE;

                // If we've not yet decided upon the type of memory model,
                // now is the time to work it out.
                if  ( iOffsetAddress > 0 && iStaticMemoryModelType == MemoryModel.TMemoryModelType.EMemoryModelUnknown )
                {
                    long address = Address;
                    iStaticMemoryModelType = MemoryModel.TypeByAddress( address );
                }
            }
        }

        private long OffsetEndAddress
        {
            set { iOffsetEndAddress = value; }
        }
        #endregion

		#region Internal constants
		protected const int KBaseHex = 16;
        private static readonly string[] KVTableOrTypeInfoPrefixes = new string[] { "vtable for ", "typeinfo for ", "typeinfo name for " };
		#endregion

		#region From System.Object
		public override string ToString()
		{
            string ret = string.Format( "{0:x8}    {1:x4}    {2} [{3}]", Address, Size, Symbol, Object );
			return ret;
		}
        
        public string ToStringForStream()
        {
            StringBuilder ret = new StringBuilder();
            //
            ret.Append( Address.ToString( "x8" ) );
            ret.Append( "    " );
            ret.Append( Size.ToString( "x4" ) );
            ret.Append( "    " );
            ret.Append( Symbol.PadRight( 40, ' ' ) );
            ret.Append( " " );
            ret.Append( Object );
            //
            return ret.ToString();
        }

        public static bool StartsWithAny( string[] aPrefixes, string aText )
        {
            bool ret = false;
            //
            foreach ( string p in aPrefixes )
            {
                if ( aText.StartsWith( p ) )
                {
                    ret = true;
                    break;
                }
            }
            //
            return ret;
        }
	    #endregion

		#region Data members
		protected long iSize;
		#endregion

		#region Internal data members
		private long iOffsetAddress;
		private long iOffsetEndAddress;
        private string iSymbol = string.Empty;
		private string iObject = string.Empty;
        private TFlags iFlags = TFlags.EFlagsNone;
        private readonly GenericSymbolCollection iCollection;
        private static MemoryModel.TMemoryModelType iStaticMemoryModelType = MemoryModel.TMemoryModelType.EMemoryModelUnknown;
		#endregion
	}
}
