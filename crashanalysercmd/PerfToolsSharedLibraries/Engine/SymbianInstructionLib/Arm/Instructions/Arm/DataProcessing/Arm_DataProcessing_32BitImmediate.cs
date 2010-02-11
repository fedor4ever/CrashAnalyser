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
using System.ComponentModel;
using SymbianUtils.BasicTypes;
using SymbianStructuresLib.Arm.Registers;
using SymbianInstructionLib.Arm.Instructions.Common;

namespace SymbianInstructionLib.Arm.Instructions.Arm.DataProcessing
{
    public class Arm_DataProcessing_32BitImmediate : Arm_DataProcessing
    {
        #region Constructors
        public Arm_DataProcessing_32BitImmediate()
        :   base( TEncoding.EEncoding32BitImmediate )
        {
            //             Cond             Op.      S       Rn       Rd    rotate_imm       imm_8
            base.SetMask( "####" + "001" + "####" + "#" + "####" + "####" +    "####"  + "########" );
        }
        #endregion

        #region From Arm_DataProcessing
        public override uint Immediate
        {
            get
            {
                byte rotate = this.RotateImm;
                SymUInt8 immediate = this.Immediate8;
                //
                int rotateAmount = (int) ( rotate * 2 );
                SymUInt32 ret = immediate.RotateRight( rotateAmount );
                return ret;
            }
        }

        public override bool SuppliesImmediate
        {
            get { return true; }
        }
        #endregion

        #region Properties
        public byte RotateImm
        {
            get
            {
                byte rotate_imm = (byte) KMaskRotate.Apply( base.AIRawValue );
                return rotate_imm;
            }
        }

        public byte Immediate8
        {
            get
            {
                byte immediate_8 = (byte) ( base.AIRawValue & 0xFF );
                return immediate_8;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private static readonly SymMask KMaskRotate = new SymMask( 0xF00, SymMask.TShiftDirection.ERight, 8 );
        #endregion
    }
}

