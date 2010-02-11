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
    public class ThreadTemporaries
    {
        #region Constructors
        public ThreadTemporaries()
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint TempObj
        {
            get { return iTempObj; }
            set { iTempObj = value; }
        }

        public uint TempAlloc
        {
            get { return iTempAlloc; }
            set { iTempAlloc = value; }
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
        private uint iTempObj = 0;
        private uint iTempAlloc = 0;
        #endregion
    }
}
