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
using System.ComponentModel;
using SymbianUtils.BasicTypes;
using SymbianStructuresLib.Arm.Registers;
using SymbianInstructionLib.Arm.Instructions.Common;

namespace SymbianInstructionLib.Arm.Instructions.Arm.DataProcessing
{
    public class Arm_DataProcessing_Shift_Immediate : Arm_DataProcessing_Shift
    {
        #region Constructors
        public Arm_DataProcessing_Shift_Immediate()
            : base( TEncoding.EEncodingShiftsImmediate )
        {
            //             Cond             Op.      S       Rn       Rd  shift_imm    shift            Rm
            base.SetMask( "####" + "000" + "####" + "#" + "####" + "####" + "#####" +   "##" + "0" + "####" );
        }
        #endregion

        #region From Arm_DataProcessing
        public override bool SuppliesImmediate
        {
            get { return true; }
        }

        public override uint Immediate
        {
            get
            {
                byte ret = base.AIRawValue[ 11, 7 ];

                TShiftType type = base.ShiftType;

                // Some of the shift types use a value of 0 to mean 32 bits.
                if ( type == TShiftType.EShiftLogicalRight && ret == 0 )
                {
                    ret = 32;
                }
                else if ( type == TShiftType.EShiftArithmeticRight && ret == 0 )
                {
                    ret = 32;
                }

                return ret;
            }
        }
        #endregion

        #region Properties
        public override TShiftType ShiftType
        {
            get
            {
                TShiftType type = base.ShiftType;
                
                // Some of the immediate values can actually alter the shift types.
                uint immediate = Immediate;
                if ( immediate == 0 && type == TShiftType.EShiftRotateRight )
                {
                    // Rotate right by 32 bits is the same as an RRX.
                    type = TShiftType.EShiftRotateRightWithExtend;
                }
                else if ( immediate == 0 && type == TShiftType.EShiftLogicalLeft )
                {
                    // LSL of 0 is actually not a shift.
                    type = TShiftType.EShiftNone;
                }

                return type;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}

