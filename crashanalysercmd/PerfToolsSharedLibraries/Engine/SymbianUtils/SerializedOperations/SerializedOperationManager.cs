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
using System.Text;
using System.Threading;
using System.ComponentModel;
using SymbianUtils.Tracer;

namespace SymbianUtils.SerializedOperations
{
    public class SerializedOperationManager : DisposableObject, ITracer
    {
        #region Enumerations
        public enum TState
        {
            EStateOperationsStarted = 0,
            EStateOperationsSuspended,
            EStateOperationsCompleted
        }
        #endregion

        #region Delegates & events
        public delegate void QueueStateHandler( TState aState );
        public event QueueStateHandler StateHandler;
        #endregion

        #region Constructors
        public SerializedOperationManager()
            : this( null )
        {
        }

        public SerializedOperationManager( ITracer aTracer )
        {
            iTracer = aTracer;
        }
        #endregion

        #region API
        public virtual void Clear()
        {
            lock ( iQueue )
            {
                Stop();
                iQueue.Clear();
            }
        }

        public void Start()
        {
            Enabled = true;
            StartNextOperation();
        }

        public void Stop()
        {
            Enabled = false;
        }

        public void Queue( SerializedOperation aOperation )
        {
            Trace( "Queue() - START - enabled: {0}, count: {1}, OpInProg: {2}", Enabled, iQueue.Count, OperationInProgress );
            lock ( iQueue )
            {
                Trace( "Queue() - aOperation.Enabled: {0}, aOperation: {1}", aOperation.Enabled, aOperation.GetType().Name );

                aOperation.OperationManager = this;
                iQueue.Add( aOperation );

                // Sort the list so that the highest priority item is first
                Comparison<SerializedOperation> sortByPriority = delegate( SerializedOperation aLeft, SerializedOperation aRight )
                {
                    // We want highest-to-lowest sort order
                    int ret = aLeft.CompareTo( aRight );
                    return ret * -1;
                };
                iQueue.Sort( sortByPriority );

                Trace( "Queue() - {{{0}}} - Queue now contains {1} entries, amEnabled: {2}", aOperation.GetType(), Count, Enabled );
            }

            StartNextOperation();
            Trace( "Queue() - END - enabled: {0}, count: {1}, OpInProg: {2}", Enabled, iQueue.Count, OperationInProgress );
            Trace( "" );
            Trace( "" );
        }
        #endregion

        #region Properties
        public int Count
        {
            get
            {
                lock( iQueue )
                {
                    return iQueue.Count;
                }
            }
        }

        public bool Enabled
        {
            get { return iEnabled; }
            private set
            {
                lock ( iEnabledLock )
                {
                    if ( iEnabled != value )
                    {
                        Trace( "Enabled - current value: {0}, new value: {1}", iEnabled, value );
                        iEnabled = value;
                        //
                        TState e = value ? TState.EStateOperationsStarted : TState.EStateOperationsSuspended;
                        if ( StateHandler != null )
                        {
                            StateHandler( e );
                        }
                    }
                }
            }
        }

        public ITracer Tracer
        {
            get
            {
                ITracer ret = this;
                //
                if ( iTracer != null )
                {
                    ret = iTracer;
                }
                //
                return ret;
            }
            set { iTracer = value; }
        }

        public bool OperationInProgress
        {
            get
            {
                lock ( iQueue )
                {
                    return iPendingOperation;
                }
            }
            private set
            {
                lock ( iQueue )
                {
                    iPendingOperation = value;
                }
            }
        }

        public bool IsCompleteAndIdle
        {
            get
            {
                bool ret = false;
                //
                lock ( iQueue )
                {
                    int enabledCount = OperationCountEnabled;
                    if ( Enabled == false )
                    {
                        // To be complete when we are disabled, then there must be
                        // no items that are ready to run.
                        ret = ( enabledCount == 0 );
                    }
                    else
                    {
                        // If we are enabled, then we are complete when there are
                        // no more enabled items left to run.
                        ret = ( enabledCount == 0 );
                    }
                }
                //
                return ret;
            }
        }
        #endregion

        #region ITracer Members
        public void Trace( string aMessage )
        {
            if ( iTracer != null )
            {
                iTracer.Trace( "[" + this.GetType().Name + "] " + aMessage );
            }
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            if ( iTracer != null )
            {
                string text = string.Format( aFormat, aParams );
                iTracer.Trace( "[" + this.GetType().Name + "] {0}", text );
            }
        }
        #endregion

        #region Internal methods
        internal void OnOperationEnabledStatusChanged( SerializedOperation aOperation )
        {
            StartNextOperation();
        }

        internal void OperationCompleted( SerializedOperation aOperation )
        {
            OperationInProgress = false;
            Trace( "OpComplete() - Operation completed {{{0}}}, queue contains {1} more entries, amEnabled: {2}", aOperation.GetType(), Count, Enabled );
            //
            StartNextOperation();
        }

        private void StartNextOperation()
        {
            TraceQueue( string.Format( "StartNextOperation() - START - Enabled: {0}", Enabled ) );
            
            lock ( iQueue )
            {
                Trace( "StartNextOperation() - count: {0}, OpInProg: {1}", iQueue.Count, OperationInProgress );
                bool enabled = Enabled;
                if ( enabled )
                {
                    if ( OperationInProgress )
                    {
                        Trace( "StartNextOperation() - Already running operation!" );
                    }
                    else
                    {
                        SerializedOperation op = FindNextOp();
                        if ( op != null )
                        {
                            iQueue.Remove( op );
                            //
                            Trace( string.Empty );
                            Trace( string.Empty );
                            Trace( "StartNextOperation() - ****************************************************************************" );
                            Trace( "StartNextOperation() - starting op: {0}/{1} [{2}]", 1, iQueue.Count + 1, op.GetType() + " - " + op.ToString() );
                            Trace( "StartNextOperation() - ****************************************************************************" );
                            //
                            op.Start();

                            OperationInProgress = true;
                        }
                        else
                        {
                            Trace( "StartNextOperation() - Queue is empty or no enabled items!" );
                            if ( StateHandler != null )
                            {
                                StateHandler( TState.EStateOperationsCompleted );
                            }
                        }
                    }
                }
                else
                {
                    Trace( "StartNextOperation() - Queue is disabled!" );
                }
            }

            Trace( "StartNextOperation() - END - count: {0}, OpInProg: {1}, Enabled: {2}", iQueue.Count, OperationInProgress, Enabled );
            Trace( "" );
        }

        private void TraceQueue( string aFrom )
        {
            StringBuilder text = new StringBuilder();

            text.AppendLine( string.Format( "========================== {0} ==========================", aFrom ) );
            text.AppendLine( string.Format( "opInPrgress: {0}", OperationInProgress ) );
            text.AppendLine( string.Format( "enabled:     {0}", Enabled ) );
            //
            lock ( iQueue )
            {
                int count = iQueue.Count;
                text.AppendLine( string.Format( "count:       {0}", count ) );
                text.AppendLine( "" );

                int i = 1;
                foreach ( SerializedOperation op in iQueue )
                {
                    text.AppendLine( string.Format( "[{0:d2}/{1:d2}] enabled: {2}, busy: {3}, pri: {4:d10}, name: {5}", i, count, System.Convert.ToInt32( op.Enabled ), System.Convert.ToInt32( op.IsBusy ), op.Priority, op.ToString() ) );
                    ++i;
                }
            }
            //
            text.AppendLine( "" );
            text.AppendLine( "" );
            //
            Trace( text.ToString() );
        }

        private SerializedOperation FindNextOp()
        {
            TraceQueue( "FindNextOp()" );
            SerializedOperation ret = null;
            //
            foreach ( SerializedOperation op in iQueue )
            {
                System.Diagnostics.Debug.Assert( !op.IsBusy );
                if ( op.Enabled )
                {
                    ret = op;
                    break;
                }
            }
            //
            return ret;
        }

        private int OperationCountEnabled
        {
            get
            {
                int ret = 0;
                //
                lock ( iQueue )
                {
                    Action<SerializedOperation> act = delegate(SerializedOperation op ) 
                    {
                        if ( op.Enabled ) 
                        { 
                            ++ret;
                        }
                    };
                    iQueue.ForEach( act ); 
                }
                //
                return ret;
            }
        }

        private int OperationCountDisabled
        {
            get
            {
                int ret = 0;
                //
                lock ( iQueue )
                {
                    Action<SerializedOperation> act = delegate( SerializedOperation op )
                    {
                        if ( !op.Enabled )
                        {
                            ++ret;
                        }
                    };
                    iQueue.ForEach( act );
                }
                //
                return ret;
            }
        }
        #endregion

        #region Data members
        private ITracer iTracer;
        private List<SerializedOperation> iQueue = new List<SerializedOperation>();
        private bool iPendingOperation = false;
        private bool iEnabled = false;
        private object iEnabledLock = new object();
        #endregion
    }
}
