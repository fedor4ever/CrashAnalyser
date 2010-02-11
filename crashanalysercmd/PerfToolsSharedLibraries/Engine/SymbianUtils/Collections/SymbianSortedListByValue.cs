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

namespace SymbianUtils.Collections
{
    public class SymbianSortedListByValue<TKey, TValue> where TValue : class
    {
        #region Constructors
        public SymbianSortedListByValue( IComparer<TValue> aComparer )
            : this( 10, aComparer )
        {
        }

        public SymbianSortedListByValue( int aGranularity, IComparer<TValue> aComparer )
        {
            iList = new List<KeyValuePair<TValue, TKey>>( aGranularity );
            iComparer = new SymbianSortedListByValueComparer( aComparer );
        }

        public SymbianSortedListByValue( SymbianSortedListByValue<TKey, TValue> aCollection )
        {
            int count = aCollection.Count;
            //
            iList = new List<KeyValuePair<TValue, TKey>>( count + 1 );
            iComparer = new SymbianSortedListByValue<TKey, TValue>.SymbianSortedListByValueComparer( aCollection.iComparer.Comparer );
            //
            for ( int i = 0; i < count; i++ )
            {
                KeyValuePair<TValue, TKey> entry = aCollection.iList[ i ];
                Add( entry.Value, entry.Key );
            }
        }
        #endregion

        #region API
        public bool Contains( TKey aKey )
        {
            return iDictionary.ContainsKey( aKey );
        }

        public void Add( TKey aKey, TValue aValue )
        {
            if ( aKey == null )
            {
                throw new ArgumentNullException( "aKey cannot be null" );
            }

            // Add item to dictionary if required
            if ( !iDictionary.ContainsKey( aKey ) )
            {
                iDictionary.Add( aKey, aValue );
            }

            // Search for item in the list. If it's already present then
            // this will return the index. Otherwise, it will return a negative
            // number.
            // If this index is greater than or equal to the size of the array, 
            // there are no elements larger than value in the array. 
            //
            // Otherwise, it is the index of the first element that is larger than value.
            KeyValuePair<TValue, TKey> entry = new KeyValuePair<TValue, TKey>( aValue, aKey );
            int index = iList.BinarySearch( entry, iComparer );
            if ( index < 0 )
            {
                index = ~index;
            }
            iList.Insert( index, entry );
        }

        public void Remove( Predicate<TValue> aValue )
        {
            Predicate<KeyValuePair<TValue, TKey>> matchPredicate = delegate( KeyValuePair<TValue, TKey> aItem )
            {
                bool remove = aValue.Invoke( aItem.Key );
                return remove;
            };

            RemoveAll( matchPredicate );
        }

        public void Remove( TValue aValue )
        {
            Predicate<KeyValuePair<TValue, TKey>> matchPredicate = delegate( KeyValuePair<TValue, TKey> aItem )
            {
                return ( aValue == aItem.Key );
            };

            RemoveAll( matchPredicate );
        }

        public void Remove( TKey aKey )
        {
            TValue v = null;
            if ( iDictionary.TryGetValue( aKey, out v ) )
            {
                iDictionary.Remove( aKey );
                Remove( v );
            }
        }

        public void Clear()
        {
            iList.Clear();
            iDictionary.Clear();
        }
        
        public void RemoveRange( int aStartIndex, int aCount )
        {
            List<KeyValuePair<TValue, TKey>> items = iList.GetRange( aStartIndex, aCount );
            RemoveAll( items );
        }

        public void Sort( IComparer<TValue> aComparer )
        {
            SymbianSortedListByValueComparer comparer = new SymbianSortedListByValueComparer( aComparer );
            iList.Sort( comparer );
            iComparer = comparer;
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iList.Count; }
        }

        public TValue this[ int aIndex ]
        {
            get
            { 
                KeyValuePair< TValue, TKey> ret = iList[ aIndex ];
                return ret.Key;
            }
        }

        public TValue this[ TKey aKey ]
        {
            get
            {
                TValue ret = null;
                bool found = iDictionary.TryGetValue( aKey, out ret );
                return ret;
            }
        }
        #endregion

        #region Internal methods
        private void RemoveAll( Predicate<KeyValuePair<TValue, TKey>> aPredicate )
        {
            List<KeyValuePair<TValue, TKey>> items = iList.FindAll( aPredicate );
            RemoveAll( items );
        }

        private void RemoveAll( List<KeyValuePair<TValue, TKey>> aItems )
        {
            foreach ( KeyValuePair<TValue, TKey> item in aItems )
            {
                // Remove from dictionary
                TKey key = item.Value;
                if ( iDictionary.ContainsKey( key ) )
                {
                    iDictionary.Remove( key );
                }

                // Remove from list
                TValue value = item.Key;
                iList.Remove( item );
            }
        }
        #endregion

        #region Internal classes
        private class SymbianSortedListByValueComparer : IComparer< KeyValuePair<TValue, TKey> >
        {
            #region Constructors
            public SymbianSortedListByValueComparer( IComparer<TValue> aComparer )
            {
                iComparer = aComparer;
            }
            #endregion

            #region Properties
            public IComparer<TValue> Comparer
            {
                get { return iComparer; }
            }
            #endregion

            #region From IComparer<KeyValuePair<TValue,TKey>>
            public int Compare( KeyValuePair<TValue, TKey> aLeft, KeyValuePair<TValue, TKey> aRight )
            {
                int ret = iComparer.Compare( aLeft.Key, aRight.Key );
                return ret;
            }
            #endregion

            #region Data members
            private readonly IComparer<TValue> iComparer;
            #endregion
        }
        #endregion

        #region Data members
        private List< KeyValuePair<TValue, TKey> > iList = null;
        private Dictionary<TKey, TValue> iDictionary = new Dictionary<TKey, TValue>();
        private SymbianSortedListByValueComparer iComparer = null;
        #endregion
    }
}
