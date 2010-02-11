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

namespace SymbianInstructionLib.Arm.Instructions.Thumb.DataTransfer
{
    public abstract class Thumb_LoadOrStore_Immediate8 : Thumb_LoadOrStore
    {
        #region Constructors
        protected Thumb_LoadOrStore_Immediate8()
        {
        }
        #endregion

        #region Framework API
        public override TArmRegisterType Rd
        {
            get
            {
                TArmRegisterType ret = (TArmRegisterType) KMaskRd.Apply( base.AIRawValue );
                return ret;
            }
        }
        #endregion

        #region Properties
        public uint Immediate
        {
            get
            {
                uint ret = base.AIRawValue & 0xFF;
                ret *= 4;
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private static readonly SymMask KMaskRd = new SymMask( 0x700, SymMask.TShiftDirection.ERight, 8 );
        #endregion
    }
}

