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
using System.Text;
using System.Collections;
using System.Collections.Generic;
using SymbolLib.CodeSegDef;
using SymbolLib.Generics;
using SymbolLib.Sources.Map.Symbol;
using SymbianUtils.Range;

namespace SymbolLib.Sources.Map.File
{
	internal class MapFile : GenericSymbolCollection
    {
        #region Static constructors
        public static MapFile NewByHostMapFileName( string aMapFileNameAndPath )
        {
            // The specified file should end in .map
            System.Diagnostics.Debug.Assert( aMapFileNameAndPath.ToUpper().EndsWith( ".MAP" ) );

            // Get rid of map file extension.
            string binaryFileName = aMapFileNameAndPath.Substring( 0, aMapFileNameAndPath.Length - 4 );
            MapFile ret = new MapFile( binaryFileName, aMapFileNameAndPath );
            return ret;
        }

        #endregion

        #region Constructors
        private MapFile( string aHostBinaryName, string aHostMapFileName )
            : base( aHostBinaryName )
        {
            base.SourceFile = aHostMapFileName;

            // Make the dummy entry in case we obtain no further symbols
            MapSymbol dummy = MapSymbol.NewUnknown( this );
            iEntries.Add( dummy );
        }
        #endregion

        #region API
        public void EnsureAllEntriesHaveSize()
        {
            int count = Count;

#if DEBUG
            // Debug support
            int x = 0;
            if ( x != 0 )
            {
                string line = string.Empty;
                for ( int i = 0; i < count; i++ )
                {
                    GenericSymbol entry = iEntries[ i ];
                    line = i.ToString( "d8" ) + "   [" + entry.Address.ToString( "x8" ) + "-" + entry.EndAddress.ToString( "x8" ) + "] " + entry.Symbol.ToString();
                    System.Diagnostics.Debug.WriteLine( line );
                }
            }
#endif

            // Now go through the entries and update their sizes (if they don't have one)
            for ( int i = 1; i < count; i++ )
            {
                GenericSymbol entry = iEntries[ i ];
                GenericSymbol previousEntry = iEntries[ i - 1 ];
                //
                if  ( previousEntry.Size == 0 )
                {
                    long calculatedPreviousEntrySize = ( entry.Address - previousEntry.Address ) - 1;
                    if ( calculatedPreviousEntrySize > 0 )
                    {
                        previousEntry.Size = calculatedPreviousEntrySize;
                    }
                }
            }
        }

        public void Fixup( CodeSegDefinition aDefinition )
        {
            // Work out base address offset based upon global address and code segment 
            // placement address.
            uint baseAddress = aDefinition.AddressStart;
            BaseAddress = baseAddress;
            iFixedUpRange = aDefinition.AddressRange;

            base.RebuildAddressRange();
        }
        #endregion

        #region Properties
        public override AddressRange AddressRange
        {
            get { return iFixedUpRange; } 
        }

        internal uint GlobalBaseAddress
        {
            get { return iGlobalBaseAddress; }
            set
            {
                iGlobalBaseAddress = value;
            }
        }
		#endregion

        #region From GenericSymbolCollection
        public override void Add( GenericSymbolEngine aEngine, GenericSymbol aSymbol )
        {
            // We don't take data symbols, just code
            MapSymbol symbol = (MapSymbol) aSymbol;
            MapSymbol.TType type = symbol.Type;

            if ( type == MapSymbol.TType.EThumbCode || type == MapSymbol.TType.EARMCode )
            {
                // Don't take code entries with no size
                if ( aSymbol.Size > 0 )
                {
                    // Make sure we remove the null symbol if this is the first 'valid' symbol for
                    // the collection.
                    if ( Count == 1 && this[ 0 ].IsUnknownSymbol )
                    {
                        RemoveAt( 0 );
                    }

                    iEntries.Add( aSymbol );
                }
                else
                {
#if TRACE
                    System.Diagnostics.Debug.WriteLine( "Discarding zero size entry: " + aSymbol.ToString() );
#endif
                }
            }
        }

		public override void Remove( GenericSymbol aSymbol )
		{
			iEntries.Remove( aSymbol );
		}

		public override void RemoveAt( int aIndex )
		{
			iEntries.RemoveAt( aIndex );
		}

        public override void Sort()
        {
            iEntries.Sort( new GenericSymbolComparer() );
        }

        public override int Count
        {
            get { return iEntries.Count; }
        }

        public override System.Collections.IEnumerator CreateEnumerator()
        {
            return new MapFileEnumerator( this );
        }

        public override System.Collections.Generic.IEnumerator<GenericSymbol> CreateGenericEnumerator()
        {
            return new MapFileEnumerator( this );
        }

        public override GenericSymbol this[ int aIndex ]
        {
            get
            {
                return iEntries[ aIndex ];
            }
        }

        public override long AddressRangeStart
		{
            get { return iFixedUpRange.Min; }
		}

		public override long AddressRangeEnd
		{
            get { return iFixedUpRange.Max; }
        }

        public override GenericSymbol SymbolForAddress( long aAddress )
        {
#if DEBUG
            int x = 0;
            if ( x > 0 )
            {
                base.Dump( aAddress );
            }
#endif
            GenericSymbol ret = null;
            //
            foreach ( GenericSymbol symbol in this )
            {
                if ( symbol.FallsWithinDomain( aAddress ) )
                {
                    ret = symbol;
                    break;
                }
            }
            //
            return ret;
        }		
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.Append( iFixedUpRange.ToString() );
            return ret.ToString();
        }
        #endregion

        #region Data members
        private uint iGlobalBaseAddress = 0;
        private AddressRange iFixedUpRange = new AddressRange();
        private List<GenericSymbol> iEntries = new List<GenericSymbol>( 50 );
		#endregion
	}
}
