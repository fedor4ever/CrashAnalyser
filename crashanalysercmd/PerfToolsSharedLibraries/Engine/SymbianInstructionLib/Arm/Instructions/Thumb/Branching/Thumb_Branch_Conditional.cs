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
using SymbianStructuresLib.Arm.Instructions;
using SymbianInstructionLib.Arm.Instructions.Common;

namespace SymbianInstructionLib.Arm.Instructions.Thumb.Branching
{
    [ArmRef( "A7.1.13 B (1)", "B<cond> <target_address>" )]
    public class Thumb_Branch_Conditional : Thumb_Branch_Immediate
    {
        #region Constructors
        public Thumb_Branch_Conditional()
        {
            base.SetMask( "1101" + "####" + "########" );
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public override int BranchOffset
        {
            get
            {
                int ret = ThumbInstructionUtils.SignExtend8BitTo32Bit( base.AIRawValue & 0xFF );
                return ret;
            }
        }
        #endregion

        #region From ArmBaseInstruction
        protected override void OnRawValueAssigned()
        {
            base.OnRawValueAssigned();
            base.AIConditionCode = (TArmInstructionCondition) (TArmInstructionCondition) base.AIRawValue[ 11, 8 ].ToUInt();
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}

