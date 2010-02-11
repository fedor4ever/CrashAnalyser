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

namespace SymbianInstructionLib.Arm.Instructions.Arm.SWI
{
    public class Arm_SWI : ArmInstruction
    {
        #region Constructors
        public Arm_SWI()
        {
            base.SetMask( "####" + "1111" + "########################" );
            base.AIGroup = SymbianStructuresLib.Arm.Instructions.TArmInstructionGroup.EGroupExceptionGenerating;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public int OpCode
        {
            get
            {
                int val = base.AIRawValue & 0xFFFFFF;
                return val;
            }
        }
        #endregion

        #region From System.Object
        #endregion

        #region From IArmInstruction
        #endregion

        #region Data members
        #endregion
    }
}

