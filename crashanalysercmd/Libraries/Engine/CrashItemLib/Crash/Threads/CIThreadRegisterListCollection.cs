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
using System.Text.RegularExpressions;
using System.Collections.Generic;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Threads;
using CrashItemLib.Crash.Registers;
using SymbianStructuresLib.Arm.Registers;

namespace CrashItemLib.Crash.Threads
{
	public class CIThreadRegisterListCollection : CIRegisterListCollection
	{
		#region Constructors
        public CIThreadRegisterListCollection( CIThread aThread )
            : base( aThread.Container, aThread  )
		{
            Add( new CIRegisterList( Container, aThread, TArmRegisterBank.ETypeUser ) );
            Add( new CIRegisterList( Container, aThread, TArmRegisterBank.ETypeSupervisor ) );
            Add( new CIRegisterList( Container, aThread, TArmRegisterBank.ETypeException ) );
            Add( new CIRegisterList( Container, aThread, TArmRegisterBank.ETypeCoProcessor ) );
        }
		#endregion

        #region API
        #endregion

        #region Properties
        public CIThread OwningThread
        {
            get { return (CIThread) base.Parent; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
