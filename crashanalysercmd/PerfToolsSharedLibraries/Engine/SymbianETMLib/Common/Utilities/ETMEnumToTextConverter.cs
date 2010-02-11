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
using SymbianETMLib.Common.Types;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Exceptions;

namespace SymbianETMLib.Common.Utilities
{
    public static class ETMEnumToTextConverter
    {
        public static string ToString( TArmExceptionType aException )
        {
            string mode = "???";
            //
            switch ( aException )
            {
            default:
            case TArmExceptionType.EUnknown:
                break;
            case TArmExceptionType.ENone:
                mode = "Normal";
                break;
            case TArmExceptionType.EHaltingDebug:
                mode = "Halting Debug";
                break;
            case TArmExceptionType.ESecureMonitorCall:
                mode = "Secure Monitor Call";
                break;
            case TArmExceptionType.EAsyncDataAbort:
                mode = "Data Abort";
                break;
            case TArmExceptionType.EJazelle:
                mode = "Jazelle";
                break;
            case TArmExceptionType.EProcessorReset:
                mode = "Reset";
                break;
            case TArmExceptionType.EUndefinedInstruction:
                mode = "Undefined Instruction";
                break;
            case TArmExceptionType.ESVC:
                mode = "SVC";
                break;
            case TArmExceptionType.EPrefetchAbortOrSWBreakpoint:
                mode = "Prefetch Abort / SW Breakpoint";
                break;
            case TArmExceptionType.ESyncDataAbortOrSWWatchpoint:
                mode = "Data Abort / SW Watchbpoint";
                break;
            case TArmExceptionType.EGeneric:
                mode = "Generic";
                break;
            case TArmExceptionType.EIRQ:
                mode = "IRQ";
                break;
            case TArmExceptionType.EFIQ:
                mode = "FIQ";
                break;
            }
            //
            return mode;
        }

        public static string ToString( TArmInstructionSet aInstructionSet )
        {
            string ret = string.Empty;
            //
            switch ( aInstructionSet )
            {
            case TArmInstructionSet.EARM:
                ret = "[A]";
                break;
            case TArmInstructionSet.ETHUMB:
                ret = "[T]";
                break;
            case TArmInstructionSet.EJAZELLE:
                ret = "[J]";
                break;
            default:
                ret = "[?]";
                break;
            }
            //
            return ret;
        }
    }
}
