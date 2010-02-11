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
using CrashDebuggerLib.Structures.Common;

namespace CrashDebuggerLib.Structures.KernelObjects
{
    public class DBase : CrashDebuggerAware
    {
        #region Constructors
        public DBase( CrashDebuggerInfo aCrashDebugger )
            : base( aCrashDebugger )
        {
        }
        #endregion

        #region API
        public virtual string ToClipboard()
        {
            return "@ 0x" + KernelAddress.ToString( "x8" );
        }
        #endregion

        #region Properties
        public uint KernelAddress
        {
            get { return iKernelAddress; }
            set { iKernelAddress = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return iKernelAddress.ToString("x8");
        }
        #endregion

        #region Data members
        private uint iKernelAddress = 0;
        #endregion
    }
}
