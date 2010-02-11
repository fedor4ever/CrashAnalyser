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
using System.Collections;
using System.Collections.Generic;
using SymbolLib.Sources.Symbol.File;
using SymbolLib.Sources.Symbol.Comparison;

namespace SymbolLib.Sources.Symbol.Collection
{
    public class SymbolsForBinaryCollection : IEnumerable<SymbolsForBinary>
    {
        #region Constructors & destructor
        public SymbolsForBinaryCollection()
            : this( 50 )
        {
        }

        public SymbolsForBinaryCollection( int aGranularity )
        {
            iSymbolsForBinaries = new List<SymbolsForBinary>( aGranularity );
        }
        #endregion

        #region API
        public void Clear()
        {
            lock ( iSymbolsForBinaries )
            {
                iSymbolsForBinaries.Clear();
            }
        }

        public void Add( SymbolsForBinary aEntry )
        {
            lock ( iSymbolsForBinaries )
            {
                if ( !iSymbolsForBinaries.Contains( aEntry ) )
                {
                    iSymbolsForBinaries.Add( aEntry );
                }
            }
        }

        public void Add( SymbolsForBinaryCollection aCollection )
        {
            foreach ( SymbolsForBinary entry in aCollection )
            {
                Add( entry );
            }
        }

        public void Remove( SymbolsForBinary aFile )
        {
            iSymbolsForBinaries.Remove( aFile );
        }

        public void RemoveLast()
        {
            // Dump the last collection - only ever called when
            // the parser detects a collection with zero entries
            lock ( iSymbolsForBinaries )
            {
                if ( Count == 0 )
                {
                    throw new ArgumentOutOfRangeException( "No last entry to remove" );
                }

                iSymbolsForBinaries.RemoveAt( Count - 1 );
            }
        }

        public void Sort()
        {
            // Sort the collections into some kind of address order
            if ( Count > 0 )
            {
                SymbolsForBinaryCompareByAddress comparer = new SymbolsForBinaryCompareByAddress();
                try
                {
                    lock ( iSymbolsForBinaries )
                    {
                        iSymbolsForBinaries.Sort( comparer );
                    }
                }
                catch ( Exception )
                {
                    SymbianUtils.SymDebug.SymDebugger.Break();
                }
            }
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iSymbolsForBinaries.Count; }
        }

        public SymbolsForBinary this[ int aIndex ]
        {
            get 
            {
                lock ( iSymbolsForBinaries )
                {
                    return iSymbolsForBinaries[ aIndex ];
                }
            }
        }

        public SymbolsForBinary LastCollection
        {
            get
            {
                lock ( iSymbolsForBinaries )
                {
                    SymbolsForBinary ret = this[ Count - 1 ];
                    return ret;
                }
            }
        }
        #endregion

        #region From IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SymbolsForBinaryCollectionEnumerator( this );
        }

        IEnumerator<SymbolsForBinary> IEnumerable<SymbolsForBinary>.GetEnumerator()
        {
            return new SymbolsForBinaryCollectionEnumerator( this );
        }
        #endregion

        #region Data members
        private readonly List<SymbolsForBinary> iSymbolsForBinaries;
        #endregion
    }
}
