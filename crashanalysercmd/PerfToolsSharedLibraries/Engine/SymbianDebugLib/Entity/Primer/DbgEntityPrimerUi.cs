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
using System.Threading;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using SymbianUtils;
using SymbianUtils.PluginManager;
using SymbianUtils.FileSystem;
using SymbianUtils.Settings;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI;

namespace SymbianDebugLib.Entity.Primer
{
    public partial class DbgEntityPrimerUi : Form, IDbgEntityPrimer
    {
        #region Constructors
        public DbgEntityPrimerUi( DbgEntity aEntity, DbgPluginEngine aPlugin )
        {
            iEntity = aEntity;
            iPlugin = aPlugin;

            // Make a new primer and seed it with the entity.
            iPrimer = aPlugin.CreatePrimer();
            iPrimer.Add( aEntity );

            // Listen to plugin primer events
            iPrimer.EventHandler += new DbgPluginPrimer.PrimeEventHandler( PrimerPlugin_EventHandler );
            //
            this.InitializeComponent();
            this.Text = string.Format( "Preparing: [{0}]", Path.GetFileName( aEntity.FullName ) );
        }
        #endregion

        #region From IDbgEntityPrimer
        public void Prime( TSynchronicity aSynchronicity )
        {
            // Timer initiates operation
            iTimer_OpStart.Tag = aSynchronicity;
            iTimer_OpStart.Start();

            // Show the dialog
            base.ShowDialog();
        }

        public string PrimeErrorMessage
        {
            get { return iEntity.PrimerResult.PrimeErrorMessage; }
        }

        public Exception PrimeException
        {
            get { return iEntity.PrimerResult.PrimeException; }
            internal set { iEntity.PrimerResult.PrimeException = value; }
        }
        #endregion

        #region Event handlers
        private void Timer_OpStart_Tick( object sender, EventArgs aArgs )
        {
            iTimer_OpStart.Stop();
            iTimer_OpStart.Enabled = false;
            //
            TSynchronicity syncMode = (TSynchronicity) iTimer_OpStart.Tag;
            PrimeException = null;

            // If requested to prime asynchronously then we don't need to 
            // do anything because this will return immediately and the
            // prime will run in a background thread.
            //
            // On the other hand, if requesting synchronous priming then 
            // we need to spawn a worker thread or else the progress dialog
            // will not redraw (since the synchronous operation runs within
            // the context of the thread in which the synchronous prime request
            // originates).
            switch ( syncMode )
            {
            case TSynchronicity.EAsynchronous:
                RunPrime( TSynchronicity.EAsynchronous );
                break;
            case TSynchronicity.ESynchronous:
                ThreadPool.QueueUserWorkItem( new WaitCallback( RunSyncPrimeInWorkerThread ) );
                break;
            default:
                throw new NotSupportedException( "Unsupported synchronicity" );
            }
        }
        #endregion

        #region Event handlers
        private void PrimerPlugin_EventHandler( DbgPluginPrimer.TPrimeEvent aEvent, object aData )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "PrimerPlugin - event handler - {0} - {1}", aEvent, aData ) );
            if ( InvokeRequired )
            {
                DbgPluginPrimer.PrimeEventHandler callback = new DbgPluginPrimer.PrimeEventHandler( PrimerPlugin_EventHandler );
                this.BeginInvoke( callback, new object[] { aEvent, aData } );
            }
            else
            {
                switch ( aEvent )
                {
                case DbgPluginPrimer.TPrimeEvent.EEventPrimingStarted:
                    iProgressBar.Maximum = 100; //%
                    iProgressBar.Minimum = 0; //%
                    iProgressBar.Value = 0;
                    OnPrimeStart();
                    break;
                case DbgPluginPrimer.TPrimeEvent.EEventPrimingProgress:
                    if ( aData != null && ( aData is int ) )
                    {
                        int prog = (int) aData;
                        OnPrimeProgress( prog );
                        iProgressBar.Value = prog;
                    }
                    break;
                case DbgPluginPrimer.TPrimeEvent.EEventPrimingComplete:
                    iProgressBar.Value = 100;
                    OnPrimeComplete();
                    Close();
                    break;
                }
            }
            //
            Application.DoEvents();
        }
        #endregion

        #region Event cascading
        protected void OnPrimeStart()
        {
            bool sent = ( ( iFlags & TFlags.EFlagsSentEventStart ) == TFlags.EFlagsSentEventStart );
            if ( !sent )
            {
                iEntity.OnPrimeStart( this );
                iFlags |= TFlags.EFlagsSentEventStart;
            }
        }

        protected void OnPrimeProgress( int aValue )
        {
            iEntity.OnPrimeProgress( this, aValue );
        }

        protected void OnPrimeComplete()
        {
            bool sent = ( ( iFlags & TFlags.EFlagsSentEventComplete ) == TFlags.EFlagsSentEventComplete );
            if ( !sent )
            {
                // Update entity's primer results
                iEntity.OnPrimeComplete( this );
                iFlags |= TFlags.EFlagsSentEventComplete;
            }
        }
        #endregion

        #region Internal enumerations
        [Flags]
        private enum TFlags
        {
            EFlagsNone = 0,
            EFlagsSentEventStart = 1,
            EFlagsSentEventComplete = 2
        }
        #endregion

        #region Internal methods
        private void RunSyncPrimeInWorkerThread( object aNotUsed )
        {
            RunPrime( TSynchronicity.ESynchronous );
        }

        private void RunPrime( TSynchronicity aSynchronicity )
        {
            try
            {
                iPrimer.Prime( aSynchronicity );
            }
            catch ( Exception e )
            {
                OnPrimeStart();
                PrimeException = e;
                OnPrimeComplete();
            }
        }
        #endregion

        #region Data members
        private readonly DbgEntity iEntity;
        private readonly DbgPluginEngine iPlugin;
        private readonly DbgPluginPrimer iPrimer;
        private TFlags iFlags = TFlags.EFlagsNone;
        #endregion
    }
}
