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
using CrashDebuggerLib.Structures.Chunk;
using CrashDebuggerLib.Structures.CodeSeg;
using CrashDebuggerLib.Structures.Cpu;
using CrashDebuggerLib.Structures.DebugMask;
using CrashDebuggerLib.Structures.Fault;
using CrashDebuggerLib.Structures.KernelObjects;
using CrashDebuggerLib.Structures.Library;
using CrashDebuggerLib.Structures.Process;
using CrashDebuggerLib.Structures.Scheduler;
using CrashDebuggerLib.Structures.Thread;
using CrashDebuggerLib.Structures.UserContextTable;
using CrashDebuggerLib.Threading;
using SymbianDebugLib.Engine;
using SymbianStructuresLib.Debug.Symbols;
using SymbianUtils;

namespace CrashDebuggerLib.Structures
{
    public class CrashDebuggerInfo : DisposableObject, IEnumerable<DObjectCon>
    {
        #region Constructors
        public CrashDebuggerInfo( DbgEngine aDebugEngine )
        {
            iDebugEngine = aDebugEngine;
            iDebugEngineView = aDebugEngine.CreateView( "CrashDebugger" );
            //
            iTheCurrentProcess = new DProcess( this );
            iTheCurrentThread = new DThread( this );
            iCodeSegs = new CodeSegCollection( this );
            iInfoCpu = new CpuInfo( this );
            iInfoFault = new FaultInfo( this );
            iInfoScheduler = new SchedulerInfo( this );
            iInfoDebugMask = new DebugMaskInfo( this );
            //
            MakeEmptyContainers();
        }
        #endregion

        #region API
        public void Clear()
        {
            iTheCurrentProcess = new DProcess( this );
            iTheCurrentThread = new DThread( this );
            iCodeSegs.Clear();
            iInfoCpu.Clear();
            iInfoFault.Clear();
            iInfoScheduler.Clear();
            iInfoDebugMask.Clear();

            iUserContextTableManager = new UserContextTableManager();
            iAsyncOperationManager.Clear();

            MakeEmptyContainers();
        }

        public DObjectCon ContainerByType( DObject.TObjectType aType )
        {
            DObjectCon ret = null;
            //
            foreach ( DObjectCon container in iContainers )
            {
                if ( container.Type == aType )
                {
                    ret = container;
                    break;
                }
            }
            //
            if ( ret == null )
            {
                throw new ArgumentException( "Bad container type: " + aType );
            }
            //
            return ret;
        }

        public DObject ObjectByAddress( uint aAddress )
        {
            DObject ret = null;
            //
            foreach ( DObjectCon container in iContainers )
            {
                DObject conObject = container[ aAddress ];
                if ( conObject != null )
                {
                    ret = conObject;
                    break;
                }
            }
            //
            return ret;
        }

        public DThread ThreadByAddress( uint aAddress )
        {
            DObjectCon con = ContainerByType( DObject.TObjectType.EThread );
            DObject ret = con[ aAddress ];
            return ( ret != null )? ret as DThread : null;
        }

        public DProcess ProcessByAddress( uint aAddress )
        {
            DObjectCon con = ContainerByType( DObject.TObjectType.EProcess );
            DObject ret = con[ aAddress ];
            return ( ret != null ) ? ret as DProcess : null;
        }

        public DChunk ChunkByAddress( uint aAddress )
        {
            DObjectCon con = ContainerByType( DObject.TObjectType.EChunk );
            DObject ret = con[ aAddress ];
            return ( ret != null ) ? ret as DChunk : null;
        }

        public DLibrary LibraryByAddress( uint aAddress )
        {
            DObjectCon con = ContainerByType( DObject.TObjectType.ELibrary );
            DObject ret = con[ aAddress ];
            return ( ret != null ) ? ret as DLibrary : null;
        }

        public CodeSegEntry CodeSegByAddress( uint aAddress )
        {
            return iCodeSegs[ aAddress ];
        }
        #endregion

        #region Properties
        public DProcess TheCurrentProcess
        {
            get { return iTheCurrentProcess; }
        }

        public DThread TheCurrentThread
        {
            get { return iTheCurrentThread; }
        }

        public CpuInfo InfoCpu
        {
            get { return iInfoCpu; }
        }

        public FaultInfo InfoFault
        {
            get { return iInfoFault; }
        }

        public SchedulerInfo InfoScheduler
        {
            get { return iInfoScheduler; }
        }

        public DebugMaskInfo InfoDebugMask
        {
            get { return iInfoDebugMask; }
        }

        public CodeSegCollection CodeSegs
        {
            get { return iCodeSegs; }
        }

        public DObjectCon this[ DObject.TObjectType aType ]
        {
            get
            {
                return ContainerByType( aType );
            }
        }

        public DbgEngine DebugEngine
        {
            get { return iDebugEngine; }
        }
        #endregion

        #region Internal methods
        internal UserContextTableManager UserContextTableManager
        {
            get { return iUserContextTableManager; }
        }

        internal Symbol LookUpSymbol( uint aAddress )
        {
            Symbol symbol = null;
            //
            if ( iDebugEngineView != null )
            {
                symbol = iDebugEngineView.Symbols[ aAddress ];
            }
            //
            return symbol;
        }

        internal AsyncOperationManager AsyncOperationManager
        {
            get { return iAsyncOperationManager; }
        }

        internal bool IsCurrentThread( DThread aThread )
        {
            return ( aThread.KernelAddress == TheCurrentThread.KernelAddress && TheCurrentThread.KernelAddress != 0 );
        }

        internal bool IsCurrentProcess( DProcess aProcess )
        {
            return ( aProcess.KernelAddress == TheCurrentProcess.KernelAddress && TheCurrentProcess.KernelAddress != 0 );
        }

        private void MakeEmptyContainers()
        {
            iContainers.Clear();
            //
            for ( int i = 0; i < (int) DObject.TObjectType.ENumObjectTypes; i++ )
            {
                DObject.TObjectType type = (DObject.TObjectType) i;
                DObjectCon container = new DObjectCon( this, type );
                iContainers.Add( container );
            }
        }
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
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
                if ( iDebugEngineView != null )
                {
                    iDebugEngineView.Dispose();
                    iDebugEngineView = null;
                }
            }
        }
        #endregion

        #region From IEnumerable<DObjectCon>
        public IEnumerator<DObjectCon> GetEnumerator()
        {
            foreach ( DObjectCon entry in iContainers )
            {
                yield return entry;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( DObjectCon entry in iContainers )
            {
                yield return entry;
            }
        }
        #endregion

        #region Data members
        private DbgEngineView iDebugEngineView;
        private DProcess iTheCurrentProcess;
        private DThread iTheCurrentThread;
        private readonly CodeSegCollection iCodeSegs;
        private readonly CpuInfo iInfoCpu;
        private readonly FaultInfo iInfoFault;
        private readonly SchedulerInfo iInfoScheduler;
        private readonly DebugMaskInfo iInfoDebugMask;
        private readonly DbgEngine iDebugEngine;
        private List<DObjectCon> iContainers = new List<DObjectCon>();
        private UserContextTableManager iUserContextTableManager = new UserContextTableManager();
        private AsyncOperationManager iAsyncOperationManager = new AsyncOperationManager();
        #endregion
    }
}
