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
using CrashDebuggerLib.Structures.Common;
using CrashDebuggerLib.Structures.KernelObjects;
using CrashDebuggerLib.Structures.Process;
using CrashDebuggerLib.Structures.NThread;
using SymbianStructuresLib.Debug.Symbols;

namespace CrashDebuggerLib.Structures.Thread
{
    public class DThread : DObject
    {
        #region Enumerations
        [System.ComponentModel.TypeConverter( typeof( SymbianParserLib.TypeConverters.SymbianEnumConverter ) )]
        public enum TThreadState
        {
            EUnknown = -1,
            ECreated = 0,
            EDead,
            EReady,
            EWaitSemaphore,
            EWaitSemaphoreSuspended,
            EWaitMutex,
            EWaitMutexSuspended,
            EHoldMutexPending,
            EWaitCondVar,
            EWaitCondVarSuspended,
        }

        [Flags, System.ComponentModel.TypeConverter( typeof( SymbianParserLib.TypeConverters.SymbianEnumConverter ) )]
        public enum TThreadFlags
        {
            EThreadFlagNone                 = 0x00000000,
            EThreadFlagProcessCritical		= 0x00000001,	// thread panic panics process
            EThreadFlagProcessPermanent		= 0x00000002,	// thread exit of any kind causes process to exit (=main)
            EThreadFlagSystemCritical		= 0x00000004,	// thread panic reboots entire system
            EThreadFlagSystemPermanent		= 0x00000008,	// thread exit of any kind reboots entire system
            EThreadFlagOriginal				= 0x00000010,
            EThreadFlagLastChance			= 0x00000020,
            EThreadFlagRealtime				= 0x00000040,	// thread will be panicked when using some non-realtime functions
            EThreadFlagRealtimeTest			= 0x00000080	// non-realtime functions only warn rather than panic
        }

        public enum TThreadWaitType
        {
            EThreadWaitingUnknown = 0,
            EThreadWaitingReady,
            EThreadWaitingOnSemaphore,
            EThreadWaitingOnMutex,
            EThreadWaitingOnDfc,
            EThreadWaitingOnResumption,
            EThreadWaitingOnRequestSemaphore,
            EThreadWaitingOnRequestSemaphoreInsideSchedulerWaitLoop
        }
        #endregion

        #region Constructors
        public DThread( CrashDebuggerInfo aCrashDebugger )
            : base( aCrashDebugger, TObjectType.EThread )
        {
            iNThread = new CrashDebuggerLib.Structures.NThread.NThread( aCrashDebugger, this );
            iStackInfoSupervisor = new ThreadStackInfo( aCrashDebugger, this, ThreadStackInfo.TType.ETypeSupervisor );
            iStackInfoUser = new ThreadStackInfo( aCrashDebugger, this, ThreadStackInfo.TType.ETypeUser );
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public TThreadState MState
        {
            get { return iState; }
            set { iState = value; }
        }

        public uint WaitObj
        {
            get { return iStateWaitObjectAddress; }
            set { iStateWaitObjectAddress = value; }
        }

        public TThreadFlags Flags
        {
            get { return iFlags; }
            set { iFlags = value; }
        }

        public uint Handles
        {
            get { return iHandles; }
            set { iHandles = value; }
        }

        public long Id
        {
            get { return iId; }
            set { iId = value; }
        }

        public ThreadAllocatorInfo AllocatorInfo
        {
            get { return iAllocatorInfo; }
        }

        public uint Frame
        {
            get { return iFrame; }
            set { iFrame = value; }
        }

        public ThreadPriorities Priorities
        {
            get { return iPriorities; }
        }

        public ExitInfo ExitInfo
        {
            get { return iExitInfo; }
        }

        public ThreadStackInfo StackInfoUser
        {
            get { return iStackInfoUser; }
        }

        public ThreadStackInfo StackInfoSupervisor
        {
            get { return iStackInfoSupervisor; }
        }

        public ThreadHandlers Handlers
        {
            get { return iHandlers; }
        }

        public ThreadTemporaries Temporaries
        {
            get { return iTemporaries; }
        }

        public uint IpcCount
        {
            get { return iIpcCount; }
            set { iIpcCount = value; }
        }

        public DProcess OwningProcess
        {
            get
            {
                DProcess ret = null;
                //
                DObject owner = Owner;
                if ( owner != null && owner is DProcess )
                {
                    ret = (DProcess) owner;
                }
                //
                return ret;
            }
        }

        public NThread.NThread NThread
        {
            get { return iNThread; }
        }

        public bool HasActiveScheduler
        {
            get
            {
                return ( iHandlers.ActiveScheduler != 0 );
            }
        }

        public TThreadWaitType WaitType
        {
            get
            {
                TThreadWaitType waitType = TThreadWaitType.EThreadWaitingUnknown;
                //
                if ( NThread.NState == CrashDebuggerLib.Structures.NThread.NThread.TNThreadState.EWaitDfc )
                {
                    waitType = TThreadWaitType.EThreadWaitingOnDfc;
                }
                else if ( NThread.NState == CrashDebuggerLib.Structures.NThread.NThread.TNThreadState.EReady )
                {
                    waitType = TThreadWaitType.EThreadWaitingReady;
                }
                else if ( NThread.NState == CrashDebuggerLib.Structures.NThread.NThread.TNThreadState.ESuspended )
                {
                    waitType = TThreadWaitType.EThreadWaitingOnResumption;
                }
                else if ( NThread.NState == CrashDebuggerLib.Structures.NThread.NThread.TNThreadState.EBlocked )
                {
                    bool blockdOnSemaphore = IsBlockedOnSemaphore;
                    if ( blockdOnSemaphore )
                    {
                        waitType = TThreadWaitType.EThreadWaitingOnSemaphore;
                    }
                }
                else if ( NThread.NState == CrashDebuggerLib.Structures.NThread.NThread.TNThreadState.EWaitFastSemaphore )
                {
                    // Check that the fast semaphore is definitely the NThread's request semaphore.
                    uint nThreadRequestSemaphoreAddress = NThread.RequestSemaphoreAddress;
                    if ( NThread.WaitObj != 0 && NThread.WaitObj == nThreadRequestSemaphoreAddress )
                    {
                        CrashDebuggerLib.Structures.NThread.NThread.TWaitType nThreadWaitType = NThread.WaitType;
                        //
                        if ( nThreadWaitType == CrashDebuggerLib.Structures.NThread.NThread.TWaitType.EWaitTypeUserWaitForAnyRequest )
                        {
                            if ( HasActiveScheduler )
                            {
                                waitType = TThreadWaitType.EThreadWaitingOnRequestSemaphoreInsideSchedulerWaitLoop;
                            }
                            else
                            {
                                waitType = TThreadWaitType.EThreadWaitingOnRequestSemaphore;
                            }
                        }
                        else if ( nThreadWaitType == CrashDebuggerLib.Structures.NThread.NThread.TWaitType.EWaitTypeUserWaitForRequest )
                        {
                            waitType = TThreadWaitType.EThreadWaitingOnRequestSemaphore;
                        }
                    }
                }
                //
                return waitType;
            }
        }

        public bool IsUserThread
        {
            get
            {
                bool ret = false;
                //
                DProcess parent = OwningProcess;
                if ( parent != null )
                {
                    string name = parent.Name.ToLower();
                    ret = ( name != Platform.ProcessNames.KKernel.ToLower() );
                }
                //
                return ret;
            }
        }

        public bool IsCurrent
        {
            get
            {
                bool ret = CrashDebugger.IsCurrentThread( this );
                return ret;
            }
        }
        #endregion

        #region Internal methods
        private bool IsBlockedOnSemaphore
        {
            get
            {
                bool ret = false;
                //
                if ( NThread.NState == CrashDebuggerLib.Structures.NThread.NThread.TNThreadState.EBlocked )
                {
                    Register.RegisterEntry linkReg = NThread.Registers[ "R14_USR" ];
                    if ( linkReg.Symbol != null )
                    {
                        string symbolText = linkReg.Symbol.Name;
                        if ( symbolText.Contains( "RSemaphore::Wait" ) || symbolText.Contains( "RCriticalSection::Wait" ) )
                        {
                            ret = true;
                        }
                        else if ( symbolText.Contains( "Exec::SemaphoreWait" ) )
                        {
                            ret = true;
                        }
                    }
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal constants
        #endregion

        #region From DBase
        public override string ToClipboard()
        {
            StringBuilder ret = new StringBuilder();
            //
            ret.AppendLine( base.ToClipboard() );
            ret.AppendFormat( "MState: {0}, Id: {1}, ExitInfo: {2}" + System.Environment.NewLine, MState, Id, ExitInfo );
            ret.AppendLine( iNThread.ToClipboard() );
            ret.AppendLine( iStackInfoUser.ToClipboard() );
            ret.AppendLine( iStackInfoSupervisor.ToClipboard() );
            //
            return ret.ToString();
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        private readonly NThread.NThread iNThread;
        private readonly ThreadStackInfo iStackInfoSupervisor;
        private readonly ThreadStackInfo iStackInfoUser;

        private TThreadState iState = TThreadState.EUnknown;
        private TThreadFlags iFlags = TThreadFlags.EThreadFlagNone;
        
        private uint iStateWaitObjectAddress = 0;
        private uint iHandles = 0;
        private long iId = 0;
        private uint iFrame = 0;
        private uint iIpcCount = 0;

        private ThreadAllocatorInfo iAllocatorInfo = new ThreadAllocatorInfo();
        private ThreadPriorities iPriorities = new ThreadPriorities();
        private ExitInfo iExitInfo = new ExitInfo();
        private ThreadHandlers iHandlers = new ThreadHandlers();
        private ThreadTemporaries iTemporaries = new ThreadTemporaries();
        #endregion
    }
}