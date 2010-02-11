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
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.PluginAPI.Types.Symbol;
using SymbianUtils;
using SymbianUtils.Threading;
using SymbianUtils.FileSystem;
using SymbianUtils.Tracer;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Container;
using CrashItemLib.PluginAPI;
using CrashItemLib.Engine.Interfaces;
using CrashItemLib.Engine.Sources;
using CrashItemLib.Engine;

namespace CrashItemLib.Crash.Container
{
    internal class CIContainerFinalizer : MultiThreadedProcessor<CIContainer>
    {
        #region Constructors
        public CIContainerFinalizer( CIContainerCollection aCollection, CIEngine aEngine )
            : base( aCollection )
        {
            iFinalizationParameters = new CIElementFinalizationParameters( aEngine );
        }
        #endregion

        #region API
        #endregion
        
        #region Properties
        #endregion

        #region Internal methods
        protected override bool Process( CIContainer aContainer )
        {
            aContainer.RunFinalizers( iFinalizationParameters );
            return true;
        }
        #endregion

        #region Data members
        private readonly CIElementFinalizationParameters iFinalizationParameters;
        #endregion
    }
}
