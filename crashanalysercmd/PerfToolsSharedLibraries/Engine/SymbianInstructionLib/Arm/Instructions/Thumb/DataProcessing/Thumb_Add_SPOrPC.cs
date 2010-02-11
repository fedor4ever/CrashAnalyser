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

namespace SymbianInstructionLib.Arm.Instructions.Thumb.DataProcessing
{
    [ArmRef( "A7.1.7 ADD (5)", "ADD <Rd>, PC, #<immed_8> * 4" )]
    [ArmRef( "A7.1.8 ADD (6)", "ADD <Rd>, SP, #<immed_8> * 4" )]
    public class Thumb_Add_SPOrPC : Thumb_AddOrSubtract
    {
        #region Constructors
        public Thumb_Add_SPOrPC()
        {
            //                     Type    Rd      immed_8
            base.SetMask( "1010" + "#" + "###" + "########" );
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            // SP or PC acts as source
            TArmRegisterType reg = this.RelativeRegister;
            return ( aRegister == reg );
        }
        #endregion

        #region From Thumb_AddOrSubtract
        public override TArmRegisterType Rd
        {
            get
            {
                TArmRegisterType ret = (TArmRegisterType) KMaskRd.Apply( base.AIRawValue );
                return ret;
            }
        }

        public TArmRegisterType RelativeRegister
        {
            get
            {
                SymBit bit11 = base.AIRawValue[ 11 ];
                TArmRegisterType ret = TArmRegisterType.EArmReg_PC;
                if ( bit11 == SymBit.ESet )
                {
                    ret = TArmRegisterType.EArmReg_SP;
                }
                return ret;
            }
        }

        public override TArmDataProcessingType OperationType
        {
            get { return TArmDataProcessingType.ADD; }
        }

        public override uint Immediate
        {
            get
            {
                uint rawValue = ( base.AIRawValue & 0xFF );
                uint ret = rawValue * 4;
                return ret;
            }
        }

        public override bool SuppliesImmediate
        {
            get { return true; }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private static readonly SymMask KMaskRd = new SymMask( 0x700, SymMask.TShiftDirection.ERight, 8 );
        #endregion
    }
}

