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
using System.Collections;
using System.Collections.Generic;
using SymbolLib.CodeSegDef;
using SymbolLib.Engines.Common;
using SymbolLib.Sources.Map.Engine;
using SymbolLib.Sources.Map.Parser;
using SymbolLib.Sources.Map.File;
using SymbolLib.Sources.Symbol.Engine;
using SymbolLib.Sources.Symbol.Collection;
using SymbolLib.Sources.Symbol.File;
using SymbolLib.Generics;
using SymbianUtils;
using SymbianUtils.Range;
using SymbianUtils.Tracer;

namespace SymbolLib.Engines.ROFS
{
	public class ROFSEngine : SymbolEngineBase
	{
		#region Constructors
		internal ROFSEngine( ITracer aTracer )
            : base( aTracer )
		{
            iEngineMap = new MapFileEngineCollection( this );
            iEngineMap.Observer += new SymbianUtils.AsyncReaderBase.Observer( MapEngine_Observer );
        
            iEngineSymbol = new SymbolFileEngineCollection( this, SymbolFileEngine.TActivationType.EOnDemand, true );
			iEngineSymbol.Observer += new SymbianUtils.AsyncReaderBase.Observer( SymbolEngine_Observer );
		}
		#endregion

        #region API
        public static TFileType IsSupported( string aFileName )
        {
            TFileType ret = TFileType.EFileNotSupported;
            //
            try
            {
                string extension = Path.GetExtension( aFileName ).ToLower();
                if ( extension == ".symbol" )
                {
                    bool sawSomeValidContent = false;
                    //
                    SymbolFileEngine tempEngine = new SymbolFileEngine(null,  SymbolFileEngine.TActivationType.EOnDemand, true );
                    SymbolsForBinary symbols = tempEngine.ReadFirstCollection( aFileName, out sawSomeValidContent );

                    // For a valid ROFS symbol file, the first symbol should have an address of zero.
                    bool valid = ( symbols != null && symbols.Count > 0 && symbols[ 0 ].Address == 0 );
                    if ( valid )
                    {
                        ret = TFileType.EFileRofsSymbol;
                    }
                    else if ( sawSomeValidContent )
                    {
                        // Probably just a file containing data files rather than code, but that's okay.
                        ret = TFileType.EFileRofsSymbol;
                    }
                }
                else if ( extension == ".map" )
                {
                    ret = MapFileEngineCollection.IsSupported( aFileName );
                }
            }
            catch(Exception)
            {
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        public CodeSegDefinitionParser DefinitionParser
        {
            get { return iEngineMap.CodeSegParser; }
        }

        public CodeSegResolver DefinitionResolver
        {
            get { return iEngineMap.CodeSegResolver; }
        }

        public SymbolFileEngineCollection SymbolFiles
        {
            get { return iEngineSymbol; }
        }

        public string[] SymbolFileNames
        {
            get
            {
                string[] ret = iEngineSymbol.SymbolFileNames;
                return ret;
            }
        }

        public string[] SymbolFileCollectionFileNames
		{
			get
			{
                string[] ret = iEngineSymbol.BinaryFileNames;
                return ret;
			}
		}

        public List<string> MapFileNames
        {
            get
            {
                List<string> ret = new List<string>();
                lock ( iEngineMap )
                {
                    ret.AddRange( iEngineMap.MapFileNames );
                }
                return ret;
            }
        }
        #endregion

        #region From SymbolEngineBase
        public override bool AddressInRange( long aAddress )
        {
            // Try map files first, then symbols (should be
            // quicker this way).
            bool ret = false;
            lock ( iEngineMap )
            {
                ret = iEngineMap.Range.Contains( aAddress );
            }
            if ( ret == false )
            {
                lock ( iEngineSymbol )
                {
                    ret = iEngineSymbol.Range.Contains( aAddress );
                }
            }
            //
            return ret;
        }

        public override int FileNameCount
        {
            get
            {
				int count = 0;
				//
                lock ( iEngineSymbol )
                {
                    count += iEngineSymbol.SymbolFileCount;
                }
                lock ( iEngineMap )
                {
                    count += iEngineMap.NumberOfCollections;
                }
				//
                return count;
            }
        }

        public override string FileName( int aIndex )
        {
            string ret = string.Empty;
            //
            lock ( iEngineMap )
            {
                lock ( iEngineSymbol )
                {
                    int countSymbolFiles = iEngineSymbol.SymbolFileCount;
                    int countMapFiles = iEngineMap.NumberOfCollections;
                    //
                    if ( aIndex < countSymbolFiles )
                    {
                        ret = iEngineSymbol.SymbolFileName( aIndex );
                    }
                    else
                    {
                        // Must be a map index
                        aIndex -= countSymbolFiles;
                        //
                        if ( aIndex >= 0 && aIndex < iEngineMap.NumberOfCollections )
                        {
                            ret = iEngineMap.MapFileName( aIndex );
                        }
                    }
                }
            }
            //            
            return ret;
        }

        public override void LoadFromFile( string aFileName, TSynchronicity aSynchronicity )
        {
            if ( SymbolFileEngine.IsSymbolFile( aFileName ) )
            {
                iEngineSymbol.LoadFromFile( aFileName, aSynchronicity );
            }
            else if ( MapFileEngine.IsMapFile( aFileName ) )
            {
                iEngineMap.LoadFromFile( aFileName, aSynchronicity );
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public override bool LoadFromDefinition( CodeSegDefinition aDefinition, TSynchronicity aSynchronicity )
        {
            // Try Symbol file first
            bool wasActivated = false;

            lock ( this )
            {
                wasActivated = iEngineSymbol.Load( aDefinition );
                if ( wasActivated == false )
                {
                    wasActivated = iEngineMap.Load( aDefinition, aSynchronicity );
                }
            }

            return wasActivated;
        }

        public override void UnloadAll()
        {
            lock ( iEngineMap )
            {
                iEngineMap.UnloadAll();
            }
            lock ( iEngineSymbol )
            {
                iEngineSymbol.UnloadAll();
            }
        }

        public override bool Unload( CodeSegDefinition aDefinition )
        {
            bool ret = false;
            lock ( this )
            {
                ret = iEngineMap.Unload( aDefinition );

                // Try with the Symbol engine if it wasn't a loaded map codeseg
                if ( ret == false )
                {
                    ret = iEngineSymbol.Unload( aDefinition );
                }
            }

            return ret;
        }

        public override bool IsLoaded( CodeSegDefinition aDefinition )
        {
            bool ret = false;
            lock ( iEngineMap )
            {
                ret = iEngineMap.IsLoaded( aDefinition );
            }
            if ( ret == false )
            {
                // Try with the Symbol engine if it wasn't a loaded map codeseg
                lock( iEngineSymbol )
                {
                    ret = iEngineSymbol.IsLoaded( aDefinition );
                }
            }

            return ret;
        }
        #endregion

        #region From GenericSymbolEngine
        public override bool IsReady
        {
            get
            {
                bool mapReady = false;
                lock( iEngineMap )
                {
                    mapReady = iEngineMap.IsReady;
                }
                //
                bool symbolReady = false;
                lock ( iEngineSymbol )
                {
                    symbolReady = iEngineSymbol.IsReady;
                }
                //
                bool ret = ( mapReady && symbolReady );
                return ret;
            }
        }

        public override void Reset()
        {
            lock ( iEngineMap )
            {
                iEngineMap.Reset();
            }
            lock( iEngineSymbol )
            {
                iEngineSymbol.Reset();
            }
        }

        public override bool IsLoaded( string aFileName )
        {
            bool loaded = false;
            lock ( iEngineMap )
            {
                loaded = iEngineMap.IsLoaded( aFileName );
            }
            if ( loaded == false )
            {
                lock ( iEngineSymbol )
                {
                    loaded = iEngineSymbol.IsLoaded( aFileName );
                }
            }
            //
            return loaded;
        }

        public override GenericSymbolCollection this[ int aIndex ]
        {
            get
            {
                GenericSymbolCollection ret = null;
                //
                lock ( iEngineMap )
                {
                    lock ( iEngineSymbol )
                    {
                        int mapCount = iEngineMap.NumberOfCollections;
                        if ( aIndex >= 0 && aIndex < mapCount )
                        {
                            ret = iEngineMap[ aIndex ];
                        }
                        else
                        {
                            int symbolBaseIndex = ( aIndex - mapCount );
                            int symbolCount = iEngineSymbol.NumberOfCollections;
                            if ( symbolBaseIndex >= 0 && symbolBaseIndex < symbolCount )
                            {
                                ret = iEngineSymbol[ symbolBaseIndex ];
                            }
                        }
                    }
                }
                //
                return ret;
            }
        }

        public override AddressRange Range
        {
            get
            {
                AddressRange ret = new AddressRange();
                //
                lock ( iEngineMap )
                {
                    ret.Update( iEngineMap.Range );
                }
                lock ( iEngineSymbol )
                {
                    ret.Update( iEngineSymbol.Range );
                }
                //
                return ret;
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
                int count = 0;
                //
                lock ( iEngineMap )
                {
                    count += iEngineMap.NumberOfCollections;
                }
                lock ( iEngineSymbol )
                {
                    count += iEngineSymbol.NumberOfCollections;
                }
                //
                return count;
            }
        }
        #endregion
        
        #region Event handlers
        private void MapEngine_Observer( SymbianUtils.AsyncReaderBase.TEvent aEvent, SymbianUtils.AsyncReaderBase aSender )
        {
            System.Diagnostics.Debug.Assert( aSender.Tag is MapFileEngine );
            MapFileEngine engine = (MapFileEngine) aSender.Tag;
            //
            switch( aEvent )
            {
            case SymbianUtils.AsyncReaderBase.TEvent.EReadingStarted:
                OnParsingStarted( engine.MapFileName );
                break;
            case SymbianUtils.AsyncReaderBase.TEvent.EReadingProgress:
                OnParsingProgress( engine.MapFileName, aSender.Progress );
                break;
            case SymbianUtils.AsyncReaderBase.TEvent.EReadingComplete:
                OnParsingCompleted( engine.MapFileName );
                break;
            }
        }

        private void MapEngine_MapLoaded( CodeSegDefinition aLoadedEntry, MapFile aMapFile )
        {
            OnCollectionCreated( aMapFile );
        }

        private void SymbolEngine_Observer( SymbianUtils.AsyncReaderBase.TEvent aEvent, SymbianUtils.AsyncReaderBase aSender )
        {
            System.Diagnostics.Debug.Assert( aSender.Tag is SymbolFileEngine );
            SymbolFileEngine engine = (SymbolFileEngine) aSender.Tag;
            //
            switch( aEvent )
            {
            case SymbianUtils.AsyncReaderBase.TEvent.EReadingStarted:
                OnParsingStarted( engine.SymbolFileName );
                break;
            case SymbianUtils.AsyncReaderBase.TEvent.EReadingProgress:
                OnParsingProgress( engine.SymbolFileName, aSender.Progress );
                break;
            case SymbianUtils.AsyncReaderBase.TEvent.EReadingComplete:
                OnParsingCompleted( engine.SymbolFileName );
                break;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly MapFileEngineCollection iEngineMap;
        private readonly SymbolFileEngineCollection iEngineSymbol;
        #endregion
    }
}
