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

namespace SymbianStructuresLib.Arm.Registers
{
    public enum TArmRegisterBank
    {
        // Special values
        ETypeUnknown = 0,
        ETypeException = -1, 
        ETypeCommon = -2,
        ETypeCoProcessor = -3,
        ETypeETM = -4,
        ETypeETB = -5,
        ETypeCoProSystemControl = -6,

        // Corresponding CPSR binary values
        ETypeUser = 0x10,           // 0b10000
        ETypeFastInterrupt = 0x11,  // 0b10001
        ETypeInterrupt = 0x12,      // 0b10010
        ETypeSupervisor = 0x13,     // 0b10011
        ETypeAbort = 0x17,          // 0b10111
        ETypeUndefined = 0x1B,      // 0b11011
        ETypeSystem = 0x1F          // 0b11111
    }

    public static class ArmRegisterBankUtils
    {
        #region API
        public static uint MakeStatusRegisterValue( TArmRegisterBank aBank )
        {
            uint val = 0;
            //
            switch ( aBank )
            {
            case TArmRegisterBank.ETypeUser:
            case TArmRegisterBank.ETypeFastInterrupt:
            case TArmRegisterBank.ETypeInterrupt:
            case TArmRegisterBank.ETypeSupervisor:
            case TArmRegisterBank.ETypeAbort:
            case TArmRegisterBank.ETypeUndefined:
            case TArmRegisterBank.ETypeSystem:
                val = (uint) aBank;
                break;
            default:
                break;
            }
            //
            return val;
        }

        public static TArmRegisterBank ExtractBank( uint aValue )
        {
            uint val = aValue & KBankMask;
            TArmRegisterBank bank = (TArmRegisterBank) val;
            //
            switch ( bank )
            {
            case TArmRegisterBank.ETypeUser:
            case TArmRegisterBank.ETypeFastInterrupt:
            case TArmRegisterBank.ETypeInterrupt:
            case TArmRegisterBank.ETypeSupervisor:
            case TArmRegisterBank.ETypeAbort:
            case TArmRegisterBank.ETypeUndefined:
            case TArmRegisterBank.ETypeSystem:
                break;
            default:
                bank = TArmRegisterBank.ETypeUndefined;
                break;
            }
            //
            return bank;
        }

        public static TArmRegisterBank ExtractBank( ArmRegister aRegister )
        {
            return ExtractBank( aRegister.Value );
        }

        public static string BankAsString( TArmRegisterBank aBank )
        {
            string ret = string.Empty;
            //
            switch ( aBank )
            {
            default:
            case TArmRegisterBank.ETypeUnknown:
            case TArmRegisterBank.ETypeCommon:
                ret = string.Empty;
                break;
            case TArmRegisterBank.ETypeException:
                ret = "EXC";
                break;
            case TArmRegisterBank.ETypeCoProcessor:
                ret = "COP";
                break;
            case TArmRegisterBank.ETypeAbort:
                ret = "ABT";
                break;
            case TArmRegisterBank.ETypeFastInterrupt:
                ret = "FIQ";
                break;
            case TArmRegisterBank.ETypeInterrupt:
                ret = "IRQ";
                break;
            case TArmRegisterBank.ETypeSupervisor:
                ret = "SVC";
                break;
            case TArmRegisterBank.ETypeSystem:
                ret = "SYS";
                break;
            case TArmRegisterBank.ETypeUndefined:
                ret = "UND";
                break;
            case TArmRegisterBank.ETypeUser:
                ret = "USR";
                break;
            }
            //
            return ret;
        }

        public static string BankAsStringLong( TArmRegisterBank aBank )
        {
            string ret = string.Empty;
            //
            switch ( aBank )
            {
            default:
            case TArmRegisterBank.ETypeUnknown:
            case TArmRegisterBank.ETypeCommon:
                ret = string.Empty;
                break;
            case TArmRegisterBank.ETypeException:
                ret = "Exception";
                break;
            case TArmRegisterBank.ETypeCoProcessor:
                ret = "Co-processor";
                break;
            case TArmRegisterBank.ETypeAbort:
                ret = "Abort";
                break;
            case TArmRegisterBank.ETypeFastInterrupt:
                ret = "Fast Interrupt";
                break;
            case TArmRegisterBank.ETypeInterrupt:
                ret = "Interrupt";
                break;
            case TArmRegisterBank.ETypeSupervisor:
                ret = "Supervisor";
                break;
            case TArmRegisterBank.ETypeSystem:
                ret = "System";
                break;
            case TArmRegisterBank.ETypeUndefined:
                ret = "Undefined";
                break;
            case TArmRegisterBank.ETypeUser:
                ret = "User";
                break;
            }
            //
            return ret;
        }
        #endregion

        #region Internal constants
        private const uint KBankMask = 0x1F; // 11111b
        #endregion
    }
}
