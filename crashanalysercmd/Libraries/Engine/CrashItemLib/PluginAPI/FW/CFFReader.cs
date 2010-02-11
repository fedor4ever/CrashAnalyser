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
using System.Collections.Generic;
using System.Text;
using SymbianUtils;
using SymbianDebugLib.Engine;
using CrashItemLib.Crash;
using CrashItemLib.Engine;

namespace CrashItemLib.PluginAPI
{
	public abstract class CFFReader
	{
        #region Enumerations
        public enum TState
        {
            EStateUninitialised = 0,
            EStateProcessing,
            EStateReady,
            EStateCorrupt
        }
        #endregion

        #region Delegates & events
		public enum TEvent
		{
			EReadingStarted = 0,
			EReadingProgress,
            EReadingContainerCreated,
			EReadingComplete,
		}

		public delegate void ReaderObserver( TEvent aEvent, CFFReader aReader, object aContext );
        public event ReaderObserver Observer;

        public delegate void ReaderExceptionHandler( Exception aException, CFFReader aReader );
        public event ReaderExceptionHandler ExceptionHandler;
		#endregion

		#region Constructors
		protected CFFReader( CFFPlugin aEngine, CFFSource aDescriptor )
		{
            iPlugin = aEngine;
            iDescriptor = aDescriptor;
		}
		#endregion

		#region API - abstract
        /// <summary>
        /// Called when Crash Analyser wants this plugin to read a
        /// native, i.e. format-specific crash file (i.e. not a trace file) 
        /// synchronously.
        /// </summary>
        public virtual void NativeReadInit()
        {
        }

        /// <summary>
        /// Called when Crash Analyser is going to start to send
        /// trace lines (that it will read on behalf of the plugin)
        /// that should be processed in order to identify embedded
        /// traces.
        /// 
        /// This method will only be called if the plugin claims to have
        /// some confidence in reading trace data.
        /// </summary>
        public virtual void TraceReadInit()
        {
        }

        /// <summary>
        /// Receive an individual trace line and process it if relevant
        /// </summary>
        /// <param name="aLine"></param>
        public virtual bool TraceReadOffer( CFFTraceLine aLine )
        {
            // Nothing to do
            return false;
        }

        /// <summary>
        /// Called when all trace lines have been delivered
        /// </summary>
        public virtual void TraceReadComplete()
        {
        }
		#endregion

        #region API - framework
        protected byte[] RawData
        {
            get { return Descriptor.RawData; }
        }

        protected void RawDataAdd()
        {
            // Save entire file
            RawDataClear();
            RawDataAdd( Descriptor.MasterFile );
        }

        protected void RawDataAdd( FileInfo aFile )
        {
            byte[] bytes = File.ReadAllBytes( aFile.FullName );
            RawDataAdd( bytes );
        }

        protected void RawDataClear()
        {
            Descriptor.RawDataClear();
        }

        protected void RawDataAdd( byte[] aRawData )
        {
            Descriptor.RawDataAdd( aRawData );
        }
        #endregion

        #region Properties
        public TState State
        {
            get
            {
                lock ( this )
                {
                    return iState;
                }
            }
            set
            {
                lock ( this )
                {
                    iState = value;
                }
            }
        }

        public CFFPlugin Plugin
        {
            get { return iPlugin; }
        }

        public CFFSource Descriptor
        {
            get { return iDescriptor; }
        }

        public CIEngine CIEngine
		{
            get { return Plugin.DataProvider.Engine; }
		}
		#endregion

		#region Internal methods
		protected void NotifyEvent( TEvent aEvent )
		{
            NotifyEvent( aEvent, null );
		}

        protected void NotifyEvent( TEvent aEvent, object aContext )
        {
            // We must notify about "reading started" and "reading complete" only
            // once.
            //
            // When the plugin sends the "reading started" event, we must transition
            // to 'EStateProcessing.'
            //
            // When the plugin sends the "reading complete" event, we must transition
            // to 'EStateReady' except if there was an exception during processing,
            // in which case we remain as 'EStateCorrupt.'
            //
            // NB: NotifyException makes the transition to EStateCorrupt.
            //
            TState oldState = State;
            //
            bool notify = false;
            switch ( aEvent )
            {
            case TEvent.EReadingContainerCreated:
                notify = true;
                break;
            case TEvent.EReadingStarted:
                State = TState.EStateProcessing;
                notify = ( State != oldState );
                break;
            case TEvent.EReadingProgress:
                notify = true;
                break;
            case TEvent.EReadingComplete:
                if ( State == TState.EStateCorrupt )
                {
                    // There was an exception during processing. Do not
                    // change to 'ready' state in this situation.
                    // 
                    // However, we must still notify about the completion event!
                    notify = true;
                }
                else
                {
                    State = TState.EStateReady;
                    notify = ( State != oldState );
                }
                break;
            default:
                throw new NotSupportedException();
            }
            
            if ( notify )
            {
                if ( Observer != null )
                {
                    Observer( aEvent, this, aContext );
                }
            }
        }

        protected void NotifyException( Exception aException )
        {
            State = TState.EStateCorrupt;
            //
            if ( ExceptionHandler != null )
            {
                ExceptionHandler( aException, this );
            }
        }
		#endregion

        #region Internal methods - called by CIEngineSource to carry out operations
        internal void OnNativeReadInit()
        {
            // Indicate reading started
            NotifyEvent( TEvent.EReadingStarted );
            try
            {
                NativeReadInit();
            }
            catch ( Exception e )
            {
                NotifyException( e );
                NotifyEvent( TEvent.EReadingComplete );
            }
        }

        internal void OnTraceReadInit()
        {
            // Indicate reading started
            NotifyEvent( TEvent.EReadingStarted );
            try
            {
                TraceReadInit();
            }
            catch ( Exception e )
            {
                NotifyException( e );
                NotifyEvent( TEvent.EReadingComplete );
            }
        }

        internal void OnTraceReadOffer( CFFTraceLine aLine )
        {
            try
            {
                bool consumed = TraceReadOffer( aLine );
                if ( consumed )
                {
                    RawDataAdd( aLine.ToBinary() );
                }
            }
            catch ( Exception e )
            {
                NotifyException( e );
            }
        }

        internal void OnTraceReadComplete()
        {
            try
            {
                TraceReadComplete();
            }
            catch ( Exception e )
            {
                NotifyException( e );
                NotifyEvent( TEvent.EReadingComplete );
            }
        }
        #endregion

		#region Data members
        private TState iState = TState.EStateUninitialised;
        private readonly CFFPlugin iPlugin;
        private readonly CFFSource iDescriptor;
		#endregion
	}
}
