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
using System.Collections;
using System.Collections.Generic;
using SymbianUtils;
using SymbianUtils.Range;
using SymbianUtils.Tracer;
using SymbolLib.CodeSegDef;
using SymbolLib.Generics;
using SymbolLib.Engines;
using SymbolLib.Sources.Map.Symbol;
using SymbolLib.Sources.Map.File;
using SymbolLib.Sources.Map.Parser;

namespace SymbolLib.Sources.Map.Engine
{
    internal class MapFileEngineCollection : GenericSymbolEngine
    {
        #region Delegates & Events
        public event AsyncReaderBase.Observer Observer;
        #endregion

        #region Constructors & destructor
        internal MapFileEngineCollection( ITracer aTracer )
            : base( aTracer )
        {
            iCodeSegResolver = new CodeSegResolver( aTracer );
            iCodeSegParser = new CodeSegDefinitionParser( iCodeSegResolver );
        }
        #endregion

        #region API
        public static TFileType IsSupported( string aFileName )
        {
            TFileType ret = TFileType.EFileNotSupported;
            //
            try
            {
                using ( StreamReader reader = new StreamReader( aFileName ) )
                {
                    const int maxLines = 100;
                    //
                    bool foundHeader = false;
                    bool foundBody = false;
                    //
                    int lineCounter = 0;
                    string line = reader.ReadLine();
                    while ( line != null && !( foundHeader && foundBody ) && lineCounter < maxLines )
                    {
                        if ( line.StartsWith( "ARM Linker, RVCT" ) )
                        {
                            foundHeader = true;
                        }
                        else if ( line.Contains( "Symbol Name" ) && line.Contains( "Value" ) && line.Contains( "Object" ) && line.Contains( "Section" ) )
                        {
                            foundBody = true;
                        }

                        line = reader.ReadLine();
                        ++lineCounter;
                    }
                    //
                    if ( foundBody && foundHeader )
                    {
                        ret = TFileType.EFileMap;
                    }
                }
            }
            catch ( Exception )
            {
            }
            //
            return ret;
        }

        public MapFileEngine LoadFromFile( string aMapFileName, TSynchronicity aSynchronicity )
        {
            // Check if already exists
            MapFileEngine engine = null;
            //
            lock ( this )
            {
                engine = FindByMapFileName( aMapFileName );
                if ( engine != null )
                {
                    iFiles.Remove( engine );
                }

                engine = new MapFileEngine( this );
                engine.Observer += new AsyncReaderBase.Observer( MapEngine_ObserverProxy );
                engine.LoadFromFile( aMapFileName, aSynchronicity );
                iFiles.Add( engine );
            }

            return engine;
        }

        public bool Load( CodeSegDefinition aDefinition, TSynchronicity aSynchronicity )
        {
            bool ret = false;
            //
            if ( string.IsNullOrEmpty( aDefinition.MapFileName ) || !aDefinition.MapFileExists )
            {
            }
            else
            {
                // First pass - try to find map engine that matches the specified
                // PC file name. 
                string mapFileName = aDefinition.MapFileName;
                if ( aDefinition.MapFileExists )
                {
                    System.Diagnostics.Debug.WriteLine( "   LOAD {M}: " + aDefinition.ToString() );

                    MapFileEngine engine = FindByMapFileName( mapFileName );
                    if ( engine != null )
                    {
                        engine.Load( aDefinition );
                        ret = true;
                    }
                    else
                    {
                        // Map file engine doesn't exist for the specified code segment.
                        // Can we load it from file?
                        engine = LoadFromFile( aDefinition.MapFileName, aSynchronicity );
                        if ( engine != null )
                        {
                            engine.Load( aDefinition );
                            ret = true;
                        }
                    }
                }
            }
            //
            return ret;
        }

        public bool Unload( CodeSegDefinition aDefinition )
        {
            bool ret = false;
            //
            foreach ( MapFileEngine engine in iFiles )
            {
                if ( engine.Unload( aDefinition ) )
                {
                    ret = true;
                    break;
                }
            }
            //
            return ret;
        }

        public void UnloadAll()
        {
            foreach ( MapFileEngine engine in iFiles )
            {
                engine.UnloadAll();
            }
        }

        public bool IsLoaded( CodeSegDefinition aDefinition )
        {
            bool ret = false;
            //
            foreach ( MapFileEngine engine in iFiles )
            {
                if ( engine.IsLoaded( aDefinition ) )
                {
                    ret = true;
                    break;
                }
            }
            //
            return ret;
        }

        public string MapFileName( int aIndex )
        {
            return iFiles[ aIndex ].MapFileName;
        }
        #endregion

        #region Properties
        public int MapFileCount
        {
            get { return iFiles.Count; }
        }

        public CodeSegResolver CodeSegResolver
        {
            get { return iCodeSegResolver; }
        }

        public CodeSegDefinitionParser CodeSegParser
        {
            get { return iCodeSegParser; }
        }

        public GenericSymbolEngine MapFileEngineAt( int aIndex )
        {
            return iFiles[ aIndex ];
        }

        public string[] BinaryFileNames
        {
            get
            {
                List<string> fileNames = new List<string>( iFiles.Count );
                //
                foreach ( MapFileEngine engine in iFiles )
                {
                    // Map files only contain symbols for one binary
                    fileNames.Add( engine[ 0 ].HostBinaryFileName );
                }
                //
                return fileNames.ToArray();
            }
        }

        public string[] MapFileNames
        {
            get
            {
                List<string> fileNames = new List<string>( iFiles.Count );
                //
                foreach ( MapFileEngine engine in iFiles )
                {
                    fileNames.Add( engine.MapFileName );
                }
                //
                return fileNames.ToArray();
            }
        }
        #endregion

        #region From GenericSymbolEngine
        public override void Reset()
        {
            iFiles.Clear();
            iCodeSegResolver.Clear();
        }

        public override bool IsLoaded( string aFileName )
        {
            MapFileEngine engine = FindByMapFileName( aFileName );
            return engine != null;
        }

        public override bool IsReady
        {
            get
            {
                int readyCount = 0;
                //
                foreach ( MapFileEngine engine in iFiles )
                {
                    if ( engine.IsReady )
                    {
                        ++readyCount;
                    }
                }
                //
                return ( readyCount == iFiles.Count );
            }
        }

        public override GenericSymbolCollection this[ int aIndex ]
        {
            get
            {
                // Map files contain only a single symbol collection;
                GenericSymbolCollection ret = iFiles[ aIndex ][ 0 ];
                return ret;
            }
        }

        public override AddressRange Range
        {
            get
            {
                AddressRange ret = new AddressRange();
                //
                foreach ( MapFileEngine engine in iFiles )
                {
                    ret.Update( engine.Range );
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
                // Map files contain only a single symbol collection
                int count = iFiles.Count;
                return count;
            }
        }
        #endregion

        #region Event handlers
        private void MapEngine_ObserverProxy( SymbianUtils.AsyncReaderBase.TEvent aEvent, SymbianUtils.AsyncReaderBase aSender )
        {
            if ( Observer != null )
            {
                Observer( aEvent, aSender );
            }
        }
        #endregion

        #region Internal methods
        private MapFileEngine FindByMapFileName( string aFileName )
        {
            MapFileEngine ret = null;
            //
            foreach ( MapFileEngine engine in iFiles )
            {
                if ( engine.MapFileName.ToLower() == aFileName.ToLower() )
                {
                    ret = engine;
                    break;
                }
            }
            //
            return ret;
        }
        #endregion

        #region Data members
        private readonly CodeSegResolver iCodeSegResolver;
        private readonly CodeSegDefinitionParser iCodeSegParser;
        private List<MapFileEngine> iFiles = new List<MapFileEngine>();
        #endregion
    }
}
