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
using System.IO;
using SymbianUtils.BasicTypes;
using SymbianStackAlgorithmAccurate.CPU;
using SymbianStackAlgorithmAccurate.Prologue;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianStructuresLib.Arm.Registers;
using SymbianInstructionLib.Arm.Instructions.Common;
using SymbianInstructionLib.Arm.Instructions.Arm;
using SymbianInstructionLib.Arm.Instructions.Arm.DataProcessing;
using SymbianInstructionLib.Arm.Instructions.Thumb;
using SymbianInstructionLib.Arm.Instructions.Thumb.DataProcessing;

namespace SymbianStackAlgorithmAccurate.Instructions.Types
{
    internal class AccInstDataProcessing : AccInstruction
    {
        #region Constructors
        public AccInstDataProcessing( IArmInstruction aInstruction )
            : base( aInstruction )
        {
            System.Diagnostics.Debug.Assert( base.Instruction.AIGroup == TArmInstructionGroup.EGroupDataProcessing );
        }
        #endregion

        #region API
        internal override void Process( ArmPrologueHelper aProlog )
        {
            IArmInstruction instruction = base.Instruction;
            
            // Only unconditional instructions are handled
            if ( instruction.AIConditionCode == TArmInstructionCondition.AL )
            {
                // Two heuristically observed requirements:
                //
                // 1) It must be an immediate instruction
                // 2) It must apply with source & destination registers both being SP
                if ( instruction is ArmInstruction )
                {
                    // Aim is to detect modifications to SP (i.e. reservation of stack space)
                    Arm_DataProcessing armDpInst = instruction as Arm_DataProcessing;
                    
                    // 1) Must supply an immediate value
                    if ( armDpInst != null && armDpInst.SuppliesImmediate )
                    {
                        // 2) Must apply to SP
                        if ( armDpInst.Rd == TArmRegisterType.EArmReg_SP &&
                             armDpInst.Rn == TArmRegisterType.EArmReg_SP )
                        {
                            uint immediate = armDpInst.Immediate;
                            HandleDPOperation( armDpInst.OperationType, immediate, aProlog );
                        }
                    }
                }
                else if ( instruction is ThumbInstruction )
                {
                    Thumb_AddOrSubtract thumbDpInst = instruction as Thumb_AddOrSubtract;

                    // 2) Must apply to SP
                    if ( thumbDpInst.Rd == TArmRegisterType.EArmReg_SP )
                    {
                        // 1) Must supply an immediate value
                        if ( thumbDpInst != null && thumbDpInst.SuppliesImmediate )
                        {
                            uint immediate = thumbDpInst.Immediate;
                            HandleDPOperation( thumbDpInst.OperationType, immediate, aProlog );
                        }
                        else if ( thumbDpInst is Thumb_Add_2Regs_High )
                        {
                            // Handle the case where one register supplies the number of
                            // words by which the stack pointer is incremented. Used when 
                            // a large stack allocation is made.
                        }
                    }
                }
                else
                {
                    throw new NotSupportedException( "Instruction type not supported" );
                }
            }
        }
        #endregion

        #region Properties
        public bool AppliesToSP
        {
            get
            {
                bool ret = false;
                IArmInstruction instruction = base.Instruction;

                // Only unconditional instructions are handled
                if ( instruction.AIConditionCode == TArmInstructionCondition.AL )
                {
                    // Two heuristically observed requirements:
                    //
                    // 1) It must be an immediate instruction
                    // 2) It must apply with source & destination registers both being SP
                    if ( instruction is ArmInstruction )
                    {
                        // Aim is to detect modifications to SP (i.e. reservation of stack space)
                        Arm_DataProcessing armDpInst = instruction as Arm_DataProcessing;

                        // 1) Must supply an immediate value
                        if ( armDpInst != null && armDpInst.SuppliesImmediate )
                        {
                            // 2) Must apply to SP
                            if ( armDpInst.Rd == TArmRegisterType.EArmReg_SP &&
                                 armDpInst.Rn == TArmRegisterType.EArmReg_SP )
                            {
                                ret = true;
                            }
                        }
                    }
                    else if ( instruction is ThumbInstruction )
                    {
                        Thumb_AddOrSubtract thumbDpInst = instruction as Thumb_AddOrSubtract;

                        // 2) Must apply to SP
                        if ( thumbDpInst.Rd == TArmRegisterType.EArmReg_SP )
                        {
                            // 1) Must supply an immediate value
                            if ( thumbDpInst != null && thumbDpInst.SuppliesImmediate )
                            {
                                ret = true;
                            }
                            else if ( thumbDpInst is Thumb_Add_2Regs_High )
                            {
                                // Handle the case where one register supplies the number of
                                // words by which the stack pointer is incremented. Used when 
                                // a large stack allocation is made.
                            }
                        }
                    }
                }

                return ret;
            }
        }
        #endregion

        #region Internal methods
        private void HandleDPOperation( TArmDataProcessingType aType, uint aImmediate, ArmPrologueHelper aProlog )
        {
            int wordsPushed = (int) aImmediate / 4;
            //
            switch( aType )
            {
            case TArmDataProcessingType.ADD:
                wordsPushed = -wordsPushed;
                break;
            case TArmDataProcessingType.SUB:
                break;
            default:
                throw new NotSupportedException( "Data processing does not (yet) support instructions of type: " + aType );
            }
            //
            aProlog.AddToNumberOfWordsPushedOnStack( wordsPushed );
        }
        #endregion

        #region Data members
        #endregion
    }
}

