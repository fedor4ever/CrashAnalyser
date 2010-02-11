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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using SymbolLib.CodeSegDef;
using SymbolLib.Generics;
using SymbolLib.Engines;
using SymbolLib.Sources.Map.File;
using SymbolLib.Sources.Map.Parser;
using SymbolLib.Sources.Map.Symbol;
using SymbianUtils;
using SymbianUtils.Range;
using SymbianUtils.Tracer;

namespace SymbolLib.Sources.Map.Engine
{
	internal class MapFileEngine : GenericSymbolEngine, SymbolEntryCreator
    {
		#region Events
        public event AsyncReaderBase.Observer Observer;
        #endregion

        #region Constructor & destructor
        public MapFileEngine( ITracer aTracer )
            : base( aTracer )
		{
		}
		#endregion

		#region API
        public static bool IsMapFile( string aFileName )
        {
            string extension = Path.GetExtension( aFileName ).ToLower();
            return ( extension == CodeSegDefinition.KMapFileExtension );
        }

        internal bool IsLoaded( CodeSegDefinition aDefinition )
        {
            bool ret = iMapFile.HostBinaryFileName == aDefinition.ImageFileNameAndPath;
            return ret;
        }

        internal void LoadFromFile( string aMapFileName, TSynchronicity aSynchronicity )
        {
            iMapFileName = aMapFileName;
            iMapFile = MapFile.NewByHostMapFileName( aMapFileName );
            //
            iParser = new MapFileParser( this, aMapFileName, this );
            iParser.Tag = this;
            iParser.iObserver += new SymbianUtils.AsyncReaderBase.Observer( Parser_Observer );
            iParser.SymbolCreated += new MapFileParser.SymbolCreatedHandler( Parser_SymbolCreated );
            iParser.BaseAddressHandler += new MapFileParser.MapFileBaseAddressHandler( Parser_BaseAddressHandler );
            iParser.Read( aSynchronicity );
        }

        internal void Load( CodeSegDefinition aDefinition )
        {
            iMapFile.Fixup( aDefinition );
        }

        internal bool Unload( CodeSegDefinition aDefinition )
        {
            return true;
        }

        internal void UnloadAll()
        {
        }
		#endregion

        #region Properties
        public MapFile MapFile
        {
            get { return iMapFile; }
        }

        public string MapFileName
        {
            get { return iMapFileName; }
        }
        #endregion

        #region From GenericSymbolEngine
        public override void Reset()
		{
            // Do nothing - we cannot unload our only file. This is taken
            // care of by parent class.
            iMapFile = null;
            iParser = null;
            iMapFileName = string.Empty;
        }

        public override bool IsReady
        {
            get
            {
                return true;
            }
        }

        public override bool IsLoaded( string aFileName )
        {
            bool ret = MapFileName == aFileName;
            return ret;
        }

        public override GenericSymbolCollection this[ int aIndex ]
        {
            get
            {
                return iMapFile;
            }
        }

        public override AddressRange Range
        {
            get
            {
                return iMapFile.AddressRange;
            }
        }

        internal override void UnloadUntagged()
        {

        }
        #endregion

        #region From IGenericSymbolCollectionStatisticsInterface
        public override int NumberOfCollections
        {
            get
			{
				return 1;
			}
        }
        #endregion

        #region From SymbolEntryCreator
        public MapSymbol CreateSymbol()
        {
            return MapSymbol.New( iMapFile );
        }
        #endregion

		#region Map file parser observer
        private void Parser_Observer( SymbianUtils.AsyncReaderBase.TEvent aEvent, SymbianUtils.AsyncReaderBase aSender )
        {
            if ( Observer != null )
            {
                Observer( aEvent, aSender );
            }

            if ( aEvent == SymbianUtils.AsyncReaderBase.TEvent.EReadingComplete )
            {
                iMapFile.EnsureAllEntriesHaveSize();
                iMapFile.Sort();

                int x = 0;
                if ( x != 0 )
                {
                    iMapFile.Dump();
                }

                iParser = null;
            }
        }

        private void Parser_SymbolCreated( MapSymbol aSymbol )
        {
            bool addItem = true;
            
            if ( iMapFile.Count > 0 )
            {
                int lastEntryIndex = iMapFile.Count - 1;
                MapSymbol lastEntry = (MapSymbol) iMapFile[ lastEntryIndex ];
                //
                long lastEntrySize = lastEntry.Size;
                long newEntrySize = aSymbol.Size;
                //
                if ( lastEntry.Address == aSymbol.Address )
                {
                    if ( lastEntrySize == 0 && newEntrySize > 0 )
                    {
                        // Remove an entry with zero size to replace it with a better definition...
                        iMapFile.RemoveAt( lastEntryIndex );
                    }
                    else if ( newEntrySize == 0 && lastEntrySize > 0 )
                    {
                        // Don't replace an entry with zero size when we already
                        // have a good item.
                        addItem = false;
                    }
                }
            }
         
            if ( addItem )
            {
                iMapFile.Add( this, aSymbol );
            }
        }

        private void Parser_BaseAddressHandler( uint aBaseAddress )
        {
            iMapFile.GlobalBaseAddress = aBaseAddress;
        }
        #endregion

		#region Internal methods
		#endregion

        #region Data members
        private MapFile iMapFile = null;
        private MapFileParser iParser = null;
        private string iMapFileName = string.Empty;
        #endregion
    }
}
