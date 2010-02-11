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
using SymbianUtils.BasicTypes;
using SymbianStructuresLib.Arm.Registers;
using SymbianStructuresLib.Arm.Instructions;
using SymbianInstructionLib.Arm.Instructions.Common;

namespace SymbianInstructionLib.Arm.Instructions.Arm.DataTransfer
{
    public abstract class Arm_LoadOrStoreMultiple : Arm_LoadOrStore
    {
        #region Constructors
        protected Arm_LoadOrStoreMultiple()
        {
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            bool ret = false;
            //
            TArmDataTransferType type = this.DataTransferType;
            if ( type == TArmDataTransferType.EStore )
            {
                // STMFD R13!, {R0 - R12, LR}
                // r13 = dest, r0->r12, r14 are source registers
                //
                // Cannot test here, requires derived class to implement this check
            }
            else if ( type == TArmDataTransferType.ELoad )
            {
                // LDMFD R13!, {R0 - R12, PC}
                // r13 = source, r0->r12, r15 are destination registers
                TArmRegisterType baseReg = this.BaseRegister;
                ret = ( aRegister == baseReg );
            }
            //
            return ret;
        }

        protected override bool DoQueryInvolvementAsDestination( TArmRegisterType aRegister )
        {
            bool ret = false;
            //
            TArmDataTransferType type = this.DataTransferType;
            if ( type == TArmDataTransferType.EStore )
            {
                // STMFD R13!, {R0 - R12, LR}
                // r13 = dest, r0->r12, r14 are source registers
                TArmRegisterType baseReg = this.BaseRegister;
                ret = ( aRegister == baseReg );
            }
            else if ( type == TArmDataTransferType.ELoad )
            {
                // LDMFD R13!, {R0 - R12, PC}
                // r13 = source, r0->r12, r15 are destination registers
                //
                // Cannot test here, requires derived class to implement this check
            }
            //
            return ret;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public abstract TArmRegisterType BaseRegister
        {
            get;
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}

