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
    [ArmRef( "A7.1.28 LDR (1)", "LDR <Rd>, [<Rn>, #<immed_5> * 4]" )]
    [ArmRef( "A7.1.58 STR (1)", "STR <Rd>, [<Rn>, #<immed_5> * 4]" )]
    public class Thumb_LoadOrStore_Immediate5 : Thumb_LoadOrStore
    {
        #region Constructors
        public Thumb_LoadOrStore_Immediate5()
        {
            //                    Type   immed_5     Rn     Rd
            base.SetMask( "0110" + "#" + "#####" + "###" + "###" );
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
                // STR <Rd>, [<Rn>, #<immed_5> * 4]
                // Rd = dest, Rn is source register
                TArmRegisterType regN = this.Rn;
                ret = ( aRegister == regN );
            }
            else if ( type == TArmDataTransferType.ELoad )
            {
                // LDR <Rd>, [<Rn>, #<immed_5> * 4]
                // Rd = source, Rn is destination register
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
                // STR <Rd>, [<Rn>, #<immed_5> * 4]
                // Rd = dest, Rn is source register
                TArmRegisterType regD = this.Rd;
                ret = ( aRegister == regD );
            }
            else if ( type == TArmDataTransferType.ELoad )
            {
                // LDR <Rd>, [<Rn>, #<immed_5> * 4]
                // Rd = source, Rn is destination register
                TArmRegisterType regN = this.Rn;
                ret = ( aRegister == regN );
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
        #endregion

        #region Properties
        public uint Immediate
        {
            get
            {
                uint ret = KMaskImmediate5.Apply( base.AIRawValue );
                ret *= 4;
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private static readonly SymMask KMaskImmediate5 = new SymMask( 0x7C0, SymMask.TShiftDirection.ERight, 6 );
        #endregion
    }
}

