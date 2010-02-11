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
using SymbianUtils.Threading;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Code;
using SymbianStructuresLib.Debug.Code.Interfaces;
using SymbianStructuresLib.Debug.Common.FileName;
using SymbianCodeLib.SourceManagement.Provisioning;

namespace SymbianCodeLib.SourceManagement.Source
{
    public abstract class CodeSource : DisposableObject, IEnumerable<CodeCollection>, ICodeCollectionRelocationHandler, ICodeCollectionInstructionConverter
    {
        #region Delegates & events
        public delegate void EventHandlerFunction( TEvent aEvent, CodeSource aSource, object aData );
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
        public CodeSource( string aURI, CodeSourceProvider aProvider )
        {
            iURI = aURI;
            iProvider = aProvider;
        }
        #endregion

        #region API
        public virtual void Read( TSynchronicity aSynchronicity )
        {
            bool isReading = this.IsReadInProgress;
            if ( isReading )
            {
                // Another thread is already reading this source.
                // If the caller asked for asynchronous reading, then when the
                // other thread completes the read the caller (in this thread) 
                // will be notified via the normal event callback framework - in which
                // case we need do nothing at all - just let the other thread get on with it.
                //
                // If, on the other hand, the caller requested a synchronous read, then they
                // will expect the code to be ready at the point in time which we return to them.
                //
                // In this situation, we should block this method until the code becomes ready.
                if ( aSynchronicity == TSynchronicity.ESynchronous )
                {
                    while ( this.IsReadInProgress )
                    {
                        System.Threading.Thread.Sleep( 0 );
                    }
                }
            }
            else
            {
                DoRead( aSynchronicity );
            }
        }

        public virtual void Add( CodeCollection aCollection )
        {
            // We can always do this task
            aCollection.IfaceInstructionConverter = this;

            // We want to be told if the collection changes it's relocation state.
            aCollection.RelocationStatusChanged += new CodeCollection.RelocationStatusChangeHandler( Collection_RelocationStatusChanged );

            lock ( iCollectionsAll )
            {
                iCollectionsAll.Add( aCollection );
            }

            CategoriseCollection( aCollection );
        }

        public virtual void Remove( CodeCollection aCollection )
        {
            aCollection.RelocationStatusChanged -= new CodeCollection.RelocationStatusChangeHandler( Collection_RelocationStatusChanged );
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
            return iAlwaysActivatedCollections.Contains( aAddress );
        }

        public virtual bool ProvideInstructions( uint aAddress, TArmInstructionSet aInstructionSet, int aCount, out IArmInstruction[] aInstructions )
        {
            lock ( iAlwaysActivatedCollections )
            {
                bool ret = iAlwaysActivatedCollections.GetInstructions( aAddress, aInstructionSet, aCount, out aInstructions );
                return ret;
            }
        }

        protected abstract void DoRead( TSynchronicity aSynchronicity );

        protected virtual void OnAddedToCollection( CodeSourceCollection aCollection )
        {
            ++iReferenceCount;
        }

        protected virtual void OnRemovedFromCollection( CodeSourceCollection aCollection )
        {
            if ( --iReferenceCount <= 0 )
            {
                this.Dispose();
            }
        }

        protected virtual void OnPrepareForRelocation( CodeCollection aCollection, uint aOldBase, uint aNewBase )
        {
            // If we read our data during priming, then we don't need to do anything... otherwise, we should
            // read the data now.
            //
            // However, don't initiate a read if we already have obtained code for the specified collection
            // (this can be the case if we are relocating a cloned code collection which has already been activated
            // itself in the past).
            if ( aCollection.IsCodeAvailable == false )
            {
                System.Diagnostics.Debug.Assert( this.IsReady == false );
                //
                if ( TimeToRead == TTimeToRead.EReadWhenNeeded )
                {
                    this.Read( TSynchronicity.ESynchronous );
                }
            }
        }

        protected virtual void OnReadComplete()
        {
        }
        #endregion

        #region API - framework
        public void ReportEvent( TEvent aEvent )
        {
            ReportEvent( aEvent, null );
        }

        public void ReportEvent( TEvent aEvent, object aData )
        {
            PreProcessEvent( aEvent );

            // Cascade
            if ( EventHandler != null )
            {
                EventHandler( aEvent, this, aData );
            }
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iCollectionsAll.Count; }
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

        public CodeSourceProvider Provider
        {
            get
            {
                lock ( iProvider )
                {
                    return iProvider;
                }
            }
        }

        public CodeCollection this[ int aIndex ]
        {
            get { return iCollectionsAll[ aIndex ]; }
        }

        public virtual CodeCollection this[ CodeSegDefinition aCodeSegment ]
        {
            get
            {
                CodeCollection ret = iCollectionsAll[ aCodeSegment ];
                return ret;
            }
        }

        public virtual CodeCollection this[ PlatformFileName aFileName ]
        {
            get
            {
                lock ( iCollectionsAll )
                {
                    CodeCollection ret = iCollectionsAll[ aFileName ];
                    return ret;
                }
            }
        }

        public bool IsReady
        {
            get
            {
                lock ( iFlagsSyncRoot )
                {
                    bool ret = ( iFlags & TFlags.EFlagsIsReady ) != 0;
                    return ret;
                }
            }
            set
            {
                lock ( iFlagsSyncRoot )
                {
                    if ( value )
                    {
                        iFlags |= TFlags.EFlagsIsReady;
                    }
                    else
                    {
                        iFlags &= ~TFlags.EFlagsIsReady;
                    }
                }
            }
        }

        public bool IsReadInProgress
        {
            get
            {
                lock ( iFlagsSyncRoot )
                {
                    bool ret = ( iFlags & TFlags.EFlagsIsReadInProgress ) != 0;
                    return ret;
                }
            }
            set
            {
                lock ( iFlagsSyncRoot )
                {
                    if ( value )
                    {
                        iFlags |= TFlags.EFlagsIsReadInProgress;
                    }
                    else
                    {
                        iFlags &= ~TFlags.EFlagsIsReadInProgress;
                    }
                }
            }
        }
        #endregion

        #region Event handlers
        private void Collection_RelocationStatusChanged( CodeCollection aCollection )
        {
            CategoriseCollection( aCollection );
        }
        #endregion

        #region Internal methods
        internal int CountActivated
        {
            get { return iAlwaysActivatedCollections.Count; }
        }

        internal void AddedToCollection( CodeSourceCollection aCollection )
        {
            OnAddedToCollection( aCollection );
        }

        internal void RemovedFromCollection( CodeSourceCollection aCollection )
        {
            OnRemovedFromCollection( aCollection );
        }

        private void CategoriseCollection( CodeCollection aCollection )
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
                // Otherwise, we must wait until the client relocates the code
                // and then we will be called back. During the callback processing
                // we'll load the code (if needed) and fixup the base address.
                aCollection.IfaceRelocationHandler = this;
            }
        }

        private void PreProcessEvent( TEvent aEvent )
        {
            switch ( aEvent )
            {
            case TEvent.EReadingStarted:
                IsReady = false;
                IsReadInProgress = true;
                break;
            case TEvent.EReadingComplete:
                IsReady = true;
                IsReadInProgress = false;
                OnReadComplete();
                break;
            }
        }
        #endregion

        #region Internal enumerations
        [Flags]
        private enum TFlags
        {
            EFlagsNone = 0,
            EFlagsIsReady = 1,
            EFlagsIsReadInProgress = 2
        }
        #endregion

        #region From ICodeCollectionRelocationHandler
        public void PrepareForRelocation( CodeCollection aCollection, uint aOldBase, uint aNewBase )
        {
            OnPrepareForRelocation( aCollection, aOldBase, aNewBase );
        }
        #endregion

        #region From ICodeCollectionInstructionConverter
        public IArmInstruction[] ConvertRawValuesToInstructions( TArmInstructionSet aInstructionSet, uint[] aRawValues, uint aStartingAddress )
        {
            IArmInstruction[] ret = iProvider.ConvertToInstructions( aInstructionSet, aRawValues, aStartingAddress );
            return ret;
        }
        #endregion

        #region From IEnumerable<CodeCollection>
        public IEnumerator<CodeCollection> GetEnumerator()
        {
            foreach ( CodeCollection col in iCollectionsAll )
            {
                yield return col;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( CodeCollection col in iCollectionsAll )
            {
                yield return col;
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
                    CodeCollection col = iCollectionsAll[ i ];
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
            if ( aObject is CodeSource )
            {
                CodeSource other = (CodeSource) aObject;
                bool ret = ( string.Compare( other.URI, this.URI, StringComparison.CurrentCultureIgnoreCase ) == 0 );
                return ret;
            }
            //
            return base.Equals( aObject );
        }
        #endregion

        #region Data members
        private readonly CodeSourceProvider iProvider;
        private string iURI = string.Empty;
        private int iReferenceCount = 0;
        private TTimeToRead iTimeToRead = TTimeToRead.EReadWhenPriming;
        private CodeCollectionList iCollectionsAll = new CodeCollectionList();
        private CodeCollectionList iAlwaysActivatedCollections = new CodeCollectionList();
        private object iFlagsSyncRoot = new object();
        private TFlags iFlags = TFlags.EFlagsNone;
        #endregion
    }
}
