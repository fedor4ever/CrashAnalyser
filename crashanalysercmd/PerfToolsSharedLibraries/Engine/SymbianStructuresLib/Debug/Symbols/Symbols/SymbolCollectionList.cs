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
using System.Threading;
using System.Collections.Generic;
using SymbianUtils;
using SymbianUtils.Range;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Common.FileName;

namespace SymbianStructuresLib.Debug.Symbols
{
    public class SymbolCollectionList : IEnumerable<SymbolCollection>
    {
        #region Constructors
        public SymbolCollectionList()
        {
        }
        #endregion

        #region API
        public void Clear()
        {
            lock ( iCollections )
            {
                iCollections.Clear();
            }
            lock ( iLookupCache )
            {
                iLookupCache.Clear();
            }
            lock ( iFileNameDictionary )
            {
                iFileNameDictionary.Clear();
            }
        }

        public void Add( SymbolCollection aCollection )
        {
            // Check not already added
            PlatformFileName name = aCollection.FileName;
            lock ( iFileNameDictionary )
            {
                if ( iFileNameDictionary.ContainsKey( name ) )
                {
                    throw new ArgumentException( string.Format( "Collection \'{0}\' already exists", name ) );
                }
            }

            // Add to file name dictionary
            iFileNameDictionary.Add( name, aCollection );

            // Add to non-optimised collection
            iCollections.Add( aCollection );
        }

        public void AddAndBuildCache( SymbolCollection aCollection )
        {
            Add( aCollection );
            UpdateCacheForCollection( aCollection );
        }

        public void Remove( SymbolCollection aCollection )
        {
            Predicate<SymbolCollection> predicate = delegate( SymbolCollection collection )
            {
                return collection.Equals( aCollection );
            };
            //
            lock ( iCollections )
            {
                iCollections.RemoveAll( predicate );
            }
            //
            RemoveFromCache( aCollection );
        }

        public void RemoveUntagged()
        {
            Predicate<SymbolCollection> predicate = delegate( SymbolCollection collection )
            {
                return collection.Tagged == false;
            };
            //
            lock ( iCollections )
            {
                iCollections.RemoveAll( predicate );
            }

            // MRU can be left intact since implicitly it should only contained tagged
            // collections
        }

        public void Serialize( Stream aStream )
        {
            using ( StreamWriter writer = new StreamWriter( aStream ) )
            {
                Action<SymbolCollection> action = delegate( SymbolCollection collection )
                {
                    if ( collection.Tagged )
                    {
                        collection.Serialize( writer );
                    }
                };
                //
                lock ( this )
                {
                    iCollections.ForEach( action );
                }
            }
        }

        public bool Contains( uint aAddress )
        {
            WaitForLookupCache();
            //
            bool ret = false;
            //
            AddressCollectionPair temp = new AddressCollectionPair( new AddressRange( aAddress, aAddress ), null );
            AddressCollectionPairComparer comparer = new AddressCollectionPairComparer();
            //
            lock ( iLookupCache )
            {
                int pos = iLookupCache.BinarySearch( temp, comparer );
                ret = ( pos >= 0 );
            }
            //
            return ret;
        }

        public bool Contains( SymbolCollection aCollection )
        {
            bool ret = false;

            // Check not already added
            PlatformFileName name = aCollection.FileName;
            lock ( iFileNameDictionary )
            {
                ret = iFileNameDictionary.ContainsKey( name );
            }
            //
            return ret;
        }

        public Symbol Lookup( uint aAddress, out SymbolCollection aCollection )
        {
            WaitForLookupCache();
            //
            Symbol ret = null;
            aCollection = null;
            //
            AddressCollectionPair temp = new AddressCollectionPair( new AddressRange( aAddress, aAddress ), null );
            AddressCollectionPairComparer comparer = new AddressCollectionPairComparer();
            //
            lock ( iLookupCache )
            {
                int pos = iLookupCache.BinarySearch( temp, comparer );
                if ( pos >= 0 )
                {
                    temp = iLookupCache[ pos ];
                    aCollection = temp.Collection;
                    ret = aCollection[ aAddress ];
                }
            }
            //
            return ret;
        }

        public void SortByCollectionAddress()
        {
            Comparison<SymbolCollection> comparer = delegate( SymbolCollection aCol1, SymbolCollection aCol2 )
            {
                int ret = aCol1.BaseAddress.CompareTo( aCol2.BaseAddress );
                return ret;
            };
            //
            iCollections.Sort( comparer );
        }

        public void BuildLookupCache()
        {
            WaitForLookupCache();
            //
            lock ( iLookupCache )
            {
                iLookupCache.Clear();
            }
            
            // Build the cache in a separate thread.
            // Must take the lock to either create or destroy waiter.
            lock ( iWaiterSyncRoot )
            {
                if ( iLookUpCacheWaiter == null )
                {
                    iLookUpCacheWaiter = new AutoResetEvent( false );
                    ThreadPool.QueueUserWorkItem( new WaitCallback( BackgroundThreadBuildLookupCache ), null );
                }
            }
        }
        #endregion

        #region Properties
        public int Count
        {
            get
            {
                lock ( iCollections )
                {
                    return iCollections.Count;
                }
            }
        }

        public bool IsEmpty
        {
            get { return Count == 0; } 
        }

        public bool Tagged
        {
            set
            {
                lock ( this )
                {
                    Action<SymbolCollection> action = delegate( SymbolCollection collection )
                    {
                        collection.Tagged = value;
                    };
                    iCollections.ForEach( action );
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

        public SymbolCollection this[ int aIndex ]
        {
            get
            {
                lock ( iCollections )
                {
                    return iCollections[ aIndex ];
                }
            }
        }

        public SymbolCollection this[ PlatformFileName aFileName ]
        {
            get
            {
                Predicate<SymbolCollection> predicate = delegate( SymbolCollection collection )
                {
                    bool same = collection.FileName.Equals( aFileName );
                    return same;
                };
                //
                SymbolCollection ret = null;
                //
                lock ( iCollections )
                {
                    ret = iCollections.Find( predicate );
                }
                //
                return ret;
            }
        }

        public SymbolCollection this[ CodeSegDefinition aCodeSegment ]
        {
            get
            {
                Predicate<SymbolCollection> predicate = delegate( SymbolCollection collection )
                {
                    bool same = collection.IsMatchingCodeSegment( aCodeSegment );
                    return same;
                };
                //
                SymbolCollection ret = null;
                //
                lock ( iCollections )
                {
                    ret = iCollections.Find( predicate );
                }
                //
                return ret;
            }
        }
        #endregion

        #region From IEnumerable<SymbolCollection>
        IEnumerator<SymbolCollection> IEnumerable<SymbolCollection>.GetEnumerator()
        {
            foreach ( SymbolCollection col in iCollections )
            {
                yield return col;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( SymbolCollection col in iCollections )
            {
                yield return col;
            }
        }
        #endregion

        #region Internal methods
        private void WaitForLookupCache()
        {
            if ( iLookUpCacheWaiter != null )
            {
                // Wait for the cache to become ready
                iLookUpCacheWaiter.WaitOne();

                // Must take the lock to either create or destroy waiter
                lock ( iWaiterSyncRoot )
                {
                    iLookUpCacheWaiter.Close();
                    iLookUpCacheWaiter = null;
                }
            }
        }

        private void RemoveFromCache( SymbolCollection aCollection )
        {
            // Must wait if it's being built already
            WaitForLookupCache();

            // Must take the lock to either create or destroy waiter
            lock ( iWaiterSyncRoot )
            {
                System.Diagnostics.Debug.Assert( iLookUpCacheWaiter == null );
                //
                iLookUpCacheWaiter = new AutoResetEvent( false );
                ThreadPool.QueueUserWorkItem( new WaitCallback( BackgroundThreadRemoveFromCache ), aCollection );
            }
        }

        private void BackgroundThreadBuildLookupCache( object aNotUsed )
        {
            System.Diagnostics.Debug.Assert( iLookUpCacheWaiter != null );

            // This comparer will help us sort the ranges
            AddressCollectionPairComparer comparer = new AddressCollectionPairComparer();

            // Make sorted list entries
            lock ( iCollections )
            {
                int colCount = iCollections.Count;
                for ( int colIndex = 0; colIndex < colCount; colIndex++ )
                {
                    SymbolCollection collection = iCollections[ colIndex ];
                    //
                    UpdateCacheForCollection( collection );
                }
            }

            // Done building cache
            iLookUpCacheWaiter.Set();
        }

        private void BackgroundThreadRemoveFromCache( object aCollection )
        {
            System.Diagnostics.Debug.Assert( iLookUpCacheWaiter != null );
            SymbolCollection collection = (SymbolCollection) aCollection;
            //
            Predicate<AddressCollectionPair> predicate = delegate( AddressCollectionPair pair )
            {
                bool match = ( pair.Collection == collection );
                return match;
            };
            //
            lock ( iLookupCache )
            {
                AddressCollectionPair temp = new AddressCollectionPair( new AddressRange(), collection );
                iLookupCache.RemoveAll( predicate );
            }
            //
            iLookUpCacheWaiter.Set();
        }

        private void UpdateCacheForCollection( SymbolCollection aCollection )
        {
            bool isEmpty = aCollection.IsEmptyApartFromDefaultSymbol;
            if ( isEmpty == false )
            {
                AddressRangeCollection ranges = aCollection.AddressRangeCollection;
                if ( ranges != null )
                {
                    // This comparer will help us sort the ranges
                    AddressCollectionPairComparer comparer = new AddressCollectionPairComparer();

                    int rangeCount = ranges.Count;
                    for ( int rangeIndex = 0; rangeIndex < rangeCount; rangeIndex++ )
                    {
                        AddressRange range = ranges[ rangeIndex ];
                        AddressCollectionPair pair = new AddressCollectionPair( range, aCollection );
                        //
                        lock ( iLookupCache )
                        {
                            int pos = iLookupCache.BinarySearch( pair, comparer );
                            if ( pos >= 0 )
                            {
                                AddressCollectionPair overlapsWith = iLookupCache[ pos ];
                                System.Diagnostics.Debug.WriteLine( string.Format( "Collection {0} [{1}] overlaps with existing collection: {2} [{3}]", pair.Collection.FileName, pair.Range, overlapsWith.Collection, overlapsWith.Range ) );
                            }
                            else
                            {
                                pos = ~pos;
                                iLookupCache.Insert( pos, pair );
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Internal classes
        private class AddressCollectionPair
        {
            #region Constructors
            public AddressCollectionPair( AddressRange aRange, SymbolCollection aCollection )
            {
                iRange = aRange;
                iCollection = aCollection;
            }
            #endregion

            #region Properties
            public AddressRange Range
            {
                get { return iRange; }
            }

            public SymbolCollection Collection
            {
                get { return iCollection; }
            }
            #endregion

            #region From System.Object
            public override string ToString()
            {
                string ret = string.Format( "{0} {1}", iRange, iCollection.FileName.FileNameInDevice );
                return ret;
            }
            #endregion

            #region Data members
            private readonly AddressRange iRange;
            private readonly SymbolCollection iCollection;
            #endregion
        }

        private class AddressCollectionPairComparer : IComparer<AddressCollectionPair>
        {
            public int Compare( AddressCollectionPair aLeft, AddressCollectionPair aRight )
            {
                int ret = -1;
                //
                AddressRange lr = aLeft.Range;
                AddressRange rr = aRight.Range;
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

        #region Data members
        private object iTag = null;
        private object iWaiterSyncRoot = new object();
        private AutoResetEvent iLookUpCacheWaiter = null;
        private List<SymbolCollection> iCollections = new List<SymbolCollection>();
        private List<AddressCollectionPair> iLookupCache = new List<AddressCollectionPair>();
        private Dictionary<PlatformFileName, SymbolCollection> iFileNameDictionary = new Dictionary<PlatformFileName, SymbolCollection>();
        #endregion
    }
}