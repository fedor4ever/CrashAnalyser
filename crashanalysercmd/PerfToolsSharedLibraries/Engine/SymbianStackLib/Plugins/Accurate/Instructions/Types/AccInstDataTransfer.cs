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
using SymbianStructuresLib.Arm.Registers.VFP;
using SymbianInstructionLib.Arm.Instructions.Common;
using SymbianInstructionLib.Arm.Instructions.Arm;
using SymbianInstructionLib.Arm.Instructions.Arm.DataTransfer;
using SymbianInstructionLib.Arm.Instructions.Thumb;
using SymbianInstructionLib.Arm.Instructions.Thumb.DataTransfer;

namespace SymbianStackAlgorithmAccurate.Instructions.Types
{
    internal class AccInstDataTransfer : AccInstruction
    {
        #region Constructors
        public AccInstDataTransfer( IArmInstruction aInstruction )
            : base( aInstruction )
        {
            System.Diagnostics.Debug.Assert( base.Instruction.AIGroup == TArmInstructionGroup.EGroupDataTransfer );
        }
        #endregion

        #region API
        internal override void Process( ArmPrologueHelper aProlog )
        {
            IArmInstruction instruction = base.Instruction;
            
            // Only unconditional instructions are handled
            if ( instruction.AIConditionCode == TArmInstructionCondition.AL )
            {
                if ( instruction is ArmInstruction )
                {
                    ArmInstruction armInst = (ArmInstruction) instruction;
                    //
                    if ( armInst is Arm_LoadOrStoreMultiple )
                    {
                        Arm_LoadOrStoreMultiple lsmInstruction = (Arm_LoadOrStoreMultiple) instruction;

                        // We're looking for store operations
                        if ( lsmInstruction.DataTransferType == TArmDataTransferType.EStore )
                        {
                            // We're looking for LSM's that involve SP.
                            if ( lsmInstruction.BaseRegister == TArmRegisterType.EArmReg_SP )
                            {
                                if ( lsmInstruction is Arm_LoadOrStoreMultiple_GP )
                                {
                                    Arm_LoadOrStoreMultiple_GP gpLsmInstruction = (Arm_LoadOrStoreMultiple_GP) lsmInstruction;
                                    HandleDTOperation( aProlog, gpLsmInstruction.Registers );
                                }
                                else if ( lsmInstruction is Arm_LoadOrStoreMultiple_VFP )
                                {
                                    Arm_LoadOrStoreMultiple_VFP vfpLsmInstruction = (Arm_LoadOrStoreMultiple_VFP) lsmInstruction;
                                    HandleDTOperation( aProlog, vfpLsmInstruction.Registers );
                                }
                            }
                        }
                    }
                }
                else if ( instruction is ThumbInstruction )
                {
                    ThumbInstruction thumbInst = (ThumbInstruction) instruction;
                    //
                    if ( thumbInst is Thumb_LoadOrStoreMultiple )
                    {
                        // Special case that loads or stores multiple registers
                        Thumb_LoadOrStoreMultiple lsmThumb = (Thumb_LoadOrStoreMultiple) thumbInst;
                        if ( lsmThumb.DataTransferType == TArmDataTransferType.EStore && lsmThumb.Rd == TArmRegisterType.EArmReg_SP )
                        {
                            HandleDTOperation( aProlog, lsmThumb.Registers );
                        }
                        else
                        {
                        }
                    }
                    else if ( thumbInst is Thumb_LDR_RelativeToPC )
                    {
                        // When the Prologue needs to establish a working stack slurry, then often
                        // the scratch registers are used to build up a large subtraction from SP.
                        HandleDTLoad( aProlog, thumbInst as Thumb_LDR_RelativeToPC );
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
        #endregion

        #region Internal methods
        private void HandleDTOperation( ArmPrologueHelper aProlog, TArmRegisterType[] aRegisterList )
        {
            int count = aRegisterList.Length;
            for ( int i = 0; i < count; i++ )
            {
                TArmRegisterType register = aRegisterList[ i ];
                int push = aProlog.IncrementNumberOfWordsPushedOnStack( register );
                aProlog.OffsetValues[ register ].Value = (uint) push;
            }
        }

        private void HandleDTOperation( ArmPrologueHelper aProlog, TArmRegisterTypeVFP[] aRegisterList )
        {
            int numberOfRegisters = aRegisterList.Length;
            if ( numberOfRegisters > 0 )
            {
                // The size of the register varies... but the list will be consistent.
                TArmRegisterTypeVFP first = aRegisterList[ 0 ];
                //
                int numberOfBitsPerRegister = ArmVectorFloatingPointUtils.RegisterSizeInBits( first );
                int numberOfBytesPerRegister = numberOfBitsPerRegister / 8;
                int totalNumberOfBytes = numberOfBytesPerRegister * numberOfRegisters;
                int numberOfWords = totalNumberOfBytes / 4;
                //
                aProlog.AddToNumberOfWordsPushedOnStack( numberOfWords );
            }
        }

        private void HandleDTLoad( ArmPrologueHelper aProlog, Thumb_LoadOrStore_Immediate8 aInstruction )
        {
            // E.g:
            //
            // LDR R0, [PC, #40] ; Load R0 from PC + 0x40 (= address of the LDR instruction + 8 + 0x40)
            //
            TArmRegisterType reg = aInstruction.Rd;
            uint immed = aInstruction.Immediate * 4u;

            // PC = Is the program counter. Its value is used to calculate the memory 
            // address. Bit 1 of the PC value is forced to zero for the purpose of 
            // this calculation, so the address is always word-aligned.
            uint pcAddress = aProlog.ProloguePC & 0xFFFFFFFC;
            pcAddress = pcAddress + immed;

            // Read code value at specified address
            uint value = aProlog.CodeHelper.LoadData( pcAddress );

            // Set the register
            aProlog.CPU[ reg ].Value = value;
        }
        #endregion

        #region Data members
        #endregion
    }
}

