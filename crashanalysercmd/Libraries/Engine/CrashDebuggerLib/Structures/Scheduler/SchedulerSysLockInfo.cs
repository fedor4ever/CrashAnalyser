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

namespace CrashDebuggerLib.Structures.Scheduler
{
    public class SchedulerSysLockInfo
    {
        #region Constructors
        public SchedulerSysLockInfo()
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint HoldingThreadAddress
        {
            get { return iHoldingThreadAddress; }
            set { iHoldingThreadAddress = value; }
        }

        public uint WaitingThreadAddress
        {
            get { return iWaitingThreadAddress; }
            set { iWaitingThreadAddress = value; }
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
        private uint iHoldingThreadAddress;
        private uint iWaitingThreadAddress;
        #endregion
    }
}
