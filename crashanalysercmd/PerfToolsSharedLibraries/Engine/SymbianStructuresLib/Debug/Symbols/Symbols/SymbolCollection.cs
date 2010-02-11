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
using System.Threading;
using SymbianUtils;
using SymbianUtils.Range;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Symbols.Interfaces;
using SymbianStructuresLib.Debug.Common.Id;
using SymbianStructuresLib.Debug.Common.Interfaces;
using SymbianStructuresLib.Debug.Common.FileName;

namespace SymbianStructuresLib.Debug.Symbols
{
    public class SymbolCollection : DisposableObject, IEnumerable<Symbol>, IComparable<SymbolCollection>, IComparer<Symbol>, IFormattable
    {
        #region Static constructors
        public static SymbolCollection New( IPlatformIdAllocator aIdAllocator, string aFileNameInHost, string aFileNameInDevice )
        {
            SymbolCollection ret = new SymbolCollection( aIdAllocator, aFileNameInHost, aFileNameInDevice );
            return ret;
        }

        public static SymbolCollection NewCopy( IPlatformIdAllocator aIdAllocator, SymbolCollection aCollection )
        {
            SymbolCollection ret = new SymbolCollection( aIdAllocator, aCollection );
            return ret;
        }

        public static SymbolCollection NewByHostFileName( IPlatformIdAllocator aIdAllocator, string aFileName )
        {
            SymbolCollection ret = new SymbolCollection( aIdAllocator, aFileName );
            return ret;
        }
        #endregion

        #region Delegates & events
        public delegate void RelocationStatusChangeHandler( SymbolCollection aCollection );
        public event RelocationStatusChangeHandler RelocationStatusChanged;
        #endregion

        #region Constructors
        private SymbolCollection( IPlatformIdAllocator aIdAllocator, string aFileNameInHost )
		{
            iOriginalCollection = null;
            iId = aIdAllocator.AllocateId();
            iIdAllocator = aIdAllocator;
            iFileName = PlatformFileName.NewByHostName( aFileNameInHost );
            DefaultSymbolAdd();
		}

        private SymbolCollection( IPlatformIdAllocator aIdAllocator, string aFileNameInHost, string aFileNameInDevice )
            : this( aIdAllocator, aFileNameInHost )
        {
            iFileName.FileNameInDevice = aFileNameInDevice;
        }

        private SymbolCollection( IPlatformIdAllocator aIdAllocator, SymbolCollection aCopy )
        {
            iId = aIdAllocator.AllocateId();
            iIdAllocator = aIdAllocator;
            iTag = aCopy.iTag;
            iOriginalCollection = aCopy;
            iFlags = aCopy.iFlags;
            iTagged = aCopy.iTagged;
            iBaseAddress = aCopy.iBaseAddress;
            iCodeSegmentResolver = aCopy.iCodeSegmentResolver;
            iRelocationHandler = aCopy.iRelocationHandler;
            iFileName = PlatformFileName.New( aCopy.FileName );
            iCodeSegmentResolver = aCopy.IfaceCodeSegmentResolver;
            iRelocationHandler = aCopy.IfaceRelocationHandler;

            // Deep copy symbols
            foreach ( Symbol symbol in aCopy )
            {
                Symbol clone = Symbol.NewClone( this, symbol );
                iSymbols.Add( clone );
            }

            // Recalculate addresses
            RecalculationAddressRange();
        }
		#endregion

        #region API
        public void Serialize( StreamWriter aWriter )
        {
            StringBuilder temp = new StringBuilder();
            //
            lock ( iFileName )
            {
                temp.AppendLine( string.Empty );
                temp.AppendLine( "From    " + iFileName.FileNameInHost );
                temp.AppendLine( string.Empty );
            }
            //
            lock ( iSymbols )
            {
                foreach ( Symbol symbol in iSymbols )
                {
                    temp.AppendLine( symbol.ToString( "stream", null ) );
                }
            }

            aWriter.Write( temp.ToString() );
        }

        public void Add( Symbol aSymbol )
        {
            if ( aSymbol.IsDefault )
            {
                throw new ArgumentException( "Cannot add default symbol" );
            }
            //
            lock ( iSymbols )
            {
                int count = iSymbols.Count;
                if ( count == 1 )
                {
                    // Possibly...
                    DefaultSymbolRemove();
                }

                // Because count might have changed if we removed default
                // symbol.
                count = iSymbols.Count;
                if ( count >= 1 )
                {
                    Symbol last = iSymbols[ count - 1 ];
                    if ( aSymbol.Address < last.Address )
                    {
                        // We appear to be adding a symbol with an earlier address,
                        // which implies we need to sort the collection.
                        lock( iFlagLock )
                        {
                            iFlags |= TFlags.EFlagsRequiresSorting;
                        }
                    }
                }
                
                // Now add it
                iSymbols.Add( aSymbol );
                //
                if ( !InTransaction )
                {
                    RecalculationAddressRange();
                }
            }
        }

        public void AddRange( IEnumerable<Symbol> aSymbols )
        {
            TransactionBegin();
            //
            try
            {
                foreach ( Symbol symbol in aSymbols )
                {
                    Add( symbol );
                }
            }
            finally
            {
                TransactionEnd();
            }
        }

        public void Clear()
        {
            lock ( iSymbols )
            {
                iSymbols.Clear();
                
                // We are definitely empty so force the default symbol to be
                // added irrespective of flags
                DefaultSymbolAdd( true );
            }
        }

        public void Remove( Symbol aSymbol )
        {
            if ( aSymbol.IsDefault )
            {
                throw new ArgumentException( "Cannot remove default symbol" );
            }
            //
            lock ( iSymbols )
            {
                iSymbols.Remove( aSymbol );
                if ( iSymbols.Count == 0 )
                {
                    // We are definitely empty so force the default symbol to be
                    // added irrespective of flags
                    DefaultSymbolAdd( true );
                    System.Diagnostics.Debug.Assert( IsEmptyApartFromDefaultSymbol );
                }
            }
        }

        public void RemoveAt( int aIndex )
        {
            lock ( iSymbols )
            {
                Symbol symbol = iSymbols[ aIndex ];
                if ( symbol.IsDefault )
                {
                    throw new ArgumentException( "Cannot remove default symbol" );
                }
                //
                iSymbols.RemoveAt( aIndex );
                if ( iSymbols.Count == 0 )
                {
                    // We are definitely empty so force the default symbol to be
                    // added irrespective of flags
                    DefaultSymbolAdd( true );
                    System.Diagnostics.Debug.Assert( IsEmptyApartFromDefaultSymbol );
                }
            }
        }
 
        public bool Contains( uint aAddress )
        {
            if ( iAddresses == null )
            {
                iAddresses = new AddressRangeCollection( (IEnumerable<AddressRange>) this );
                RecalculationAddressRange();
            }
            //
            bool found = iAddresses.Contains( aAddress );
            return found;
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

        public void Sort()
        {
            if ( ( iFlags & TFlags.EFlagsRequiresSorting ) == TFlags.EFlagsRequiresSorting )
            {
                iSymbols.Sort( this );
                RecalculationAddressRange();
            }
        }

        public void SortAsync()
        {
            if ( ( iFlags & TFlags.EFlagsRequiresSorting ) == TFlags.EFlagsRequiresSorting )
            {
                ThreadPool.QueueUserWorkItem( new WaitCallback( InitiateAsyncSort ) );
            }
        }

        public void Relocate( uint aTo )
        {
            uint old = iBaseAddress;
            iBaseAddress = aTo;
            //
            if ( iRelocationHandler != null )
            {
                iRelocationHandler.PrepareForRelocation( this, old, BaseAddress );
            }
            //
            RecalculationAddressRange();
        }

        public void TransactionBegin()
        {
            lock ( iFlagLock )
            {
                iFlags |= TFlags.EFlagsInTransaction;
            }
        }

        public void TransactionEnd()
        {
            lock ( iFlagLock )
            {
                iFlags &= ~TFlags.EFlagsInTransaction;
            }

            RecalculationAddressRange();
        }

        public void Clone( IEnumerable<Symbol> aSymbols )
        {
            // Deep copy symbols
            Clear();
            try
            {
                TransactionBegin();
                foreach ( Symbol symbol in aSymbols )
                {
                    // Make sure we don't try to add the default symbol. The symbol collection
                    // manages this automatically.
                    if ( symbol.IsDefault )
                    {
                    }
                    else
                    {
                        Symbol clone = Symbol.NewClone( this, symbol );
                        Add( clone );
                    }
                }
            }
            finally
            {
                TransactionEnd();
            }
        }
        #endregion

		#region Properties
		public int Count
		{
            get
            {
                lock ( iSymbols )
                {
                    return iSymbols.Count;
                }
            }
		}

		public Symbol this[ int aIndex ]
		{
            get
            {
                lock ( iSymbols )
                {
                    return iSymbols[ aIndex ];
                }
            }
        }

        public Symbol this[ uint aAddress ]
        {
            get
            {
                // For debugging
                int x = 0;
                if ( x > 0 )
                {
                    string dump = Dump( aAddress );
                    System.Diagnostics.Debug.WriteLine( dump );
                }
                //
                Symbol ret = null;
                Symbol temp = Symbol.NewTemp( this, aAddress );
                //
                lock ( iSymbols )
                {
                    AddressFindingComparer comparer = new AddressFindingComparer();
                    int pos = iSymbols.BinarySearch( temp, comparer );
                    int count = iSymbols.Count;
                    //
                    if ( pos >= 0 && pos < count )
                    {
                        ret = iSymbols[ pos ];
                        System.Diagnostics.Debug.Assert( ret.AddressRange.Contains( aAddress ) );
                    }
                }
                //
                return ret;
            }
        }

        public Symbol FirstSymbol
		{
			get
			{
				Symbol ret = null;
                lock ( iSymbols )
                {
                    if ( Count > 0 )
                    {
                        ret = this[ 0 ];
                    }
                }
				return ret;
			}
		}

		public Symbol LastSymbol
		{
			get
			{
				Symbol ret = null;
                lock ( iSymbols )
                {
                    if ( Count > 0 )
                    {
                        ret = this[ Count - 1 ];
                    }
                }
				return ret;
			}
		}

        public bool Tagged
        {
            get { return iTagged; }
            set
            {
                // The 'tagged' property is one of the few that we must (and can safely)
                // cascade to the original underlying collection. 
                //
                // We do this, because when working with ROFS/relocated symbols, we generally
                // clone and fixup, rather than fixup the original. This permits us to use
                // a symbol collection at multiple base addresses (i.e. use the same symbols within
                // different process-relative views of the "world").
                //
                // If we don't cascade the tagged attribute to the original (i.e. primary) symbol
                // collection, then when the client application wants to serialized tagged collections
                // the ROFS collections will potentially be missing (if they have been unloaded).
                if ( iOriginalCollection != null )
                {
                    iOriginalCollection.Tagged = value;
                    iTagged = value;
                }
                else if ( iTagged != value )
                {
                    iTagged = value;
                    if ( iTagged )
                    {
                        System.Diagnostics.Debug.WriteLine( string.Format( "[S] TAGGING: 0x{0:x8}, {1}", this.BaseAddress, iFileName ) );
                    }
                }
            }
        }

        public bool InTransaction
        {
            get { return ( iFlags & TFlags.EFlagsInTransaction ) == TFlags.EFlagsInTransaction; }
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
            set
            {
                IsRelocatable = !value;
            }
        }

        public bool IsEmptyApartFromDefaultSymbol
        {
            get
            { 
                bool ret = ( iFlags & TFlags.EFlagsIsEmptyApartFromDefaultSymbol ) == TFlags.EFlagsIsEmptyApartFromDefaultSymbol;
                return ret;
            }
            private set
            {
                lock ( iFlagLock )
                {
                    if ( value )
                    {
                        iFlags |= TFlags.EFlagsIsEmptyApartFromDefaultSymbol;
                    }
                    else
                    {
                        iFlags &= ~TFlags.EFlagsIsEmptyApartFromDefaultSymbol;
                    }
                }
            }
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

        public PlatformId Id
        {
            get { return iId; }
        }

        public PlatformFileName FileName
		{
			get { return iFileName; }
		}

        public SymbolCollectionList ParentList
        {
            get { return iParentList; }
            internal set { iParentList = value; }
        }

        public AddressRange SubsumedPrimaryRange
        {
            get
            {
                AddressRange ret = new AddressRange( this.BaseAddress, this.BaseAddress );
                //
                if ( iAddresses != null )
                {
                    int count = iAddresses.Count;
                    for( int i=0; i<count; i++ )
                    {
                        AddressRange segment = iAddresses[ i ];
                        
                        // Ranges should be reliably sorted/calculated so that we
                        // shouldn't end up going backwards beyond the base address
                        // of the code segment
                        System.Diagnostics.Debug.Assert( segment.Min >= ret.Min );

                        // If the segmented address range doesn't sit within the existing
                        // range we are building, then we may need to extend our return
                        // value.
                        if ( !ret.Contains( segment ) )
                        {
                            uint diff = segment.Min - ret.Max;
                            if ( diff <= KMaximumNonConsecutiveAddressRangeDifferenceToSubsume )
                            {
                                ret.UpdateMax( segment.Max );
                            }
                        }
                    }
                }
                //
                return ret;
            }
        }

        public object SyncRoot
        {
            get
            {
                return iSymbols;
            }
        }
		#endregion

        #region Properties - interfaces
        public ISymbolCodeSegmentResolver IfaceCodeSegmentResolver
        {
            get { return iCodeSegmentResolver; }
            set { iCodeSegmentResolver = value; }
        }

        public ISymbolCollectionRelocationHandler IfaceRelocationHandler
        {
            get { return iRelocationHandler; }
            set { iRelocationHandler = value; }
        }
        #endregion

        #region From IComparable<SymbolCollection>
        public int CompareTo( SymbolCollection aCollection )
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

        #region From IComparer<Symbol>
        public int Compare( Symbol aLeft, Symbol aRight )
        {
            System.Diagnostics.Debug.Assert( aLeft.EndAddress >= aLeft.Address );
            System.Diagnostics.Debug.Assert( aRight.EndAddress >= aRight.Address );
            //
            int ret = -1;
            //
            if ( aLeft.Address == aRight.Address && aLeft.EndAddress == aRight.EndAddress )
            {
                ret = 0;
            }
            else if ( aLeft.EndAddress == aRight.Address )
            {
                System.Diagnostics.Debug.Assert( aLeft.Address < aRight.Address );
                System.Diagnostics.Debug.Assert( aRight.EndAddress >= aLeft.EndAddress );
                //
                ret = -1;
            }
            else if ( aLeft.Address == aRight.EndAddress )
            {
                System.Diagnostics.Debug.Assert( aRight.Address < aLeft.Address );
                System.Diagnostics.Debug.Assert( aLeft.EndAddress >= aRight.EndAddress );
                //
                ret = 1;
            }
            else if ( aLeft.Address > aRight.EndAddress )
            {
                System.Diagnostics.Debug.Assert( aLeft.EndAddress > aRight.EndAddress );
                System.Diagnostics.Debug.Assert( aLeft.EndAddress > aRight.Address );
                ret = 1;
            }
            else if ( aLeft.EndAddress < aRight.Address )
            {
                System.Diagnostics.Debug.Assert( aLeft.Address < aRight.EndAddress );
                System.Diagnostics.Debug.Assert( aRight.EndAddress > aLeft.EndAddress );
                ret = -1;
            }
            //
            return ret;
        }
        #endregion

        #region From IEnumerable<Symbol>
        IEnumerator<Symbol> IEnumerable<Symbol>.GetEnumerator()
        {
            foreach ( Symbol entry in iSymbols )
            {
                yield return entry;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( Symbol entry in iSymbols )
            {
                yield return entry;
            }
        }
        #endregion

        #region From IFormattable
        public string ToString( string aFormat, IFormatProvider aFormatProvider )
        {
            string ret = string.Empty;
            //
            if ( aFormat == null )
            {
                ret = iFileName.ToString();
            }
            else if ( aFormat.ToUpper() == "FULL" )
            {
                ret = Dump();
            }
            else
            {
                throw new FormatException( string.Format( "Invalid format string: '{0}'.", aFormat ) );
            }
            //
            return ret;
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return iFileName.ToString();
        }

        public override bool Equals( object aObject )
        {
            if ( aObject != null && aObject is SymbolCollection )
            {
                SymbolCollection col = (SymbolCollection) aObject;
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

        #region Internal enumerations
        [Flags]
        private enum TFlags : byte
        {
            EFlagsNone = 0,
            EFlagsInTransaction = 1,
            EFlagsIsRelocatable = 2,
            EFlagsIsEmptyApartFromDefaultSymbol = 4,
            EFlagsRequiresSorting = 8,
        };
        #endregion

        #region Internal constants
        private const uint KMaximumNonConsecutiveAddressRangeDifferenceToSubsume = 512;
        #endregion

        #region Internal methods
        private string Dump()
        {
            string ret = Dump( uint.MaxValue );
            return ret;
        }

        private string Dump( uint aAddress )
        {
#if SYMCOL_INVARIANT_CHECK
            DebugCheckInvariant();
#endif
            //
            StringBuilder ret = new StringBuilder();
            //
            int i = 0;
            string line = string.Empty;
            //
            lock ( iSymbols )
            {
                foreach ( Symbol entry in iSymbols )
                {
                    if ( aAddress != uint.MaxValue && entry.Contains( aAddress ) )
                    {
                        line = i.ToString( "d8" ) + " * [" + entry.Address.ToString( "x8" ) + "-" + entry.EndAddress.ToString( "x8" ) + "] " + entry.Name;
                    }
                    else
                    {
                        line = i.ToString( "d8" ) + "   [" + entry.Address.ToString( "x8" ) + "-" + entry.EndAddress.ToString( "x8" ) + "] " + entry.Name;
                    }
                    //
                    ret.AppendLine( line );
                    i++;
                }
            }
            //
            return ret.ToString();
        }

        private void DefaultSymbolAdd()
        {
            DefaultSymbolAdd( false );
        }

        private void DefaultSymbolAdd( bool aForce )
        {
#if SYMCOL_INVARIANT_CHECK
            DebugCheckInvariant();
#endif
            //
            if ( !IsEmptyApartFromDefaultSymbol || aForce )
            {
                lock ( iSymbols )
                {
                    System.Diagnostics.Debug.Assert( Count == 0 );
                    Symbol def = Symbol.NewDefault( this );
                    iSymbols.Add( def );
                    IsEmptyApartFromDefaultSymbol = true;
                }
            }
        }

        private void DefaultSymbolRemove()
        {
#if SYMCOL_INVARIANT_CHECK
            DebugCheckInvariant();
#endif
            //
            if ( IsEmptyApartFromDefaultSymbol )
            {
                lock ( iSymbols )
                {
                    int count = iSymbols.Count;
                    //
                    if ( IsEmptyApartFromDefaultSymbol )
                    {
                        System.Diagnostics.Debug.Assert( count == 1 && this.FirstSymbol.IsDefault );
                        iSymbols.RemoveAt( 0 );
                        IsEmptyApartFromDefaultSymbol = false;
                    }
#if SYMCOL_INVARIANT_CHECK
                else if ( count > 0 )
                {
                    System.Diagnostics.Debug.Assert( this.FirstSymbol.IsDefault == false );
                }
#endif
                }
            }
        }

        private void InitiateAsyncSort( object aNotUsed )
        {
            Sort();
        }

#if SYMCOL_INVARIANT_CHECK
        private void DebugCheckInvariant()
        {
            lock ( iSymbols )
            {
                int count = Count;
                if ( count > 0 )
                {
                    Symbol first = this.FirstSymbol;
                    if ( first.IsDefault )
                    {
                        System.Diagnostics.Debug.Assert( IsEmptyApartFromDefaultSymbol );
                        System.Diagnostics.Debug.Assert( count == 1 );
                    }
                }
            }
        }
#endif

        private void RecalculationAddressRange()
        {
#if SYMCOL_INVARIANT_CHECK
            DebugCheckInvariant();
#endif
            //
            if ( !InTransaction )
            {
                AddressRangeCollection range = new AddressRangeCollection();
                lock ( iSymbols )
                {
                    foreach ( Symbol entry in iSymbols )
                    {
                        range.Add( entry.AddressRange );
                    }
                }
                //
                iAddresses = range;
            }
        }
        #endregion

        #region Internal properties
        internal IPlatformIdAllocator IdAllocator
        {
            get { return iIdAllocator; }
        }

        internal AddressRangeCollection AddressRangeCollection
        {
            get { return iAddresses; }
        }
        #endregion

        #region Internal classes
        internal class AddressFindingComparer : IComparer<Symbol>
        {
            public int Compare( Symbol aLeft, Symbol aRight )
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
                if ( iAddresses != null )
                {
                    iAddresses.Clear();
                }
                iSymbols.Clear();
                iParentList = null;
                iCodeSegmentResolver = null;
                iRelocationHandler = null;
            }
        }
        #endregion

        #region Data members
        private readonly PlatformId iId;
        private readonly IPlatformIdAllocator iIdAllocator;
        private readonly PlatformFileName iFileName;
        private readonly SymbolCollection iOriginalCollection = null;
        private object iTag = null;
        private object iFlagLock = new object();
        private bool iTagged = false;
        private TFlags iFlags = TFlags.EFlagsNone;
        private uint iBaseAddress = 0;
        private AddressRangeCollection iAddresses = null;
        private List<Symbol> iSymbols = new List<Symbol>();
        private SymbolCollectionList iParentList = null;
        private ISymbolCodeSegmentResolver iCodeSegmentResolver = null;
        private ISymbolCollectionRelocationHandler iRelocationHandler = null;
		#endregion
    }
}
