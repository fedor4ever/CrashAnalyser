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
using SymbianStructuresLib.Arm.Instructions;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Common.Id;
using SymbianStructuresLib.Debug.Common.FileName;
using SymbianStructuresLib.Debug.Common.Interfaces;
using SymbianStructuresLib.Debug.Code.Interfaces;

namespace SymbianStructuresLib.Debug.Code.Interfaces
{
    public interface ICodeCollectionInstructionConverter
    {
        IArmInstruction[] ConvertRawValuesToInstructions( TArmInstructionSet aInstructionSet, uint[] aRawValues, uint aStartingAddress );
    }
}
