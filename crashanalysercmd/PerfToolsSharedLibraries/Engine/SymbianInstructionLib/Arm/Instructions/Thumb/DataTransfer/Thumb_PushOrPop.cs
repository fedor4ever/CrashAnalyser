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

namespace SymbianInstructionLib.Arm.Instructions.Thumb.DataTransfer
{
    [ArmRef( "A7.1.50 PUSH", "PUSH <registers>" )]
    [ArmRef( "A7.1.49 POP",  "POP  <registers>" )]
    public class Thumb_PushOrPop : Thumb_LoadOrStoreMultiple
    {
        #region Constructors
        public Thumb_PushOrPop()
        {
            //                     Type          R     register_list
            base.SetMask( "1011" + "#" + "10" + "#"    + "########" );
            base.AIGroup = SymbianStructuresLib.Arm.Instructions.TArmInstructionGroup.EGroupDataTransfer;
        }
        #endregion

        #region From Thumb_LoadOrStore
        public override TArmRegisterType Rd
        {
            get { return TArmRegisterType.EArmReg_SP; }
        }
        #endregion

        #region Properties
        public override TArmDataTransferType DataTransferType
        {
            get
            {
                TArmDataTransferType ret = (TArmDataTransferType) base.AIRawValue[ 11 ];
                return ret;
            }
        }

        protected override List<TArmRegisterType> RegistersAsList
        {
            get
            {
                List<TArmRegisterType> regs = new List<TArmRegisterType>();

                // Bit 8 represents LR or PC, depending on whether it's a PUSH or POP.
                //
                // PUSH = R bit is set if LR is to be included
                // POP  = R bit is set if PC is to be included
                SymBit rBit = base.AIRawValue[ 8 ];
                if ( rBit == SymBit.ESet )
                {
                    if ( DataTransferType == TArmDataTransferType.ELoad )
                    {
                        regs.Add( TArmRegisterType.EArmReg_PC );
                    }
                    else if ( DataTransferType == TArmDataTransferType.EStore )
                    {
                        regs.Add( TArmRegisterType.EArmReg_LR );
                    }
                }

                // Bits 7-0 represent R7 -> R0.
                uint value = base.AIRawValue & 0xFF;
                for ( int i = 7; i >= 0; i-- )
                {
                    uint mask = (uint) ( 1 << i );
                    if ( ( mask & value ) == mask )
                    {
                        TArmRegisterType reg = (TArmRegisterType) i;
                        regs.Add( reg );
                    }
                }
                //
                return regs;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}

