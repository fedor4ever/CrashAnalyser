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

namespace CrashDebuggerLib.Structures.Common
{
    public class LinkedListInfo
    {
        #region Constructors
        public LinkedListInfo()
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint Next
        {
            get { return iNext; }
            set { iNext = value; }
        }

        public uint Previous
        {
            get { return iPrevious; }
            set { iPrevious = value; }
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
        private uint iNext = 0;
        private uint iPrevious = 0;
        #endregion
    }
}
