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
using System.Threading;
using System.ComponentModel;

namespace CrashDebuggerLib.Threading
{
    [System.ComponentModel.DesignerCategory( "code" )]
    internal class AsyncOperation : BackgroundWorker
    {
        #region Constructors
        public AsyncOperation()
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public object Tag
        {
            get { return iTag; }
            set { iTag = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private object iTag = null;
        #endregion
    }
}
