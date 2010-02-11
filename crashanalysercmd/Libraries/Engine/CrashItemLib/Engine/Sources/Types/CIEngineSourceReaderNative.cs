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
    internal sealed class CIEngineSourceReaderNative : CIEngineSourceReader
    {
        #region Constructors
        public CIEngineSourceReaderNative( CIEngineSource aSource, CFFSource aPluginSource )
            : base( aSource )
        {
            iPluginSource = aPluginSource;
        }
        #endregion

        #region From CIEngineSourceReader
        public override CIEngineSource.TState Read()
        {
            CIEngineSource.TState ret = CIEngineSource.TState.EStateReadyCorrupt;
            CFFReader reader = iPluginSource.Reader;
            //
            try
            {
                reader.Observer += new CFFReader.ReaderObserver( CFFReader_Observer );
                reader.ExceptionHandler += new CFFReader.ReaderExceptionHandler( CFFReader_ExceptionHandler );

                // Perform synchronous read
                reader.OnNativeReadInit();

                // Decide final state
                CFFReader.TState readerState = reader.State;
                switch ( readerState )
                {
                default:
                case CFFReader.TState.EStateProcessing:
                case CFFReader.TState.EStateUninitialised:
                    SymbianUtils.SymDebug.SymDebugger.Assert( false );
                    break;
                case CFFReader.TState.EStateCorrupt:
                    ret = CIEngineSource.TState.EStateReadyCorrupt;
                    break;
                case CFFReader.TState.EStateReady:
                    if ( base.CrashItemCount > 0 )
                    {
                        ret = CIEngineSource.TState.EStateReady;
                    }
                    else
                    {
                        ret = CIEngineSource.TState.EStateReadyNoItems;
                    }
                    break;
                }
            }
            finally
            {
                reader.Observer -= new CFFReader.ReaderObserver( CFFReader_Observer );
                reader.ExceptionHandler -= new CFFReader.ReaderExceptionHandler( CFFReader_ExceptionHandler );
            }
            //
            return ret;
        }

        public override CFFSource.TReaderOperationType OpType
        {
            get { return CFFSource.TReaderOperationType.EReaderOpTypeNative; }
        }
        #endregion

        #region Properties
        #endregion

        #region Event Handlers
        private void CFFReader_Observer( CFFReader.TEvent aEvent, CFFReader aReader, object aContext )
        {
            base.Trace( "[CIEngineSourceNative] CFFReader_Observer() - START - aEvent: " + aEvent + ", file: " + base.Source.FileName );

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

            base.Trace( "[CIEngineSourceNative] CFFReader_Observer() - END - aEvent: " + aEvent + ", file: " + base.Source.FileName );
        }

        private void CFFReader_ExceptionHandler( Exception aException, CFFReader aReader )
        {
            base.AddException( aException );
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly CFFSource iPluginSource;
        #endregion
    }
}
