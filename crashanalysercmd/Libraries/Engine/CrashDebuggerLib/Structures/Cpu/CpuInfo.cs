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
using CrashDebuggerLib.Structures.KernelObjects;
using CrashDebuggerLib.Structures.Register;
using CrashDebuggerLib.Structures.Common;
using SymbianStructuresLib.Arm.Registers;

namespace CrashDebuggerLib.Structures.Cpu
{
    public class CpuInfo : RegisterSet
    {
        #region Constructors
        public CpuInfo( CrashDebuggerInfo aCrashDebugger )
            : base( aCrashDebugger )
        {
        }
        #endregion

        #region API
        public RegisterCollection GetRegisters()
        {
            RegisterCollection.TType bankType = CPSRBankType;
            RegisterCollection bank = this[ bankType ];
            //
            RegisterCollection ret = new RegisterCollection( CrashDebugger, bankType );
            foreach ( RegisterEntry entry in bank )
            {
                ret.Add( entry.OriginalName, entry.Value );
            }

            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private void TryToUpdateRegister( ArmRegister aObjectToUpdate, RegisterCollection aSource )
        {
            RegisterEntry entry = aSource[ aObjectToUpdate.RegType ];
            if ( entry != null )
            {
                aObjectToUpdate.Value = entry.Value;
            }
        }
        #endregion

        #region Internal constants
        #endregion

        #region Data members
        #endregion
    }
}
