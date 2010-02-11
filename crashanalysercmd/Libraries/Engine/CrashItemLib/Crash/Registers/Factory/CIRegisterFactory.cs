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
using System.Text;
using System.Collections.Generic;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Threads;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Registers.Special;
using SymbianStructuresLib.Arm.Registers;

namespace CrashItemLib.Crash.Registers.Factory
{
    internal static class CIRegisterFactory
	{
        public static CIRegister New( TArmRegisterType aType, uint aValue, CIRegisterList aList )
        {
            CIRegister ret = New( aType, aValue, ArmRegister.GetTypeName( aType ), aList );
            return ret;
        }

        public static CIRegister New( TArmRegisterType aType, uint aValue, string aName, CIRegisterList aList )
        {
            CIRegister ret = null;
            //
            switch ( aType )
            {
            case TArmRegisterType.EArmReg_CPSR:
                ret = new CIRegisterCPSR( aList, aValue );
                break;
            case TArmRegisterType.EArmReg_FSR:
                ret = new CIRegisterFSR( aList, aValue );
                break;
            case TArmRegisterType.EArmReg_EXCCODE:
                ret = new CIRegisterExcCode( aList, aValue );
                break;
            default:
                ret = new CIRegister( aList, aType, aName, aValue );
                break;
            }
            //
            System.Diagnostics.Debug.Assert( ret.Type == aType );
            System.Diagnostics.Debug.Assert( ret.Value == aValue );
            System.Diagnostics.Debug.Assert( ret.Name == aName );
            //
            return ret;
        }
    }
}
