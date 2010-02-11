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
using SymbianInstructionLib.Arm.Instructions.Common;

namespace SymbianInstructionLib.Arm.Instructions.Thumb.DataProcessing
{
    [ArmRef( "A7.1.3 ADD (1)",  "ADD <Rd>, <Rn>, #<immed_3>" )]
    [ArmRef( "A7.1.65 SUB (1)", "SUB <Rd>, <Rn>, #<immed_3>" )]
    public class Thumb_AddOrSubtract_Immediate : Thumb_AddOrSubtract_2Regs
    {
        #region Constructors
        public Thumb_AddOrSubtract_Immediate()
        {
            //                      Type   imed_3    Rn      Rd
            base.SetMask( "000111" + "#" + "###" + "###" + "###" );
        }
        #endregion

        #region Properties
        public override uint Immediate
        {
            get
            {
                uint ret = KMaskImm.Apply( base.AIRawValue );
                return ret;
            }
        }

        public override bool SuppliesImmediate
        {
            get { return true; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private static readonly SymMask KMaskImm = new SymMask( 0x1C0, SymMask.TShiftDirection.ERight, 6 );
        #endregion
    }
}

