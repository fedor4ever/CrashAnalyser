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
using SymbianStackAlgorithmAccurate.Instructions;
using SymbianStructuresLib.Arm.Registers;
using SymbianStructuresLib.Arm;

namespace SymbianStackAlgorithmAccurate.CPU
{
    public class ArmCpu
    {
        #region Constructors
        public ArmCpu()
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public ArmRegisterCollection Registers
        {
            get { return iRegisters; }
            set
            { 
                // Copy the registers, don't take a reference to the supplied ones..
                // Otherwise we cannot invoke stack walking twice since we blatted
                // the original (external) copy.
                ArmRegisterCollection regs = new ArmRegisterCollection( value );
                iRegisters = regs;
            }
        }

        public ArmRegister SP
        {
            get
            {
                ArmRegister ret = Registers[ TArmRegisterType.EArmReg_SP ];
                return ret;
            }
        }

        public ArmRegister LR
        {
            get
            {
                ArmRegister ret = Registers[ TArmRegisterType.EArmReg_LR ];
                return ret;
            }
        }

        public ArmRegister PC
        {
            get
            {
                ArmRegister ret = Registers[ TArmRegisterType.EArmReg_PC ];
                return ret;
            }
        }

        public ArmRegister CPSR
        {
            get
            {
                ArmRegister ret = Registers[ TArmRegisterType.EArmReg_CPSR ];
                return ret;
            }
        }

        public TArmInstructionSet CurrentProcessorMode
        {
            get
            {
                TArmInstructionSet ret = TArmInstructionSet.EARM;
                ArmRegister cpsr = CPSR;
                //
                if ( ( cpsr.Value & 0x20 ) != 0 )
                {
                    ret = TArmInstructionSet.ETHUMB;
                }
                //
                return ret;
            }
            set
            {
                if ( value == TArmInstructionSet.EARM )
                {
                    // Clear CPSR Thumb bit
                    CPSR.Value &= 0xFFFFFFDF;
                }
                else if ( value == TArmInstructionSet.ETHUMB )
                {
                    // Set CPSR Thumb bit
                    CPSR.Value |= 0x00000020;
                }
            }
        }

        public ArmRegister this[ TArmRegisterType aType ]
        {
            get { return iRegisters[ aType ]; }
        }
        #endregion

        #region Data members
        private ArmRegisterCollection iRegisters = new ArmRegisterCollection();
        #endregion
    }
}
