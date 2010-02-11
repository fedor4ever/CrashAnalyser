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
using SymbianUtils.Tracer;
using SymbolLib.Generics;
using SymbolLib.Engines.ROFS;
using SymbolLib.Engines.ROM;
using SymbolLib.Sources.Map.Engine;
using SymbolLib.Sources.Symbol.Engine;
using SymbolLib.CodeSegDef;

namespace SymbolLib.Engines
{
	public class SymbolManager : ITracer
    {
        #region Construct & destruct
        public SymbolManager()
            : this( null )
		{
		}

        public SymbolManager( ITracer aTracer )
        {
            iTracer = aTracer;
            //
            iEngineROM = new ROMEngine( this );
            iEngineROFS = new ROFSEngine( this );
        }
		#endregion

		#region API
        public void Clear()
        {
            ClearTags();
            ROFSEngine.Reset();
            ROMEngine.Reset();
        }

        public Dictionary<string, string> GetSupportedExtensions()
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            //
            ret.Add( "*.symbol", "Symbian OS Symbol File" );
            ret.Add( "*.map", "Symbian OS Map File" );
            ret.Add( "*.oby", "Symbian OS Obey File" );
            //
            return ret;
        }

		public bool IsLoaded( string aFileName )
		{
            string fileName = aFileName.ToLower();
			bool loaded = iEngineROM.IsLoaded( fileName );
            //
            if  ( loaded == false )
            {
                loaded = iEngineROFS.IsLoaded( fileName );
            }
            //
			return loaded;
		}

        public void ClearTags()
        {
            iEngineROM.ClearTags();
            iEngineROFS.ClearTags();
        }

        public void LoadDynamicCodeSegment( CodeSegDefinition aCodeSegment, TSynchronicity aSynchronicity )
        {
            ROFSEngine.LoadFromDefinition( aCodeSegment, aSynchronicity );
        }

        public void LoadDynamicCodeSegments( CodeSegDefinitionCollection aCodeSegments, TSynchronicity aSynchronicity )
        {
            // Unload any pre-existing dynamically loaded content
            ROFSEngine.UnloadAll();

            // We must attempt to dynamically load all the code segements listed. 
            // For codesegments that we have no corresponding collection for, we'll allow
            // a stub codesegment to be created in the ROFS engine - this ensures we
            // at least show the codesegment name (though not function addresses) when we
            // encounter an unrecognised address within the codesegment address space.
            //
            // For everything else, we load it (if it exists) or then if it's already been
            // loaded by the CORE ROM symbol file, we ignore the request.

            // These are the code segs that we'll eventually push through to the
            // ROFS engine.
            CodeSegDefinitionCollection codeSegsToAttemptToLoad = new CodeSegDefinitionCollection();

            // First pass - identify which code seg entries we already have loaded.
            foreach ( CodeSegDefinition def in aCodeSegments )
            {
                // Check if there is already a valid definition for this entry in
                // the CORE ROM symbol table. If there is, the we don't need to do
                // anything.
                bool loaded = ROMEngine.IsLoaded( def );
                if ( !loaded )
                {
                    Trace( "SymbolManager.LoadDynamicCodeSegments() - will attempt to load: " + def );
                    codeSegsToAttemptToLoad.Add( def );
                }
                else
                {
                    Trace( "SymbolManager.LoadDynamicCodeSegments() - not loading XIP code seg: " + def );
                }
            }

            // Now, we have a ratified list that contains only code segments that weren't already managed
            // by the CORE ROM engine.
            ROFSEngine.LoadFromDefinitionCollection( codeSegsToAttemptToLoad, aSynchronicity );
        }

        public static TFileType IsSupported( string aFileName )
        {
            TFileType ret = TFileType.EFileNotSupported;
            //
            FileInfo fileInfo = new FileInfo( aFileName );
            if ( fileInfo.Exists )
            {
                // Try rom then rofs
                ret = ROMEngine.IsSupported( aFileName );
                if ( ret == TFileType.EFileNotSupported )
                {
                    ret = ROFSEngine.IsSupported( aFileName );
                }
            }
            //
            return ret;
        }
		#endregion

		#region Properties
        public ROMEngine ROMEngine
		{
			get { return iEngineROM; }
		}
		
		public ROFSEngine ROFSEngine
		{
			get { return iEngineROFS; }
		}

		public int NumberOfCollections
		{
			get
			{
				int count = 0;
				//
				count += iEngineROM.NumberOfCollections;
				count += iEngineROFS.NumberOfCollections;
				//
				return count;
			}
		}

		public int NumberOfEntries
		{
			get
			{
				int count = 0;
				//
				count += iEngineROM.NumberOfEntries;
				count += iEngineROFS.NumberOfEntries;
				//
				return count;
			}
		}

		public bool IsReady
		{
			get
			{
				bool readyROM = iEngineROM.IsReady;
                bool readyROFS = iEngineROFS.IsReady;
                //
				return ( readyROM && readyROFS );
			}
		}
		#endregion

		#region Lookup API
		public bool AddressInRange( long aAddress )
		{
			bool ret = iEngineROFS.AddressInRange( aAddress );
			if	( !ret )
			{
				ret = iEngineROM.AddressInRange( aAddress );
			}
			return ret;
		}

        /// <summary>
        /// Performs as substring match within each collection's host binary file name
        /// for aFileName.
        /// </summary>
        /// <param name="aHostBinaryFileName"></param>
        /// <returns></returns>
        public GenericSymbolCollection[] CollectionsByHostBinarySearch( string aFileName )
        {
            List<GenericSymbolCollection> ret = new List<GenericSymbolCollection>();
            //
            ret.AddRange( iEngineROM.CollectionsByHostBinarySearch( aFileName ) );
            ret.AddRange( iEngineROFS.CollectionsByHostBinarySearch( aFileName ) );
            //
            return ret.ToArray();
        }

        public string SymbolNameByAddress( long aAddress )
        {
            string ret = string.Empty;

            SymbolLib.Generics.GenericSymbol symbol = EntryByAddress( aAddress );
            if  ( symbol != null )
            {
                ret = symbol.Symbol;
            }

            return ret;
        }

		public GenericSymbol EntryByAddress( long aAddress )
		{
			GenericSymbolCollection collection = null;
			GenericSymbol ret = EntryByAddress( aAddress, out collection );
			return ret;
		}

		public GenericSymbol EntryByAddress( long aAddress, out GenericSymbolCollection aCollection )
		{
            aCollection = null;
            GenericSymbolCollection rofsCollection = null;

			// First check with the map file engine to see if there is a loaded
			// code segment entry that matches the specified address.
			GenericSymbol ret = iEngineROFS.EntryByAddress( aAddress, ref rofsCollection );
			//
			if	( ret == null )
			{
                GenericSymbolCollection romCollection = null;
                ret = iEngineROM.EntryByAddress( aAddress, ref romCollection );

                // Decide which collection to return in the case that 
                // a) we found a matching symbol, or 
                // b) when we didn't.
                if ( ret == null && rofsCollection != null )
                {
                    // ROFS wins by default if we found something inside the ROFS engine...
                    aCollection = rofsCollection;
                }
                else
                {
                    // Otherwise we'll use the ROM collection (irrespective of whether something
                    // was found or not)
                    aCollection = romCollection;
                }
			}
            else
            {
                // ROFS wins
                aCollection = rofsCollection;
            }

			return ret;
		}

		public GenericSymbolCollection CollectionByAddress( long aAddress )
		{
			// First check with the map file engine to see if there is a loaded
			// code segment entry that matches the specified address.
			GenericSymbolCollection ret = iEngineROFS.CollectionByAddress( aAddress );
			//
			if	( ret == null )
			{
				ret = iEngineROM.CollectionByAddress( aAddress );
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
        private readonly ROMEngine iEngineROM;
        private readonly ROFSEngine iEngineROFS;
		#endregion
	}
}
