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

namespace CrashDebuggerLib.Structures.Thread
{
    public class ThreadHandlers
    {
        #region Constructors
        public ThreadHandlers()
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint ActiveScheduler
        {
            get { return iActiveScheduler; }
            set { iActiveScheduler = value; }
        }

        public uint TrapHandler
        {
            get { return iTrapHandler; }
            set { iTrapHandler = value; }
        }

        public uint ExceptionHandler
        {
            get { return iExceptionHandler; }
            set { iExceptionHandler = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        private uint iActiveScheduler = 0;
        private uint iTrapHandler = 0;
        private uint iExceptionHandler = 0;
        #endregion
    }
}
