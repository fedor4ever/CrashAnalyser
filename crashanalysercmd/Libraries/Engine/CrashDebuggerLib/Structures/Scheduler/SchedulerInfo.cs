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
using CrashDebuggerLib.Structures.KernelObjects;
using CrashDebuggerLib.Structures.Register;
using CrashDebuggerLib.Structures.Common;
using CrashDebuggerLib.Structures.NThread;
using CrashDebuggerLib.Structures.Thread;

namespace CrashDebuggerLib.Structures.Scheduler
{
    public class SchedulerInfo : CrashDebuggerAware
    {
        #region Constructors
        public SchedulerInfo( CrashDebuggerInfo aCrashDebugger )
            : base( aCrashDebugger )
        {
            iExtraRegisters = new RegisterCollection( aCrashDebugger, RegisterCollection.TType.ETypeGeneral );
        }
        #endregion

        #region API
        public void Clear()
        {
            iAddress = 0;
            iCurrentNThreadAddress = 0;
            iRescheduleNeeded = 0;
            iDfcPending = 0;
            iKernCSLocked = 0;
            iDFCs = new LinkedListInfo();
            iProcessHandlerAddress = 0;
            iAddressSpace = 0;
            iSysLockInfo = new SchedulerSysLockInfo();
            iExtraRegisters.Clear();
        }
        #endregion

        #region Properties
        public uint Address
        {
            get { return iAddress; }
            set { iAddress = value; }
        }

        public uint CurrentNThreadAddress
        {
            get { return iCurrentNThreadAddress; }
            set { iCurrentNThreadAddress = value; }
        }

        public NThread.NThread CurrentNThread
        {
            get
            {
                NThread.NThread ret = null;
                //
                DObjectCon threads = CrashDebugger.ContainerByType( DObject.TObjectType.EThread );
                foreach ( DObject obj in threads )
                {
                    DThread thread = (DThread) obj;
                    NThread.NThread nThread = thread.NThread;
                    if ( nThread.Address == CurrentNThreadAddress )
                    {
                        ret = nThread;
                        break;
                    }
                }
                //
                return ret;
            }
        }

        public uint RescheduleNeeded
        {
            get { return iRescheduleNeeded; }
            set { iRescheduleNeeded = value; }
        }

        public uint DfcPending
        {
            get { return iDfcPending; }
            set { iDfcPending = value; }
        }

        public uint KernCSLocked
        {
            get { return iKernCSLocked; }
            set { iKernCSLocked = value; }
        }

        public LinkedListInfo DFCs
        {
            get { return iDFCs; }
        }

        public uint ProcessHandlerAddress
        {
            get { return iProcessHandlerAddress; }
            set { iProcessHandlerAddress = value; }
        }

        public uint AddressSpace
        {
            get { return iAddressSpace; }
            set { iAddressSpace = value; }
        }

        public SchedulerSysLockInfo SysLockInfo
        {
            get { return iSysLockInfo; }
        }

        public RegisterCollection ExtraRegisters
        {
            get { return iExtraRegisters; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        private uint iAddress;
        private uint iCurrentNThreadAddress;
        private uint iRescheduleNeeded;
        private uint iDfcPending;
        private uint iKernCSLocked;
        private LinkedListInfo iDFCs = new LinkedListInfo();
        private uint iProcessHandlerAddress;
        private uint iAddressSpace;
        private SchedulerSysLockInfo iSysLockInfo = new SchedulerSysLockInfo();
        private readonly RegisterCollection iExtraRegisters;
        #endregion
    }
}
