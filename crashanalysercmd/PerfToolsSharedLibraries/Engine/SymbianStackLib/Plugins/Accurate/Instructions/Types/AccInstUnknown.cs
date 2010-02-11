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
using SymbianUtils.BasicTypes;
using SymbianStackAlgorithmAccurate.CPU;
using SymbianStackAlgorithmAccurate.Prologue;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianStructuresLib.Arm.Registers;
using SymbianInstructionLib.Arm.Instructions.Common;
using SymbianInstructionLib.Arm.Instructions.Arm;
using SymbianInstructionLib.Arm.Instructions.Arm.DataProcessing;
using SymbianInstructionLib.Arm.Instructions.Thumb;
using SymbianInstructionLib.Arm.Instructions.Thumb.DataProcessing;

namespace SymbianStackAlgorithmAccurate.Instructions.Types
{
    internal class AccInstUnknown : AccInstruction
    {
        #region Constructors
        public AccInstUnknown( IArmInstruction aInstruction )
            : base( aInstruction )
        {
        }
        #endregion

        #region API
        internal override void Process( ArmPrologueHelper aProlog )
        {
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}

