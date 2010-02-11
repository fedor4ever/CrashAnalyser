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
    [ArmRefAttribute( "", "" )]
    public class Arm_BL : Arm_Branch_Immediate
    {
        #region Constructors
        public Arm_BL()
        {
            //        cond 101 L      signed_immed_24           
            //        1110 101 1 00001000    00100111 00100101  => 0x00209cc8
            base.SetMask( "#### 101 1 ########", "######## ########" );
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsDestination( TArmRegisterType aRegister )
        {
            bool ret = false;
            //
            bool linkSaved = base[ 24 ] == SymbianUtils.BasicTypes.SymBit.ESet;
            if ( linkSaved )
            {
                ret = ( aRegister == TArmRegisterType.EArmReg_LR );
            }
            //
            return ret;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}

