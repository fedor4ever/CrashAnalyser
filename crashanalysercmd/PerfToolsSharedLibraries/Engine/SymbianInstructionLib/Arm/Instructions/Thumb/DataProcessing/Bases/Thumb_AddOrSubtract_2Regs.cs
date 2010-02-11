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
    public abstract class Thumb_AddOrSubtract_2Regs : Thumb_AddOrSubtract
    {
        #region Constructors
        protected Thumb_AddOrSubtract_2Regs()
        {
        }
        #endregion

        #region From Thumb_AddOrSubtract
        public override TArmDataProcessingType OperationType
        {
            get
            {
                TArmDataProcessingType ret = TArmDataProcessingType.ADD;
                //
                if ( base.AIRawValue[ 9 ] == SymBit.ESet )
                {
                    ret = TArmDataProcessingType.SUB;
                }
                //
                return ret;
            }
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            TArmRegisterType reg = this.Rn;
            return ( aRegister == reg );
        }
        #endregion

        #region Properties
        public TArmRegisterType Rn
        {
            get
            {
                TArmRegisterType ret = (TArmRegisterType) RnByte;
                return ret;
            }
        }

        public override TArmRegisterType Rd
        {
            get
            {
                TArmRegisterType ret = (TArmRegisterType) RdByte;
                return ret;
            }
        }
        #endregion

        #region Internal methods
        protected byte RnByte
        {
            get
            {
                byte ret = (byte) KMaskRn.Apply( base.AIRawValue );
                return ret;
            }
        }

        protected byte RdByte
        {
            get
            {
                byte ret = (byte) KMaskRd.Apply( base.AIRawValue );
                return ret;
            }
        }
        #endregion

        #region Data members
        private static readonly SymMask KMaskRn = new SymMask( 0x38, SymMask.TShiftDirection.ERight, 3 );
        private static readonly SymMask KMaskRd = new SymMask( 0x7 );
        #endregion
    }
}

