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
using SymbianStructuresLib.Arm.Exceptions;
using SymbianStructuresLib.Arm.Registers;

namespace SymbianETMLib.Common.Utilities
{
    internal static class ETMTextToEnumConverter
    {
        public static TArmRegisterType ToRegisterType( string aText )
        {
            TArmRegisterType ret = TArmRegisterType.EArmReg_Other;
            //
            string text = aText.ToUpper();
            if ( text == "ETB11_IDENTIFICATION" )
            {
                ret = TArmRegisterType.EArmReg_ETB_Id;
            }
            else if ( text == "ETB11_RAM_DEPTH" )
            {
                ret = TArmRegisterType.EArmReg_ETB_RamDepth;
            }
            else if ( text == "ETB11_RAM_WIDTH" )
            {
                ret = TArmRegisterType.EArmReg_ETB_RamWidth;
            }
            else if ( text == "ETB11_STATUS" )
            {
                ret = TArmRegisterType.EArmReg_ETB_Status;
            }
            else if ( text == "ETB11_RAM_WRITE_POINTER" )
            {
                ret = TArmRegisterType.EArmReg_ETB_RamWritePointer;
            }
            else if ( text == "ETB11_TRIGGER_COUNTER" )
            {
                ret = TArmRegisterType.EArmReg_ETB_TriggerCounter;
            }
            else if ( text == "ETB11_CONTROL" )
            {
                ret = TArmRegisterType.EArmReg_ETB_Control;
            }
            else if ( text == "ETM_ETM_CONTROL" )
            {
                ret = TArmRegisterType.EArmReg_ETM_Control;
            }
            else if ( text == "ETM_ETM_ID" )
            {
                ret = TArmRegisterType.EArmReg_ETM_Id;
            }
            //
            return ret;
        }

        public static TArmExceptionVector ToExceptionVector( string aText )
        {
            TArmExceptionVector ret = TArmExceptionVector.EUndefinedInstruction;
            //
            string text = aText.ToUpper();
            if ( text == "RST" )
            {
                ret = TArmExceptionVector.EReset;
            }
            else if ( text == "UND" )
            {
                ret = TArmExceptionVector.EUndefinedInstruction;
            }
            else if ( text == "SWI" )
            {
                ret = TArmExceptionVector.ESVC;
            }
            else if ( text == "ETB11_STATUS" )
            {
                ret = TArmExceptionVector.EPrefetchAbort;
            }
            else if ( text == "PRE" )
            {
                ret = TArmExceptionVector.EPrefetchAbort;
            }
            else if ( text == "DAT" )
            {
                ret = TArmExceptionVector.EDataAbort;
            }
            else if ( text == "IRQ" )
            {
                ret = TArmExceptionVector.EIRQ;
            }
            else if ( text == "FIQ" )
            {
                ret = TArmExceptionVector.EFIQ;
            }
            //
            return ret;
        }
    }
}
