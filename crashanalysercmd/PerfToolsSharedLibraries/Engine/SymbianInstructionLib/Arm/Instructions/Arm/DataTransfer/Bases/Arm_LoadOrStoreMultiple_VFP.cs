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
using SymbianStructuresLib.Arm.Registers.VFP;
using SymbianStructuresLib.Arm.Instructions;
using SymbianInstructionLib.Arm.Instructions.Common;

namespace SymbianInstructionLib.Arm.Instructions.Arm.DataTransfer
{
    public abstract class Arm_LoadOrStoreMultiple_VFP : Arm_LoadOrStoreMultiple
    {
        #region Constructors
        protected Arm_LoadOrStoreMultiple_VFP()
        {
            base.AITarget = TArmInstructionTarget.EVectorFloatingPoint;
        }
        #endregion

        #region Frin Arm_LoadOrStoreMultiple
        public override TArmRegisterType BaseRegister
        {
            get
            {
                SymMask mask = new SymMask( 0xF << 16, SymMask.TShiftDirection.ERight, 16 );
                TArmRegisterType ret = (TArmRegisterType) mask.Apply( base.AIRawValue );
                return ret;
            }
        }
        #endregion

        #region Framework API
        public abstract TArmRegisterTypeVFP FirstRegister
        {
            get;
        }

        public abstract TArmRegisterTypeVFP[] Registers
        {
            get;
        }
        #endregion

        #region Properties
        public TArmInstructionAddressingModeVFP AddressingMode
        {
            get
            {
                // The address mode is a combination of P U and W bits.
                TArmInstructionAddressingModeVFP ret = TArmInstructionAddressingModeVFP.EUndefined;

                // First extract P and U bits. NB: We shift this only 22 to the right because
                // we are about to inject the 'W' bit at bit zero.
                SymMask maskPU = new SymMask( 3u << 23, SymMask.TShiftDirection.ERight, 22 );
                uint bits = maskPU.Apply( base.AIRawValue );

                // Extract W bit
                SymBit wBit = base.AIRawValue[ ArmInstruction.KBitIndexW ];
                bits |= (uint) wBit;

                switch ( bits )
                {
                case 0:
                case 1:
                case 7:
                default:
                    break;
                //         P U W 
                case 2: // 0 1 0
                    ret = TArmInstructionAddressingModeVFP.EUnindexed;
                    break;
                case 3: // 0 1 0
                    ret = TArmInstructionAddressingModeVFP.EIncrement;
                    break;
                case 4: // 1 0 0
                    ret = TArmInstructionAddressingModeVFP.EOffsetNegative;
                    break;
                case 5: // 1 0 1
                    ret = TArmInstructionAddressingModeVFP.EDecrement;
                    break;
                case 6: // 1 1 0
                    ret = TArmInstructionAddressingModeVFP.EOffsetPositive;
                    break;
                }
                //
                return ret;
            }
        }

        public uint Offset
        {
            get
            {
                uint offset = base.AIRawValue & 0xFF;
                return offset;
            }
        }
        #endregion

        #region Internal constants
        protected readonly static SymMask KMaskFirstReg = new SymMask( 0xF000, SymMask.TShiftDirection.ERight, 12 );
        #endregion

        #region Data members
        #endregion
    }
}

