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
    public abstract class Arm_DataProcessing_Shift : Arm_DataProcessing
    {
        #region Enumerations
        public enum TShiftType
        {
            EShiftNone = -1,
            EShiftLogicalLeft = 0,
            EShiftLogicalRight,
            EShiftArithmeticRight,
            EShiftRotateRight,
            EShiftRotateRightWithExtend
        }
        #endregion

        #region Constructors
        protected Arm_DataProcessing_Shift( TEncoding aEncoding )
            : base( aEncoding )
        {
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            TArmRegisterType reg1 = base.Rn;
            TArmRegisterType reg2 = this.Rm;
            return ( aRegister == reg1 || aRegister == reg2 );
        }
        #endregion

        #region From ArmInstruction
        public override bool Matches( uint aOpCode )
        {
            // If the following values hold true then it is not a data processing instruction at all.
            //
            // bit[25] == 0
            // bit[ 4] == 1
            // bit[ 7] == 1
            //
            bool match = base.Matches( aOpCode );
            if ( match )
            {
                SymUInt32 raw = aOpCode;
                if ( raw[ 25 ] == SymBit.EClear && raw[ 4 ] == SymBit.ESet && raw[ 7 ] == SymBit.ESet )
                {
                    match = false;
                }
            }
            //
            return match;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public virtual TShiftType ShiftType
        {
            get
            {
                TShiftType ret = (TShiftType) base.AIRawValue[ 6, 5 ].ToUInt();
                return ret;
            }
        }

        public TArmRegisterType Rm
        {
            get
            {
                TArmRegisterType ret = (TArmRegisterType) base.AIRawValue[ 3, 0 ].ToUInt();
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}

