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
using System.Collections.Generic;
using System.Text;

namespace SymbianStructuresLib.Arm.Exceptions
{
    public enum TArmExceptionVectorLocation : uint
    {
        ENormal = 0x00000000,
        EHigh = 0xFFFF0000
    }

    public enum TArmExceptionType
    {
        EUnknown = -1,
        ENone = 0,
        EHaltingDebug,
        ESecureMonitorCall,
        EAsyncDataAbort,
        EJazelle,
        EProcessorReset,
        EUndefinedInstruction,
        ESVC,
        EPrefetchAbortOrSWBreakpoint,
        ESyncDataAbortOrSWWatchpoint,
        EGeneric,
        EIRQ,
        EFIQ
    }

    public enum TArmExceptionVector
    {
        EReset                  = 0x00000000,
        EUndefinedInstruction   = 0x00000004,
        ESVC                    = 0x00000008,
        EPrefetchAbort          = 0x0000000C,
        EDataAbort              = 0x00000010,
        EIRQ                    = 0x00000018,
        EFIQ                    = 0x0000001C
    }
}
