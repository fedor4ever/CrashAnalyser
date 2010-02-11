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
using SymbianUtils.Tracer;

namespace SymbolLib.Generics
{
    public abstract class GenericSymbolEngine : IGenericSymbolCollectionLookupInterface, IGenericSymbolCollectionStatisticsInterface, IEnumerable<GenericSymbolCollection>, ITracer
    {
        #region Constructors
        protected internal GenericSymbolEngine( ITracer aTracer )
        {
            iTracer = aTracer;
        }
        #endregion

        #region Framework API
        public virtual void ClearTags()
        {
            lock ( this )
            {
                foreach ( GenericSymbolCollection collection in this )
                {
                    collection.ClearTag();
                }
            }
        }

        public virtual void SaveTaggedCollections( string aFileName )
        {
            using ( StreamWriter writer = new StreamWriter( aFileName, false ) )
            {
                lock ( this )
                {
                    foreach ( GenericSymbolCollection collection in this )
                    {
                        if ( collection.Tagged )
                        {
                            collection.WriteToStream( writer );
                        }
                    }
                }
            }
        }

        public abstract void Reset();

        public abstract bool IsReady { get; }

        public abstract bool IsLoaded( string aFileName );

        public abstract GenericSymbolCollection this[ int aIndex ]
        {
            get;
        }

        public abstract AddressRange Range { get; }

        internal abstract void UnloadUntagged();
        #endregion

        #region From IEnumerable<GenericSymbolCollection>
        IEnumerator<GenericSymbolCollection> IEnumerable<GenericSymbolCollection>.GetEnumerator()
        {
            return new GenericSymbolEngineEnumerator( this );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new GenericSymbolEngineEnumerator( this );
        }
        #endregion

        #region From IGenericSymbolCollectionStatisticsInterface
        public int NumberOfEntries
        {
            get
            {
                int count = 0;
                //
                lock ( this )
                {
                    foreach ( GenericSymbolCollection collection in this )
                    {
                        count += collection.Count;
                    }
                }
                // 
                return count;
            }
        }

        public abstract int NumberOfCollections
        {
            get;
        }
        #endregion

        #region From IGenericSymbolCollectionLookupInterface
        public GenericSymbolCollection[] CollectionsByHostBinarySearch( string aFileName )
        {
            string searchingFor = aFileName.ToUpper();
            //
            List<GenericSymbolCollection> ret = new List<GenericSymbolCollection>();
            //
            lock ( this )
            {
                for( int i=NumberOfCollections - 1; i>=0; i-- )
                {
                    GenericSymbolCollection collection = this[ i ];
                    string hostBinName = collection.HostBinaryFileName.ToUpper();
                    //
                    if ( hostBinName.Contains( searchingFor ) )
                    {
                        ret.Add( collection );
                    }
                }
            }
            //
            return ret.ToArray();
        }
        
        public GenericSymbol EntryByAddress( long aAddress )
        {
            GenericSymbolCollection notUsed = null;
            return EntryByAddress( aAddress, ref notUsed );
        }

        public GenericSymbol EntryByAddress( long aAddress, ref GenericSymbolCollection aCollection )
        {
            GenericSymbol symbol = null;
            aCollection = null;
            //
            lock ( this )
            {
                // Debugging code
                int debug = 0;
                if ( debug != 0 )
                {
                    int count = NumberOfCollections;
                    for ( int i = 0; i < count; i++ )
                    {
                        GenericSymbolCollection collection = this[ i ];
                        string line = string.Empty;
                        if ( collection.AddressFallsWithinRange( aAddress ) )
                        {
                            line = i.ToString( "d8" ) + " * [" + collection.AddressRangeStart.ToString( "x8" ) + " - " + collection.AddressRangeEnd.ToString( "x8" ) + "] " + " [" + ( collection.HostBinaryExists ? "Exists] " : "Notfnd] " ) + collection.HostBinaryFileName;
                        }
                        else
                        {
                            line = i.ToString( "d8" ) + "   [" + collection.AddressRangeStart.ToString( "x8" ) + " - " + collection.AddressRangeEnd.ToString( "x8" ) + "] " + " [" + ( collection.HostBinaryExists ? "Exists] " : "Notfnd] " ) + collection.HostBinaryFileName;
                        }
                        System.Diagnostics.Debug.WriteLine( line );
                    }
                }

                // Production search code
                if ( Range.Contains( aAddress ) )
                {
                    // Search for a suitable symbol entry that matches aAddress
                    int count = NumberOfCollections;
                    for ( int i = 0; i < count; i++ )
                    {
                        GenericSymbolCollection collection = this[ i ];
                        if ( collection.AddressFallsWithinRange( aAddress ) )
                        {
                            // Should be able to locate a symbol within this collection...
                            symbol = collection.SymbolForAddress( aAddress );

                            aCollection = collection;
                            aCollection.Tagged = true;
                            break;
                        }
                    }
                }
            }
            //
            return symbol;
        }

        public GenericSymbolCollection CollectionByAddress( long aAddress )
        {
            GenericSymbolCollection ret = null;

            lock ( this )
            {
                // Search for a suitable symbol entry that matches aAddress
                int count = NumberOfCollections;
                for ( int i = 0; i < count; i++ )
                {
                    GenericSymbolCollection collection = this[ i ];
                    if ( collection.AddressFallsWithinRange( aAddress ) )
                    {
                        ret = collection;
                        ret.Tagged = true;
                        break;
                    }
                }
            }
            //
            return ret;
        }
        #endregion

        #region ITracer Members
        public void Trace( string aMessage )
        {
            if ( iTracer != null )
            {
                iTracer.Trace( aMessage );
            }
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            Trace( string.Format( aFormat, aParams ) );
        }
        #endregion

        #region Data members
        private readonly ITracer iTracer;
        #endregion
    }
}
