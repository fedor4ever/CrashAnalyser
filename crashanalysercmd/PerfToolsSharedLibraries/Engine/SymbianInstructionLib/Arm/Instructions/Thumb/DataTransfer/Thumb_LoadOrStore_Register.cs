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

namespace SymbianInstructionLib.Arm.Instructions.Thumb.DataTransfer
{
    [ArmRef( "A7.1.29 LDR (2)", "LDR <Rd>, [<Rn>, <Rm>]" )]
    [ArmRef( "A7.1.59 STR (2)", "STR <Rd>, [<Rn>, <Rm>]" )]
    public class Thumb_LoadOrStore_Register : Thumb_LoadOrStore
    {
        #region Constructors
        public Thumb_LoadOrStore_Register()
        {
            //                    Type            Rm      Rn     Rd
            base.SetMask( "0101" + "#" + "00" + "###" + "###" + "###" );
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            bool ret = false;
            //
            TArmDataTransferType type = this.DataTransferType;
            if ( type == TArmDataTransferType.EStore )
            {
                // STR <Rd>, [<Rn>, <Rm>]
                // Rd = dest, Rn, Rm are source registers
                TArmRegisterType regN = this.Rn;
                TArmRegisterType regM = this.Rm;
                ret = ( aRegister == regN || aRegister == regM );
            }
            else if ( type == TArmDataTransferType.ELoad )
            {
                // LDR <Rd>, [<Rn>, <Rm>]
                // Rd = source, Rn, Rm are destination registers
                TArmRegisterType regD = this.Rd;
                ret = ( aRegister == regD );
            }
            //
            return ret;
        }

        protected override bool DoQueryInvolvementAsDestination( TArmRegisterType aRegister )
        {
            bool ret = false;
            //
            TArmDataTransferType type = this.DataTransferType;
            if ( type == TArmDataTransferType.EStore )
            {
                // STR <Rd>, [<Rn>, <Rm>]
                // Rd = dest, Rn, Rm are source registers
                TArmRegisterType regD = this.Rd;
                ret = ( aRegister == regD );
            }
            else if ( type == TArmDataTransferType.ELoad )
            {
                // LDR <Rd>, [<Rn>, <Rm>]
                // Rd = source, Rn, Rm are destination registers
                TArmRegisterType regN = this.Rn;
                TArmRegisterType regM = this.Rm;
                ret = ( aRegister == regN || aRegister == regM );
            }
            //
            return ret;
        }
        #endregion

        #region Framework API
        public override TArmDataTransferType DataTransferType
        {
            get
            {
                TArmDataTransferType ret = (TArmDataTransferType) base.AIRawValue[ 11 ];
                return ret;
            }
        }

        public override TArmRegisterType Rd
        {
            get
            {
                TArmRegisterType ret = (TArmRegisterType) Thumb_LoadOrStore.KBits2To0.Apply( base.AIRawValue );
                return ret;
            }
        }

        public TArmRegisterType Rn
        {
            get
            {
                TArmRegisterType ret = (TArmRegisterType) Thumb_LoadOrStore.KBits5To3.Apply( base.AIRawValue );
                return ret;
            }
        }

        public TArmRegisterType Rm
        {
            get
            {
                TArmRegisterType ret = (TArmRegisterType) KMaskRm.Apply( base.AIRawValue );
                return ret;
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private static readonly SymMask KMaskRm = new SymMask( 0x1C0, SymMask.TShiftDirection.ERight, 6 );
        #endregion
    }
}

