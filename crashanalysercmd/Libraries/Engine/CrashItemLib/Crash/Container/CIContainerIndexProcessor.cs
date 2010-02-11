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
using System.Threading;
using SymbianUtils;
using SymbianDebugLib.Engine;
using SymbianDebugLib.Entity.Configurations;
using CrashItemLib.Engine;
using System;
using CrashItemLib.Crash.Messages;

namespace CrashItemLib.Crash.Container
{
    internal class CIContainerIndexProcessor
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
        #endregion

        #region Constructors
        public CIContainerIndexProcessor( CIContainerIndex aIndex, CIEngine aEngine )
        {
            iIndex = aIndex;
            iEngine = aEngine;
        }
        #endregion

        #region API
        public void Start( TSynchronicity aSynchronicity )
        {
            if ( aSynchronicity == TSynchronicity.EAsynchronous )
            {
                ThreadPool.QueueUserWorkItem( new WaitCallback( RunWorker ) );
            }
            else
            {
                RunWorker();
            }
        }
        #endregion
        
        #region Properties
        #endregion

        #region Internal methods
        private void RunWorker()
        {            
            RunWorker(null);
        }

        private void RunWorker( object aNotUsed )
        {
            iEngine.Trace( "[CIContainerIndexProcessor] RunWorker() - START - index groupings: {0}", iIndex.Count );

            EventHandler( TEvent.EEventStarting );

            DbgEngine debugEngine = iEngine.DebugEngine;
            bool needToPrimeDebugEngine = debugEngine.MetaDataConfig.IsConfigurationDataAvailable;
            iEngine.Trace( "[CIContainerIndexProcessor] RunWorker() - needToPrimeDebugEngine: {0}", needToPrimeDebugEngine );

            // Process the index "buckets" until all are exhausted.
            for ( CIContainerCollection collection = iIndex.DequeueNextContainer(); collection != null; collection = iIndex.DequeueNextContainer() )
            {
                try
                {
                    if ( collection.Count > 0 )
                    {
                        // Get the rom serial number - all containers in the collection share a common serial
                        uint serialNumber = CIContainerIndex.GetRomChecksum( collection[ 0 ] );
                        iEngine.Trace( "[CIContainerIndexProcessor] RunWorker() - {0} containers for rom checksum: 0x{1:x8}", collection.Count, serialNumber );

                        // Prepare debug engine meta-data as needed.
                        if ( needToPrimeDebugEngine )
                        {
                            DbgEntityConfigIdentifier identifier = new DbgEntityConfigIdentifier( serialNumber );

                            iEngine.Trace( "[CIContainerIndexProcessor] RunWorker() - synchronously switching debug meta-data config..." );
                            debugEngine.ConfigManager.SwitchConfigurationSynchronously( identifier );
                            iEngine.Trace( "[CIContainerIndexProcessor] RunWorker() - switch complete." );
                        }

                        // Process the list of crash item containers in separate threads until all are handled.
                        // This is quite a heavyweight operation since it also potentially primes the debug engine with
                        // the needed symbols and then finalizes every associated crash container.
                        // However, we run this in a separate thread so it will not block the UI.
                        iEngine.Trace( "[CIContainerIndexProcessor] RunWorker() - running finalizer for {0} items with rom checksum: 0x{1:x8}", collection.Count, serialNumber );

                        // We wait until the finalizer is finished, but we're running in a worker thread so this is OK.
                        CIContainerFinalizer finalizer = new CIContainerFinalizer( collection, iEngine );
                        finalizer.Start( SymbianUtils.TSynchronicity.ESynchronous );

                        iEngine.Trace( "[CIContainerIndexProcessor] RunWorker() - finalization complete for {0} items with rom checksum: 0x{1:x8}", collection.Count, serialNumber );
                    }
                }                       
                catch (Exception e)
                {
                    iEngine.Trace("Error: RunWorker() hit an unexpected exception!");

                    foreach (CIContainer container in collection)
                    {
                        CIMessageError error = new CIMessageError(container, "RunWorker failed");
                        error.AddLine("Unexpected exception encountered during container processing - analysis has failed!");
                        container.Messages.Add(error);
                    }
                }
            }
            iEngine.Trace( "[CIContainerIndexProcessor] RunWorker() - Notifying about completion..." );

            EventHandler( TEvent.EEventCompleted );

            iEngine.Trace( "[CIContainerIndexProcessor] RunWorker() - END" );
        }
        #endregion

        #region Data members
        private readonly CIContainerIndex iIndex;
        private readonly CIEngine iEngine;
        #endregion
    }
}
