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
using CrashDebuggerLib.Structures.Mutex;

namespace CrashDebuggerLib.Structures.Process
{
    public class ProcessPageDir
    {
        #region Constructors
        public ProcessPageDir()
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint Local
        {
            get { return iLocalPageDir; }
            set { iLocalPageDir = value; }
        }

        public uint Global
        {
            get { return iGloalPageDir; }
            set { iGloalPageDir = value; }
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
        private uint iLocalPageDir = 0;
        private uint iGloalPageDir = 0;
        #endregion
    }
}
