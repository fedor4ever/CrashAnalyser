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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.ComponentModel;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianUtils.FileTypes;
using SymbianUtils.TextUtilities.Readers.Types.Array;
using SymbianStructuresLib.Debug.Symbols;
using SymbianStructuresLib.Debug.Symbols.Utilities;
using SymbianSymbolLib.SourceManagement.Source;
using SymbianSymbolLib.SourceManagement.Provisioning;
using SLPluginSymbol.Data;
using SLPluginSymbol.Source;

namespace SLPluginSymbol.Reader
{
    internal class SymbolFileSegmentReader : DisposableObject
    {
        #region Delegates & events
        public delegate void ProgressHandler( SymbolFileSegmentReader aReader, long aTotalNumberOfLines, long aNumberProcessed );
        public event ProgressHandler Progress;

        public delegate void OperationHandler( SymbolFileSegmentReader aReader );
        public event OperationHandler OperationStarted;
        public event OperationHandler OperationCompleted;
        #endregion

        #region Constructors
        public SymbolFileSegmentReader( SymbolFileSegment aSegment )
		{
            iSegment = aSegment;
		}
		#endregion

        #region API
        public void Read( TSynchronicity aSynchronicity )
        {
            switch ( aSynchronicity )
            {
            default:
            case TSynchronicity.EAsynchronous:
                ThreadPool.QueueUserWorkItem( new WaitCallback( InitiateReadAsync ) );
                break;
            case TSynchronicity.ESynchronous:
                InitiateRead();
                break;
            }
        }
        #endregion

        #region Properties
        public SymbolSource Source
        {
            get { return iSegment.Source; }
        }

        public SymbolFileSegment FileSegment
        {
            get { return iSegment; }
        }
		#endregion

        #region Internal methods
        private void InitiateReadAsync( object aNotUsed )
        {
            InitiateRead();
        }

        private void InitiateRead()
        {
            if ( OperationStarted != null )
            {
                OperationStarted( this );
            }
            //
            int count = iSegment.Count;
            for ( int i = count - 1; i >= 0; i-- )
            {
                SymbolCollectionSegment segment = iSegment[ i ];
                //
                ReadLines( segment );
            }
            //
            if ( OperationCompleted != null )
            {
                OperationCompleted( this );
            }
        }

        private void ReadLines( SymbolCollectionSegment aSegment )
        {
            long count = 0;
           
            // Make a new collection to which we'll add symbols
            SymbolCollection collection = aSegment.Collection;
            collection.TransactionBegin();
             
            // We'll use this to filter out bad symbols
            try
            {
                using ( SymbolCollectionHarmoniser harmoniser = new SymbolCollectionHarmoniser( collection, SymbolCollectionHarmoniser.TCollectionType.EPossiblyXIP ) )
                {
                    // Then create symbols
                    SymbolCreator creator = new SymbolCreator();
                    foreach ( string line in aSegment )
                    {
                        Symbol symbol = creator.Parse( line, collection );
                        if ( symbol != null )
                        {
                            harmoniser.Add( symbol );
                        }
                        //
                        ++count;
                    }
                }
            }
            finally
            {
                collection.TransactionEnd();

                // Collection is now complete - run final validation in
                // background thread
                ValidateAndSaveCollection( collection );
            }

            ReportProgress( count );

#if INSPECT_SYMBOL_DATA
            using ( StreamWriter writer = new StreamWriter( @"C:\Temp\NewSymbols\" + Path.GetFileName( collection.FileName.FileNameInHost ) + ".symbol" ) )
            {
                WriteToStream( collection, writer );
            }
#endif
        }

        private void ValidateAndSaveCollection( SymbolCollection aCollection )
        {
            // We don't save empty collections since they have no size
            bool isEmpty = aCollection.IsEmptyApartFromDefaultSymbol;
            if ( !isEmpty )
            {
                // Make sure that the collection contains at least one entry with a valid size
                bool save = false;

                int count = aCollection.Count;
                for ( int i = 0; i < count; i++ )
                {
                    Symbol sym = aCollection[ i ];
                    if ( sym.Size > 0 )
                    {
                        save = true;
                        break;
                    }
                }

                if ( save )
                {
                    try
                    {
                        // If the source does not accept the collection then just continue to the next
                        // entry in the file.
                        aCollection.SortAsync();
                        Source.Add( aCollection );
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void ReportProgress( long aChunkSize )
        {
            if ( Progress != null )
            {
                Progress( this, iSegment.NumberOfLines, aChunkSize );
            }
        }

#if INSPECT_SYMBOL_DATA
        private void WriteToStream( SymbolCollection aCollection, StreamWriter aWriter )
        {
            // First write the binary name
            aWriter.WriteLine( string.Empty );
            aWriter.WriteLine( "From    " + aCollection.FileName.FileNameInHost );
            aWriter.WriteLine( string.Empty );

            foreach ( Symbol symbol in aCollection )
            {
                StringBuilder ret = new StringBuilder();
                //
                ret.Append( symbol.Address.ToString( "x8" ) );
                ret.Append( "    " );
                ret.Append( symbol.Size.ToString( "x4" ) );
                ret.Append( "    " );
                ret.Append( symbol.Name.PadRight( 40, ' ' ) );
                ret.Append( " " );
                ret.Append( symbol.Object );
                //
                aWriter.WriteLine( ret.ToString() );
            }
        }
#endif
        #endregion

        #region From DisposableObject
        protected override void CleanupManagedResources()
        {
            try
            {
                base.CleanupManagedResources();
            }
            finally
            {
            }
        }
        #endregion

        #region Data members
        private readonly SymbolFileSegment iSegment;
		#endregion
	}
}
