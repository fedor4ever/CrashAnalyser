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
    public class RequestStatus
    {
        #region Constructors
        public RequestStatus()
        {
        }

        public RequestStatus( uint aAddress )
        {
            iAddress = aAddress;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint Address
        {
            get { return iAddress; }
            set { iAddress = value; }
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
        private uint iAddress = 0;
        #endregion
    }
}
