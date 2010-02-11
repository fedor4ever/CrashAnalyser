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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SymbianUtils;
using CrashItemLib.PluginAPI;
using CrashItemLib.Crash.Source;
using CrashItemLib.Crash.Container;
using CrashItemLib.Engine.Sources.Types;

namespace CrashItemLib.Engine.Sources
{
    public class CIEngineSource : IEnumerable<CIContainer>
    {
        #region Enumerations
        public enum TState
        {
            EStateUninitialised = 0,
            EStateProcessing,
            EStateReady,
            EStateReadyNoItems,
            EStateReadyCorrupt
        }
        #endregion

        #region Static constructors
        /// <summary>
        /// Create a source that is linked with a single crash file format plugin
        /// </summary>
        public static CIEngineSource NewNative( CIEngine aEngine, CFFSource aEntry )
        {
            CIEngineSource ret = new CIEngineSource( aEngine, aEntry );
            return ret;
        }

        /// <summary>
        /// Create a source that is associated with a trace file and potentially one
        /// or more plugins which all think they are capable of interpreting trace-based
        /// content.
        /// </summary>
        public static CIEngineSource NewTrace( CIEngine aEngine, CFFSource[] aEntries )
        {
            CIEngineSource ret = new CIEngineSource( aEngine, aEntries );
            return ret;
        }
        #endregion

        #region Constructors
        private CIEngineSource( CIEngine aEngine, CFFSource aEntry )
        {
            iEngine = aEngine;
            iFile = aEntry.MasterFile;
            iReader = new CIEngineSourceReaderNative( this, aEntry );
        }

        private CIEngineSource( CIEngine aEngine, CFFSource[] aEntries )
        {
            if ( aEntries == null || aEntries.Length == 0 )
            {
                throw new ArgumentException( "Cannot create trace-based source without entry list" );
            }

            iEngine = aEngine;
            iFile = aEntries[0].MasterFile;
            iReader = new CIEngineSourceReaderTrace( this, aEntries );
        }
        #endregion

        #region Framework API
        #endregion

        #region API
        internal void SaveContainer( CIContainer aContainer )
        {
            // Update list of created containers
            lock ( iCreatedContainers )
            {
                iCreatedContainers.Add( aContainer );
            }

            // Notify the engine which will cascade to UI
            iEngine.Add( aContainer );
        }

        internal void Read()
        {
            lock ( iCreatedContainers )
            {
                iCreatedContainers.Clear();
            }
            State = TState.EStateProcessing;

            // We need this in order to report progress to the engine
            System.Diagnostics.Debug.Assert( iCollection != null );

            // Initiate synchronous read and record any exceptions
            TState finalState = iReader.Read();
            this.State = finalState;
        }

        internal void OnSourceReadingProgress( int aProgress )
        {
            iCollection.OnSourceProgress( this, aProgress );
        }
        #endregion

        #region Properties
        public TState State
        {
            get
            {
                lock ( iSyncRoot )
                {
                    return iState;
                }
            }
            protected set
            {
                lock ( iSyncRoot )
                {
                    iState = value;
                }
                //
                iCollection.OnSourceStateChanged( this );
            }
        }

        public bool IsReady
        {
            get
            {
                lock ( iSyncRoot )
                {
                    bool ready = false;
                    //
                    switch ( iState )
                    {
                    case TState.EStateReady:
                    case TState.EStateReadyCorrupt:
                    case TState.EStateReadyNoItems:
                        ready = true;
                        break;
                    default:
                        break;
                    }
                    //
                    return ready;
                }
            }
        }

        public FileInfo File
        {
            get { return iFile; }
        }

        public string FileName
        {
            get { return iFile.FullName; }
        }

        public int ContainerCount
        {
            get
            {
                lock ( iCreatedContainers )
                {
                    return iCreatedContainers.Count;
                }
            }
        }

        internal CFFSource.TReaderOperationType OpType
        {
            get { return iReader.OpType; }
        }

        internal CIEngine Engine
        {
            get { return iEngine; }
        }

        internal CIEngineSourceCollection Collection
        {
            get { return iCollection; }
            set { iCollection = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From IEnumerable<CIContainer>
        public IEnumerator<CIContainer> GetEnumerator()
        {
            foreach ( CIContainer container in iCreatedContainers )
            {
                yield return container;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach ( CIContainer container in iCreatedContainers )
            {
                yield return container;
            }
        }
        #endregion

        #region Data members
        private readonly CIEngine iEngine;
        private readonly FileInfo iFile;
        private readonly CIEngineSourceReader iReader;
        private object iSyncRoot = new object();
        private TState iState = TState.EStateUninitialised;
        private CIContainerCollection iCreatedContainers = new CIContainerCollection();
        private CIEngineSourceCollection iCollection = null;
        #endregion
    }
}
