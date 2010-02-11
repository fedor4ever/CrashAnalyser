/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using SymbianStructuresLib.Debug.Symbols;
using CrashDebuggerLib.Structures.KernelObjects;
using SymbianStructuresLib.Arm.Registers;

namespace CrashDebuggerLib.Structures.Register
{
    public class RegisterEntryCPSR : RegisterEntry
    {
        #region Constructors
        internal RegisterEntryCPSR( RegisterCollection aParent, uint aValue )
            : base( aParent, ArmRegister.GetTypeName( TArmRegisterType.EArmReg_CPSR ), aValue )
        {
        }
        #endregion

        #region API
        public RegisterCollection.TType CurrentBank
        {
            get
            {
                RegisterCollection.TType type = RegisterCollection.TType.ETypeUser;
                //
                uint maskedCPSR = Value & KMaskValue;
                type = (RegisterCollection.TType) maskedCPSR;
                //
                return type;
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        private const uint KMaskValue = 0x1F;
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.Append( base.ToString() );
            return ret.ToString();
        }
        #endregion

        #region Data members
        #endregion
    }
}
