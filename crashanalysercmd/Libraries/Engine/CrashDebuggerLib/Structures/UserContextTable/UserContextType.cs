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

namespace CrashDebuggerLib.Structures.UserContextTable
{
    [System.ComponentModel.TypeConverter( typeof( SymbianParserLib.TypeConverters.SymbianEnumConverter ) )]
    public enum TUserContextType
    {
        EContextNone = 0,                       /**< Thread has no user context */
        EContextException = 1,	    	        /**< Hardware exception while in user mode */
        EContextUndefined = 2,
        EContextUserInterrupt =3,       	    /**< Preempted by interrupt taken in user mode */
        EContextUserInterruptDied = 4,          /**< Killed while preempted by interrupt taken in user mode */
        EContextSvsrInterrupt1 = 5,             /**< Preempted by interrupt taken in executive call handler */
        EContextSvsrInterrupt1Died = 6,         /**< Killed while preempted by interrupt taken in executive call handler */
        EContextSvsrInterrupt2 = 7,             /**< Preempted by interrupt taken in executive call handler */
        EContextSvsrInterrupt2Died = 8,         /**< Killed while preempted by interrupt taken in executive call handler */
        EContextWFAR = 9,                       /**< Blocked on User::WaitForAnyRequest() */
        EContextWFARDied = 10,                  /**< Killed while blocked on User::WaitForAnyRequest() */
        EContextExec = 11,				        /**< Slow executive call */
        EContextKernel = 12				        /**< Kernel side context (for kernel threads) */
    }
}

