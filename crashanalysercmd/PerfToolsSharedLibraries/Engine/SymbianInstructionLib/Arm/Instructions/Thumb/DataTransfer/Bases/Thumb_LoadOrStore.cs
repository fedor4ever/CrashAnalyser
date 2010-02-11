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
    public abstract class Thumb_LoadOrStore : ThumbInstruction
    {
        #region Constructors
        protected Thumb_LoadOrStore()
        {
            base.AIGroup = SymbianStructuresLib.Arm.Instructions.TArmInstructionGroup.EGroupDataTransfer;
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            bool ret = false;
            //
            TArmDataTransferType type = this.DataTransferType;
            if ( type == TArmDataTransferType.EStore )
            {
                // STR <Rd>, [<Rn>, #<immed_5> * 4]
                // Rd = dest, Rn is source
                //
                // Cannot test here, requires derived class to implement this check
            }
            else if ( type == TArmDataTransferType.ELoad )
            {
                // LDR <Rd>, [<Rn>, #<immed_5> * 4]
                // LDR <Rd>, [<Rn>, <Rm>]
                // LDR <Rd>, [PC, #<immed_8> * 4]
                // LDR <Rd>, [SP, #<immed_8> * 4]
                TArmRegisterType reg = this.Rd;
                ret = ( reg == aRegister );
            }
            //
            return ret;
        }

        protected override bool DoQueryInvolvementAsDestination( TArmRegisterType aRegister )
        {
            bool ret = false;
            //
            TArmDataTransferType type = this.DataTransferType;
            if ( type == TArmDataTransferType.EStore )
            {
                // STR <Rd>, [<Rn>, #<immed_5> * 4]
                // Rd = dest, Rn is source
                TArmRegisterType reg = this.Rd;
                ret = ( reg == aRegister );
            }
            else if ( type == TArmDataTransferType.ELoad )
            {
                // LDR <Rd>, [<Rn>, #<immed_5> * 4]
                // LDR <Rd>, [<Rn>, <Rm>]
                // LDR <Rd>, [PC, #<immed_8> * 4]
                // LDR <Rd>, [SP, #<immed_8> * 4]
                //
                // Cannot test here, requires derived class to implement this check
            }
            //
            return ret;
        }
        #endregion

        #region Framework API
        public abstract TArmDataTransferType DataTransferType
        {
            get;
        }

        public abstract TArmRegisterType Rd
        {
            get;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        protected static readonly SymMask KBits2To0 = new SymMask( 0x7 );
        protected static readonly SymMask KBits5To3 = new SymMask( 0x38 );
        #endregion

        #region Data members
        #endregion
    }
}

