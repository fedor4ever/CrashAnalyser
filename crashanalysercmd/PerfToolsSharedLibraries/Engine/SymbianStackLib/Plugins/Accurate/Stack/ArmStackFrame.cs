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
using SymbianStackAlgorithmAccurate.Prologue;
using SymbianStructuresLib.Arm.Registers;

namespace SymbianStackAlgorithmAccurate.Stack
{
    internal class ArmStackFrame
    {
        #region Constructors
        internal ArmStackFrame( TArmRegisterType aRegister )
        {
            iAssociatedRegister = aRegister;
        }

        internal ArmStackFrame( ArmPrologueHelper aPrologEntry )
        {
            iPrologEntry = aPrologEntry;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public bool IsStackBasedEntry
        {
            get { return iAddress != 0; }
        }

        public bool IsRegisterBasedEntry
        {
            get { return !IsStackBasedEntry; }
        }

        public uint Address
        {
            get { return iAddress; }
            set { iAddress = value; }
        }

        public uint Data
        {
            get { return iData; }
            set { iData = value; }
        }

        public TArmRegisterType AssociatedRegister
        {
            get { return iAssociatedRegister; }
            set { iAssociatedRegister = value; }
        }
        #endregion

        #region Internal properties
        internal ArmPrologueHelper PrologEntry
        {
            get { return iPrologEntry; }
        }
        #endregion

        #region Data members
        private ArmPrologueHelper iPrologEntry = null;
        private uint iAddress = 0;
        private uint iData = 0;
        private TArmRegisterType iAssociatedRegister = TArmRegisterType.EArmReg_Other;
        #endregion
    }
}
