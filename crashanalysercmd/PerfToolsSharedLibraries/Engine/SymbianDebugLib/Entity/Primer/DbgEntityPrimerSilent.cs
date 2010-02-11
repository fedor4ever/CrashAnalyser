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
using SymbianUtils;
using SymbianUtils.PluginManager;
using SymbianUtils.FileSystem;
using SymbianUtils.Settings;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI;

namespace SymbianDebugLib.Entity.Primer
{
    internal class DbgEntityPrimerSilent : IDbgEntityPrimer
    {
        #region Constructors
        public DbgEntityPrimerSilent( DbgEntity aEntity, DbgPluginEngine aPlugin )
        {
            iEntity = aEntity;
            iPlugin = aPlugin;

            // Make a new primer and seed it with the entity.
            iPrimer = aPlugin.CreatePrimer();
            iPrimer.Add( aEntity );

            // Listen to plugin primer events
            iPrimer.EventHandler += new DbgPluginPrimer.PrimeEventHandler( PrimerPlugin_EventHandler );
        }
        #endregion

        #region From IDbgEntityPrimer
        public void Prime( TSynchronicity aSynchronicity )
        {
            PrimeException = null;
            //
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

        #region Properties
        public bool PrimedOkay
        {
            get { return iEntity.PrimerResult.PrimedOkay; }
        }

        public DbgEntity Entity
        {
            get { return iEntity; }
        }

        public DbgPluginEngine Plugin
        {
            get { return iPlugin; }
        }
        #endregion

        #region Event handlers
        private void PrimerPlugin_EventHandler( DbgPluginPrimer.TPrimeEvent aEvent, object aData )
        {
            switch ( aEvent )
            {
            case DbgPluginPrimer.TPrimeEvent.EEventPrimingStarted:
                OnPrimeStart();
                break;
            case DbgPluginPrimer.TPrimeEvent.EEventPrimingProgress:
                if ( aData != null && ( aData is int ) )
                {
                    int prog = (int) aData;
                    OnPrimeProgress( prog );
                }
                break;
            case DbgPluginPrimer.TPrimeEvent.EEventPrimingComplete:
                OnPrimeComplete();
                break;
            }
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

        #region Internal methods
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

        #region Data members
        private readonly DbgEntity iEntity;
        private readonly DbgPluginEngine iPlugin;
        private readonly DbgPluginPrimer iPrimer;
        private TFlags iFlags = TFlags.EFlagsNone;
        #endregion
    }
}
