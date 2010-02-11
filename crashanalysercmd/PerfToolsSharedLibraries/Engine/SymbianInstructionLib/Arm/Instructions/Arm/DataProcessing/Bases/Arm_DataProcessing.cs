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
using SymbianStructuresLib.Arm.Instructions;
using SymbianInstructionLib.Arm.Instructions.Common;

namespace SymbianInstructionLib.Arm.Instructions.Arm.DataProcessing
{
    public abstract class Arm_DataProcessing : ArmInstruction
    {
        #region Enumerations
        public enum TEncoding
        {
            EEncodingNotSupported = -1,
            EEncoding32BitImmediate = 0,
            EEncodingShiftsImmediate,
            EEncodingShiftsRegister
        }
        #endregion

        #region Constructors
        protected Arm_DataProcessing( TEncoding aEncoding )
        {
            iEncoding = aEncoding;
            base.AIGroup = SymbianStructuresLib.Arm.Instructions.TArmInstructionGroup.EGroupDataProcessing;
        }
        #endregion

        #region From ArmBaseInstruction
        protected override bool DoQueryInvolvementAsSource( TArmRegisterType aRegister )
        {
            TArmRegisterType reg1 = Rn;
            return ( aRegister == reg1 );
        }

        protected override bool DoQueryInvolvementAsDestination( TArmRegisterType aRegister )
        {
            TArmRegisterType reg = Rd;
            return ( aRegister == reg );
        }
        #endregion

        #region Framework API
        public virtual uint Immediate
        {
            get { return 0; }
        }

        public virtual bool SuppliesImmediate
        {
            get { return false; }
        }
        #endregion

        #region Properties
        public TEncoding Encoding
        {
            get { return iEncoding; }
        }

        public TArmDataProcessingType OperationType
        {
            get
            {
                TArmDataProcessingType ret = (TArmDataProcessingType) base.AIRawValue[ 24, 21 ].ToUInt();
                return ret;
            }
        }

        public TArmRegisterType Rn
        {
            get
            {
                TArmRegisterType ret = (TArmRegisterType) base.AIRawValue[ 19, 16 ].ToUInt();
                return ret;
            }
        }

        public TArmRegisterType Rd
        {
            get
            {
                TArmRegisterType ret = (TArmRegisterType) base.AIRawValue[ 15, 12 ].ToUInt();
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly TEncoding iEncoding;
        #endregion
    }
}

