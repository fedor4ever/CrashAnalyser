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
using System.Text;
using SymbianUtils;
using CrashItemLib.PluginAPI;
using CrashItemLib.Crash.Source;
using CrashItemLib.Crash.Container;

namespace CrashItemLib.Engine.Sources.Types
{
    internal sealed class CIEngineSourceReaderTrace : CIEngineSourceReader
    {
        #region Constructors
        public CIEngineSourceReaderTrace( CIEngineSource aSource, CFFSource[] aPluginSources )
            : base( aSource )
		{
            iPluginSources.AddRange( aPluginSources );
        }
        #endregion

        #region From CIEngineSourceReader
        public override CIEngineSource.TState Read()
        {
            CIEngineSource.TState ret = CIEngineSource.TState.EStateReadyCorrupt;
            
            // Listen to container creation events in all plugin reader objects
            foreach ( CFFSource source in iPluginSources )
            {
                CFFReader reader = source.Reader;
                reader.Observer += new CFFReader.ReaderObserver( CFFReader_Observer );
                reader.ExceptionHandler += new CFFReader.ReaderExceptionHandler( CFFReader_ExceptionHandler );
            }
            //
            try
            {
                CIEngineTraceReader traceReader = new CIEngineTraceReader( this );
                traceReader.Read( TSynchronicity.ESynchronous );
            }
            finally
            {
                ret = CalculateFinalState();

                // Stop listening to container creation events in all plugin reader objects
                foreach ( CFFSource source in iPluginSources )
                {
                    CFFReader reader = source.Reader;
                    reader.Observer -= new CFFReader.ReaderObserver( CFFReader_Observer );
                    reader.ExceptionHandler -= new CFFReader.ReaderExceptionHandler( CFFReader_ExceptionHandler );
                }
            }
            //
            return ret;
        }

        public override CFFSource.TReaderOperationType OpType
        {
            get { return CFFSource.TReaderOperationType.EReaderOpTypeTrace; }
        }
        #endregion

        #region API
        #endregion

        #region Event handlers
        private void CFFReader_Observer( CFFReader.TEvent aEvent, CFFReader aReader, object aContext )
        {
            base.Trace( "[CIEngineSourceReaderTrace] CFFReader_Observer() - START - aEvent: " + aEvent + ", file: " + base.Source.FileName );

            // This method is called for both native and trace based events
            if ( aEvent == CFFReader.TEvent.EReadingContainerCreated )
            {
                CIContainer container = aContext as CIContainer;
                if ( container != null )
                {
                    base.SaveCrash( container );
                }
            }
            else if ( aEvent == CFFReader.TEvent.EReadingProgress )
            {
                int progress = ( aContext != null && aContext is int ) ? (int) aContext : 0;
                base.Source.OnSourceReadingProgress( progress );
            }

            base.Trace( "[CIEngineSourceReaderTrace] CFFReader_Observer() - END - aEvent: " + aEvent + ", file: " + base.Source.FileName );
        }

        private void CFFReader_ExceptionHandler( Exception aException, CFFReader aReader )
        {
            base.AddException( aException );
        }
        #endregion

        #region Multiplexing methods
        internal void OnTraceReadInit()
        {
            lock ( iPluginSources )
            {
                foreach ( CFFSource entry in iPluginSources )
                {
                    entry.Reader.OnTraceReadInit();
                }
            }
        }

        internal void OnTraceReadComplete()
        {
            lock ( iPluginSources )
            {
                foreach ( CFFSource entry in iPluginSources )
                {
                    entry.Reader.OnTraceReadComplete();
                }
            }
        }

        internal void OnTraceReadOffer( CFFTraceLine aLine )
        {
            lock ( iPluginSources )
            {
                foreach ( CFFSource entry in iPluginSources )
                {
                    CFFTraceLine line = new CFFTraceLine( aLine.Line, aLine.LineNumber, entry );
                    entry.Reader.OnTraceReadOffer( line );
                }
            }
        }

        internal void OnTraceReadException( Exception aException )
        {
            base.AddException( aException );
        }

        internal void OnTraceReaderProgress( int aProgress )
        {
            base.Source.OnSourceReadingProgress( aProgress );
        }
        #endregion

        #region Internal methods
        private CIEngineSource.TState CalculateFinalState()
        {
            CIEngineSource.TState ret = CIEngineSource.TState.EStateReadyCorrupt;

            // For trace-based operations we'll potentially be firing each trace
            // line at multiple readers, in which case we only mark the file as
            // corrupt if no readers completed successfully.
            int countCorrupt = 0;
            int countReady = 0;
            int countContainers = base.CrashItemCount;
            
            // Count number of ready vs corrupt readers
            foreach ( CFFSource source in iPluginSources )
            {
                CFFReader reader = source.Reader;
                CFFReader.TState readerState = reader.State;
                //
                switch ( readerState )
                {
                case CFFReader.TState.EStateCorrupt:
                    ++countCorrupt;
                    break;
                case CFFReader.TState.EStateReady:
                    ++countReady;
                    break;
                default:
                case CFFReader.TState.EStateProcessing:
                case CFFReader.TState.EStateUninitialised:
                    SymbianUtils.SymDebug.SymDebugger.Assert( false );
                    break;
                }
            }

            // If we created at least one container, then we did still manage to create
            // some kind of crash output irrespective of how many of the readers indicated 
            // the source was corrupt. In that case, the underlying source file is treated
            // as valid.
            base.Trace( "[CIEngineSourceReaderTrace] CalculateFinalState() - total: {0}, ready: {1}, corrupt: {2}", countContainers, countReady, countCorrupt );
            if ( countContainers > 0 )
            {
                ret = CIEngineSource.TState.EStateReady;
            }
            else
            {
                // We didn't manage to create any crash items at all from this source
                // file.
                if ( countCorrupt > 0 )
                {
                    // At least one reader indicated that the source file was corrupt, 
                    // and since no other reader could create any kind of valid output
                    // we'll treat the source file as entirely corrupt.
                    ret = CIEngineSource.TState.EStateReadyCorrupt;
                }
                else
                {
                    // No crash container created, but nobody said the file was corrupt
                    // either. It's just a "no items" file.
                    ret = CIEngineSource.TState.EStateReadyNoItems;
                }

            }
            
            base.Trace( "[CIEngineSourceReaderTrace] CalculateFinalState() - END - ret: {0}, file: {1}", ret, base.Source.FileName );
            return ret;
        }
        #endregion

        #region Internal class - actual trace file reader
        internal class CIEngineTraceReader : AsyncTextFileReader
        {
            #region Constructors
            public CIEngineTraceReader( CIEngineSourceReaderTrace aMultiplexer )
                : base( aMultiplexer.Source.FileName )
            {
                iMultiplexer = aMultiplexer;
            }
            #endregion

            #region API
            public void Read( TSynchronicity aSynchronicity )
            {
                base.StartRead( aSynchronicity );
            }
            #endregion

            #region From AsyncTextFileReader
            protected override void HandleReadStarted()
            {
                try
                {
                    base.HandleReadStarted();
                }
                finally
                {
                    iMultiplexer.OnTraceReadInit();
                }
            }

            protected override void HandleReadCompleted()
            {
                try
                {
                    base.HandleReadCompleted();
                }
                finally
                {
                    iMultiplexer.OnTraceReadComplete();
                }
            }

            protected override void HandleFilteredLine( string aLine )
            {
                CFFTraceLine line = new CFFTraceLine( aLine, LineNumber, null );
                iMultiplexer.OnTraceReadOffer( line );
            }

            protected override void HandleReadException( Exception aException )
            {
                try
                {
                    base.HandleReadException( aException );
                }
                finally
                {
                    iMultiplexer.OnTraceReadException( aException );
                }
            }

            protected override void OnProgressChanged( int aProgress )
            {
                iMultiplexer.OnTraceReaderProgress( aProgress );
            }
            #endregion

            #region Data members
            private readonly CIEngineSourceReaderTrace iMultiplexer;
            #endregion
        }
        #endregion

        #region Data members
        private List<CFFSource> iPluginSources = new List<CFFSource>();
        #endregion
    }
}
