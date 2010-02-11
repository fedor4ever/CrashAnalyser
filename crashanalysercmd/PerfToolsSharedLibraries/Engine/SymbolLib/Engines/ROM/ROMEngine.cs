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
using SymbolLib.CodeSegDef;
using SymbolLib.Engines.Common;
using SymbolLib.Sources.Map.Engine;
using SymbolLib.Sources.Map.Parser;
using SymbolLib.Sources.Map.File;
using SymbolLib.Sources.Symbol.Engine;
using SymbolLib.Sources.Symbol.File;
using SymbolLib.Generics;
using SymbianUtils;
using SymbianUtils.Range;
using SymbianUtils.Tracer;

namespace SymbolLib.Engines.ROM
{
    public class ROMEngine : SymbolEngineBase
    {
        #region Constructor & destructor
        internal ROMEngine( ITracer aTracer )
            : base( aTracer )
        {
            iEngineSymbol = new SymbolFileEngine( this, SymbolFileEngine.TActivationType.EImmediate, false ); 
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
                    SymbolFileEngine tempEngine = new SymbolFileEngine( null, SymbolFileEngine.TActivationType.EOnDemand, false );
                    SymbolsForBinary symbols = tempEngine.ReadFirstCollection( aFileName );

                    // For a valid ROFS symbol file, the first symbol should have an address of zero.
                    bool containedNonZeroFirstSymbolCollection = ( symbols != null && symbols.Count > 0 && symbols[ 0 ].Address != 0 );
                    if ( containedNonZeroFirstSymbolCollection )
                    {
                        ret = TFileType.EFileRomSymbol;
                    }
                }
            }
            catch ( Exception )
            {
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region From SymbolEngineBase
        public override bool AddressInRange( long aAddress )
        {
            bool ret = iEngineSymbol.Range.Contains( aAddress );
            return ret;
        }

        public override int FileNameCount
        {
            get
            {
                return 1;
            }
        }

        public override string FileName( int aIndex )
        {
            string ret = string.Empty;
            //
            if  ( aIndex == 0 )
            {
                ret = iEngineSymbol.SymbolFileName;
            }
            //            
            return ret;
        }

        public override void LoadFromFile( string aFileName, TSynchronicity aSynchronicity )
        {
            string extn = Path.GetExtension( aFileName ).ToLower();
            if  ( extn == ".symbol" )
            {
				// ROM engine only supports loading a single symbol file
				// at once. First unload any old file, then load new one.
				UnloadAll();
                iEngineSymbol.LoadFromFile( aFileName, aSynchronicity );
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public override void UnloadAll()
        {
            Reset();
        }

        public override bool IsLoaded( CodeSegDefinition aDefinition )
        {
            bool loaded = iEngineSymbol.IsLoaded( aDefinition );
            return loaded;
        }
        #endregion

        #region From GenericSymbolEngine
        public override bool IsReady
        {
            get
            {
                return iEngineSymbol.IsReady;
            }
        }

        public override void Reset()
        {
            iEngineSymbol.Reset();
        }

        public override bool IsLoaded( string aFileName )
        {
            bool loaded = iEngineSymbol.IsLoaded( aFileName );
            return loaded;
        }

        public override GenericSymbolCollection this[ int aIndex ]
        {
            get
            {
                GenericSymbolCollection ret = iEngineSymbol[ aIndex ];
                return ret;
            }
        }

        public override AddressRange Range
        {
            get
            {
                return iEngineSymbol.Range;
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
                return iEngineSymbol.NumberOfCollections;
            }
        }
        #endregion

        #region Event handlers
        private void SymbolEngine_Observer( SymbianUtils.AsyncReaderBase.TEvent aEvent, SymbianUtils.AsyncReaderBase aSender )
        {
            switch( aEvent )
            {
                case SymbianUtils.AsyncReaderBase.TEvent.EReadingStarted:
                    OnParsingStarted( iEngineSymbol.SymbolFileName );
                    break;
                case SymbianUtils.AsyncReaderBase.TEvent.EReadingProgress:
                    OnParsingProgress( iEngineSymbol.SymbolFileName, aSender.Progress );
                    break;
                case SymbianUtils.AsyncReaderBase.TEvent.EReadingComplete:
                    OnParsingCompleted( iEngineSymbol.SymbolFileName );
                    break;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly SymbolFileEngine iEngineSymbol;
        #endregion
    }
}
