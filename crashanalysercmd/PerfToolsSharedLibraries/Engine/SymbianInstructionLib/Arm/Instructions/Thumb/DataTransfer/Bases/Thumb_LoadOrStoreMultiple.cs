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
    public abstract class Thumb_LoadOrStoreMultiple : Thumb_LoadOrStore
    {
        #region Constructors
        protected Thumb_LoadOrStoreMultiple()
        {
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            TArmDataTransferType type = this.DataTransferType;
            //
            bool ret = base.DoQueryInvolvementAsSource( aRegister );
            if ( ret == false && type == TArmDataTransferType.EStore )
            {
                // PUSH {r4, r5, r14}
                // r13 = dest, r4, r5, r14 are source registers
                List<TArmRegisterType> regs = this.RegistersAsList;
                ret = regs.Contains( aRegister );
            }
            //
            return ret;
        }

        protected override bool DoQueryInvolvementAsDestination( TArmRegisterType aRegister )
        {
            TArmDataTransferType type = this.DataTransferType;
            //
            bool ret = base.DoQueryInvolvementAsDestination( aRegister );
            if ( ret == false && type == TArmDataTransferType.ELoad )
            {
                // POP {r4, r5, r15}
                // r13 = source, r4, r5, r14 are destination registers
                List<TArmRegisterType> regs = this.RegistersAsList;
                ret = regs.Contains( aRegister );
            }
            //
            return ret;
        }
        #endregion

        #region Framework API
        protected abstract List<TArmRegisterType> RegistersAsList
        {
            get;
        }
        #endregion

        #region Properties
        public TArmRegisterType[] Registers
        {
            get { return RegistersAsList.ToArray(); }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}

