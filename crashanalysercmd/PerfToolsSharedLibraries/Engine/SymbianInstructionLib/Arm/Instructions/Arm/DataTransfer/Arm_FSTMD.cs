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
using SymbianStructuresLib.Arm.Registers.VFP;
using SymbianStructuresLib.Arm.Instructions;
using SymbianInstructionLib.Arm.Instructions.Common;

namespace SymbianInstructionLib.Arm.Instructions.Arm.DataTransfer
{
    [ArmRefAttribute( "C4.1.56 FSTMD", "FSTM<addressing_mode>D{<cond>} <Rn>{!}, <registers>" )]
    public class Arm_FSTMD : Arm_LoadOrStoreMultiple_VFP
    {
        #region Constructors
        public Arm_FSTMD()
        {
            // Same as FSTMDBD (non-stacking)
            //
            // 1110 => Condition = "Always"
            //  110 => FSTMD (1) instruction signature
            //    1 => P = addressing mode
            //    0 => U = addressing mode
            //    0 => ?
            //    1 => W = write a modified value back to its base register Rn
            //    0 => ?
            // 1101 => Rn = Specifies the base register used by <addressing_mode>. => SP
            // 
            //             Cond             PU     SWL      Rn       Dd                offset
            base.SetMask( "####" + "110" + "##" + "0#0" + "####" + "####" + "1011" + "########" );
        }
        #endregion

        #region From Arm_LoadOrStore
        public override TArmDataTransferType DataTransferType
        {
            get { return TArmDataTransferType.EStore; }
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public override TArmRegisterTypeVFP FirstRegister
        {
            get
            {
                uint firstReg = Arm_LoadOrStoreMultiple_VFP.KMaskFirstReg.Apply( base.AIRawValue );
                TArmRegisterTypeVFP ret = (TArmRegisterTypeVFP) firstReg;
                return ret;
            }
        }

        public override TArmRegisterTypeVFP[] Registers
        {
            get
            {
                uint offset = base.Offset;
                uint firstReg = (uint) this.FirstRegister;
                uint regCount = (uint) ( offset / 2 );

                List<TArmRegisterTypeVFP> regs = new List<TArmRegisterTypeVFP>();
                for ( uint i = firstReg; i < regCount + firstReg; i++ )
                {
                    regs.Add( (TArmRegisterTypeVFP) i );
                }
                //
                return regs.ToArray();
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}

