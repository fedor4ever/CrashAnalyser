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
﻿using System;
using System.Collections.Generic;
using System.Text;
using CrashItemLib.Engine;
using CrashItemLib.Crash.InfoSW;

namespace CrashItemLib.Crash.Container
{
    sealed internal class CIContainerIndex : IEnumerable< KeyValuePair<uint, CIContainerCollection> >
    {
        #region Constructors
        public CIContainerIndex( CIEngine aEngine )
        {
            aEngine.CrashObservers += new CIEngine.CIEngineCrashObserver( Engine_CrashObserver );
        }
        #endregion

        #region API
        public CIContainerCollection DequeueNextContainer()
        {
            CIContainerCollection ret = null;
            //
            lock ( iDictionary )
            {
                Dictionary<uint, CIContainerCollection>.Enumerator enumerator = iDictionary.GetEnumerator();
                bool next = enumerator.MoveNext();
                if ( next )
                {
                    ret = enumerator.Current.Value;
                    iDictionary.Remove( enumerator.Current.Key );
                }
            }
            //
            return ret;
        }

        public static uint GetRomChecksum( CIContainer aContainer )
        {
            uint ret = 0;

            // Find the infosw item in order to obtain the image checksum
            CIInfoSW infoSW = aContainer.ChildByType( typeof( CIInfoSW ) ) as CIInfoSW;
            if ( infoSW != null )
            {
                ret = infoSW.ImageCheckSum;
            }

            return ret;
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iDictionary.Count; }
        }
        #endregion

        #region Event handlers
        private void Engine_CrashObserver( CIEngine.TCrashEvent aEvent, CIContainer aContainer )
        {
            if ( aEvent == CIEngine.TCrashEvent.EEventCrashRemovedAll )
            {
                lock ( iDictionary )
                {
                    iDictionary.Clear();
                }
            }
            else if ( aEvent == CIEngine.TCrashEvent.EEventCrashRemoved )
            {
                RemoveFromDictionary( aContainer );
            }
            else if ( aEvent == CIEngine.TCrashEvent.EEventCrashAdded )
            {
                AddToDictionary( aContainer );
            }
        }
        #endregion

        #region Internal methods
        private void RemoveFromDictionary( CIContainer aContainer )
        {
            uint checksum = GetRomChecksum( aContainer );
            CIContainerCollection collection = null;
            //
            lock ( iDictionary )
            {
                if ( iDictionary.TryGetValue( checksum, out collection ) )
                {
                    collection.Remove( aContainer );

                    // If the collection is empty, remove the mapping also
                    if ( collection.Count == 0 )
                    {
                        iDictionary.Remove( checksum );
                    }
                }
            }
        }

        private void AddToDictionary( CIContainer aContainer )
        {
            uint checksum = GetRomChecksum( aContainer );

            // Check if there is a collection for this key already
            CIContainerCollection collection = null;
            lock ( iDictionary )
            {
                if ( iDictionary.TryGetValue( checksum, out collection ) == false )
                {
                    // Nope, collection not yet registers
                    collection = new CIContainerCollection();
                    iDictionary.Add( checksum, collection );
                }
            }

            // Now save
            collection.Add( aContainer );
        }
        #endregion
            
        #region From IEnumerable< KeyValuePair<uint, CIContainerCollection> >
        public IEnumerator<KeyValuePair<uint,CIContainerCollection>> GetEnumerator()
        {
            foreach( KeyValuePair<uint,CIContainerCollection> kvp in iDictionary )
            {
                yield return kvp;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach( KeyValuePair<uint,CIContainerCollection> kvp in iDictionary )
            {
                yield return kvp;
            }
        }
        #endregion

        #region Data members
        private Dictionary<uint, CIContainerCollection> iDictionary = new Dictionary<uint, CIContainerCollection>();
        #endregion
    }
}
