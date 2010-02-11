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

namespace CrashDebuggerLib.Structures.NThread
{
    public class NThreadCountInfo
    {
        #region Enumerations
        public enum TCsFunctionType
        {
            EDoNothing = 0,
            ESuspendNTimes,
            EExitPending = -1,
            EExitInProgress = -2
        }
        #endregion

        #region Constructors
        public NThreadCountInfo()
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public int RequestSemaphoreCount
        {
            get { return iRequestSemaphoreCount; }
            set { iRequestSemaphoreCount = value; }
        }

        public int SuspendCount
        {
            get { return iSuspendCount; }
            set { iSuspendCount = value; }
        }

        public int CsCount
        {
            get { return iCsCount; }
            set { iCsCount = value; }
        }

        public int CsFunction
        {
            get { return iCsFunction; }
            set { iCsFunction = value; }
        }

        public uint CsFunctionRaw
        {
            get { return (uint) iCsFunction; }
            set { iCsFunction = (int) value; }
        }

        public int CsFunctionSuspendCount
        {
            get
            {
                int ret = 0;
                //
                if ( CsFunctionType != TCsFunctionType.ESuspendNTimes )
                {
                    throw new ArgumentException( "CsFunction is not \'Suspend N Times\'" );
                }
                //
                ret = iCsFunction;
                return ret;
            }
        }

        public TCsFunctionType CsFunctionType
        {
            get
            {
                TCsFunctionType ret = TCsFunctionType.EDoNothing;
                //
                if ( CsFunction > 0 )
                {
                    ret = TCsFunctionType.ESuspendNTimes;
                }
                else if ( CsFunction == -1 )
                {
                    ret = TCsFunctionType.EExitPending;
                }
                else if ( CsFunction == -2 )
                {
                    ret = TCsFunctionType.EExitInProgress;
                }
                //
                return ret;
            }
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
        private int iRequestSemaphoreCount = 0;
        private int iSuspendCount = 0; // -how many times we have been suspended
        private int iCsCount = 0; // critical section count
        private int iCsFunction = 0; // what to do on leaving CS: +n=suspend n times, 0=nothing, -1=exit
        #endregion
    }
}
