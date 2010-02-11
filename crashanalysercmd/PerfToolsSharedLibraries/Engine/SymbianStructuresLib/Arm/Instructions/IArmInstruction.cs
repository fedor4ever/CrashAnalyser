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
using SymbianUtils.BasicTypes;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib;
using SymbianStructuresLib.Arm.Registers;

namespace SymbianStructuresLib.Arm.Instructions
{
    public interface IArmInstruction
    {
        #region Properties

        /// <summary>
        /// The instruction set encoding for the
        /// instruction
        /// </summary>
        TArmInstructionSet AIType
        {
            get;
        }

        /// <summary>
        /// The instruction group/categorisation
        /// </summary>
        TArmInstructionGroup AIGroup
        {
            get;
        }

        /// <summary>
        /// The arm entity which the instruction operates upon
        /// </summary>
        TArmInstructionTarget AITarget
        {
            get;
        }

        /// <summary>
        /// The condition codes associated with the instruction
        /// </summary>
        TArmInstructionCondition AIConditionCode
        {
            get;
        }

        /// <summary>
        /// The raw instruction as a 32bit value
        /// </summary>
        SymUInt32 AIRawValue
        {
            get;
        }

        /// <summary>
        /// The raw instruction in binary notation, encoded as a string.
        /// No leading prefix.
        /// </summary>
        string AIBinaryString
        {
            get;
        }

        /// <summary>
        /// The raw instruction in hexadecimal notation, encoded as a string.
        /// No leading prefix.
        /// </summary>
        string AIHexString
        {
            get;
        }

        /// <summary>
        /// An optional disassembled interpretation of the instruction.
        /// </summary>
        string AIDisassembly
        {
            get;
        }

        /// <summary>
        /// The address from which the instruction was read.
        /// </summary>
        uint AIAddress
        {
            get;
        }

        /// <summary>
        /// The size, in bytes, of the instruction. E.g. ARM=4, THUMB=2 etc.
        /// </summary>
        uint AISize
        {
            get;
        }

        /// <summary>
        /// If the instruction was not recognised, then this returns true
        /// </summary>
        bool AIIsUnknown
        {
            get;
        }
        
        /// <summary>
        /// Get a bit by index
        /// </summary>
        SymBit this[ int aIndex ]
        {
            get;
        }

        #endregion

        #region API
        bool QueryInvolvement( TArmRegisterType aRegister );

        bool QueryInvolvementAsSource( TArmRegisterType aRegister );

        bool QueryInvolvementAsDestination( TArmRegisterType aRegister );
        #endregion
    }
}
