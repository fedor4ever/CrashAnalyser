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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SymbianDebugLib.Engine;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianUtils.Threading;
using SymbianUtils.FileSystem;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Container;
using CrashItemLib.Engine.Interfaces;
using CrashItemLib.Engine.Operations;
using CrashItemLib.Engine.Primer;
using CrashItemLib.Engine.ProblemDetectors;
using CrashItemLib.Engine.Sources;
using CrashItemLib.PluginAPI;
using CrashItemLib.Sink;

namespace CrashItemLib.Engine
{
    public class CIEngine : DisposableObject, IEnumerable<CIContainer>, ITracer
    {
        #region Enumerations
        public enum TState
        {
            // We've started to process the crash sources
            EStateProcessingStarted = 0,

            // Engine is idle & fully ready for display & output
            EStateProcessingComplete
        }

        public enum TSourceEvent
        {
            EEventSourceReady = 0,
            EEventSourceProgress,
            EEventSourceStateChanged
        }

        public enum TCrashEvent
        {
            EEventCrashAdded = 0,
            EEventCrashRemoved,
            EEventCrashRemovedAll
        }
        #endregion

        #region Delegates & events
        public delegate void CIEngineStateHandler( TState aEvent );
        public event CIEngineStateHandler StateChanged = delegate( TState aState ) { };

        public delegate void CIEngineSourceObserver( TSourceEvent aEvent, CIEngineSource aSource, object aParameter );
        public event CIEngineSourceObserver SourceObservers = delegate( TSourceEvent aEvent, CIEngineSource aSource, object aParameter ) { };

        public delegate void CIEngineCrashObserver( TCrashEvent aEvent, CIContainer aContainer );
        public event CIEngineCrashObserver CrashObservers;

        public delegate void CIExceptionHandler( Exception aException );
        public event CIExceptionHandler ExceptionHandlers = delegate( Exception aException ) { };
        #endregion

        #region Constructors
        public CIEngine( DbgEngine aDebugEngine, ICIEngineUI aUI )
        {
            iUI = aUI;
            iDebugEngine = aDebugEngine;
            //
            iPrimer = new CIEnginePrimer( this );
            iPlugins = new CFFPluginRegistry( this );
            iSinkManager = new CISinkManager( this );
            iSources = new CIEngineSourceCollection( this );
            iContainerIndex = new CIContainerIndex( this );
            iContainerCollection = new CIContainerCollection();
            iOperationManager = new CIEngineOperationManager( this );
            iProblemDetectorManager = new CIProblemDetectorManager( this );
        }
        #endregion

        #region API
        public void ClearAll()
        {
            lock ( iSources )
            {
                iSources.Clear();
            }
            ClearCrashes();
        }

        public void ClearCrashes()
        {
            bool notify = true;
            lock ( iContainerCollection )
            {
                notify = iContainerCollection.Count > 0;
                iContainerCollection.Clear();
            }
            //
            if ( notify )
            {
                OnContainerRemovedAll();
            }
        }

        public bool Prime( FileInfo aFile )
        {
            bool success = false;
            //
            try
            {
                success = iPrimer.Prime( aFile );
            }
            catch ( Exception )
            {
            }
            //
            return success;
        }

        public bool Prime( DirectoryInfo aDirectory )
        {
            bool success = false;
            //
            try
            {
                iPrimer.Prime( aDirectory );
                success = true;
            }
            catch ( Exception )
            {
            }
            //
            return success;
        }

        public bool PrimeRecursive( DirectoryInfo aDirectory )
        {
            bool success = false;
            //
            try
            {
                iPrimer.PrimeRecursive( aDirectory );
                success = true;
            }
            catch ( Exception )
            {
            }
            //
            return success;
        }

        public void IdentifyCrashes( TSynchronicity aSynchronicity )
        {
            // If we must operate in synchronous mode, that is, we must not
            // return until the all the crash item containers are prepared,
            // then we created a blocking object that will be signalled when
            // all the asynchronous operations are complete.
            DestroyBlocker();
            if ( aSynchronicity == TSynchronicity.ESynchronous )
            {
                iSynchronousBlocker = new ManualResetEvent( false );
            }

            // Next, we start processing the source files in order to create
            // crash containers.
            DestroySourceProcessor();
            iSourceProcessor = new CIEngineSourceProcessor( iSources );
            iSourceProcessor.EventHandler += new CIEngineSourceProcessor.ProcessorEventHandler( SourceProcessor_EventHandler );
            iSourceProcessor.Start( TSynchronicity.EAsynchronous );

            // Now we operate asynchronously. When the source processor has read
            // all of the CIEngineSource objects, it will trigger and event
            // callback (SourceProcessor_EventHandler) which will cause us to start
            // the serialized operation manager.
            //
            // When the serialized operation manager completes, it will again indicate
            // this via an event callback at which point we will trigger the manual
            // reset event (blocker) and therefore resume this main thread.
            if ( aSynchronicity == TSynchronicity.ESynchronous )
            {
                // Now wait. 
                using ( iSynchronousBlocker )
                {
                    iSynchronousBlocker.WaitOne();
                }
                iSynchronousBlocker = null;
            }
        }
        #endregion

        #region Properties
        public int Count
        {
            get
            {
                lock ( iContainerCollection )
                {
                    return iContainerCollection.Count;
                }
            }
        }

        public CIContainer this[ int aIndex ]
        {
            get
            {
                lock ( iContainerCollection )
                {
                    return iContainerCollection[ aIndex ];
                }
            }
        }

        public DbgEngine DebugEngine
        {
            get { return iDebugEngine; }
        }

        public CISinkManager SinkManager
        {
            get { return iSinkManager; }
        }

        public CFFPluginRegistry PluginRegistry
        {
            get { return iPlugins; }
        }

        public CIEngineSourceCollection Sources
        {
            get { return iSources; }
        }
        #endregion

        #region Event handlers
        private void SourceProcessor_EventHandler( CIEngineSourceProcessor.TEvent aEvent )
        {
            this.Trace( "[CIEngine] SourceProcessor_EventHandler() - START - aEvent: {0}", aEvent );
            //
            if ( aEvent == CIEngineSourceProcessor.TEvent.EEventStarting )
            {
                OnStateChanged( TState.EStateProcessingStarted );
                ClearCrashes();
            }
            else if ( aEvent == CIEngineSourceProcessor.TEvent.EEventCompleted )
            {
                DestroySourceProcessor();
                //
                DestroyIndexProcessor();
                iIndexProcessor = new CIContainerIndexProcessor( iContainerIndex, this );
                iIndexProcessor.EventHandler += new CIContainerIndexProcessor.ProcessorEventHandler( IndexProcessor_EventHandler );
                iIndexProcessor.Start( TSynchronicity.EAsynchronous );
            }
            //
            this.Trace( "[CIEngine] SourceProcessor_EventHandler() - END - aEvent: {0}", aEvent );
        }

        private void IndexProcessor_EventHandler( CIContainerIndexProcessor.TEvent aEvent )
        {
            this.Trace( "[CIEngine] IndexProcessor_EventHandler() - START - aEvent: {0}", aEvent );
            //
            if ( aEvent == CIContainerIndexProcessor.TEvent.EEventStarting )
            {
            }
            else if ( aEvent == CIContainerIndexProcessor.TEvent.EEventCompleted )
            {
                DestroyIndexProcessor();
   
                // All the sources have been processed. Create any problem detectors.
                iProblemDetectorManager.EventHandler += new MultiThreadedProcessor<CIProblemDetector>.ProcessorEventHandler( ProblemDetectorManager_EventHandler );
                iProblemDetectorManager.Start( TSynchronicity.EAsynchronous );
            }
            //
            this.Trace( "[CIEngine] IndexProcessor_EventHandler() - END - aEvent: {0}", aEvent );
        }

        private void ProblemDetectorManager_EventHandler( MultiThreadedProcessor<CIProblemDetector>.TEvent aEvent )
        {
            this.Trace( "[CIEngine] ProblemDetectorManager_EventHandler() - START - aEvent: {0}", aEvent );
            //
            if ( aEvent == MultiThreadedProcessor<CIProblemDetector>.TEvent.EEventStarting )
            {
            }
            else if ( aEvent == MultiThreadedProcessor<CIProblemDetector>.TEvent.EEventCompleted )
            {
                iProblemDetectorManager.EventHandler -= new MultiThreadedProcessor<CIProblemDetector>.ProcessorEventHandler( ProblemDetectorManager_EventHandler );

                // Run any serialized operations
                iOperationManager.StateHandler += new CIEngineOperationManager.QueueStateHandler( OperationManager_StateHandler );
                iOperationManager.Start();
            }
            //
            this.Trace( "[CIEngine] ProblemDetectorManager_EventHandler() - END - aEvent: {0}", aEvent );
        }
     
        private void OperationManager_StateHandler( CIEngineOperationManager.TState aState )
        {
            this.Trace( "[CIEngine] OperationManager_StateHandler() - START - aState: {0}", aState );
            //
            if ( aState == SymbianUtils.SerializedOperations.SerializedOperationManager.TState.EStateOperationsCompleted )
            {
                iOperationManager.StateHandler -= new CIEngineOperationManager.QueueStateHandler( OperationManager_StateHandler );
                //
                IdentifyCrashesComplete();
            }
            //
            this.Trace( "[CIEngine] OperationManager_StateHandler() - END - aState: {0}", aState );
        }
        #endregion

        #region Internal delegates
        private delegate void VoidHandler();
        #endregion

        #region Internal methods
        internal void Add( CIContainer aContainer )
        {
            string fileName = aContainer.Source.MasterFileName;
            if ( string.IsNullOrEmpty( fileName ) )
            {
                throw new ArgumentException( "Container source file name cannot be empty" );
            }
            //
            lock ( iContainerCollection )
            {
                iContainerCollection.Add( aContainer );
            }
            //
            if ( CrashObservers != null )
            {
                CrashObservers( TCrashEvent.EEventCrashAdded, aContainer );
            }
        }

        internal int GetNextElementId()
        {
            return iIdProvider.GetNextId();
        }
        
        internal void QueueOperation( CIEngineOperation aOperation )
        {
            this.Trace( "[CIEngine] QueueOperation() - aOperation: {0} ({1})", aOperation, aOperation.GetType() );
            iOperationManager.Queue( aOperation );
        }

        internal void OnSourceStateChanged( CIEngineSource aSource )
        {
            this.Trace( "[CIEngine] OnSourceStateChanged() - START - aSource: {0}, aSource.IsReady: {1}, aSource.State: {2}", aSource.FileName, aSource.IsReady, aSource.State );
            
            SourceObservers( TSourceEvent.EEventSourceStateChanged, aSource, null );
            if ( aSource.IsReady )
            {
                SourceObservers( TSourceEvent.EEventSourceReady, aSource, null );
            }

            this.Trace( "[CIEngine] OnSourceStateChanged() - END - aSource: {0}, aSource.IsReady: {1}, aSource.State: {2}", aSource.FileName, aSource.IsReady, aSource.State );
        }

        internal void OnSourceProgress( CIEngineSource aSource, int aProgress )
        {
            this.Trace( "[CIEngine] OnSourceProgress() - START - aSource: {0}, aSource.IsReady: {1}, aProgress: {2}", aSource.FileName, aSource.IsReady, aProgress );

            SourceObservers( TSourceEvent.EEventSourceProgress, aSource, aProgress );

            this.Trace( "[CIEngine] OnSourceProgress() - END - aSource: {0}, aSource.IsReady: {1}, aProgress: {2}", aSource.FileName, aSource.IsReady, aProgress );
        }

        internal void OnException( Exception aException )
        {
            this.Trace( "[CIEngine] OnException() - {0} / {1}", aException.Message, aException.StackTrace );
            ExceptionHandlers( aException );
        }

        internal void OnContainerRemovedAll()
        {
            if ( CrashObservers != null )
            {
                CrashObservers( TCrashEvent.EEventCrashRemovedAll, null );
            }
        }

        private void Remove( CIContainer aContainer )
        {
            lock ( iContainerCollection )
            {
                if ( iContainerCollection.Contains( aContainer ) )
                {
                    iContainerCollection.Remove( aContainer );
                    if ( CrashObservers != null )
                    {
                        CrashObservers( TCrashEvent.EEventCrashRemoved, aContainer );
                    }
                }
            }
        }

        private void OnStateChanged( TState aEvent )
        {
            this.Trace( "[CIEngine] OnStateChanged() - START - aEvent: {0}", aEvent );
            if ( StateChanged != null )
            {
                StateChanged( aEvent );
            }
            this.Trace( "[CIEngine] OnStateChanged() - END - aEvent: {0}", aEvent );
        }

        private void IdentifyCrashesComplete()
        {
            this.Trace( "[CIEngine] IdentifyCrashesComplete()" );
            OnStateChanged( TState.EStateProcessingComplete );
            //
            ReleaseBlocker();
        }

        private void DestroyBlocker()
        {
            if ( iSynchronousBlocker != null )
            {
                iSynchronousBlocker.Close();
                iSynchronousBlocker = null;
            }
        }

        private void ReleaseBlocker()
        {
            if ( iSynchronousBlocker != null )
            {
                iSynchronousBlocker.Set();
            }
        }

        private void DestroySourceProcessor()
        {
            if ( iSourceProcessor != null )
            {
                iSourceProcessor.EventHandler -= new CIEngineSourceProcessor.ProcessorEventHandler( SourceProcessor_EventHandler );
                iSourceProcessor.Dispose();
                iSourceProcessor = null;
            }
        }

        private void DestroyIndexProcessor()
        {
            if ( iIndexProcessor != null )
            {
                iIndexProcessor.EventHandler -= new CIContainerIndexProcessor.ProcessorEventHandler( IndexProcessor_EventHandler );
                iIndexProcessor = null;
            }
        }

        private void DestroyProblemDetectorManager()
        {
            if ( iProblemDetectorManager != null )
            {
                iProblemDetectorManager.EventHandler -= new MultiThreadedProcessor<CIProblemDetector>.ProcessorEventHandler( ProblemDetectorManager_EventHandler );
                iProblemDetectorManager.Dispose();
            }
        }

        private void DestroyOperationManager()
        {
            if ( iOperationManager != null )
            {
                iOperationManager.StateHandler -= new CIEngineOperationManager.QueueStateHandler( OperationManager_StateHandler );
                iOperationManager.Dispose();
            }
        }
        #endregion

        #region Internal properties
        internal ICIEngineUI UI
        {
            get { return iUI; }
        }
        #endregion

        #region From System.Object
        #endregion

        #region From ITracer 
        public void Trace( string aMessage )
        {
            UI.CITrace( aMessage );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            UI.CITrace( aFormat, aParams );
        }
        #endregion

        #region From IEnumerable<CIContainer>
        public IEnumerator<CIContainer> GetEnumerator()
        {
            lock ( iContainerCollection )
            {
                foreach ( CIContainer container in iContainerCollection )
                {
                    yield return container;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock ( iContainerCollection )
            {
                foreach ( CIContainer container in iContainerCollection )
                {
                    yield return container;
                }
            }
        }
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
                DestroyIndexProcessor();
                DestroySourceProcessor();
                DestroyProblemDetectorManager();
                DestroyOperationManager();
                DestroyBlocker();
            }
        }
        #endregion

        #region Data members
        private readonly DbgEngine iDebugEngine;
        private readonly ICIEngineUI iUI;
        private readonly CIEnginePrimer iPrimer;
        private readonly CISinkManager iSinkManager;
        private readonly CFFPluginRegistry iPlugins;
        private readonly CIEngineSourceCollection iSources;
        private readonly CIContainerCollection iContainerCollection;
        private readonly CIEngineOperationManager iOperationManager;
        private readonly CIProblemDetectorManager iProblemDetectorManager;
        private readonly CIContainerIndex iContainerIndex;
        private ManualResetEvent iSynchronousBlocker = null;
        private CIEngineSourceProcessor iSourceProcessor = null;
        private CIContainerIndexProcessor  iIndexProcessor = null;
        private CIElementIdProvider iIdProvider = new CIElementIdProvider();
		#endregion
    }
}
