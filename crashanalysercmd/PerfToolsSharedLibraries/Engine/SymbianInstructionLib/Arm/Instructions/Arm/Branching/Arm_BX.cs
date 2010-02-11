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
using SymbianInstructionLib.Arm.Instructions.Common;

namespace SymbianInstructionLib.Arm.Instructions.Arm.Branching
{
    [ArmRefAttribute( "A4.1.10 BX", "BX{<cond>} <Rm>" )]
    public class Arm_BX : Arm_Branch_Register
    {
        #region Constructors
        public Arm_BX()
        {
            //        cond           SBO  SBO  SBO       Rm
            //        1110 00010010 1111 1111 1111 0001 0001
            base.SetMask( "####" + "00010010" + "1111" + "1111" + "1111" + "0001" + "####" );
        }
        #endregion

        #region API
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}

