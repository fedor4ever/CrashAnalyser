/*
* Copyright (c) 2004-2005 Nokia Corporation and/or its subsidiary(-ies).
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
using SymbianUtils.Collections;

namespace SymbianSymbolLib.SourceManagement.Source
{
    public class SymSourceCollection : DisposableObject, IEnumerable<SymSource>
    {
        #region Constructors
        public SymSourceCollection()
        {
        }

        public SymSourceCollection( SymSource aSource )
        {
            Add( aSource );
        }
        #endregion

        #region API
        public void Clear()
        {
            lock ( iSources )
            {
                IList<SymSource> list = iSources.Values;
                int count = list.Count;
                for ( int i = count - 1; i >= 0; i-- )
                {
                    SymSource source = list[ i ];
                    Remove( source );
                }
            }
        }

        public void Add( SymSource aSource )
        {
            bool added = false;
            //
            lock ( iSources )
            {
                if ( !Contains( aSource ) )
                {
                    string uri = aSource.URI;
                    iSources.Add( uri, aSource );
                    added = true;
                }
            }
            //
            if ( added )
            {
                OnAdded( aSource );
            }
        }

        public void AddRange( IEnumerable<SymSource> aSources )
        {
            foreach ( SymSource source in aSources )
            {
                Add( source );
            }
        }

        public bool Remove( SymSource aSource )
        {
            bool ret = false;
            SymSource source = null;
            //
            lock( iSources )
            {
                string uri = aSource.URI;
                //
                if ( iSources.TryGetValue( uri, out source ) )
                {
                    ret = iSources.Remove( uri );
                }
            }

            // Notify outside of the lock
            if ( source != null )
            {
                OnRemoved( source );
            }
            //
            return ret;
        }

        public bool Contains( string aURI )
        {
            lock ( iSources )
            {
                return iSources.ContainsKey( aURI );
            }
        }

        public bool Contains( SymSource aSource )
        {
            return Contains( aSource.URI );
        }
        #endregion

        #region Properties
        public int Count
        {
            get
            {
                lock ( iSources )
                {
                    return iSources.Count;
                }
            }
        }

        public bool IsEmpty
        {
            get { return Count == 0; } 
        }

        public SymSource this[ int aIndex ]
        {
            get
            {
                lock ( iSources )
                {
                    string key = iSources.Keys[ aIndex ];
                    return iSources[ key ];
                }
            }
        }
        #endregion

        #region Internal methods
        protected virtual void OnAdded( SymSource aSource )
        {
            aSource.AddedToCollection( this );
        }
    
        protected virtual void OnRemoved( SymSource aSource )
        {
            aSource.RemovedFromCollection( this );
        }
        #endregion

        #region From IEnumerable<SymSource>
        public IEnumerator<SymSource> GetEnumerator()
        {
            foreach ( KeyValuePair<string, SymSource> kvp in iSources )
            {
                yield return kvp.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( KeyValuePair<string, SymSource> kvp in iSources )
            {
                yield return kvp.Value;
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
                foreach ( KeyValuePair<string, SymSource> kvp in iSources )
                {
                    SymSource source = kvp.Value;
                    source.Dispose();
                }
                iSources.Clear();
            }
        }
        #endregion

        #region Data members
        private SortedList<string, SymSource> iSources = new SortedList<string, SymSource>();
        #endregion
    }
}