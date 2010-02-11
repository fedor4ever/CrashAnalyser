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
using System.Text;
using System.Threading;
using System.ComponentModel;
using SymbianDebugLib.Engine;
using CrashItemLib.Crash;
using CrashItemLib.PluginAPI;
using CrashItemLib.Engine.Sources;
using SymbianUtils.SerializedOperations;
using SymbianUtils.Tracer;

namespace CrashItemLib.Engine.Operations
{
    /// <summary>
    /// Serialises asynchronous requests - mainly because the symbol engine doesn't much
    /// like having dynamically loaded codesegments unloaded underneath it - i.e. it doesn't
    /// work multithreaded!
    /// </summary>
    internal class CIEngineOperationManager : SerializedOperationManager, ITracer
    {
        #region Constructors
        public CIEngineOperationManager( CIEngine aEngine )
        {
            iEngine = aEngine;

            // Uncomment this line if you want operation tracing
            //base.Tracer = (ITracer) this;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        #endregion

        #region Event handlers
        #endregion

        #region ITracer Members
        void ITracer.Trace( string aMessage )
        {
            iEngine.Trace( aMessage );
        }

        void ITracer.Trace( string aFormat, params object[] aParams )
        {
            iEngine.Trace( aFormat, aParams );
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly CIEngine iEngine;
        #endregion
    }
}
