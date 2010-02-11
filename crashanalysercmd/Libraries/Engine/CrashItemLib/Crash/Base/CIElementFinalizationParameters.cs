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
using System.Collections.Generic;
using SymbianUtils;
using SymbianStructuresLib.CodeSegments;
using SymbianDebugLib.Engine;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Messages;
using CrashItemLib.Engine;

namespace CrashItemLib.Crash.Base
{
	internal sealed class CIElementFinalizationParameters : DisposableObject
	{
		#region Constructors
        public CIElementFinalizationParameters( CIEngine aEngine )
        {
            iEngine = aEngine;

            // We create a (default) debug engine view that is not associated with any particular process-relative
            // view of the symbols etc. In other words, it can resolve XIP-only content.
            iDebugEngineView = aEngine.DebugEngine.CreateView( "CIEngine Global Debug Engine View" );
        }

        public CIElementFinalizationParameters( CIEngine aEngine, string aName, CodeSegDefinitionCollection aCodeSegments )
        {
            iEngine = aEngine;
            iDebugEngineView = aEngine.DebugEngine.CreateView( aName, aCodeSegments );
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public CIEngine Engine
        {
            get { return iEngine; }
        }

        public DbgEngine DebugEngine
        {
            get { return iEngine.DebugEngine; }
        }

        public DbgEngineView DebugEngineView
        {
            get { return iDebugEngineView; }
        }
        #endregion

        #region From DisposableObject
        protected override void CleanupManagedResources()
        {
            try
            {
                base.CleanupManagedResources();
            }
            finally
            {
                if ( iDebugEngineView != null )
                {
                    iDebugEngineView.Dispose();
                    iDebugEngineView = null;
                }
            }
        }
        #endregion

        #region Data members
        private readonly CIEngine iEngine;
        private DbgEngineView iDebugEngineView = null;
		#endregion
	}
}
