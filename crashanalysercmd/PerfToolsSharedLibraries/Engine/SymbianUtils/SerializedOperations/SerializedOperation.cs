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

namespace SymbianUtils.SerializedOperations
{
    public abstract class SerializedOperation : DisposableObject, IComparable<SerializedOperation>
    {
        #region Delegates & events
        public delegate void EventHandler( SerializedOperation aOperation );
        public event EventHandler Started;
        public event EventHandler Completed;
        #endregion

        #region Constructors
        protected SerializedOperation()
            : this( true )
        {
        }

        protected SerializedOperation( bool aEnabled )
        {
            Enabled = aEnabled;
            //
            iWorker.DoWork += new DoWorkEventHandler( DoWork );
        }
        #endregion

        #region Framework API
        protected abstract void PerformOperation();

        public virtual long Priority
        {
            get { return 0; }
        }
        #endregion

        #region API
        public void EnableAndWait()
        {
            Trace( "[{0}] EnableAndWait() - START - enabled: {1}, busy: {2}", this.GetType().Name, Enabled, IsBusy );
            System.Diagnostics.Debug.Assert( Enabled == false );
            System.Diagnostics.Debug.Assert( !IsBusy );

            if ( IsBusy )
            {
                while ( IsBusy )
                {
                    Trace( "[{0}] EnableAndWait() - still busy?: {1}", this.GetType().Name, IsBusy );
                    Thread.Sleep( 10 );
                }
            }
            else
            {
                SymbianUtils.SymDebug.SymDebugger.Assert( iWaitForCompletion == null );
                using ( iWaitForCompletion = new AutoResetEvent( false ) )
                {
                    // Enable the operation
                    Trace( "[{0}] EnableAndWait() - enabling item...", this.GetType().Name );
                    Enabled = true;

                    // And now block until it completes
                    Trace( "[{0}] EnableAndWait() - sleeping...", this.GetType().Name );
                    iWaitForCompletion.WaitOne();
                    Trace( "[{0}] EnableAndWait() - slept!", this.GetType().Name );
                }
            }

            Trace( "[{0}] EnableAndWait() - END", this.GetType().Name );
        }
        #endregion

        #region Properties
        public object Tag
        {
            get { return iTag; }
            set { iTag = value; }
        }

        public bool IsBusy
        {
            get { return iWorker.IsBusy; }
        }

        public bool Enabled
        {
            get { return iEnabled; }
            set
            {
                lock ( iEnabledLock )
                {
                    if ( value != iEnabled )
                    {
                        iEnabled = value;
                        if ( OperationManager != null )
                        {
                            OperationManager.OnOperationEnabledStatusChanged( this );
                        }
                    }
                }
            }
        }

        internal SerializedOperationManager OperationManager
        {
            get { return iManager; }
            set { iManager = value; }
        }
        #endregion

        #region Event handlers
        private void DoWork( object aSender, DoWorkEventArgs aArgs )
        {
            System.Diagnostics.Debug.Assert( OperationManager != null );
            NotifyStarted();
            //
            try
            {
                PerformOperation();
            }
            catch ( Exception opException )
            {
                OperationManager.Trace( string.Format( "OPERATION EXCEPTION MSG - op: {0} ({1}) - msg: {2}", this.ToString(), this.GetType(), opException.Message ) );
                OperationManager.Trace( string.Format( "OPERATION EXCEPTION STK       {0}", opException.StackTrace ) );
            }
            //
            NotifyCompleted();
        }

        private void WorkerCompleted()
        {
            Trace( "[{0}] WorkerCompleted() - START", this.GetType().Name );
            //
            System.Diagnostics.Debug.Assert( OperationManager != null );
            OperationManager.OperationCompleted( this );
            //
            if ( iWaitForCompletion != null )
            {
                Trace( "[{0}] WorkerCompleted() - unblocking main thread...", this.GetType().Name );
                iWaitForCompletion.Set();
            }
            //
            Trace( "[{0}] WorkerCompleted() - END", this.GetType().Name );
        }
        #endregion

        #region Internal methods
        protected void Trace( string aMessage )
        {
            iManager.Trace( aMessage );
        }

        protected void Trace( string aFormat, params object[] aParams )
        {
            iManager.Trace( aFormat, aParams );
        }

        internal void Start()
        {
            Start( null );
        }

        internal void Start( object aParameter )
        {
            System.Diagnostics.Debug.Assert( Enabled );
            iWorker.RunWorkerAsync( aParameter );
        }

        private void NotifyStarted()
        {
            try
            {
                if ( Started != null )
                {
                    Started( this );
                }
            }
            catch ( Exception )
            {
            }
        }

        private void NotifyCompleted()
        {
            try
            {
                if ( Completed != null )
                {
                    Completed( this );
                }
                try
                {
                    WorkerCompleted();
                }
                catch ( Exception )
                {
                }
            }
            catch ( Exception )
            {
            }
        }
        #endregion

        #region From IComparable<SerializedOperation>
        public int CompareTo( SerializedOperation aOther )
        {
            int ret = 1;
            //
            if ( aOther != null )
            {
                ret = this.Priority.CompareTo( aOther.Priority );
            }
            //
            return ret;
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
                if ( iWaitForCompletion != null )
                {
                    iWaitForCompletion.Close();
                    iWaitForCompletion = null;
                }
                if ( iWorker != null )
                {
                    iWorker.Dispose();
                    iWorker = null;
                }
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return this.GetType().Name;
        }
        #endregion

        #region Data members
        private object iTag = null;
        private object iEnabledLock = new object();
        private bool iEnabled = true;
        private SerializedOperationManager iManager = null;
        private BackgroundWorker iWorker = new BackgroundWorker();
        private AutoResetEvent iWaitForCompletion = null;
        #endregion
    }
}
