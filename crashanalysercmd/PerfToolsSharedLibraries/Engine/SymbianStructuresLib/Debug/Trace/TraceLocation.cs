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
using System.Collections.Generic;
using System.IO;
using System.Text;
using SymbianUtils;
using SymbianUtils.Threading;

namespace SymbianStructuresLib.Debug.Trace
{
    public class TraceLocation
    {
        #region Constructors
        public TraceLocation()
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public string File
        {
            get { return iFile; }
            set { iFile = value; }
        }

        public uint Line
        {
            get { return iLine; }
            set { iLine = value; }
        }

        public string Class
        {
            get { return iClass; }
            set { iClass = value; }
        }

        public string Method
        {
            get { return iMethod; }
            set { iMethod = value; }
        }
        #endregion

        #region Data members
        private string iFile = string.Empty;
        private string iClass = string.Empty;
        private string iMethod = string.Empty;
        private uint iLine = 0;
        #endregion
    }
}
