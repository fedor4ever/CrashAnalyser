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
using System.IO;
using SymbianStructuresLib.Arm;
using SymbianInstructionLib.Arm.Instructions.Common;

namespace SymbianInstructionLib.Arm.Instructions.Thumb
{
    public abstract class ThumbInstruction : ArmBaseInstruction
    {
        #region Constructors
        protected ThumbInstruction()
            : base( TArmInstructionSet.ETHUMB )
        {
            // Very few thumb instructions are conditional
            base.AIConditionCode = SymbianStructuresLib.Arm.Instructions.TArmInstructionCondition.AL;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        #endregion
    }
}

