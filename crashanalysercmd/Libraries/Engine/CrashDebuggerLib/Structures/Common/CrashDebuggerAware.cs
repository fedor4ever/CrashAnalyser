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

namespace CrashDebuggerLib.Structures.Common
{
    public class CrashDebuggerAware
    {
        #region Constructors
        public CrashDebuggerAware( CrashDebuggerInfo aCrashDebugger )
        {
            iCrashDebugger = aCrashDebugger;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public CrashDebuggerInfo CrashDebugger
        {
            get { return iCrashDebugger; }
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
        private readonly CrashDebuggerInfo iCrashDebugger;
        #endregion
    }
}
