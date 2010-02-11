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
using SymbianStructuresLib.Arm.Registers;
using SymbianInstructionLib.Arm.Instructions.Common;

namespace SymbianInstructionLib.Arm.Instructions.Arm.DataProcessing
{
    public class Arm_DataProcessing_Shift_Register : Arm_DataProcessing_Shift
    {
        #region Constructors
        public Arm_DataProcessing_Shift_Register()
            : base( TEncoding.EEncodingShiftsRegister )
        {
            //             Cond             Op.      S       Rn       Rd       Rs         shift            Rm
            base.SetMask( "####" + "000" + "####" + "#" + "####" + "####" + "####" + "0" + "##" + "1" + "####" );
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            bool ret = base.DoQueryInvolvementAsSource( aRegister );
            //
            if ( ret == false )
            {
                TArmRegisterType reg3 = this.Rs;
                ret = ( reg3 == aRegister );
            }
            //
            return ret;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public TArmRegisterType Rs
        {
            get
            {
                TArmRegisterType ret = (TArmRegisterType) base.AIRawValue[ 11, 8 ].ToUInt();
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}

