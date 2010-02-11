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
using System.Text;
using System.IO;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianUtils.FileTypes;
using SymbianStructuresLib.Debug.Symbols;
using SymbianStructuresLib.CodeSegments;
using SymbianDebugLib.Entity;
using SymbianDebugLib.PluginAPI;
using SymbianCodeLib.SourceManagement.Source;
using SymbianCodeLib.SourceManagement.Provisioning;

namespace SymbianCodeLib.DbgEnginePlugin
{
    internal class CodePrimer : DbgPluginPrimer
    {
        #region Constructors
        public CodePrimer( CodePlugin aPlugin )
            : base( aPlugin )
		{
		}
		#endregion

        #region From DbgPluginPrimer
        public override void Add( DbgEntity aEntity )
        {
            CodeSourceProvider provider = null;
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
                    using ( CodeSourceCollection sources = provider.CreateSources( aEntity.FullName ) )
                    {
                        // Make sure the time to read attribute is setup in alignment with
                        // whether the entity was explicitly added by the user or found implicitly
                        // by scanning.
                        if ( aEntity.WasAddedExplicitly == false )
                        {
                            foreach ( CodeSource source in sources )
                            {
                                // This means, don't read this source until it is actually
                                // referenced by the client. I.e. until the client activates
                                // a code segment that refers to this 
                                source.TimeToRead = CodeSource.TTimeToRead.EReadWhenNeeded;
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
                throw new ArgumentException( "SymbianCodeLib does not support directory entities" );
            }
        }

        public override void Prime( TSynchronicity aSynchronicity )
        {
            CodePlugin plugin = this.Plugin;

            // Wipe any state ready for new priming attempt
            base.OnPrepareToPrime();
            if ( base.ResetEngineBeforePriming )
            {
                plugin.Clear();
            }
    
            // Report the "priming started event" prior to adding the sources
            base.ReportEvent( TPrimeEvent.EEventPrimingStarted, null );

            // Any sources already registered with the source manager at the start of a new
            // priming request are assumed to already be complete.
            RecordAlreadyCompletedSources();

            // Initially, whilst adding the sources to the source manager, we'll
            // operate synchronously. This prevents us from triggering the read
            // operation in the SourceAdded callback below.
            iSynchronicity = TSynchronicity.ESynchronous;

            // Listen to source manager events in case any new sources are
            // created whilst reading those we already know about.
            CodeSourceManager sourceManager = this.SourceManager;
            sourceManager.SourceAdded += new CodeSourceManager.SourceEventHandler( SourceManager_SourceAdded );
            sourceManager.SourceRemoved += new CodeSourceManager.SourceEventHandler( SourceManager_SourceRemoved );

            // Tell the source manager which sources we are reading. This also will
            // call our event handler (added above) so that we can observe source events.
            CodeSourceCollection sources = iSources;
            iSources = new CodeSourceCollection();
            sourceManager.AddRange( sources );

            // If we're operating asynchronously, then the loop below will potentially
            // complete quite quickly. This means that any (new/additional) sources which 
            // are created (asynchronously) whilst reading is underway will not themselves 
            // be read. 
            // 
            // Therefore, we store the synchronisation mode as a data member, and when
            // 'SourceAdded' is called, if operating asynchronously, we'll also initiate
            // a read operation for the source (as soon as it is added).
            //
            // If we're operating synchronously, then this isn't important. Because we're
            // behaving synchronously, the loop below will only process one source at a
            // time (before waiting) and therefore even if that source adds new/additional
            // sources to the source manager, we'll catch them as soon as we move the
            // next iteration around the loop.
            iSynchronicity = aSynchronicity;

            // TODO: possibly re-write this so that it uses two separate code paths
            // for synchronous and asynchronous priming? By trying to use one path
            // this code looks rather complex and isn't terribly robust.
            try
            {
                // Now we can start the sources running.
                int count = iSourcesYetToBePrimed.Count;
                while ( count > 0 )
                {
                    // Get the head source and remove it from the pending list
                    CodeSource source = iSourcesYetToBePrimed[ 0 ];
                    iSourcesYetToBePrimed.Remove( source );
                        
                    // If the source wants to read it's data immediately, then activated
                    // it right now...
                    if ( source.TimeToRead == CodeSource.TTimeToRead.EReadWhenPriming )
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

                    count = iSourcesYetToBePrimed.Count;
                }
            }
            catch( Exception e )
            {
                // If priming failed, report completion before rethrowing...
                OnPrimeComplete();
                throw e;
            }
       }

        protected override void OnPrimeComplete()
        {
            System.Diagnostics.Debug.Assert( iSourcesYetToBePrimed.Count == 0 );
            try
            {
                SourceManager.SourceAdded -= new CodeSourceManager.SourceEventHandler( SourceManager_SourceAdded );
                SourceManager.SourceRemoved -= new CodeSourceManager.SourceEventHandler( SourceManager_SourceRemoved );
                //
                base.OnPrimeComplete();
            }
            finally
            {
                SourceEventsUnsubscribe();
            }
        }

        protected override int Count
        {
            get
            {
                int count = SourceManager.Count;
                return count; 
            }
        }
        #endregion

        #region API
        #endregion

		#region Properties
        internal CodePlugin Plugin
        {
            get { return base.Engine as CodePlugin; }
        }

        internal CodeSourceManager SourceManager
        {
            get { return Plugin.SourceManager; }
        }

        internal CodeSourceProviderManager ProvisioningManager
        {
            get { return Plugin.ProvisioningManager; }
        }
        #endregion

        #region Event handlers
        private void SourceManager_SourceAdded( CodeSource aSource )
        {
            base.Engine.Trace( "[CodePrimer] SourceManager_SourceAdded - aSource: {0}, time to read: {1}", aSource, aSource.TimeToRead );

            aSource.EventHandler += new CodeSource.EventHandlerFunction( Source_EventHandler );
            bool needToSave = true;

            // If we're in async mode and the source wants to be read immediately
            // then kick it off. If we're operating in sync mode, then this will
            // be done as part of the loop within 'Prime'.
            if ( iSynchronicity == TSynchronicity.EAsynchronous )
            {
                if ( aSource.TimeToRead == CodeSource.TTimeToRead.EReadWhenPriming )
                {
                    aSource.Read( iSynchronicity );
                }
                else
                {
                    // This source will read it's data on it's own terms so skip
                    // it to ensure that we can track when all the other sources 
                    // (that do actually read their files now..) are ready.
                    Skip( aSource );
                }

                // We don't need to add it to the 'yet to be primed' list because
                // it's either been 'read' or then it only supports read-on-demand.
                needToSave = false;
            }

            if ( needToSave )
            {
                lock ( iSourcesYetToBePrimed )
                {
                    iSourcesYetToBePrimed.Add( aSource );
                }
            }

            base.Engine.Trace( string.Format( "[SourceManager_SourceAdded] {0}, srcCount: {1}, yetToBePrimed: {2}", aSource.URI, SourceManager.Count, iSourcesYetToBePrimed.Count ) );
        }

        private void SourceManager_SourceRemoved( CodeSource aSource )
        {
            base.Engine.Trace( string.Format( "[SourceManager_SourceRemoved] START - {0}, srcCount: {1}, yetToBePrimed: {2}", aSource.URI, SourceManager.Count, iSourcesYetToBePrimed.Count ) );
            
            aSource.EventHandler -= new CodeSource.EventHandlerFunction( Source_EventHandler );

            base.RemoveFromCompleted( aSource );
            lock ( iSourcesYetToBePrimed )
            {
                iSourcesYetToBePrimed.Remove( aSource );
            }

            // Check for completion since removing a source might mean that we've now
            // reached completed state (if it was the last one that we were waiting for)
            bool amComplete = base.IsComplete;
            if ( amComplete )
            {
                CheckForCompletion( amComplete );
            }

            base.Engine.Trace( string.Format( "[SourceManager_SourceRemoved] END - {0}, srcCount: {1}, yetToBePrimed: {2}, amComplete: {3}", aSource.URI, SourceManager.Count, iSourcesYetToBePrimed.Count, amComplete ) );
        }

        private void Source_EventHandler( CodeSource.TEvent aEvent, CodeSource aSource, object aData )
        {
            bool primeCompleted = false;

            // Map source event onto a primer event
            if ( aEvent == CodeSource.TEvent.EReadingProgress )
            {
                base.SaveLatestProgress( aSource, (int) aData );
            }
            
            // If all sources are complete, then are we also done?
            if ( aEvent == CodeSource.TEvent.EReadingComplete )
            {
                // We don't need to listen to this source anymore
                aSource.EventHandler -= new CodeSource.EventHandlerFunction( Source_EventHandler );

                // Source is 100% complete now.
                base.SaveLatestProgress( aSource, 100 );

                // It's complete, so record as such so that we can tell when all the sources
                // are now ready.
                primeCompleted = base.AddToCompleted( aSource );
            }

            CheckForCompletion( primeCompleted );
        }
        #endregion

        #region Internal methods
        private void Skip( CodeSource aSource )
        {
            System.Diagnostics.Debug.Assert( aSource.TimeToRead == CodeSource.TTimeToRead.EReadWhenNeeded );
            bool primeCompleted = base.AddToCompleted( aSource );
            CheckForCompletion( primeCompleted );
        }

        private void SourceEventsUnsubscribe()
        {
            foreach ( CodeSource source in SourceManager )
            {
                source.EventHandler -= new CodeSource.EventHandlerFunction( Source_EventHandler );
            }
        }

        private void CheckForCompletion( bool aAmIComplete )
        {
            // Report any progress
            base.ReportProgressIfNeeded( aAmIComplete );

            // Tidy up and report completion
            if ( aAmIComplete )
            {
                OnPrimeComplete();
            }
        }

        private void RecordAlreadyCompletedSources()
        {
            CodeSourceManager sourceManager = this.SourceManager;
            foreach ( CodeSource source in sourceManager )
            {
                base.AddToCompleted( source );
            }
        }
        #endregion

        #region Data members
        private TSynchronicity iSynchronicity = TSynchronicity.ESynchronous;
        private CodeSourceCollection iSources = new CodeSourceCollection();
        private CodeSourceCollection iSourcesYetToBePrimed = new CodeSourceCollection();
        #endregion
    }
}
