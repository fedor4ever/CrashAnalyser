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
    [ArmRef( "A7.1.4 ADD (2)",  "ADD <Rd>, #<immed_8>" )]
    [ArmRef( "A7.1.66 SUB (2)", "SUB <Rd>, #<immed_8>" )]
    public class Thumb_AddOrSubtract_Large : Thumb_AddOrSubtract
    {
        #region Constructors
        public Thumb_AddOrSubtract_Large()
        {
            //                     Type    Rd     immed_8
            base.SetMask( "0011" + "#" + "###" + "########" );
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            // Rd acts as source and destination
            TArmRegisterType reg = this.Rd;
            return ( aRegister == reg );
        }
        #endregion

        #region From Thumb_AddOrSubtract
        public override TArmDataProcessingType OperationType
        {
            get
            {
                TArmDataProcessingType ret = TArmDataProcessingType.ADD;
                //
                if ( base.AIRawValue[ 11 ] == SymBit.ESet )
                {
                    ret = TArmDataProcessingType.SUB;
                }
                //
                return ret;
            }
        }

        public override TArmRegisterType Rd
        {
            get
            {
                TArmRegisterType ret = (TArmRegisterType) KMaskRd.Apply( base.AIRawValue );
                return ret;
            }
        }

        public override uint Immediate
        {
            get
            {
                uint ret = ( base.AIRawValue & 0xFF );
                return ret;
            }
        }

        public override bool SuppliesImmediate
        {
            get { return true; }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private static readonly SymMask KMaskRd = new SymMask( 0x700, SymMask.TShiftDirection.ERight, 8 );
        #endregion
    }
}

