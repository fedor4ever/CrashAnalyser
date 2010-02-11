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
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;

namespace SymbianUtils.Threading
{
    public class MultiThreadedProcessor<T> : DisposableObject
    {
        #region Enumerations
        public enum TEvent
        {
            EEventStarting = 0,
            EEventCompleted
        }
        #endregion

        #region Delegates & events
        public delegate void ProcessorEventHandler( TEvent aEvent );
        public event ProcessorEventHandler EventHandler = delegate { };
        //
        public delegate void ItemProcessor( T aItem );
        public event ItemProcessor ProcessItem = null;
        //
        public delegate void ExceptionHandler( Exception aException );
        public event ExceptionHandler Exception;
        #endregion
        
        #region Constructors
        public MultiThreadedProcessor( IEnumerable<T> aCollection )
            : this( aCollection, ThreadPriority.Normal )
        {
        }

        public MultiThreadedProcessor( IEnumerable<T> aCollection, ThreadPriority aPriority )
        {
            PopulateQueue( aCollection );
            iThreadPriorities = aPriority;
            iProcessorCount = System.Environment.ProcessorCount;
        }
        #endregion

        #region Framework API
        public virtual void Start( TSynchronicity aSynchronicity )
        {
            iSynchronicity = aSynchronicity;
            OnEvent( MultiThreadedProcessor<T>.TEvent.EEventStarting );

            int count = iQueue.Count;
            if ( count == 0 )
            {
                // Nothing to do!
                OnEvent( MultiThreadedProcessor<T>.TEvent.EEventCompleted );
            }
            else
            {
                // For sync mode, we need to block until the operation
                // completes.
                DestroyBlocker();
                if ( aSynchronicity == TSynchronicity.ESynchronous )
                {
                    iSynchronousBlocker = new ManualResetEvent( false );
                }

                // Create worker threads to process queue items. One per
                // processor core.
                CreateThreads();

                if ( aSynchronicity == TSynchronicity.ESynchronous )
                {
                    System.Diagnostics.Debug.Assert( iSynchronousBlocker != null );

                    // Now wait.
                    using ( iSynchronousBlocker )
                    {
                        iSynchronousBlocker.WaitOne();
                    }
                    iSynchronousBlocker = null;

                    // See comments in "RunThread" below for details about why
                    // we do this here - it avoids a race condition.
                    OperationComplete();
                }
            }
        }

        protected virtual bool Process( T aItem )
        {
            return false;
        }

        protected virtual void OnException( Exception aException )
        {
            if ( Exception != null )
            {
                Exception( aException );
            }
        }
        #endregion
        
        #region Properties
        #endregion

        #region Internal methods
        protected void PopulateQueue( IEnumerable<T> aCollection )
        {
            iQueue.Clear();
            //
            foreach ( T item in aCollection )
            {
                iQueue.Enqueue( item );
            }
        }

        private void CreateThreads()
        {
            Random r = new Random( DateTime.Now.Millisecond );
            //
            int count = System.Environment.ProcessorCount;
            for ( int i = 0; i < count; i++ )
            {
                string name = string.Format( "Processor Thread {0:d3} {1:d8}", i, r.Next() );
                Thread t = new Thread( new ThreadStart( RunThread ) );
                t.IsBackground = true;
                t.Priority = iThreadPriorities;
                iThreads.Add( t );
                //
                t.Start();
            }
        }

        private void RunThread()
        {
            // Process items until none are left.
            while ( iQueue.Count > 0 )
            {
                T item;
                //
                bool dequeued = iQueue.TryToDequeue( out item );
                if ( dequeued )
                {
                    // First try virtual function call. If that fails then 
                    // we'll resort to trying an event handler.
                    try
                    {
                        bool processed = Process( item );
                        if ( processed == false && ProcessItem != null )
                        {
                            ProcessItem( item );
                        }
                    }
                    catch ( Exception e )
                    {
                        // Let the derived class handle exceptions
                        OnException( e );
                    }
                }
            }

            // If all the threads have finished then the entire
            // operation is complete.
            bool finished = false;
            lock ( iThreads )
            {
                iThreads.Remove( Thread.CurrentThread );
                finished = ( iThreads.Count == 0 );
            }

            // Check for completion
            if ( finished )
            {
                // If we're operating synchronously, then let the main
                // thread (the one that is currently blocked) report
                // completion. This prevents a race condition whereby the worker
                // threads (i.e. the thread in which this function is running)
                // notifies about completion before the main thread has started
                // blocking (waiting). This can cause an exception whereby 
                // the client might dispose of this object twice.
                if ( iSynchronicity == TSynchronicity.EAsynchronous )
                {
                    OperationComplete();
                }
                else
                {
                    // Will be done by "Start"
                }

                // Always release the blocker to "unblock" the main thread
                ReleaseBlocker();
            }
        }

        private void OperationComplete()
        {
            OnEvent( MultiThreadedProcessor<T>.TEvent.EEventCompleted );
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

        protected virtual void OnEvent( TEvent aEvent )
        {
            EventHandler( aEvent );
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
                DestroyBlocker();
            }
        }
        #endregion

        #region Data members
        private readonly int iProcessorCount;
        private readonly ThreadPriority iThreadPriorities;
        private BlockingQueue<T> iQueue = new BlockingQueue<T>();
        private List<Thread> iThreads = new List<Thread>();
        private ManualResetEvent iSynchronousBlocker = null;
        private TSynchronicity iSynchronicity = TSynchronicity.ESynchronous;
        #endregion
    }
}
