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
using SymbianStructuresLib.Arm.Registers;
using SymbianStructuresLib.Arm.Instructions;
using SymbianInstructionLib.Arm.Instructions.Common;

namespace SymbianInstructionLib.Arm.Instructions.Arm.DataTransfer
{
    [ArmRefAttribute( "A4.1.98 STM (1)", "STM{<cond>}<addressing_mode> <Rn>{!}, <registers>" )]
    public class Arm_STM : Arm_LoadOrStoreMultiple_GP
    {
        #region Enumerations
        public enum TType
        {
            ETypeNotSupported = -1,
            ETypeNonEmptyGP = 0,
            ETypeNonEmptyUserMode
        }
        #endregion

        #region Constructors
        public Arm_STM()
        {
            // STM (1) (Store Multiple) stores a non-empty subset (or possibly all) of the general-purpose registers to
            // sequential memory locations.
            //
            //  4    3                  4               16 bits
            // ---------------------------------------------------
            // cond 100 P U S W L       Rn          register list
            // 1110 100 1 0 0 1 0      1101       0100000000010000
            //
            // 1110 => Condition = "Always"
            //  100 => STM (1) instruction signature
            //    1 => P = addressing mode
            //    0 => U = addressing mode
            //    0 => S = indicates that when the processor is in a privileged mode
            //    1 => W = write a modified value back to its base register Rn
            //    0 => L = distinguishes between a Load (L==1) and a Store (L==0) instruction.
            // 1101 => Rn = Specifies the base register used by <addressing_mode>. => SP
            // 
            base.SetMask( "####" + "100" + "##" + "##0" + "####" + "######## ########" );
        }
        #endregion

        #region From Arm_LoadOrStore
        public override TArmDataTransferType DataTransferType
        {
            get { return TArmDataTransferType.EStore; }
        }
        #endregion

        #region From ArmInstruction
        public override bool Matches( uint aOpCode )
        {
            bool match = base.Matches( aOpCode );
            if ( match )
            {
                TType type = StaticInternalType( aOpCode );
                match = ( type != TType.ETypeNotSupported );
            }
            return match;
        }
        #endregion

        #region Properties
        public TType Type
        {
            get { return iType; }
        }
        #endregion

        #region Internal methods
        private new SymBit SBit // Hide
        {
            get { return SymBit.EClear; }
        }

        private TType InternalType
        {
            get
            {
                uint raw = base.AIRawValue;
                TType ret = StaticInternalType( raw );
                return ret;
            }
        }

        private static TType StaticInternalType( uint aRaw )
        {
            TType ret = TType.ETypeNotSupported;
            //
            if ( KSTM1.IsMatch( aRaw ) )
            {
                ret = TType.ETypeNonEmptyGP;
            }
            else if ( KSTM2.IsMatch( aRaw ) )
            {
                ret = TType.ETypeNonEmptyUserMode;
            }
            //
            return ret;
        }
        #endregion

        #region Data members
        private TType iType = TType.ETypeNotSupported;
        private static readonly SymMask KSTM1 = new SymMask( "####" + "100" + "##" + "0#0" + "####" + "######## ########" );
        private static readonly SymMask KSTM2 = new SymMask( "####" + "100" + "##" + "101" + "####" + "0####### ########" );
        #endregion
    }
}

