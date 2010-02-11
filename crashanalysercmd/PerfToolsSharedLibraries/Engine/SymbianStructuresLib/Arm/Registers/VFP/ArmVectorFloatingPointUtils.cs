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

namespace SymbianStructuresLib.Arm.Registers.VFP
{
    public static class ArmVectorFloatingPointUtils
    {
        public static int RegisterSizeInBits( TArmRegisterTypeVFP aRegister )
        {
            int ret = 32;
            int value = (int) aRegister;
            //
            if ( value >= 0 && value < (int) TArmRegisterTypeVFP.S00 )
            {
                ret *= 2;
            }
            //
            return ret;
        }
    }
}
