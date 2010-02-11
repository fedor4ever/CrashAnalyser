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

namespace CrashItemLib.Engine.Operations
{
    internal abstract class CIEngineOperation : SerializedOperation
    {
        #region Constructors
        protected CIEngineOperation( CIEngine aEngine )
            : this( aEngine, 0 )
        {
        }

        protected CIEngineOperation( CIEngine aEngine, long aPriority )
        {
            iEngine = aEngine;
            iPriority = aPriority;
        }
        #endregion

        #region Properties
        protected CIEngine Engine
        {
            get { return iEngine; }
        }

        public override long Priority
        {
            get { return iPriority; }
        }
        #endregion

        #region Internal methods
        internal void SetPriority( long aPriority )
        {
            iPriority = aPriority;
        }
        #endregion

        #region Data members
        private readonly CIEngine iEngine;
        private long iPriority = 0;
        #endregion
    }
}
