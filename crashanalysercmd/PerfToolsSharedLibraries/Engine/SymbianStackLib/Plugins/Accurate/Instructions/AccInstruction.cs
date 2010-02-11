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

namespace SymbianStackAlgorithmAccurate.Instructions
{
    internal abstract class AccInstruction
    {
        #region Constructors
        protected AccInstruction( IArmInstruction aInstruction )
        {
            iInstruction = aInstruction;
        }
        #endregion

        #region API
        internal abstract void Process( ArmPrologueHelper aProlog );

        internal virtual void Prefilter( AccInstructionList aInstructions, int aMyIndex, int aInstructionCountOffsetToPC )
        {
        }
        #endregion

        #region Properties
        public IArmInstruction Instruction
        {
            get { return iInstruction; }
        }

        public bool Ignored
        {
            get { return iIgnored; }
            internal set { iIgnored = value; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return iInstruction.ToString();
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly IArmInstruction iInstruction;
        private bool iIgnored = false;
        #endregion
    }
}

