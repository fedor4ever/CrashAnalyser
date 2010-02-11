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
using System.ComponentModel;

namespace SymbianStructuresLib.Arm.Instructions
{
    public enum TArmInstructionCondition
    {
        [Description( "" )]
        ENotApplicable = -1,

        [Description( "EQ" )]
        EQ = 0,        // 0000 EQ Equal Z set

        [Description( "NE" )]
        NE = 1,        // 0001 NE Not equal Z clear

        [Description( "CS" )]
        CS = 2,        // 0010 CS/HS Carry set/unsigned higher or same C set

        [Description( "CC" )]
        CC = 3,        // 0011 CC/LO Carry clear/unsigned lower C clear0011 CC/LO Carry clear/unsigned lower C clear

        [Description( "MI" )]
        MI = 4,        // 0100 MI Minus/negative N set

        [Description( "PL" )]
        PL = 5,        // 0101 PL Plus/positive or zero N clear

        [Description( "VS" )]
        VS = 6,        // 0110 VS Overflow V set

        [Description( "VC" )]
        VC = 7,        // 0111 VC No overflow V clear

        [Description( "HI" )]
        HI = 8,        // 1000 HI Unsigned higher C set and Z clear

        [Description( "LS" )]
        LS = 9,        // 1001 LS Unsigned lower or same C clear or Z set

        [Description( "GE" )]
        GE = 10,        // 1010 GE Signed greater than or equal. N set and V set, or N clear and V clear (N == V)

        [Description( "LT" )]
        LT = 11,        // 1011 LT Signed less than. N set and V clear, or N clear and V set (N != V)

        [Description( "GT" )]
        GT = 12,        // 1100 GT Signed greater than. Z clear, and either N set and V set, or N clear and V clear (Z == 0,N == V)

        [Description( "LE" )]
        LE = 13,        // 1101 LE Signed less than or equal. Z set, or N set and V clear, or N clear and V set (Z == 1 or N != V)

        [Description( "" )]
        AL = 14,        // 1110 AL Always (unconditional) -

        [Description( "" )]
        EX = 15,        // 1111 EX - Extension - See Condition code 0b1111
    }
}
