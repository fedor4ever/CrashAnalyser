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
using SymbolLib.Generics;
using SymbolLib.CodeSegDef;
using SymbolLib.Sources.Symbol.Symbol;
using SymbolLib.Sources.Symbol.File;
using SymbolLib.Sources.Symbol.Parser;
using SymbolLib.Sources.Symbol.Collection;

namespace SymbolLib.Sources.Symbol.Engine
{
    public class SymbolFileEngineCollection : GenericSymbolEngine
    {
        #region Delegates & Events
        public event AsyncReaderBase.Observer Observer;
        #endregion

        #region Constructorss
        internal SymbolFileEngineCollection( ITracer aTracer, SymbolFileEngine.TActivationType aActivationType, bool aAllowNonRomSymbols )
            : base( aTracer )
        {
            iActivationType = aActivationType;
            iAllowNonRomSymbols = aAllowNonRomSymbols;
        }
        #endregion

        #region API
        public static bool IsSymbolFile( string aFileName )
        {
            return SymbolFileEngine.IsSymbolFile( aFileName );
        }

        public void LoadFromFile( string aSymbolFileName, TSynchronicity aSynchronicity )
        {
            // Check if already exists
            SymbolFileEngine engine = null;
            //
            lock ( this )
            {
                engine = EngineByFileName( aSymbolFileName );
                if ( engine != null )
                {
                    iFiles.Remove( engine );
                }
            }

            engine = new SymbolFileEngine( this, iActivationType, iAllowNonRomSymbols );
            
            lock ( this )
            {
                iFiles.Add( engine );
            }

            engine.Observer += new AsyncReaderBase.Observer( SymbolEngine_ObserverProxy );
            engine.LoadFromFile( aSymbolFileName, aSynchronicity );
        }

        public bool Load( CodeSegDefinition aDefinition )
        {
            bool ret = false;
            //
            foreach ( SymbolFileEngine engine in iFiles )
            {
                if ( engine.Load( aDefinition ) )
                {
                    ret = true;
                    break;
                }
            }
            //
            return ret;
        }

        public bool Unload( CodeSegDefinition aDefinition )
        {
            bool ret = false;
            //
            foreach ( SymbolFileEngine engine in iFiles )
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
            foreach ( SymbolFileEngine engine in iFiles )
            {
                engine.UnloadAll();
            }
        }

        public bool IsLoaded( CodeSegDefinition aDefinition )
        {
            bool ret = false;
            //
            foreach ( SymbolFileEngine engine in iFiles )
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

        public string SymbolFileName( int aIndex )
        {
            return iFiles[ aIndex ].SymbolFileName;
        }
        #endregion

        #region Properties
        public int SymbolFileCount
        {
            get { return iFiles.Count; }
        }

        public GenericSymbolEngine SymbolFileEngineAt( int aIndex )
        {
            return iFiles[ aIndex ];
        }

        public string[] BinaryFileNames
        {
            get
            {
                List<string> fileNames = new List<string>( AllSymbols.Count );
                //
                foreach ( GenericSymbolCollection collection in AllSymbols )
                {
                    fileNames.Add( collection.HostBinaryFileName );
                }
                //
                return fileNames.ToArray();
            }
        }

        public string[] SymbolFileNames
        {
            get
            {
                List<string> fileNames = new List<string>( iFiles.Count );
                //
                foreach ( SymbolFileEngine engine in iFiles )
                {
                    fileNames.Add( engine.SymbolFileName );
                }
                //
                return fileNames.ToArray();
            }
        }

        public SymbolsForBinaryCollection AllSymbols
        {
            get
            {
                SymbolsForBinaryCollection ret = new SymbolsForBinaryCollection();
                //
                foreach ( SymbolFileEngine engine in iFiles )
                {
                    ret.Add( engine.AllSymbols );
                }
                //
                return ret;
            }
        }
        #endregion

        #region From GenericSymbolEngine
        public override void Reset()
        {
            iFiles.Clear();
        }

        public override bool IsLoaded( string aFileName )
        {
            SymbolFileEngine engine = EngineByFileName( aFileName );
            return engine != null;
        }

        public override bool IsReady
        {
            get
            {
                int readyCount = 0;
                //
                foreach ( SymbolFileEngine engine in iFiles )
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
                GenericSymbolCollection ret = null;
                //
                foreach ( SymbolFileEngine engine in iFiles )
                {
                    int count = engine.NumberOfCollections;
                    if ( aIndex < count )
                    {
                        ret = engine[ aIndex ];
                        break;
                    }
                    else
                    {
                        aIndex -= count;
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
                foreach ( SymbolFileEngine engine in iFiles )
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
                int count = 0;
                //
                foreach ( SymbolFileEngine engine in iFiles )
                {
                    count += engine.NumberOfCollections;
                }
                //
                return count;
            }
        }
        #endregion

        #region Event handlers
        private void SymbolEngine_ObserverProxy( SymbianUtils.AsyncReaderBase.TEvent aEvent, SymbianUtils.AsyncReaderBase aSender )
        {
            if ( Observer != null )
            {
                Observer( aEvent, aSender );
            }
        }
        #endregion

        #region Internal methods
        private SymbolFileEngine EngineByFileName( string aFileName )
        {
            SymbolFileEngine ret = null;
            //
            foreach ( SymbolFileEngine engine in iFiles )
            {
                if ( engine.SymbolFileName.ToLower() == aFileName.ToLower() )
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
        private readonly SymbolFileEngine.TActivationType iActivationType;
        private readonly bool iAllowNonRomSymbols;
        private List<SymbolFileEngine> iFiles = new List<SymbolFileEngine>();
        #endregion
    }
}
