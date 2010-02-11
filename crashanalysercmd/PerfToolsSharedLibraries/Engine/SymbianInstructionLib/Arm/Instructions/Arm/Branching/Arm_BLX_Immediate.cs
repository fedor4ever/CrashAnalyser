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
    [ArmRefAttribute( "A4.1.8 BLX (1)", "BLX <target_addr>" )]
    public class Arm_BLX_Immediate : Arm_Branch_Immediate
    {
        #region Constructors
        public Arm_BLX_Immediate()
        {
            //                  101 H      signed_immed_24           
            //             1111 101 0 00001000    00100111 00100101
            base.SetMask( "1111 101 # ########", "######## ########" );
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsDestination( TArmRegisterType aRegister )
        {
            return ( aRegister == TArmRegisterType.EArmReg_LR );
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}

