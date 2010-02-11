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

namespace SymbianInstructionLib.Arm.Instructions.Arm
{
    public class Arm_Unknown : ArmInstruction
    {
        #region Constructors
        public Arm_Unknown()
        {
        }
        #endregion

        #region API
        public override bool Matches( uint aOpCode )
        {
            // Matches everything
            return true;
        }
        #endregion

        #region Properties
        #endregion

        #region From System.Object
        #endregion

        #region From IArmInstruction
        public new bool AIIsUnknown
        {
            get { return true; }
        }
        #endregion

        #region From ArmBaseInstruction
        internal override int SortOrder
        {
            get { return int.MinValue; }
        }
        #endregion

        #region Data members
        #endregion
    }
}

