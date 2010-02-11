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
    public abstract class Arm_LoadOrStoreMultiple_GP : Arm_LoadOrStoreMultiple
    {
        #region Constructors
        protected Arm_LoadOrStoreMultiple_GP()
        {
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            TArmDataTransferType type = this.DataTransferType;
            //
            bool ret = base.DoQueryInvolvementAsSource( aRegister );
            if ( ret == false && type == TArmDataTransferType.EStore )
            {
                // STMFD R13!, {R0 - R12, LR}
                // r13 = dest, r0->r12, r14 are source registers
                List<TArmRegisterType> regs = this.RegistersAsList;
                ret = regs.Contains( aRegister );
            }
            //
            return ret;
        }

        protected override bool DoQueryInvolvementAsDestination( TArmRegisterType aRegister )
        {
            TArmDataTransferType type = this.DataTransferType;
            //
            bool ret = base.DoQueryInvolvementAsDestination( aRegister );
            if ( ret == false && type == TArmDataTransferType.ELoad )
            {
                // LDMFD R13!, {R0 - R12, PC}
                // r13 = source, r0->r12, r15 are destination registers
                List<TArmRegisterType> regs = this.RegistersAsList;
                ret = regs.Contains( aRegister );
            }
            //
            return ret;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public bool AffectsPC
        {
            get
            {
                SymBit pcBit = SymBitUtils.GetBit( base.AIRawValue, 15 );
                return ( pcBit == SymBit.ESet );
            }
        }

        public override TArmRegisterType BaseRegister
        {
            get
            {
                SymMask mask = new SymMask( 0xF << 16, SymMask.TShiftDirection.ERight, 16 );
                TArmRegisterType ret = (TArmRegisterType) mask.Apply( base.AIRawValue );
                return ret;
            }
        }

        public TArmRegisterType[] Registers
        {
            get
            {
                TArmRegisterType[] list = RegistersAsList.ToArray();
                return list;
            }
        }

        public TArmInstructionAddressingMode AddressingMode
        {
            get
            {
                SymMask mask = new SymMask( 3u << 23, SymMask.TShiftDirection.ERight, 23 );
                TArmInstructionAddressingMode mode = (TArmInstructionAddressingMode) mask.Apply( base.AIRawValue );
                return mode;
            }
        }
        #endregion

        #region Internal methods
        private List<TArmRegisterType> RegistersAsList
        {
            get
            {
                List<TArmRegisterType> list = ArmInstructionUtils.ExtractGPRegisterList( base.AIRawValue & 0x0000FFFF );
                return list;
            }
        }
        #endregion

        #region Data members
        #endregion
    }
}

