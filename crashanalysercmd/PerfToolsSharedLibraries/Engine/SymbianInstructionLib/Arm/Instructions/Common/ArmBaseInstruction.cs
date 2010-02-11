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
using SymbianStructuresLib.Arm.Registers;
using SymbianStructuresLib.Arm.Instructions;
using SymbianInstructionLib.Arm.Instructions.Arm;
using SymbianInstructionLib.Arm.Instructions.Thumb;

namespace SymbianInstructionLib.Arm.Instructions.Common
{
    public abstract class ArmBaseInstruction : IArmInstruction
    {
        #region Constructors
        protected ArmBaseInstruction( TArmInstructionSet aInstructionSet )
        {
            iInstructionSet = aInstructionSet;
        }
        #endregion

        #region API
        public virtual bool Matches( uint aOpCode )
        {
            uint masked = (uint) ( aOpCode & BitMask );
            if ( masked == BitValue )
            {
                return true;
            }

            return false;
        }

        protected void SetMask( string aMostSignificantByte, string aLeastSignificantByte )
        {
            SetMask( aMostSignificantByte + aLeastSignificantByte );
        }

        protected void SetMask( string aSignificantBitValues )
        {
            int bit = 0;
            uint mask = 0;
            uint value = 0;

            // Loop through all characters in the mask, starting from the RHS, working
            // towards the left hand side.
            int count = aSignificantBitValues.Length;
            for ( int charIndex = count - 1; charIndex >= 0; charIndex-- )
            {
                // Get a character from the string
                char c = aSignificantBitValues[ charIndex ];
                //
                if ( c == KBitIsSet )
                {
                    mask |= (uint) ( 1 << bit );
                    value |= (uint) ( 1 << bit );
                }
                else if ( c == KBitIsClear )
                {
                    mask |= (uint) ( 1 << bit );
                }
                else if ( c == KBitIsNotApplicable )
                {
                }
                //
                if ( c != ' ' )
                {
                    ++bit;
                }
            }
            //
            iBitMask = mask;
            iBitValue = value;
        }
        #endregion

        #region Properties
        protected uint BitMask
        {
            get { return iBitMask; }
        }

        protected uint BitValue
        {
            get { return iBitValue; }
        }

        internal virtual int SortOrder
        {
            get { return 0; }
        }
        #endregion

        #region From IArmInstruction
        public TArmInstructionSet AIType
        {
            get { return iInstructionSet; }
        }

        public TArmInstructionGroup AIGroup
        {
            get { return iGroup; }
            internal set
            {
                iGroup = value;
            }
        }

        public TArmInstructionTarget AITarget
        {
            get { return iTarget; }
            internal set { iTarget = value; }
        }

        public TArmInstructionCondition AIConditionCode
        {
            get { return iConditionCode; }
            internal set { iConditionCode = value; }
        }

        public SymUInt32 AIRawValue
        {
            get { return iRawValue; }
            internal set 
            {
                if ( iRawValue != value )
                {
                    iRawValue = value;
                    OnRawValueAssigned();
                }
            }
        }

        public string AIBinaryString
        {
            get
            {
                return iRawValue.Binary;
            }
        }

        public string AIHexString
        {
            get
            {
                uint size = this.AISize * 2;
                string format = "x" + size;
                string ret = this.AIRawValue.ToString( format );
                return ret;
            }
        }

        public string AIDisassembly
        {
            get { return iDisassembly; }
            internal set { iDisassembly = value; }
        }

        public uint AIAddress
        {
            get { return iAddress; }
            internal set { iAddress = value; }
        }

        public uint AISize
        {
            get
            { 
                TArmInstructionSet instSet = this.AIType;
                uint ret = (uint) instSet;
                return ret;
            }
        }

        public bool AIIsUnknown
        {
            get { return ( this is Arm_Unknown ) || ( this is Thumb_Unknown ); }
        }

        public SymBit this[ int aIndex ]
        {
            get
            {
                SymBit ret = SymBitUtils.GetBit( AIRawValue, aIndex );
                return ret;
            }
        }

        public bool QueryInvolvement( TArmRegisterType aRegister )
        {
            bool source = QueryInvolvementAsSource( aRegister );
            bool destination = QueryInvolvementAsDestination( aRegister );
            return ( source || destination );
        }

        public bool QueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            bool ret = DoQueryInvolvementAsSource( aRegister );
            return ret;
        }

        public bool QueryInvolvementAsDestination( TArmRegisterType aRegister )
        {
            bool ret = DoQueryInvolvementAsDestination( aRegister );
            return ret;
        }
        #endregion

        #region Framework API
        protected virtual void OnRawValueAssigned()
        {
        }

        protected virtual bool DoQueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            return false;
        }

        protected virtual bool DoQueryInvolvementAsDestination( TArmRegisterType aRegister )
        {
            return false;
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder t = new StringBuilder();
            //
            if ( this.AIRawValue == 0u )
            {
                t.Append( GetType().Name );
            }
            else
            {
                const int padAmount = 60;
                //
                string type = "A";
                if ( this.AIType == TArmInstructionSet.ETHUMB )
                {
                    type = "T";
                }
                //
                bool isRecognised = this.AIIsUnknown == false;
                t.AppendFormat( "[{0:x8}] [{1}] 0x{2:x8} {3} {4} ",
                    this.AIAddress,
                    type,
                    this.AIRawValue,
                    this.AIBinaryString,
                    isRecognised ? "Y" : "?" );
                //
                string disassembly = this.AIDisassembly;
                string typeName = isRecognised ? this.GetType().Name : string.Empty;
                //
                if ( !isRecognised )
                {
                    t.AppendFormat( "  {0} ", typeName.PadRight( padAmount, ' ' ) );
                    t.Append( disassembly );
                }
                else
                {
                    if ( disassembly.Contains( "r13" ) || disassembly.Contains( "SP" ) )
                    {
                        t.AppendFormat( "* {0} ", disassembly.PadRight( padAmount, ' ' ) );
                        t.Append( typeName );
                    }
                    else
                    {
                        t.AppendFormat( "  {0} ", disassembly.PadRight( padAmount, ' ' ) );
                        t.Append( typeName );
                    }
                }
            }
            //
            return t.ToString();
        }
        #endregion

        #region Internal constants
        private const char KBitIsSet = '1';
        private const char KBitIsClear = '0';
        private const char KBitIsNotApplicable = '#';
        #endregion

        #region Data members
        private readonly TArmInstructionSet iInstructionSet;
        private SymUInt32 iRawValue = new SymUInt32( 0 );
        private TArmInstructionGroup iGroup = TArmInstructionGroup.EGroupUndefined;
        private TArmInstructionTarget iTarget = TArmInstructionTarget.EDefault;
        private TArmInstructionCondition iConditionCode = TArmInstructionCondition.ENotApplicable;
        private uint iAddress = 0;
        private string iDisassembly = string.Empty;
        private uint iBitMask = 0;
        private uint iBitValue = 0;
        #endregion
    }
}

