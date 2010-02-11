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

namespace SymbianCodeLib.SourceManagement.Source
{
    public class CodeSourceCollection : DisposableObject, IEnumerable<CodeSource>
    {
        #region Constructors
        public CodeSourceCollection()
        {
        }

        public CodeSourceCollection( CodeSource aSource )
        {
            Add( aSource );
        }
        #endregion

        #region API
        public void Clear()
        {
            lock ( iSources )
            {
                IList<CodeSource> list = iSources.Values;
                int count = list.Count;
                for ( int i = count - 1; i >= 0; i-- )
                {
                    CodeSource source = list[ i ];
                    Remove( source );
                }
            }
        }

        public bool Add( CodeSource aSource )
        {
            bool added = false;
            //
            lock ( iSources )
            {
                if ( Contains( aSource ) == false )
                {
                    string uri = aSource.URI;
                    iSources.Add( uri, aSource );
                    added = true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine( "**** WARNING **** discarding duplicate source: " + aSource );
                }
            }
            //
            if ( added )
            {
                OnAdded( aSource );
            }
            //
            return added;
        }

        public void AddRange( IEnumerable<CodeSource> aSources )
        {
            foreach ( CodeSource source in aSources )
            {
                Add( source );
            }
        }

        public bool Remove( CodeSource aSource )
        {
            bool ret = false;
            CodeSource source = null;
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

        public bool Contains( CodeSource aSource )
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

        public CodeSource this[ int aIndex ]
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

        #region From IEnumerable<CodeSource>
        public IEnumerator<CodeSource> GetEnumerator()
        {
            foreach ( KeyValuePair<string, CodeSource> kvp in iSources )
            {
                yield return kvp.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( KeyValuePair<string, CodeSource> kvp in iSources )
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
                foreach ( KeyValuePair<string, CodeSource> kvp in iSources )
                {
                    CodeSource source = kvp.Value;
                    source.Dispose();
                }

                Clear();
            }
        }
        #endregion

        #region Internal methods
        protected virtual void OnAdded( CodeSource aSource )
        {
            aSource.AddedToCollection( this );
        }
    
        protected virtual void OnRemoved( CodeSource aSource )
        {
            aSource.RemovedFromCollection( this );
        }
        #endregion

        #region Data members
        private SortedList<string, CodeSource> iSources = new SortedList<string, CodeSource>();
        #endregion
    }
}