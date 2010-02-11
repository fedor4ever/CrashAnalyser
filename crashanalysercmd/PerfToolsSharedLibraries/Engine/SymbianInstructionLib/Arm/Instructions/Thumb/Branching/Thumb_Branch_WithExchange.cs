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
    public class Thumb_Branch_WithExchange : Thumb_Branch
    {
        #region Enumerations
        public enum TType
        {
            ETypeBX = 0,
            ETypeBLX
        }
        #endregion

        #region Constructors
        public Thumb_Branch_WithExchange()
        {
            //                          L    H2      Rm     SBZ
            base.SetMask( "01000111" + "#" + "#" + "###" + "000" );
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            TArmRegisterType branchReg = this.Register;
            return ( aRegister == branchReg );
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public TType Type
        {
            get
            {
                TType ret = (TType) base.AIRawValue[ 7 ];
                return ret;
            }
        }

        public TArmRegisterType Register
        {
            get
            {
                // 10001111 0 011 000    0x4798              BLX      r3
                // 10001110 1 110 000    0x4770              BX       lr
                // 
                TArmRegisterType ret = (TArmRegisterType) base.AIRawValue[ 6, 3 ].ToUInt();
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

