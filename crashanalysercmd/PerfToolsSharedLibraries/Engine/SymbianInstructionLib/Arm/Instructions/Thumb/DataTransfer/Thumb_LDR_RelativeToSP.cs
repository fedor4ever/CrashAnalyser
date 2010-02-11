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

namespace SymbianInstructionLib.Arm.Instructions.Thumb.DataTransfer
{
    [ArmRef( "A7.1.31 LDR (4)", "LDR <Rd>, [SP, #<immed_8> * 4]" )]
    public class Thumb_LDR_RelativeToSP : Thumb_LoadOrStore_Immediate8
    {
        #region Constructors
        public Thumb_LDR_RelativeToSP()
        {
            //                       Rd       immed_8
            base.SetMask( "10011" + "###" + "########" );
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            // SP acts as source
            return ( aRegister == TArmRegisterType.EArmReg_SP );
        }

        protected override bool DoQueryInvolvementAsDestination( TArmRegisterType aRegister )
        {
            // Rd acts as destination
            TArmRegisterType reg = this.Rd;
            return ( aRegister == reg );
        }        
        #endregion

        #region From Thumb_LoadOrStore
        public override TArmDataTransferType DataTransferType
        {
            get { return TArmDataTransferType.ELoad; }
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

