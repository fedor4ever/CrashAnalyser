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
using SymbianStructuresLib.Arm.Registers;
using SymbianInstructionLib.Arm.Instructions.Common;

namespace SymbianInstructionLib.Arm.Instructions.Arm.Branching
{
    public abstract class Arm_Branch_Register : Arm_Branch
    {
        #region Constructors
        protected Arm_Branch_Register()
        {
        }
        #endregion

        #region From Arm_Branch
        public override int BranchOffset
        {
            get
            {
                int offset = ArmInstructionUtils.SignExtend24BitTo32Bit( base.AIRawValue & 0x00FFFFFF );
                return offset;
            }
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            TArmRegisterType branchReg = this.RegisterContainingBranchAddress;
            return ( aRegister == branchReg );
        }
        #endregion

        #region Properties
        public TArmRegisterType RegisterContainingBranchAddress
        {
            get
            {
                TArmRegisterType ret = (TArmRegisterType) base.AIRawValue[ 3, 0 ].ToUInt();
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}

