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

namespace SymbianInstructionLib.Arm.Instructions.Arm.DataTransfer
{
    public abstract class Arm_LoadOrStore : ArmInstruction
    {
        #region Constructors
        protected Arm_LoadOrStore()
        {
            base.AIGroup = SymbianStructuresLib.Arm.Instructions.TArmInstructionGroup.EGroupDataTransfer;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public abstract TArmDataTransferType DataTransferType
        {
            get;
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}

