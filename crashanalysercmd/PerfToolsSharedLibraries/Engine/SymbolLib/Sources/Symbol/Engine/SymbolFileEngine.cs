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
using SymbianUtils.FileSystem;
using SymbianUtils.FileSystem.Utilities;
using SymbolLib.Generics;
using SymbolLib.CodeSegDef;
using SymbolLib.Sources.Symbol.Symbol;
using SymbolLib.Sources.Symbol.File;
using SymbolLib.Sources.Symbol.Parser;
using SymbolLib.Sources.Symbol.Collection;

namespace SymbolLib.Sources.Symbol.Engine
{
    internal class SymbolFileEngine : GenericSymbolEngine, SymbolCollectionCreator, SymbolEntryCreator
	{
		#region Events
		public event AsyncReaderBase.Observer Observer;
		#endregion

        #region Enumerations
        public enum TActivationType
        {
            EImmediate,
            EOnDemand
        }
        #endregion

		#region Constructors
        public SymbolFileEngine( ITracer aTracer, TActivationType aActivationType, bool aAllowNonRomSymbols )
            : base( aTracer )
		{
            iAllowNonRomSymbols = aAllowNonRomSymbols;
            iActivationType = aActivationType;
		}
		#endregion

		#region API
        public static bool IsSymbolFile( string aFileName )
        {
            string extension = Path.GetExtension( aFileName ).ToLower();
            return ( extension == KSymbolFileExtensionPrimary );
        }

        public SymbolsForBinary ReadFirstCollection( string aFileName )
        {
            bool isValid = false;
            return ReadFirstCollection( aFileName, out isValid );
        }

        public SymbolsForBinary ReadFirstCollection( string aFileName, out bool aIsSymbolFile )
        {
            iCurrentBinary = null;
            iSymbolFileName = aFileName;
            //
            iParser = new SymbolFileParser( this, this, aFileName, this );
            iParser.CollectionCompleted += new SymbolLib.Sources.Symbol.Parser.SymbolFileParser.CollectionCompletedHandler( Parser_CollectionCompletedSingleOnly );
            iParser.SymbolCreated += new SymbolLib.Sources.Symbol.Parser.SymbolFileParser.SymbolCreatedHandler( Parser_SymbolCreated );
            iParser.Read( TSynchronicity.ESynchronous );
            //
            SymbolsForBinary ret = null;
            if ( iAllSymbols.Count > 0 )
            {
                ret = iAllSymbols[ 0 ];
                aIsSymbolFile = true;
            }

            // Did we see any collections whatsoever (all be they data or code?)
            aIsSymbolFile = iParser.ContainedAtLeastOneCollectionFileName;

            return ret;
        }

        public void LoadFromFile( string aSymbolFileName, TSynchronicity aSynchronicity )
        {
			iSymbolFileName = aSymbolFileName;
			//
            iParser = new SymbolFileParser( this, this, aSymbolFileName, this );
            iParser.Tag = this;
			iParser.iObserver += new SymbianUtils.AsyncReaderBase.Observer( ParserEventHandler );
            iParser.CollectionCompleted += new SymbolLib.Sources.Symbol.Parser.SymbolFileParser.CollectionCompletedHandler( Parser_CollectionCompleted );
            iParser.SymbolCreated += new SymbolLib.Sources.Symbol.Parser.SymbolFileParser.SymbolCreatedHandler( Parser_SymbolCreated );
            iParser.Read( aSynchronicity );
        }

        public bool IsLoaded( CodeSegDefinition aDefinition )
        {
            // In ROM, variation might mean that the file name
            // doesn't match the definition name from the actual run-time code
            // segments loaded by a process...
            //
            // If we don't find a file name match, but we do find an address match
            // then we'll treat it as "good enough"
            bool activated = false;
            string searchingFor = aDefinition.ImageFileName.ToLower();
            string searchingForWithoutExtension = FSUtilities.StripAllExtensions( searchingFor );

            // Try to promote a symbol 
            foreach ( SymbolsForBinary file in iActivatedSymbols )
            {
                string entryName = System.IO.Path.GetFileName( file.HostBinaryFileName ).ToLower();
                if ( entryName == searchingFor )
                {
                    activated = true;
                    break;
                }
                else if ( file.AddressRangeStart == aDefinition.AddressStart && file.AddressRangeEnd == aDefinition.AddressEnd )
                {
                    // E.g: ROM.symbol says:         "From    \epoc32\release\armv5\urel\xxx.22.dll"
                    //      runtime code seg says:   "xxx.dll"
                    // We must use the base address instead...

                    activated = true;
                    break;
                }
                else
                {
                    // Try a fuzzy match
                    string entryNameWithoutExtension = FSUtilities.StripAllExtensions( entryName );
                    if ( entryNameWithoutExtension.Contains( searchingForWithoutExtension ) )
                    {
                        // Also make sure the base addresses are the same
                        if ( file.AddressRangeStart == aDefinition.AddressStart )
                        {
                            // Fuzzy match
                            activated = true;
                            break;
                        }
                    }
                }
            }

            return activated;
        }

        public bool Load( CodeSegDefinition aDefinition )
        {
            bool activated = false;
            string searchingFor = aDefinition.EnvironmentFileName.ToLower();
            if ( string.IsNullOrEmpty( searchingFor ) )
            {
                // Try to use on-target name instead, if valid
                searchingFor = aDefinition.ImageFileName.ToLower();
            }

            // Try to promote a symbol 
            foreach( SymbolsForBinary file in iIdleSymbols )
            {
                string entryName = System.IO.Path.GetFileName( file.HostBinaryFileName ).ToLower();
                if  ( entryName == searchingFor )
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine( "   LOAD {S}: " + aDefinition.ToString() );
#endif

                    // Fix up the symbols in this collection
                    file.Fixup( aDefinition.AddressStart );
					
                    // Update ranges
                    iRange.UpdateMin( file.AddressRangeStart );
                    iRange.UpdateMax( file.AddressRangeEnd );

                    // Housekeeping
                    iActivatedSymbols.Add( file );
                    iIdleSymbols.Remove( file );
                    iActivatedSymbols.Sort();

                    // Even though the symbols for this binary may not be explicitly referenced
                    // they are definitely required by the callee, therefore we tag them
                    // immediately.
                    file.Tagged = true;

                    // Indicate we loaded the code seg from a symbol file
                    aDefinition.Source = CodeSegDefinition.TSourceType.ESourceWasSymbolFile;

                    activated = true;
                    break;
                }
            }

            return activated;
        }

        public bool Unload( CodeSegDefinition aDefinition )
        {
            bool suspended = false;
            string searchingFor = aDefinition.EnvironmentFileName.ToLower();

            // Try to promote a symbol 
            foreach( SymbolsForBinary file in iActivatedSymbols )
            {
                string entryName = System.IO.Path.GetFileName( file.HostBinaryFileName ).ToLower();
                if  ( entryName == searchingFor )
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine( " UNLOAD {S}: " + aDefinition.ToString() );
#endif
                    //
                    iActivatedSymbols.Remove( file );
                    iIdleSymbols.Add( file );
                    //
                    iRange.UpdateMin( file.AddressRangeStart );
                    iRange.UpdateMax( file.AddressRangeEnd );
                    //
                    suspended = true;

                    // NB: We don't untag the file since it was obviously needed at
                    // some point.
                    break;
                }
            }

            return suspended;
        }

		public void UnloadAll()
		{
			iActivatedSymbols.Clear();
			iIdleSymbols.Clear();
			//
			foreach( SymbolsForBinary file in iAllSymbols )
			{
				iIdleSymbols.Add( file );
			}
		}
		#endregion

        #region Properties
		public string SymbolFileName
		{
			get { return iSymbolFileName; }
		}

        public bool AllowNonRomSymbols
        {
            get { return iAllowNonRomSymbols; }
        }

		public int Progress
		{
			get
			{
				int prog = 0;
				//
				if	( iParser != null )
				{
					prog = iParser.Progress;
				}
				//
				return prog;
			}
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

		public SymbolsForBinaryCollection AllSymbols
		{
			get { return iAllSymbols; }
		}
		#endregion

        #region From GenericSymbolEngine
        public override void Reset()
		{
            iActivatedSymbols.Clear();
			iIdleSymbols.Clear();
			iAllSymbols.Clear();
            iSymbolFileName = string.Empty;
            iCurrentBinary = null;
            iRange = new AddressRange();
		}

        public override bool IsLoaded( string aFileName )
        {
            return ( aFileName.ToLower() == iSymbolFileName.ToLower() );
        }

        public override bool IsReady
        {
            get
            {
                bool ready = ( SymbolFileName.Length > 0 );
                //
                if	( ready && iParser != null )
                {
                    ready = iParser.IsReady;
                }
                //
                return ready;
            }
        }

        public override GenericSymbolCollection this[ int aIndex ]
        {
            get
            {
                return iActivatedSymbols[ aIndex ];
            }
        }

        public override void SaveTaggedCollections( string aFileName )
        {
            // We override this so that we search through 'iAllSymbols' rather
            // than just the activated symbols (which would be the case if we
            // used the base class version of this method).
            using ( StreamWriter writer = new StreamWriter( aFileName, false ) )
            {
                foreach ( GenericSymbolCollection collection in iAllSymbols )
                {
                    if ( collection.Tagged )
                    {
                        System.Diagnostics.Debug.WriteLine( "STORING: " + collection.HostBinaryFileName );
                        collection.WriteToStream( writer );
                    }
                }
            }
        }

        public override AddressRange Range
        {
            get
            {
                return iRange;
            }
        }

        internal override void UnloadUntagged()
        {

        }
        #endregion

        #region From IGenericSymbolCollectionStatisticsInterface
        public override int NumberOfCollections
        {
            get { return iActivatedSymbols.Count; }
        }
        #endregion

		#region AsyncReaderBase observer
		private void ParserEventHandler( SymbianUtils.AsyncReaderBase.TEvent aEvent, SymbianUtils.AsyncReaderBase aObject )
		{
			if	( Observer != null )
			{
				Observer( aEvent, aObject );
			}

            if  ( aEvent == AsyncReaderBase.TEvent.EReadingComplete )
            {
                iActivatedSymbols.Sort();
                iParser = null;
            }
		}
		#endregion

        #region Parser observer - for normal parsing
        private bool Parser_CollectionCompleted( SymbolsForBinary aCollection )
        {
            // Check whether the collection contains any item. If it doesn't, ditch it.
            // Remove empty collections or sort completed ones.
            bool takeCollection = false;
            int count = aCollection.Count;
            if ( count > 0 )
            {
                // Check whether the collection contains at least 2 symbols, and if not
                // does the one and only symbol just have a length of zero?
                if ( count == 1 )
                {
                    GenericSymbol symbol = aCollection.FirstSymbol;
                    takeCollection = ( symbol.Size > 0 ) || symbol.IsUnknownSymbol;
                }
                else
                {
                    takeCollection = true;
                }
            }

            // If its okay to take the collection, let's sort it and activate if necessary.
            if ( takeCollection )
            {
#if INSPECT_SYMBOL_DATA
                using ( StreamWriter writer = new StreamWriter( @"C:\Temp\OldSymbols\" + Path.GetFileName( aCollection.HostBinaryFileName ) + ".symbol" ) )
                {
                    aCollection.WriteToStream( writer );
                }
#endif
                // All the symbol collections - whether they are loaded or idle.
                iAllSymbols.Add( aCollection );

                // Then put the collection in the correct container depending on
                // activation type.
                if ( iActivationType == TActivationType.EImmediate )
                {
                    aCollection.Sort();
                    iActivatedSymbols.Add( aCollection );
                    //
                    iRange.UpdateMin( aCollection.AddressRangeStart );
                    iRange.UpdateMax( aCollection.AddressRangeEnd );
                }
                else if ( iActivationType == TActivationType.EOnDemand )
                {
                    ThreadPool.QueueUserWorkItem( new WaitCallback( SortCollection ), aCollection );
                    iIdleSymbols.Add( aCollection );
                }
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine( "Discarded Symbol Collection: " + aCollection.TargetBinary );
                //System.Diagnostics.Debug.WriteLine( " " );
            }

            iCurrentBinary = null;
            return SymbolFileParser.KCollectionCompletedAndContinueParsing;
        }

        private void Parser_SymbolCreated( SymbolSymbol aSymbol )
        {
            // 1) We accept symbols with an address of zero, providing that their size is greater
            //    than zero.
            //
            // 2) We accept symbols with an address greater than zero, irrespective of their size.
			if	( ( aSymbol.Address >= 0 || aSymbol.Size > 0 ) || // 1
                  ( aSymbol.Address == 0 && aSymbol.Size > 0 )    // 2
                )
			{
                iCurrentBinary.Add( this, aSymbol, AllowNonRomSymbols );
			}
            else
            {
                System.Diagnostics.Debug.WriteLine( "  Discarded symbol: " + aSymbol );
            }
        }
        #endregion

        #region Parser observer - for peeking at a symbol file's first collection
        private bool Parser_CollectionCompletedSingleOnly( SymbolLib.Sources.Symbol.File.SymbolsForBinary aCollection )
        {
            // Call our standard function to handle the collection
            Parser_CollectionCompleted( aCollection );

            // Indicate no more parsing required
            bool ret = SymbolFileParser.KCollectionCompletedAndContinueParsing;
            if ( aCollection.Count > 0 && iAllSymbols.Count > 0 )
            {
                ret = SymbolFileParser.KCollectionCompletedAndAbortParsing;
            }
            //
            return ret;
        }
        #endregion

        #region SymbolCollectionCreator interface
        public SymbolsForBinary CreateCollection( string aHostFileName )
        {
            iCurrentBinary = new SymbolsForBinary( aHostFileName );
            iCurrentBinary.SourceFile = SymbolFileName;
            return iCurrentBinary;
        }
        #endregion

        #region SymbolEntryCreator interface
        public SymbolSymbol CreateSymbol()
        {
            return SymbolSymbol.New( iCurrentBinary );
        }
        #endregion

        #region Internal methods
        private void CalculateRange()
        {
            iRange.Reset();
            //
            foreach( SymbolsForBinary file in iActivatedSymbols )
            {
                iRange.UpdateMin( file.AddressRangeStart );
                iRange.UpdateMax( file.AddressRangeEnd );
            }
        }

        private void SortCollection( object aCollection )
        {
            SymbolsForBinary symbols = aCollection as SymbolsForBinary;
            if ( symbols != null )
            {
                symbols.Sort();
            }
        }
        #endregion

        #region Internal constants
        private const string KSymbolFileExtensionPrimary = ".symbol";
        #endregion

        #region Data members
        private readonly TActivationType iActivationType;
        private readonly bool iAllowNonRomSymbols;
		private string iSymbolFileName = string.Empty;
        private SymbolsForBinary iCurrentBinary = null;
        private SymbolFileParser iParser = null;
        private AddressRange iRange = new AddressRange();
		private SymbolsForBinaryCollection iAllSymbols = new SymbolsForBinaryCollection();
		private SymbolsForBinaryCollection iIdleSymbols = new SymbolsForBinaryCollection();
        private SymbolsForBinaryCollection iActivatedSymbols = new SymbolsForBinaryCollection();
        #endregion
    }
}
