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
using System.Text.RegularExpressions;
using System.IO;

namespace SymbianStructuresLib.Arm.Instructions
{
    // NB: Do not reorder these
    public enum TArmDataProcessingType
    {
        EUndefined = -1,
        //
        AND = 0,    // Logical AND
        EOR,        // Logical EOR
        SUB,        // Subtract
        RSB,        // Reverse Subtract
        ADD,        // Add
        ADC,        // Add with Carry
        SBC,        // Subtract with Carry
        RSC,        // Reverse Subtract with Carry
        TST,        // Test
        TEQ,        // Test Equivalence
        CMP,        // Compare
        CMN,        // Compare Negative
        ORR,        // Logical OR
        MOV,        // Move
        BIC,        // Logical Bit Clear
        MVN,        // Move Not
    }
}
