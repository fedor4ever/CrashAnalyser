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
using SymbianUtils.Range;
using SymbianStructuresLib.Arm;

namespace SymbianStructuresLib.Arm.Instructions
{
    public interface IArmInstructionProvider
    {
        uint GetDataUInt32( uint aAddress );

        ushort GetDataUInt16( uint aAddress );

        bool IsInstructionAddressValid( uint aAddress );

        bool GetInstructions( uint aAddress, TArmInstructionSet aInstructionSet, int aCount, out IArmInstruction[] aInstructions );
    }
}
