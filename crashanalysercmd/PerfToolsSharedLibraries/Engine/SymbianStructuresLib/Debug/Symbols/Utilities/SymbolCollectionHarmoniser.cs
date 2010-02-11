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
using SymbianUtils;
using SymbianStructuresLib.MemoryModel;

namespace SymbianStructuresLib.Debug.Symbols.Utilities
{
    public class SymbolCollectionHarmoniser : DisposableObject
    {
        #region Enumerations
        public enum TCollectionType
        {
            EXIP = 0,
            ENotXIP,
            EPossiblyXIP
        }
        #endregion

        #region Constructors
        public SymbolCollectionHarmoniser( SymbolCollection aCollection )
            : this( aCollection, TCollectionType.ENotXIP )
        {
        }
        
        public SymbolCollectionHarmoniser( SymbolCollection aCollection, TCollectionType aType )
        {
            iCollection = aCollection;
            iType = aType;

            // If the collection is not XIP, then we can definitely say it is relocatable
            if ( aType == TCollectionType.ENotXIP )
            {
                iCollection.IsFixed = false;
            }
            else if ( aType == TCollectionType.EXIP )
            {
                iCollection.IsFixed = true;
            }
        }
        #endregion

        #region API
        public bool Add( Symbol aSymbol )
        {
            Debug( aSymbol );

            // Perform pre-filter, which might currently include a blanket ban
            // on new symbols for this collection
            bool save = PreFilterBasedUponFlags( aSymbol );
            if ( save )
            {
                // Flags indicate we can accept the symbol, so perform
                // normal checks based upon symbol meta-data
                Symbol last = this.LastSymbol;
                if ( last != null && !last.IsDefault )
                {
                    if ( aSymbol.Size == 0 )
                    {
                        save = ShouldSaveWhenNewSymbolHasNoSize( last, aSymbol );
                    }
                    else
                    {
                        save = ShouldSaveWhenNewSymbolHasValidSize( last, aSymbol );
                    }
                }
                //
                if ( save )
                {
                    Collection.Add( aSymbol );
                }
            }

            // Perform any final updates
            PostFilterBasedUponFlags( aSymbol );
            //
            return save;
        }
        #endregion

        #region Properties
        public bool DisallowSymbolsOnceReadOnlyLimitReached
        {
            get 
            {
                bool disallow = false;

                // We can only do this once we have one non-default symbol and after
                // we have set the base address.
                if ( ( iFlags & TFlags.EFlagsHaveSetXIPBaseAddress ) == TFlags.EFlagsHaveSetXIPBaseAddress )
                {
                    bool isEmpty = Collection.IsEmptyApartFromDefaultSymbol;
                    if ( !isEmpty )
                    {
                        Symbol first = Collection.FirstSymbol;
                        disallow = ( first.Address == 0 );
                    }
                }
                //
                return disallow; 
            }
        }
        #endregion

        #region Internal enumerations
        [Flags]
        private enum TFlags
        {
            EFlagsNone = 0,
            EFlagsUpdateLengthOfPreviousSymbol = 1,
            EFlagsDisallowFurtherSymbolsForCollection = 2,
            EFlagsHaveSetXIPBaseAddress = 4,
        }
        #endregion

        #region Internal constants
        private const uint KMaxAutomaticLengthUpdateDelta = 4 * 1024 * 1024;
        private const string KSectionNameMarker = "$$";
        private const string KSectionNameUserBase = "Image$$ER_RO$$Base";
        private const string KSectionNameUserLimit = "Image$$ER_RO$$Limit";
        #endregion

        #region Internal properties
        private SymbolCollection Collection
        {
            get { return iCollection; }
        }

        private Symbol LastSymbol
        {
            get
            {
                Symbol ret = null;
                //
                if ( Collection.Count > 0 )
                {
                    ret = Collection.LastSymbol;
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        private void Debug( Symbol aSymbol )
        {
        }

        private bool PreFilterBasedUponFlags( Symbol aSymbol )
        {
            // Do we need to update the length of the previous symbol?
            if ( ( iFlags & TFlags.EFlagsUpdateLengthOfPreviousSymbol ) == TFlags.EFlagsUpdateLengthOfPreviousSymbol )
            {
                FlagBasedUpdateOfLastSymbolLength( aSymbol );
            }

            // Check for Image$$ER_RO$$Base
            // This symbol is emitted for user-side code and can be used to work around some maksym problems.
            string symbolName = aSymbol.Name;
            if ( symbolName == KSectionNameUserBase )
            {
                int count = iCollection.Count;
                if ( !iCollection.IsEmptyApartFromDefaultSymbol )
                {
                    // Discard all the entries we've seen so far because most likely
                    // they are invalid.
                    System.Diagnostics.Debug.WriteLine( string.Format( "Discarding {0} invalid symbols for library: {1}", count, iCollection.FileName ) );
                    iCollection.Clear();

                    // At this point, we need to reset the base address because any symbols that have gone 
                    // before are invalid.
                    iFlags &= ~TFlags.EFlagsHaveSetXIPBaseAddress;
                }
            }

            // Do we need to set the base address for the symbol collection?
            bool haveSetXIPBase = ( ( iFlags & TFlags.EFlagsHaveSetXIPBaseAddress ) == TFlags.EFlagsHaveSetXIPBaseAddress );
            if ( !haveSetXIPBase && iType != TCollectionType.ENotXIP )
            {
                // If we're seeing the first valid symbol, then try to set the base address
                bool needToSet = true;
                if ( iType == TCollectionType.EPossiblyXIP )
                {
                    // Perhaps we're dealing with an XIP collection. In which case, we need to check
                    // with the memory model utility to decide if it really is or not.
                    //
                    // If the symbol address is zero (NULL), then we can't be dealing with an
                    // XIP collection.
                    if ( aSymbol.Address == 0 )
                    {
                        // ROFS
                        needToSet = true;
                    }
                    else
                    {
                        TMemoryModelRegion region = MMUtilities.RegionByAddress( aSymbol.Address );
                        needToSet = ( region == TMemoryModelRegion.EMemoryModelRegionROM );
                    }
                }

                if ( needToSet )
                {
                    SetCollectionBaseAddress( aSymbol );
                }
            }

            bool disallowNewSymbols = ( iFlags & TFlags.EFlagsDisallowFurtherSymbolsForCollection ) == TFlags.EFlagsDisallowFurtherSymbolsForCollection;
            return !disallowNewSymbols;
        }

        private void PostFilterBasedUponFlags( Symbol aSymbol )
        {
            string symbolName = aSymbol.Name;
            if ( symbolName.Contains( KSectionNameMarker ) )
            {
                if ( symbolName == KSectionNameUserLimit )
                {
                    // User data follows - don't update length of previous symbol
                    iFlags &= ~TFlags.EFlagsUpdateLengthOfPreviousSymbol;

                    // If we're reading a ROFS symbol then most likely we don't want
                    // to allow any more entries since they start to be a little wacky...
                    bool disallow = this.DisallowSymbolsOnceReadOnlyLimitReached;
                    if ( disallow )
                    {
                        iFlags |= TFlags.EFlagsDisallowFurtherSymbolsForCollection;
                    }
                }
                else if ( symbolName.Contains( "$$Limit" ) )
                {
                    // Don't change the length of a limit symbol should we happen to encounter a data item.
                    iFlags &= ~TFlags.EFlagsUpdateLengthOfPreviousSymbol;
                }
            }
        }

        /// <summary>
        /// Called when the new proposed symbol has no size.
        /// </summary>
        private bool ShouldSaveWhenNewSymbolHasNoSize( Symbol aLastSymbol, Symbol aNewSymbol )
        {
            bool save = true;
            //
            if ( aLastSymbol.Contains( aNewSymbol.Address ) )
            {
                // The new symbol overlaps the previous one. Additionally, the new symbol has a size of zero.
                //
                // E.g. #1:
                //
                //  _E32Dll                                         0x00008000   ARM Code      40  uc_dll_.o(.emb_text)
                //  Symbian$$CPP$$Exception$$Descriptor             0x00008014   Data           0  uc_dll_.o(.emb_text)
                //
                // E.g. #2:
                //
                //  RArray<unsigned long>::RArray()                 0x00009289   Thumb Code    10  abcd.in(t._ZN6RArrayImEC1Ev)
                //  RArray<unsigned long>::RArray__sub_object()     0x00009289   Thumb Code     0  abcd.in(t._ZN6RArrayImEC1Ev)
                //
                //      => NEW SYMBOL IS DISCARDED
                //
                save = false;
            }
            else if ( aNewSymbol.Address > aLastSymbol.EndAddress )
            {
                // The new symbol has a size of zero, but it doesn't overlap with prior symbol address.
                //
                //             _E32Dll_Body                             0x00008ecd   Thumb Code    34  uc_dll.o(.text)
                //             __DLL_Export_Table__                     0x00008f5c   ARM Code       0  abcd{000a0000}.exp(ExportTable)
                //             DLL##ExportTableSize                     0x00008f60   Data           0  abcd{000a0000}.exp(ExportTable)
                //             DLL##ExportTable                         0x00008f64   Data           0  abcd{000a0000}.exp(ExportTable)
                //             CActive::Cancel()                        0x00008f9c   ARM Code       0  euser{000a0000}-1088.o(StubCode)
                // aSymbol ==> CActive::SetActive()                     0x00008fa4   ARM Code       0  euser{000a0000}-1090.o(StubCode)
                //             CActive::CActive__sub_object(int)        0x00008fac   ARM Code       0  euser{000a0000}-1091.o(StubCode)
                //             CActive::~CActive__sub_object()          0x00008fb4   ARM Code       0  euser{000a0000}-1094.o(StubCode)
                //             RArrayBase::At(int) const                0x00008fbc   ARM Code       0  euser{000a0000}-1507.o(StubCode)
                //
                //      => NEW SYMBOL IS SAVED
                //
                iFlags |= TFlags.EFlagsUpdateLengthOfPreviousSymbol;
            }
            //
            return save;
        }

        /// <summary>
        /// Called when the new symbol has a valid size
        /// </summary>
        private bool ShouldSaveWhenNewSymbolHasValidSize( Symbol aLastSymbol, Symbol aNewSymbol )
        {
            bool save = true;

            if ( aLastSymbol.Contains( aNewSymbol.Address ) )
            {
                // The new symbol and the last symbol somehow overlap.
                if ( aLastSymbol.Address == aNewSymbol.Address && aLastSymbol.Size == aNewSymbol.Size )
                {
                    // The symbols start at the same address:
                    //
                    // E.g. #1:
                    //
                    //      CABCMonitor::~CABCMonitor()              0x000091a5   Thumb Code     0  abcmonitor.in(i._ZN11CABCMonitorD2Ev)
                    //      CABCMonitor::~CABCMonitor__sub_object()  0x000091a5   Thumb Code     8  abcmonitor.in(i._ZN11CABCMonitorD2Ev)
                    //
                    // E.g. #2: 
                    //
                    //      8022ab10    0060    Math::DivMod64(long long, long long, long long&)  
                    //      8022ab70    0014    Math::UDivMod64(unsigned long long, unsigned long long, unsigned long long&)
                    //      8022ab84    0000    $v0                                      
                    // >>>  8022ab84    0018    TRealX::Set(int)                         
                    //      8022ab9c    0044    TRealX::operator =(int)                  
                    //
                    // For example #1, we want to discard the new symbol and keep the original.
                    // For example #2, we want to discard the original symbol and keep the new one.
                    //
                    //
                    if ( !aLastSymbol.IsFunction )
                    {
                        // E.g. #2 => discard old symbol
                        Collection.RemoveAt( Collection.Count - 1 );
                    }
                    else
                    {
                        // E.g. #1 => discard new symbol
                        save = false;
                    }
                }
                else if ( aLastSymbol.Address == aNewSymbol.Address && aLastSymbol.Size == 0 )
                {
                    // E.g. : 
                    //
                    //      8342d6b8    0000    Image$$ER_RO$$Base                       anon$$obj.o(linker$$defined$$symbols)
                    // >>>  8342d6b8    0070    _E32Startup                              uc_exe_.o(.emb_text)
                    //
                    Collection.RemoveAt( Collection.Count - 1 );
                }
                else
                {
                    // The symbols start at different addresses, but somehow they still overlap.
                    //
                    // E.g.:
                    //
                    //    typeinfo name for CABCMonitorCapMapper   0x000094a8   Data          23  accmonitor.in(.constdata__ZTS20CABCMonitorCapMapper)
                    //    typeinfo name for CABCMonitorContainer   0x000094bf   Data          23  accmonitor.in(.constdata__ZTS20CABCMonitorContainer)
                    //
                    // In this scenario, the size of the 0x94a8 entry (23 bytes) causes it's address to overlap
                    // with the first byte from the entry starting at 0x94bf.
                    //
                    // In this scenario, we make an assumption that the size of the 0x94A8 entry is incorrect/invalid
                    uint overlap = aLastSymbol.AddressRange.Max - aNewSymbol.Address + 1;
                    aLastSymbol.Size = aLastSymbol.Size - overlap;
                }
            }
            else
            {
                if ( aLastSymbol.Address > aNewSymbol.Address )
                {

                }
            }
            //
            return save;
        }

        private void FlagBasedUpdateOfLastSymbolLength( Symbol aSymbol )
        {
            System.Diagnostics.Debug.Assert( ( iFlags & TFlags.EFlagsUpdateLengthOfPreviousSymbol ) == TFlags.EFlagsUpdateLengthOfPreviousSymbol );
            bool clearFlag = true;

            // Don't set the length of the default symbol!
            if ( !Collection.IsEmptyApartFromDefaultSymbol )
            {
                // Must have some existing symbol.
                int count = Collection.Count;
                System.Diagnostics.Debug.Assert( count > 0 );

                // Last symbol must have bad size?
                Symbol previousSymbol = LastSymbol;
                System.Diagnostics.Debug.Assert( previousSymbol.Size == 0 );

                // The new symbol must be exactly the same address as the last symbol
                // (in which case, the new symbol must have a valid size or else we're
                // unable to do anything sensible with it) 
                //
                // OR
                //
                // The new symbol must be after the last symbol. It cannot be before.
                if ( aSymbol.Address < previousSymbol.Address )
                {
                    // Data can confuse us, so skip when the address is earlier. E.g.:
                    //
                    //    83409b88    0000    .ARM.exidx$$Base                          uc_exe_.o(.ARM.exidx)
                    //    83409f80    0000    .ARM.exidx$$Limit                         xxx.in(.ARM.exidx)
                    //    83409f80    0000    Image$$ER_RO$$Limit                       anon$$obj.o(linker$$defined$$symbols)
                    // >> 00400000    0008    AllCapabilities                           xxx.in(.data)
                    //    00400008    0008    DisabledCapabilities                      xxx.in(.data)
                    //
                    clearFlag = true;
                }
                else
                {
                    if ( aSymbol.Address == previousSymbol.Address )
                    {
                        if ( aSymbol.Size > 0 )
                        {
                            // Okay, the new symbol has a valid size, the old one didn't.
                            previousSymbol.Size = aSymbol.Size;
                        }
                        else
                        {
                            // Hmm, neither the last or new symbol have a valid size.
                            // Nothing we can do in this case...
                            clearFlag = false;
                        }
                    }
                    else
                    {
                        // Need to work out the length of the previous symbol by comparing the
                        // address of this symbol against it.
                        uint delta = aSymbol.Address - previousSymbol.Address;

                        if ( delta > KMaxAutomaticLengthUpdateDelta )
                        {
                            // The delta is huge. Don't allow this kind of update.
                        }
                        else if ( delta > 1 )
                        {
                            // It's okay, this symbol had a later address than the last one
                            // This is normal.
                            previousSymbol.Size = delta - 1;
                        }
                        else
                        {
                            // This is not good. Two symbols both have the same address. In this
                            // situation discard the old symbol and take the new one instead because
                            // in all aspects other than name, they are identical.
                            Collection.RemoveAt( count - 1 );
                        }
                    }
                }
            }

            if ( clearFlag )
            {
                iFlags &= ~TFlags.EFlagsUpdateLengthOfPreviousSymbol;
            }
        }

        private void SetCollectionBaseAddress( Symbol aSymbolToUse )
        {
            uint baseAddress = aSymbolToUse.Address;
            bool haveSet = ( iFlags & TFlags.EFlagsHaveSetXIPBaseAddress ) == TFlags.EFlagsHaveSetXIPBaseAddress;
            System.Diagnostics.Debug.Assert( haveSet == false );

            // Relocate (changes base address)
            iCollection.Relocate( baseAddress );
            iFlags |= TFlags.EFlagsHaveSetXIPBaseAddress;

            // The symbol address needs to be reset to zero (i.e. start of collection)
            uint symbolSize = aSymbolToUse.Size;
            aSymbolToUse.OffsetAddress = 0;
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
                // When we aren't sure if we're being used to harmonise an XIP collection
                // we must check whether the base address has been set to something other than
                // zero and that the address is in the XIP range.
                // NB: Other cases are handled in the constructor.
                if ( iType == TCollectionType.EPossiblyXIP )
                {
                    if ( !iCollection.IsEmptyApartFromDefaultSymbol )
                    {
                        Symbol first = iCollection.FirstSymbol;
                        uint address = first.Address;
                        if ( address > 0 )
                        {
                            TMemoryModelRegion region = MMUtilities.RegionByAddress( address );
                            bool isFixed = ( region == TMemoryModelRegion.EMemoryModelRegionROM );
                            iCollection.IsFixed = isFixed;
                        }
                        else
                        {
                            // First address is zero, indicates RAM-loaded code and therefore
                            // non-XIP.
                            iCollection.IsFixed = false;
                        }
                    }
                    else
                    {
                        // The collection only contains the default symbol so in that case
                        // it can be thought to be relocatable (although in practise that wouldn't
                        // be very helpful). The main point is that we don't want this collection
                        // to start matching null addresses (quite common when performing symbolic
                        // lookup).
                        iCollection.IsFixed = false;
                    }
                }
            }
        }
        #endregion

        #region Data members
        private readonly SymbolCollection iCollection;
        private readonly TCollectionType iType;
        private TFlags iFlags = TFlags.EFlagsNone;
        #endregion
    }
}
