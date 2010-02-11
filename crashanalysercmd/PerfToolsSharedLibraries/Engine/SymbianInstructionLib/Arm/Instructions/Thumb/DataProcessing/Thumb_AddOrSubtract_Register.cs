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
using SymbianInstructionLib.Arm.Instructions.Common;

namespace SymbianInstructionLib.Arm.Instructions.Thumb.DataProcessing
{
    [ArmRef( "A7.1.5 ADD (3)",  "ADD <Rd>, <Rn>, <Rm>" )]
    [ArmRef( "A7.1.67 SUB (3)", "SUB <Rd>, <Rn>, <Rm>" )]
    public class Thumb_AddOrSubtract_Register : Thumb_AddOrSubtract_2Regs
    {
        #region Constructors
        public Thumb_AddOrSubtract_Register()
        {
            //                      Type     Rm      Rn      Rd
            base.SetMask( "000110" + "#" + "###" + "###" + "###" );
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            TArmRegisterType reg1 = this.Rm;
            TArmRegisterType reg2 = this.Rn;
            return ( aRegister == reg1 || aRegister == reg2 );
        }
        #endregion

        #region Properties
        public TArmRegisterType Rm
        {
            get
            {
                TArmRegisterType ret = (TArmRegisterType) KMaskRm.Apply( base.AIRawValue );
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private static readonly SymMask KMaskRm = new SymMask( 0x1C0, SymMask.TShiftDirection.ERight, 6 );
        #endregion
    }
}

