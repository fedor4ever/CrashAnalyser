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
    public class ThreadPriorities
    {
        #region Constructors
        public ThreadPriorities()
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public int Default
        {
            get { return iDefault; }
            set { iDefault = value; }
        }

        public int WaitLink
        {
            get { return iWaitLink; }
            set { iWaitLink = value; }
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
        private int iDefault = 0;
        private int iWaitLink = 0;
        #endregion
    }
}
