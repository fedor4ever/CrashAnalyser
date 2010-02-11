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
    [ArmRef( "A7.1.9 ADD (7)",  "ADD SP, #<immed_7> * 4" )]
    [ArmRef( "A7.1.68 SUB (4)", "SUB SP, #<immed_7> * 4" )]
    public class Thumb_AddOrSubtract_SP : Thumb_AddOrSubtract
    {
        #region Constructors
        public Thumb_AddOrSubtract_SP()
        {
            //                         Type   immed_7
            base.SetMask( "10110000" + "#" + "#######" );
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            // SP acts as source & destination
            return ( aRegister == TArmRegisterType.EArmReg_SP );
        }
        #endregion

        #region From Thumb_AddOrSubtract
        public override TArmDataProcessingType OperationType
        {
            get
            {
                TArmDataProcessingType ret = TArmDataProcessingType.ADD;
                //
                if ( base.AIRawValue[ 7 ] == SymBit.ESet )
                {
                    ret = TArmDataProcessingType.SUB;
                }
                //
                return ret;
            }
        }
        #endregion

        #region Properties
        public override uint Immediate
        {
            get
            {
                uint rawValue = ( base.AIRawValue & 0x7F );
                uint ret = rawValue * 4;
                return ret;
            }
        }

        public override bool SuppliesImmediate
        {
            get { return true; }
        }

        public override TArmRegisterType Rd
        {
            get { return TArmRegisterType.EArmReg_SP; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private static readonly SymMask KMaskRn = new SymMask( 0x38, SymMask.TShiftDirection.ERight, 3 );
        private static readonly SymMask KMaskRd = new SymMask( 0x7 );
        #endregion
    }
}

