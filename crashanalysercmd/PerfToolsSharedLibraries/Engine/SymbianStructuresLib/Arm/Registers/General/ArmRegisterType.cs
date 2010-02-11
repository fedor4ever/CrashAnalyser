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
using System.ComponentModel;
using System.Text;

namespace SymbianStructuresLib.Arm.Registers
{
    public enum TArmRegisterType : int
    {
        [Description( "??" )]
        EArmReg_Other = -1,

        /////////////////////
        // COMMON REGISTERS
        /////////////////////
        [Description("R00" )]
        EArmReg_00 = 0,

        [Description( "R01" )]
        EArmReg_01 = 1,

        [Description( "R02" )]
        EArmReg_02 = 2,

        [Description( "R03" )]
        EArmReg_03 = 3,

        [Description( "R04" )]
        EArmReg_04 = 4,

        [Description( "R05" )]
        EArmReg_05 = 5,

        [Description( "R06" )]
        EArmReg_06 = 6,

        [Description( "R07" )]
        EArmReg_07 = 7,

        [Description( "R08" )]
        EArmReg_08 = 8,

        [Description( "R09" )]
        EArmReg_09 = 9,

        [Description( "R10" )]
        EArmReg_10 = 10,

        [Description( "R11" )]
        EArmReg_11 = 11,

        [Description( "R12" )]
        EArmReg_12 = 12,

        [Description( "SP" )]
        EArmReg_SP = 13,

        [Description( "LR" )]
        EArmReg_LR = 14,

        [Description( "PC" )]
        EArmReg_PC = 15,

        [Description( "CPSR" )]
        EArmReg_CPSR = 16,

        [Description( "SPSR" )]
        EArmReg_SPSR,

        [Description( "DACR" )]
        EArmReg_DACR,

        [Description( "FSR" )]
        EArmReg_FSR,

        [Description( "FAR" )]
        EArmReg_FAR,

        [Description( "CAR" )]
        EArmReg_CAR,

        [Description( "MMUID" )]
        EArmReg_MMUID,

        [Description( "MMUCR" )]
        EArmReg_MMUCR,

        [Description( "AUXCR" )]
        EArmReg_AUXCR,

        [Description( "FPEXC" )]
        EArmReg_FPEXC,

        [Description( "CTYPE" )]
        EArmReg_CTYPE,

        [Description( "EXC_CODE" )]
        EArmReg_EXCCODE,

        [Description( "EXC_PC" )]
        EArmReg_EXCPC,

        /////////////////////////////////
        // CO-PROCESSOR SYSTEM CONTROL
        /////////////////////////////////
        [Description( "SYSCON_CONTROL" )]
        EArmReg_SysCon_Control,

        /////////////////////
        // ETM
        /////////////////////
        [Description( "ETM_CONTROL" )]
        EArmReg_ETM_Control,

        [Description( "ETM_ID")]
        EArmReg_ETM_Id,

        /////////////////////
        // ETB
        /////////////////////
        [Description( "ETB_RAM_DEPTH" )]
        EArmReg_ETB_RamDepth,
        
        [Description( "ETB_RAM_WIDTH" )]
        EArmReg_ETB_RamWidth,

        [Description( "ETB_STATUS" )]
        EArmReg_ETB_Status,

        [Description( "ETB_RAM_WRITE_POINTER" )]
        EArmReg_ETB_RamWritePointer,

        [Description( "ETB_TRIGGER_COUNTER" )]
        EArmReg_ETB_TriggerCounter,

        [Description( "ETB_CONTROL" )]
        EArmReg_ETB_Control,

        [Description( "ETB_ID" )]
        EArmReg_ETB_Id
    }
}
