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
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianStructuresLib.Arm.Registers;
using SymbianInstructionLib.Arm.Instructions.Common;
using SymbianInstructionLib.Arm.Instructions.Arm;
using SymbianInstructionLib.Arm.Instructions.Arm.DataProcessing;
using SymbianInstructionLib.Arm.Instructions.Thumb;
using SymbianInstructionLib.Arm.Instructions.Thumb.DataProcessing;
using SymbianStackAlgorithmAccurate.CPU;
using SymbianStackAlgorithmAccurate.Prologue;

namespace SymbianStackAlgorithmAccurate.Instructions.Types
{
    internal class AccInstBranch : AccInstruction
    {
        #region Constructors
        public AccInstBranch( IArmInstruction aInstruction )
            : base( aInstruction )
        {
            System.Diagnostics.Debug.Assert( base.Instruction.AIGroup == TArmInstructionGroup.EGroupBranch );
        }
        #endregion

        #region API
        internal override void Process( ArmPrologueHelper aProlog )
        {
        }

        internal override void Prefilter( AccInstructionList aInstructions, int aMyIndex, int aInstructionCountOffsetToPC )
        {
            if ( this.Ignored == false )
            {
                int count = aInstructions.Count;
                //
                if ( base.Instruction.AIConditionCode == TArmInstructionCondition.AL )
                {
                    // As soon as we see any unconditional branch statement we can be sure that we are past the Prologue.
                    for ( int i = aMyIndex + 1; i < count; i++ )
                    {
                        aInstructions[ i ].Ignored = true;
                    }
                }
                else
                {
                    // Count the number of interesting instructions before the conditional branch
                    int interestingInstructionCount = 0;
                    for ( int i = 0; i < aMyIndex; i++ )
                    {
                        AccInstruction accInstruction = aInstructions[ i ];
                        IArmInstruction inst = accInstruction.Instruction;
                        //
                        bool involvesSP = inst.QueryInvolvement( TArmRegisterType.EArmReg_SP );
                        if ( involvesSP )
                        {
                            ++interestingInstructionCount;
                        }
                    }

                    // If we have seen at least one interesting instruction then assume prologue is complete.
                    if ( interestingInstructionCount >= 1 )
                    {
                        for ( int i = aMyIndex; i < count; i++ )
                        {
                            aInstructions[ i ].Ignored = true;
                        }
                    }
                }
            }
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

