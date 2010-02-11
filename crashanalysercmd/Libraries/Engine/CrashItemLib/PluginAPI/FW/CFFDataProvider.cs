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
using System.IO;
using System.Collections.Generic;
using System.Text;
using SymbianUtils;
using CrashItemLib.Engine;
using CrashItemLib.Crash;
using SymbianDebugLib.Engine;
using CrashItemLib.Crash.Container;

namespace CrashItemLib.PluginAPI
{
    public abstract class CFFDataProvider
    {
        #region Constructors
        protected CFFDataProvider()
        {
        }
        #endregion

        #region Framework API
        /// <summary>
        /// Get crash engine handle
        /// </summary>
        public abstract CIEngine Engine
        {
            get;
        }
        #endregion

        #region API
        public CIContainer CreateContainer( CFFSource aDescriptor )
        {
            CIContainer item = CIContainer.New( Engine, aDescriptor );
            return item;
        }

        public CIContainer CreateErrorContainer( CFFSource aDescriptor )
        {
            CIContainer item = CIContainer.NewErrorContainer( Engine, aDescriptor );
            return item;
        }
        #endregion

        #region Properties
        #endregion
    }
}
