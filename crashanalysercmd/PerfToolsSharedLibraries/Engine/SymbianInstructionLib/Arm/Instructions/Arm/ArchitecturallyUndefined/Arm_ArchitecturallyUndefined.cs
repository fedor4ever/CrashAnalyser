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

namespace SymbianInstructionLib.Arm.Instructions.Arm.ArchitecturallyUndefined
{
    public class Arm_ArchitecturallyUndefined : ArmInstruction
    {
        #region Constructors
        public Arm_ArchitecturallyUndefined()
        {
            base.SetMask( "####" + "011" + "11111" + "############" + "1111" + "####" );
            base.AIGroup = SymbianStructuresLib.Arm.Instructions.TArmInstructionGroup.EGroupUndefined;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        #endregion

        #region From System.Object
        #endregion

        #region From IArmInstruction
        #endregion

        #region Data members
        #endregion
    }
}

