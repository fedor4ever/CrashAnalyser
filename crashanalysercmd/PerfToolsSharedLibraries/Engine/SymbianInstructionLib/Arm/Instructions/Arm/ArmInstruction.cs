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
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianInstructionLib.Arm.Instructions.Common;

namespace SymbianInstructionLib.Arm.Instructions.Arm
{
    public abstract class ArmInstruction : ArmBaseInstruction
    {
        #region Constructors
        protected ArmInstruction()
            : base( TArmInstructionSet.EARM )
        {
        }
        #endregion

        #region API
        public SymBit IBit
        {
            get { return this[ ArmInstruction.KBitIndexI ]; }
        }

        public SymBit PBit
        {
            get { return this[ ArmInstruction.KBitIndexP ]; }
        }

        public SymBit UBit
        {
            get { return this[ ArmInstruction.KBitIndexU ]; }
        }

        public SymBit SBit
        {
            get { return this[ ArmInstruction.KBitIndexS ]; }
        }

        public SymBit WBit
        {
            get { return this[ ArmInstruction.KBitIndexW ]; }
        }

        public SymBit LBit
        {
            get { return this[ ArmInstruction.KBitIndexL ]; }
        }
        #endregion

        #region From ArmBaseInstruction
        protected override void OnRawValueAssigned()
        {
            base.OnRawValueAssigned();
            base.AIConditionCode = (TArmInstructionCondition) KMask_Condition.Apply( base.AIRawValue );
        }
        #endregion

        #region Constants
        // Type-specific
        public const int KBitIndexI = 25;

        // Common
        public const int KBitIndexP = 24;
        public const int KBitIndexU = 23;
        public const int KBitIndexS = 22;
        public const int KBitIndexW = 21;
        public const int KBitIndexL = 20;

        // VFP specific
        public const int KVFPBitIndexD = 22;
        #endregion

        #region Internal constants
        private static readonly SymMask KMask_Condition = new SymMask( 15u << 28, SymMask.TShiftDirection.ERight, 28 );
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        #endregion
    }
}

