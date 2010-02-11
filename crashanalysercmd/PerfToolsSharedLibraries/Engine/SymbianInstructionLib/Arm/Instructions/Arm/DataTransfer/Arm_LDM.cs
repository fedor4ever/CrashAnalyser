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
    [ArmRefAttribute( "A4.1.20 LDM(1) + A5.4.1 Encoding", "LDM{<cond>}<addressing_mode> <Rn>{!}, <registers>" )]
    public class Arm_LDM : Arm_LoadOrStoreMultiple_GP
    {
        #region Enumerations
        public enum TType
        {
            ETypeNotSupported = -1,
            ETypeNonEmptyGP = 0,
            ETypeUserModeWhenInPrivilaged,
            ETypeGPAndPCAndUpdateCPSR
        }
        #endregion

        #region Constructors
        public Arm_LDM()
        {
            //  4    3                  4               16 bits
            // ---------------------------------------------------
            // cond 100 P U S W L       Rn          register list
            // 1110 100 0 1 0 1 1     1101         1000111111110000
            //
            // P  = indicates that the word addressed by Rn is included in the range of memory
            //      locations accessed, lying at the top (U==0) or bottom (U==1) of that range.
            //
            // U  = Indicates that the transfer is made upwards (U==1) or downwards (U==0) from 
            //      the base register.
            //
            // S  = For LDMs that load the PC, the S bit indicates that the CPSR is loaded 
            //      from the SPSR.
            //
            // W  = Indicates that the base register is updated after the transfer. The base 
            //      register is incremented (U==1) or decremented (U==0) by four times the 
            //      number of registers in the register list.
            //
            // L  = Distinguishes between Load (L==1) and Store (L==0) instructions.
            //
            // Rn = Specifies the base register used by <addressing_mode>. Using R15 as 
            //      the base register <Rn> gives an UNPREDICTABLE result.
            // 
            // This mask translates to "LDM's that load PC from SP"
            //        1110 100 01011 1101    1000111111110000
            base.SetMask( "####" + "100" + "##" + "##1" + "####" + "######## ########" );
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

        #region From Arm_LoadOrStore
        public override TArmDataTransferType DataTransferType
        {
            get { return TArmDataTransferType.ELoad; }
        }
        #endregion

        #region Properties
        public TType Type
        {
            get { return iType; }
        }
        #endregion

        #region From ArmBaseInstruction
        protected override void OnRawValueAssigned()
        {
            base.OnRawValueAssigned();
            iType = InternalType;
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
            if ( KLDM1.IsMatch( aRaw ) )
            {
                ret = TType.ETypeNonEmptyGP;
            }
            else if ( KLDM2.IsMatch( aRaw ) )
            {
                ret = TType.ETypeUserModeWhenInPrivilaged;
            }
            else if ( KLDM3.IsMatch( aRaw ) )
            {
                ret = TType.ETypeGPAndPCAndUpdateCPSR;
            }
            //
            return ret;
        }
        #endregion

        #region Data members
        private TType iType = TType.ETypeNotSupported;
        private static readonly SymMask KLDM1 = new SymMask( "####" + "100" + "##" + "0#1" + "####" + "######## ########" );
        private static readonly SymMask KLDM2 = new SymMask( "####" + "100" + "##" + "101" + "####" + "0####### ########" );
        private static readonly SymMask KLDM3 = new SymMask( "####" + "100" + "##" + "1#1" + "####" + "1####### ########" );
        #endregion
    }
}

