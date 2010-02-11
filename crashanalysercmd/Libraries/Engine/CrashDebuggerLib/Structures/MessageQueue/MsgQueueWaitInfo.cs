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
using CrashDebuggerLib.Structures.Thread;
using CrashDebuggerLib.Structures.Common;

namespace CrashDebuggerLib.Structures.MessageQueue
{
    public class MsgQueueWaitInfo
    {
        #region Enumerations
        public enum TType
        {
            EWaitTypeSpace = 0,
            EWaitTypeData
        }
        #endregion

        #region Constructors
        public MsgQueueWaitInfo( TType aType, CrashDebuggerInfo aCrashDebugger )
        {
            iType = aType;
            iCrashDebugger = aCrashDebugger;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint WaitingThreadAddress
        {
            get { return iWaitingThreadAddress; }
            set { iWaitingThreadAddress = value; }
        }

        public DThread WaitingThread
        {
            get { return iCrashDebugger.ThreadByAddress( WaitingThreadAddress ); }
        }

        public RequestStatus RequestStatus
        {
            get { return iRequestStatus; }
            set { iRequestStatus = value; }
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
        private readonly TType iType;
        private readonly CrashDebuggerInfo iCrashDebugger;
        private uint iWaitingThreadAddress = 0;
        private RequestStatus iRequestStatus = new RequestStatus();
        #endregion
    }
}
