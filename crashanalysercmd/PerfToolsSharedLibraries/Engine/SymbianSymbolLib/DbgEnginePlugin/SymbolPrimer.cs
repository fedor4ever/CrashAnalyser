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
using System.Collections.Generic;
using SymbianDebugLib.Entity;
using SymbianDebugLib.PluginAPI;
using SymbianSymbolLib.SourceManagement.Provisioning;
using SymbianSymbolLib.SourceManagement.Source;
using SymbianUtils;

namespace SymbianSymbolLib.DbgEnginePlugin
{
    internal class SymbolPrimer : DbgPluginPrimer, IEnumerable<SymSource>
    {
        #region Constructors
        public SymbolPrimer( SymbolPlugin aPlugin )
            : base( aPlugin )
		{
            // If we've been created then presumably this is the reason...
            ProvisioningManager.PrepareToCreateSources();
        }
		#endregion

        #region From DbgPluginPrimer
        public override void Add( DbgEntity aEntity )
        {
            SymSourceProvider provider = null;
            //
            if ( aEntity.FSEntity.IsFile )
            {
                if ( aEntity.Exists && aEntity.FSEntity.IsValid )
                {
                    provider = ProvisioningManager.GetProvider( aEntity.FSEntity.FullName );
                }
                //
                if ( provider != null )
                {
                    using ( SymSourceCollection sources = provider.CreateSources( aEntity.FullName ) )
                    {
                        // Make sure the time to read attribute is setup in alignment with
                        // whether the entity was explicitly added by the user or found implicitly
                        // by scanning.
                        if ( aEntity.WasAddedExplicitly == false )
                        {
                            foreach ( SymSource source in sources )
                            {
                                // This means, don't read this source until it is actually
                                // referenced by the client. I.e. until the client activates
                                // a code segment that refers to this 
                                source.TimeToRead = SymSource.TTimeToRead.EReadWhenNeeded;
                            }
                        }

                        // Ownership is transferred
                        iSources.AddRange( sources );
                        sources.Clear();
                    }
                }
                else
                {
                    throw new NotSupportedException( "Specified file type is not supported" );
                }
            }
            else
            {
                throw new ArgumentException( "SymbianSymbolLib does not support directory entities" );
            }
        }

        public override void Prime( TSynchronicity aSynchronicity )
        {
            SymbolPlugin plugin = this.Plugin;

            // Wipe any state ready for new priming attempt
            base.OnPrepareToPrime();
            if ( base.ResetEngineBeforePriming )
            {
                plugin.Clear();
            }

            // Tell the plugin which sources we are reading
            plugin.StoreSourcesThatWillBePrimed( this );
  
            // Listen to soure events so that we can report progress
            SourceEventsSubscribe();

            // Report the "priming started event"
            base.ReportEvent( TPrimeEvent.EEventPrimingStarted, null );

            // Now we can start the sources running.
            foreach ( SymSource source in iSources )
            {
                // If the source wants to read it's data immediately, then activated
                // it right now...
                if ( source.TimeToRead == SymSource.TTimeToRead.EReadWhenPriming )
                {
                    source.Read( aSynchronicity );
                }
                else
                {
                    // This source will read it's data on it's own terms so skip
                    // it to ensure that we can track when all the other sources 
                    // (that do actually read their files now..) are ready.
                    Skip( source );
                }
            }
        }

        protected override int Count
        {
            get { return iSources.Count; }
        }
        #endregion
        
        #region API
        internal void Skip( SymSource aSource )
        {
            System.Diagnostics.Debug.Assert( aSource.TimeToRead == SymSource.TTimeToRead.EReadWhenNeeded );
            bool primeCompleted = base.AddToCompleted( aSource );
            CheckForCompletion( primeCompleted );
        }
        #endregion

        #region Properties
        internal SymbolPlugin Plugin
        {
            get { return base.Engine as SymbolPlugin; }
        }

        internal SymSourceManager SourceManager
        {
            get { return Plugin.SourceManager; }
        }

        internal SymSourceProviderManager ProvisioningManager
        {
            get { return Plugin.ProvisioningManager; }
        }
        #endregion

        #region Internal methods
        private void SourceEventsSubscribe()
        {
            SourceEventsUnsubscribe();
            //
            foreach ( SymSource source in iSources )
            {
                source.EventHandler += new SymSource.EventHandlerFunction( Source_EventHandler );
            }
        }

        private void SourceEventsUnsubscribe()
        {
            foreach ( SymSource source in iSources )
            {
                source.EventHandler -= new SymSource.EventHandlerFunction( Source_EventHandler );
            }
        }

        private void Source_EventHandler( SymSource.TEvent aEvent, SymSource aSource, object aData )
        {
            bool primeCompleted = false;

            // Map source event onto a primer event
            if ( aEvent == SymSource.TEvent.EReadingProgress )
            {
                base.SaveLatestProgress( aSource, (int) aData );
            }
            
            // If all sources are complete, then are we also done?
            if ( aEvent == SymSource.TEvent.EReadingComplete )
            {
                // We don't need to listen to this source anymore
                aSource.EventHandler -= new SymSource.EventHandlerFunction( Source_EventHandler );

                // Source is 100% complete now.
                base.SaveLatestProgress( aSource, 100 );

                // It's complete, so record as such so that we can tell when all the sources
                // are now ready.
                primeCompleted = base.AddToCompleted( aSource );
            }

            CheckForCompletion( primeCompleted );
        }

        private void CheckForCompletion( bool aAmIComplete )
        {
            // Report any progress
            base.ReportProgressIfNeeded( aAmIComplete );

            // Tidy up and report completion
            if ( aAmIComplete )
            {
                base.OnPrimeComplete();
            }
        }
        #endregion

        #region From IEnumerable<SymSource>
        public IEnumerator<SymSource> GetEnumerator()
        {
            return iSources.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return iSources.GetEnumerator();
        }
        #endregion

        #region Data members
        private SymSourceCollection iSources = new SymSourceCollection();
        #endregion
    }
}
