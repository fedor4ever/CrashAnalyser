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
using System.IO;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianUtils.FileTypes;
using SymbianStructuresLib.Debug.Symbols;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Common.FileName;
using SymbianSymbolLib.SourceManagement.Source;
using SymbianSymbolLib.Relocator;

namespace SymbianSymbolLib.QueryAPI
{
    internal class SymbolQueryAPI : IEnumerable<SymbolCollection>
    {
        #region Constructors
        internal SymbolQueryAPI( SymbolRelocator aRelocator )
		{
            iRelocator = aRelocator;
		}
		#endregion

        #region API
        public bool Contains( uint aAddress )
        {
            // First check with the relocated/activated symbol collections,
            // i.e. RAM-loaded code that has been fixed up.
            bool ret = iRelocator.CollectionList.Contains( aAddress );
            if ( ret == false )
            {
                // Wasn't a relocated symbol collection, so search through
                // all sources for ROM/XIP symbols that might match.
                foreach ( SymSource source in SourceManager )
                {
                    if ( source.Contains( aAddress ) )
                    {
                        ret = true;
                        break;
                    }
                }
            }
            //
            return ret;
        }

        public Symbol Lookup( uint aAddress, out SymbolCollection aCollection )
        {
            aCollection = null;

            // First check with the relocated/activated symbol collections,
            // i.e. RAM-loaded code that has been fixed up.
            Symbol ret = iRelocator.CollectionList.Lookup( aAddress, out aCollection );
            if ( ret == null && aCollection == null )
            {
                foreach ( SymSource source in SourceManager )
                {
                    if ( source.Contains( aAddress ) )
                    {
                        ret = source.Lookup( aAddress, out aCollection );
                        break;
                    }
                }
            }

            // Tag the collection because it provided a symbol
            if ( aCollection != null )
            {
                aCollection.Tagged = true;
            }

            return ret;
        }

        public SymbolCollection CollectionByAddress( uint aAddress )
        {
            SymbolCollection collection = null;
            Symbol symbol = Lookup( aAddress, out collection );
            return collection;
        }
        #endregion

		#region Properties
        public Symbol this[ uint aAddress ]
        {
            get
            {
                SymbolCollection collection = null;
                Symbol ret = Lookup( aAddress, out collection );
                return ret;
            }
        }

        public SymbolCollection this[ CodeSegDefinition aCodeSeg ]
        {
            get
            {
                SymbolCollection ret = null;
                SymSourceAndCollection pair = SourceManager[ aCodeSeg ];
                if ( pair != null )
                {
                    ret = pair.Collection;
                }
                return ret;
            }
        }

        public SymbolCollection this[ PlatformFileName aFileName ]
        {
            get
            {
                SymbolCollection ret = null;
                //
                foreach ( SymSource source in SourceManager )
                {
                    SymbolCollection col = source[ aFileName ];
                    if ( col != null )
                    {
                        ret = col;
                        break;
                    }
                }
                //
                return ret;
            }
        }
		#endregion

        #region Internal methods
        internal SymSourceManager SourceManager
        {
            get { return iRelocator.SourceManager; }
        }
        #endregion

        #region Internal enumerator
        #endregion

        #region From IEnumerable<SymbolCollection>
        public IEnumerator<SymbolCollection> GetEnumerator()
        {
            // This gives us explicit activations
            SymbolCollectionList list = iRelocator.CollectionList;
            foreach ( SymbolCollection col in list )
            {
                yield return col;
            }

            // Next we need fixed collections
            IEnumerable<SymbolCollection> fixedCols = iRelocator.SourceManager.GetFixedCollectionEnumerator();
            foreach ( SymbolCollection col in fixedCols )
            {
                if ( col.IsFixed )
                {
                    yield return col;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            // This gives us explicit activations
            SymbolCollectionList list = iRelocator.CollectionList;
            foreach ( SymbolCollection col in list )
            {
                yield return col;
            }

            // Next we need fixed collections
            IEnumerable<SymbolCollection> fixedCols = iRelocator.SourceManager.GetFixedCollectionEnumerator();
            foreach ( SymbolCollection col in fixedCols )
            {
                if ( col.IsFixed )
                {
                    yield return col;
                }
            }
        }
        #endregion

        #region Data members
        private readonly SymbolRelocator iRelocator;
        #endregion
    }
}
