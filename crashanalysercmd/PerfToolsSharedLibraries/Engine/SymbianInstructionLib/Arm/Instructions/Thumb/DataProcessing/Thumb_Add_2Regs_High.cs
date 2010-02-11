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
    [ArmRef( "A7.1.6 ADD (4)", "ADD <Rd>, <Rm>" )]
    public class Thumb_Add_2Regs_High : Thumb_AddOrSubtract_2Regs
    {
        #region Constructors
        public Thumb_Add_2Regs_High()
        {
            //                          H1    H2    Rm      Rd
            base.SetMask( "01000100" + "#" + "#" + "###" + "###" );
        }
        #endregion

        #region From Thumb_AddOrSubtract
        public override TArmRegisterType Rd
        {
            get
            {
                byte reg = base.RdByte;
                byte h1 = (byte) base.AIRawValue[ 7 ];
                h1 <<= 3;
                reg |= h1;
                return (TArmRegisterType) reg;
            }
        }

        public override TArmDataProcessingType OperationType
        {
            get { return TArmDataProcessingType.ADD; }
        }
        #endregion

        #region Properties
        public TArmRegisterType Rm
        {
            get
            {
                byte reg = base.RdByte;
                byte h2 = (byte) base.AIRawValue[ 6 ];
                h2 <<= 3;
                reg |= h2;
                return (TArmRegisterType) reg;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}

