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
using SymbianUtils;
using SymbianUtils.Range;
using SymbianStructuresLib.Debug.Symbols;
using SymbianStructuresLib.Debug.Symbols.Interfaces;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Common.FileName;
using SymbianSymbolLib.SourceManagement.Provisioning;

namespace SymbianSymbolLib.SourceManagement.Source
{
    public class SymSource : DisposableObject, IEnumerable<SymbolCollection>, ISymbolCollectionRelocationHandler
    {
        #region Delegates & events
        public delegate void EventHandlerFunction( TEvent aEvent, SymSource aSource, object aData );
        public event EventHandlerFunction EventHandler;
        #endregion

        #region Enumerations
        public enum TEvent
        {
            EReadingStarted = 0,
            EReadingProgress,
            EReadingComplete
        }

        public enum TTimeToRead
        {
            EReadWhenPriming = 0,
            EReadWhenNeeded // i.e. relocated
        }
        #endregion

        #region Constructors
        public SymSource( string aURI, SymSourceProvider aProvider )
        {
            iURI = aURI;
            iProvider = aProvider;
        }

        public SymSource( string aURI, SymSourceProvider aProvider, SymbolCollection aCollection )
            : this( aURI, aProvider )
        {
            Add( aCollection );
        }
        #endregion

        #region API
        public virtual void Read( TSynchronicity aSynchronicity )
        {
            iProvider.ReadSource( this, aSynchronicity );
        }

        public virtual void Add( SymbolCollection aCollection )
        {
            // We want to be told if the collection changes it's relocation state.
            aCollection.RelocationStatusChanged += new SymbolCollection.RelocationStatusChangeHandler( Collection_RelocationStatusChanged );

            lock ( iCollectionsAll )
            {
                iCollectionsAll.Add( aCollection );
            }

            CategoriseCollection( aCollection );
        }

        public virtual void Remove( SymbolCollection aCollection )
        {
            aCollection.RelocationStatusChanged -= new SymbolCollection.RelocationStatusChangeHandler( Collection_RelocationStatusChanged );
            //
            lock ( iCollectionsAll )
            {
                iCollectionsAll.Remove( aCollection );
            }
            lock ( iAlwaysActivatedCollections )
            {
                iAlwaysActivatedCollections.Remove( aCollection );
            }
        }

        public virtual bool Contains( uint aAddress )
        {
            lock ( iAlwaysActivatedCollections )
            {
                return iAlwaysActivatedCollections.Contains( aAddress );
            }
        }

        public virtual Symbol Lookup( uint aAddress, out SymbolCollection aCollection )
        {
            lock ( iAlwaysActivatedCollections )
            {
                aCollection = null;
                //
                Symbol ret = iAlwaysActivatedCollections.Lookup( aAddress, out aCollection );
                return ret;
            }
        }

        protected virtual void OnAddedToCollection( SymSourceCollection aCollection )
        {
            ++iReferenceCount;
        }

        protected virtual void OnRemovedFromCollection( SymSourceCollection aCollection )
        {
            if ( --iReferenceCount <= 0 )
            {
                this.Dispose();
            }
        }

        protected virtual void OnPrepareForRelocation( SymbolCollection aCollection, uint aOldBase, uint aNewBase )
        {
            // If we read our data during priming, then we don't need to do anything... otherwise, we should
            // read the data now.
            if ( TimeToRead == TTimeToRead.EReadWhenNeeded )
            {
                this.Read( TSynchronicity.ESynchronous );
            }
        }
        #endregion

        #region API - framework
        public void ReportEvent( TEvent aEvent )
        {
            if ( aEvent == TEvent.EReadingComplete )
            {
                iAlwaysActivatedCollections.BuildLookupCache();
            }

            ReportEvent( aEvent, null );
        }

        public void ReportEvent( TEvent aEvent, object aData )
        {
            if ( EventHandler != null )
            {
                EventHandler( aEvent, this, aData );
            }
        }
        #endregion

        #region Event handlers
        private void Collection_RelocationStatusChanged( SymbolCollection aCollection )
        {
            CategoriseCollection( aCollection );
        }
        #endregion

        #region Properties
        public int Count
        {
            get
            {
                lock ( iCollectionsAll )
                {
                    return iCollectionsAll.Count;
                }
            }
        }

        public string URI
        {
            get
            {
                lock ( iURI )
                {
                    return iURI;
                }
            }
            set
            {
                lock ( iURI )
                {
                    iURI = value;
                }
            }

        }

        public string FileName
        {
            get { return iProvider.GetFileName( this ); }
        }

        public TTimeToRead TimeToRead
        {
            get { return iTimeToRead; }
            set
            {
                iTimeToRead = value;
            }
        }

        public SymSourceProvider Provider
        {
            get
            {
                lock ( iProvider )
                {
                    return iProvider;
                }
            }
        }

        public SymbolCollection this[ int aIndex ]
        {
            get 
            {
                lock ( iCollectionsAll )
                {
                    return iCollectionsAll[ aIndex ];
                }
            }
        }

        public virtual SymbolCollection this[ CodeSegDefinition aCodeSegment ]
        {
            get
            {
                lock ( iCollectionsAll )
                {
                    SymbolCollection ret = iCollectionsAll[ aCodeSegment ];
                    return ret;
                }
            }
        }

        public virtual SymbolCollection this[ PlatformFileName aFileName ]
        {
            get
            {
                lock ( iCollectionsAll )
                {
                    SymbolCollection ret = iCollectionsAll[ aFileName ];
                    return ret;
                }
            }
        }
        #endregion

        #region Internal methods
        internal int CountActivated
        {
            get
            {
                lock ( iAlwaysActivatedCollections )
                {
                    return iAlwaysActivatedCollections.Count;
                }
            }
        }

        internal void AddedToCollection( SymSourceCollection aCollection )
        {
            OnAddedToCollection( aCollection );
        }

        internal void RemovedFromCollection( SymSourceCollection aCollection )
        {
            OnRemovedFromCollection( aCollection );
        }

        private void CategoriseCollection( SymbolCollection aCollection )
        {
            // Reset state
            lock ( iAlwaysActivatedCollections )
            {
                iAlwaysActivatedCollections.Remove( aCollection );
                aCollection.IfaceRelocationHandler = null;
            }

            // Collections which do not move from their pre-determined base address
            // are transparently "activated" which means that they will be queried
            // automatically during symbolic look up.
            if ( aCollection.IsFixed )
            {
                lock ( iAlwaysActivatedCollections )
                {
                    iAlwaysActivatedCollections.Add( aCollection );
                }
            }
            else
            {
                aCollection.IfaceRelocationHandler = this;
            }
        }
        #endregion
    
        #region From ISymbolCollectionRelocationHandler
        public void PrepareForRelocation( SymbolCollection aCollection, uint aOldBase, uint aNewBase )
        {
            OnPrepareForRelocation( aCollection, aOldBase, aNewBase );
        }
        #endregion

        #region From IEnumerable<SymbolCollection>
        public IEnumerator<SymbolCollection> GetEnumerator()
        {
            lock ( iCollectionsAll )
            {
                foreach ( SymbolCollection col in iCollectionsAll )
                {
                    yield return col;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock ( iCollectionsAll )
            {
                foreach ( SymbolCollection col in iCollectionsAll )
                {
                    yield return col;
                }
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
                int count = iCollectionsAll.Count;
                for ( int i = count - 1; i >= 0; i-- )
                {
                    SymbolCollection col = iCollectionsAll[ i ];
                    Remove( col );
                    col.Dispose();
                }

                // These should both be empty in any case
                iCollectionsAll.Clear();
                iAlwaysActivatedCollections.Clear();
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return URI;
        }

        public override int GetHashCode()
        {
            return URI.GetHashCode();
        }

        public override bool Equals( object aObject )
        {
            if ( aObject is SymSource )
            {
                SymSource other = (SymSource) aObject;
                bool ret = ( string.Compare( other.URI, this.URI, StringComparison.CurrentCultureIgnoreCase ) == 0 );
                return ret;
            }
            //
            return base.Equals( aObject );
        }
        #endregion

        #region Data members
        private readonly SymSourceProvider iProvider;
        private string iURI = string.Empty;
        private int iReferenceCount = 0;
        private TTimeToRead iTimeToRead = TTimeToRead.EReadWhenPriming;
        private SymbolCollectionList iCollectionsAll = new SymbolCollectionList();
        private SymbolCollectionList iAlwaysActivatedCollections = new SymbolCollectionList();
        #endregion
    }
}
