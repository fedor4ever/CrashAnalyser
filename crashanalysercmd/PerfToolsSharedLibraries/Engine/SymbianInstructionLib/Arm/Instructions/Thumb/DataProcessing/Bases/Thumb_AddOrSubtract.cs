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

namespace SymbianInstructionLib.Arm.Instructions.Thumb.DataProcessing
{
    public abstract class Thumb_AddOrSubtract : ThumbInstruction
    {
        #region Enumerations
        #endregion

        #region Constructors
        protected Thumb_AddOrSubtract()
        {
            base.AIGroup = SymbianStructuresLib.Arm.Instructions.TArmInstructionGroup.EGroupDataProcessing;
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsDestination( TArmRegisterType aRegister )
        {
            TArmRegisterType reg = this.Rd;
            return ( aRegister == reg );
        }
        #endregion

        #region Framework API
        public abstract TArmRegisterType Rd
        {
            get;
        }

        public virtual TArmDataProcessingType OperationType
        {
            get { return TArmDataProcessingType.EUndefined; }
        }

        public virtual uint Immediate
        {
            get { return 0; }
        }

        public virtual bool SuppliesImmediate
        {
            get { return false; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}

