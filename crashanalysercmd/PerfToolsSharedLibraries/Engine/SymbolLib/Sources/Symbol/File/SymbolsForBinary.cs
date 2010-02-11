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
using System.Collections;
using System.Collections.Generic;
using SymbianUtils;
using SymbianUtils.Range;
using SymbolLib.Generics;
using SymbolLib.Sources.Symbol.Symbol;

namespace SymbolLib.Sources.Symbol.File
{
    public class SymbolsForBinary : GenericSymbolCollection
	{
		#region Construct & destruct
        public SymbolsForBinary( string aHostFileName )
            : base( aHostFileName )
		{
            // Add default unknown entry
            SymbolSymbol nullEntry = SymbolSymbol.NewUnknown( this );
            iEntries.Add( nullEntry );
		}
		#endregion

        #region Properties
        internal GenericSymbol InternalLastSymbol
        {
            get
            {
                // We don't want to return the last symbol
                GenericSymbol ret = base.LastSymbol;
                //
                if ( ret != null && ret.IsUnknownSymbol )
                {
                    ret = null;
                }
                //
                return ret;
            }
        }
        #endregion

        #region API
        internal void Add( GenericSymbolEngine aEngine, GenericSymbol aSymbol, bool aAllowNonRomSymbols )
        {
            System.Diagnostics.Debug.Assert( aSymbol is SymbolSymbol );
 
            // Check for Image$$ER_RO$$Base or Image$$ER_RO$$Limit. 
            // This symbol is emitted for user-side code and can be used to work around some maksym problems.
            string symbolName = aSymbol.Symbol;
            if ( symbolName.StartsWith( KSymbolNameImageBaseOrLimitPrefix ) )
            {
                bool isBase = symbolName.Contains( "Base" );

                // If we've just seen the base entry, but we already have some stored symbols, then
                // probably this is a maksym problem that we must try to work around.
                if ( isBase )
                {
                    int count = iEntries.Count;
                    if ( count > 0 && !iEntries[ 0 ].IsUnknownSymbol )
                    {
                        // Discard all the entries we've seen so far because most likely
                        // they are invalid.
                        System.Diagnostics.Debug.WriteLine( string.Format( "Discarding {0} invalid symbols for library: {1}", count, base.HostBinaryFileName ) );
                        iEntries.Clear();

                        // At this point, we need to reset the base address because any symbols that have gone 
                        // before are invalid.
                        iFlags &= ~TFlags.EFlagsHaveSeenFirstSymbol;
                    }
                }
                else
                {
                    // Reached the limit - stop storing symbols at this point as everything else is likely data.
                    iFlags |= TFlags.EFlagsDisallowFurtherSymbolsForCollection;
                }
            }

            // Don't save the entry if we're in disabled state.
            bool newAdditionsDisabled = ( iFlags & TFlags.EFlagsDisallowFurtherSymbolsForCollection ) == TFlags.EFlagsDisallowFurtherSymbolsForCollection;
            if ( !newAdditionsDisabled )
            {
                // Whether or not we keep the entry
                bool addEntry = false;

                // Set base address
                UpdateCollectionBaseAddressBasedUponFirstSymbol( aSymbol );

                GenericSymbol lastSymbol = InternalLastSymbol;
                if ( lastSymbol != null )
                {
                    if ( lastSymbol.Address > aSymbol.Address )
                    {
                        // Work-around for maksym problem where it fails to parse some MAP files correctly.
                    }
                    else
                    {
                        // If we have a last symbol, and it's address is prior to that of the new symbol, we can
                        // try to update the last symbol's size (if it needs it updating - the method will check that).
                        UpdateLengthOfPreviousSymbol( aSymbol );
  
                        // Check to see if we already have a symbol within this address range
                        bool overlappingSymbol = LastSymbolSharesSameAddressRange( aSymbol );
                        if ( overlappingSymbol )
                        {
                            // They overlap - which one do we keep? 
                            addEntry = FilterOutCommonAddressEntry( lastSymbol, aSymbol );
                        }
                        else
                        {
                            addEntry = TakeEntry( aSymbol, aAllowNonRomSymbols );
                        }
                    }
                }
                else
                {
                    addEntry = TakeEntry( aSymbol, aAllowNonRomSymbols );
                }

                // If we need to keep the symbol, then save it now...
                if ( addEntry )
                {
                    DoAddEntry( aSymbol );
                }
            }
        }

        internal void Fixup( long aNewBaseAddress )
        {
            BaseAddress = aNewBaseAddress;
            if ( aNewBaseAddress != 0 )
            {
                iFlags |= TFlags.EFlagsHaveDoneFixup;
            }

            base.RebuildAddressRange();
        }
        #endregion

        #region From GenericSymbolCollection
        public override void WriteToStream( StreamWriter aWriter )
        {
            if ( ( iFlags & TFlags.EFlagsHaveDoneFixup ) == TFlags.EFlagsHaveDoneFixup )
            {
                long originalBaseAddress = BaseAddress;
                try
                {
                    // For fixed up symbol collections, i.e. those with a base address of zero
                    // that have subsequently been fixed up at runtime (rofs) then we
                    // must ensure we write base addresses of zero again when externalising
                    // the symbol data.
                    BaseAddress = 0;
                    base.WriteToStream( aWriter );
                }
                finally
                {
                    BaseAddress = originalBaseAddress;
                }
            }
            else
            {
                base.WriteToStream( aWriter );
            }
        }

        public override void Add( GenericSymbolEngine aEngine, GenericSymbol aSymbol )
        {
            Add( aEngine, aSymbol, true );
        }

		public override void Remove( GenericSymbol aSymbol )
		{
			iEntries.Remove( aSymbol );
		}

		public override void RemoveAt( int aIndex )
		{
			iEntries.RemoveAt( aIndex );
		}

        public override int Count
        {
            get
            {
                return iEntries.Count;
            }
        }

        public override void Sort()
        {
            iEntries.Sort( new GenericSymbolComparer() );
#if PROFILING
            System.DateTime startTime = DateTime.Now;
            System.DateTime endTime = DateTime.Now;
            long tickDuration = ( ( endTime.Ticks - startTime.Ticks ) / 100 );
            System.Diagnostics.Debug.WriteLine( "SORT TIME " + tickDuration.ToString( "d6" ) );
#endif
        }

        public override GenericSymbol SymbolForAddress( long aAddress )
        {
#if DEBUG
            int x = 0;
            if ( x > 0 )
            {
                base.Dump( aAddress );
            }
#endif
            GenericSymbol ret = null;
            //
            AddressFindingComparer comparer = new AddressFindingComparer();
            SymbolSymbol temp = SymbolSymbol.NewUnknown( (uint) aAddress, 0, string.Empty );
            int pos = iEntries.BinarySearch( temp, comparer );
            if ( pos >= 0 && pos < iEntries.Count )
            {
                ret = iEntries[ pos ];
                System.Diagnostics.Debug.Assert( ret.AddressRange.Contains( aAddress ) );
            }
            //
            return ret;
        }

        public override IEnumerator CreateEnumerator()
        {
            IEnumerator<GenericSymbol> self = (IEnumerator<GenericSymbol>) this;
            return self;
        }

        public override IEnumerator<GenericSymbol> CreateGenericEnumerator()
        {
            foreach ( GenericSymbol sym in iEntries )
            {
                yield return sym;
            }
        }

        public override GenericSymbol this[ int aIndex ]
        {
            get
            {
                return iEntries[ aIndex ];
            }
        }
        #endregion

        #region Internal enumerations
        [Flags]
		private enum TFlags
		{
			EFlagsNone = 0,
			EFlagsCalculateLengthOfPreviousSymbol = 1,
			EFlagsDisallowFurtherSymbolsForCollection = 2,
			EFlagsHaveSeenFirstSymbol = 4,
            EFlagsHaveDoneFixup = 8
		}
		#endregion

        #region Internal constants
        private const string KSymbolNameImageBaseOrLimitPrefix = "Image$$ER_RO$$";
        private const long KMaxDifferenceBetweenConsecutiveSymbols = 1024 * 64;
        #endregion

        #region Internal methods
        private void UpdateCollectionBaseAddressBasedUponFirstSymbol( GenericSymbol aSymbol )
        {
            // If we are not processing the first Symbol in the collection, then we
            // can rely on the base address being set. Otherwise, we are
            // defining the base address itself.
            if ( !( ( iFlags & TFlags.EFlagsHaveSeenFirstSymbol ) == TFlags.EFlagsHaveSeenFirstSymbol ) )
            {
                // Set collection base address to symbol starting address
                BaseAddress = aSymbol.Address;

                // Since this is the first symbol in the collection, and we're going to use
                // its address as the offset (base) address for the entire collection, 
                // if we just set the collection base address to the symbol's address and
                // the continue, this entry will be double offset (the new offset for the collection
                // + the offset of the symbol itself). We must therefore set the symbol's offset
                // address to zero.
                SymbolSymbol realSymbol = (SymbolSymbol) aSymbol;
                realSymbol.ResetOffsetAddress( 0 );

                // Make sure we set a flag so that we don't attempt to do this again.
                iFlags |= TFlags.EFlagsHaveSeenFirstSymbol;
            }
        }

        private void UpdateLengthOfPreviousSymbol( GenericSymbol aNewSymbol )
        {
            bool clearFlag = false;
            //
            if ( ( iFlags & TFlags.EFlagsCalculateLengthOfPreviousSymbol ) == TFlags.EFlagsCalculateLengthOfPreviousSymbol )
            {
                // Must have some existing symbol.
                System.Diagnostics.Debug.Assert( Count > 0 );

                // Last symbol must have bad size?
                GenericSymbol previousSymbol = InternalLastSymbol;
                System.Diagnostics.Debug.Assert( previousSymbol.Size == 0 );

                // The new symbol must be exactly the same address as the last symbol
                // (in which case, the new symbol must have a valid size or else we're
                // unable to do anything sensible with it) 
                //
                // OR
                //
                // The new symbol must be after the last symbol. It cannot be before.
                System.Diagnostics.Debug.Assert( aNewSymbol.Address >= previousSymbol.Address );
                if ( aNewSymbol.Address == previousSymbol.Address )
                {
                    if ( aNewSymbol.Size > 0 )
                    {
                        // Okay, the new symbol has a valid size, the old one didn't.
                        clearFlag = true;
                        previousSymbol.Size = aNewSymbol.Size;
                    }
                    else
                    {
                        // Hmm, neither the last or new symbol have a valid size.
                        // Nothing we can do in this case...
                    }
                }
                else
                {
                    // Need to work out the length of the previous symbol by comparing the
                    // address of this symbol against it.
                    //
                    // Only do this if the region type hasn't changed.
                    MemoryModel.TMemoryModelRegion previousType = MemoryModel.RegionByAddress( previousSymbol.Address, aNewSymbol.MemoryModelType );
                    MemoryModel.TMemoryModelRegion regionType = MemoryModel.RegionByAddress( aNewSymbol.Address, aNewSymbol.MemoryModelType );
                    if ( regionType == previousType )
                    {
                        // If this new symbol and the old symbol have the same address, then
                        // also check the size of the previous symbol. If it was zero, then discard it
                        // and keep this new entry. Otherwise, discard this new one instead.
                        long delta = aNewSymbol.Address - previousSymbol.Address;
                        if ( delta > 1 )
                        {
                            // It's okay, this symbol had a later address than the last one
                            // This is normal.
                            previousSymbol.Size = delta;
                        }
                        else
                        {
                            // This is not good. Two symbols both have the same address.
                            iEntries.Remove( previousSymbol );
                        }
                    }

                    clearFlag = true;
                }
            }

            if ( clearFlag )
            {
                iFlags &= ~TFlags.EFlagsCalculateLengthOfPreviousSymbol;
            }
        }

        private void DoAddEntry( GenericSymbol aSymbol )
        {
            // Make sure we remove the null symbol if this is the first 'valid' symbol for
            // the collection.
            if ( OnlyContainsDefaultSymbol )
            {
                RemoveAt( 0 );
            }

            // If the symbol has no size, then try to work it out next time around
            if ( aSymbol.Size == 0 )
            {
                iFlags |= TFlags.EFlagsCalculateLengthOfPreviousSymbol;
            }

            // Save it
            iEntries.Add( aSymbol );
        }

        private bool LastSymbolSharesSameAddressRange( GenericSymbol aSymbol )
        {
            bool ret = false;
            //
            GenericSymbol last = InternalLastSymbol;
            if ( last != null && last.FallsWithinDomain( aSymbol.Address ) )
            {
                ret = true;
            }
            //
            return ret;
        }

        private bool FilterOutCommonAddressEntry( GenericSymbol aLast, GenericSymbol aNew )
        {
            bool acceptNew = false;
            //
            if ( aLast.IsUnknownSymbol )
            {
                // Always discard the unknown symbol in preference of anything better
                iEntries.Remove( aLast );
                acceptNew = true;
            }
            else if ( aNew.Size > 0 )
            {
                if ( aLast.Size == 0 )
                {
                    // New is 'better' because it contains proper sizing information
                    iEntries.Remove( aLast );
                    acceptNew = true;
                }
                else if ( aLast.Size < aNew.Size )
                {
                    // New is 'better' because it is bigger
                    iEntries.Remove( aLast );
                    acceptNew = true;
                }
                else if ( aLast.Size == aNew.Size )
                {
                    // Both the same size. Take symbols over everything else.
                    if ( aNew.IsSymbol && !aLast.IsSymbol )
                    {
                        // Keep the symbol (new)
                        iEntries.Remove( aLast );
                        acceptNew = true;
                    }
                    else if ( !aNew.IsSymbol && aLast.IsSymbol )
                    {
                        // Keep the symbol (last)
                        acceptNew = false;
                    }
                    else
                    {
                        // Take higher priority...
                        if ( aNew.AddressType > aLast.AddressType )
                        {
                            iEntries.Remove( aLast );
                            acceptNew = true;
                        }
                        else
                        {
                            acceptNew = false;
                        }
                    }
                }
            }
            else if ( aLast.Size > 0 )
            {
                // Last is 'better' because the new entry doesn't have any size
                acceptNew = false;
            }
            else if ( aLast.Size == 0 && aNew.Size == 0 )
            {
                // Both entries have no size and the same address. We cannot
                // accept both, therefore we make an arbitrary decision about
                // which to keep.
                if ( aLast.IsSubObject && !aNew.IsSubObject )
                {
                    // Discard the sub object (last)
                    iEntries.Remove( aLast );
                    acceptNew = true;
                }
                else if ( aNew.IsSubObject && !aLast.IsSubObject )
                {
                    // Discard the sub object (new)
                    acceptNew = false;
                }
                else if ( aNew.IsSymbol && !aLast.IsSymbol )
                {
                    // Keep the symbol (new)
                    iEntries.Remove( aLast );
                    acceptNew = true;
                }
                else if ( !aNew.IsSymbol && aLast.IsSymbol )
                {
                    // Keep the symbol (last)
                    acceptNew = false;
                }
                else
                {
                    // Couldn't make a good decision. Junk the new entry
                    acceptNew = false;
                }
            }
            //
            return acceptNew;
        }

        private bool IsEntryReallyData( GenericSymbol aSymbol )
        {
            bool isData = false;

            // Check to see if its from a data=<something> IBY entry...
            string baseName = Path.GetFileName( HostBinaryFileName ).ToLower();
            string symbolName = aSymbol.Symbol.ToLower();

            if ( symbolName == baseName && aSymbol.Size == 0 )
            {
                // Its from a data entry
                isData = true;

                // Data entries only consist of a single symbol (the data itself)
                iFlags |= TFlags.EFlagsDisallowFurtherSymbolsForCollection;
            }

            return isData;
        }

        private bool TakeEntry( GenericSymbol aSymbol, bool aAllowNonRomSymbols )
        {
            bool take = IsEntryReallyData( aSymbol );

            if ( !take )
            {
                // We'll take the entry if all we have at the moment is
                // the default entry or then no entries at all.
                if ( OnlyContainsDefaultSymbol || iEntries.Count == 0 )
                {
                    take = true;
                }
                else
                {
                    GenericSymbol last = LastSymbol;
                    System.Diagnostics.Debug.Assert( last != null );
                    GenericSymbol.TAddressType addressType = aSymbol.AddressType;
                    //
                    switch ( addressType )
                    {
                    // We always take these...
                    case GenericSymbol.TAddressType.EAddressTypeROMSymbol:
                        take = true;
                        break;

                    // We sometimes take these...
                    case GenericSymbol.TAddressType.EAddressTypeRAMSymbol:
                        take = aAllowNonRomSymbols;
                        break;

                    case GenericSymbol.TAddressType.EAddressTypeLabel:
                        take = true;
                        break;
                    case GenericSymbol.TAddressType.EAddressTypeSubObject:
                        take = ( aSymbol.Size > 0 ) || ( last.EndAddress < aSymbol.Address );
                        break;

                    // We never take these
                    default:
                    case GenericSymbol.TAddressType.EAddressTypeReadOnlySymbol:
                    case GenericSymbol.TAddressType.EAddressTypeUnknown:
                    case GenericSymbol.TAddressType.EAddressTypeKernelGlobalVariable:
                        break;
                    }
                }
            }
            //
            return take;
        }

        private bool OnlyContainsDefaultSymbol
        {
            get
            {
                bool ret = ( Count == 1 && this[ 0 ].IsUnknownSymbol );
                return ret;
            }
        }
        #endregion

        #region Data members
        private TFlags iFlags = TFlags.EFlagsNone;
        private List<GenericSymbol> iEntries = new List<GenericSymbol>( 200 );
        #endregion
    }

    #region Internal classes
    internal class AddressFindingComparer : IComparer<GenericSymbol>
    {
        public int Compare( GenericSymbol aLeft, GenericSymbol aRight )
        {
            int ret = -1;
            //
            AddressRange lr = aLeft.AddressRange;
            AddressRange rr = aRight.AddressRange;
            //
            if ( lr.Contains( rr ) || rr.Contains( lr ) )
            {
                ret = 0;
            }
            else
            {
                ret = lr.CompareTo( rr );
            }
            //
            return ret;
        }
    }
    #endregion
}
