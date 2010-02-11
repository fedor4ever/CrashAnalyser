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
using SymbianStructuresLib.Arm;
using SymbianStackAlgorithmAccurate.Instructions;

namespace SymbianStackAlgorithmAccurate.CPU
{
    internal static class ArmCpuUtils
    {
        #region API
        public static uint InstructionSize( TArmInstructionSet aSet )
        {
            uint ret = 2;
            //
            if ( aSet == TArmInstructionSet.EARM )
            {
                ret = 4;
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        #endregion
    }
}
