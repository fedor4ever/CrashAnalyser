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
using SymbianUtils.Threading;
using SymbianUtils;
using SymbianUtils.PluginManager;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Messages;

namespace CrashItemLib.Engine.ProblemDetectors
{
    internal class CIProblemDetectorManager : MultiThreadedProcessor<CIProblemDetector>
    {
        #region Constructors
        public CIProblemDetectorManager( CIEngine aEngine )
            : base( KDetectors )
		{
            iEngine = aEngine;
        }

        static CIProblemDetectorManager()
        {
            KDetectors.LoadFromCallingAssembly();
        }
		#endregion

        #region API
        #endregion

        #region From MultiThreadedProcessor<CIProblemDetector>
        public override void Start( TSynchronicity aSynchronicity )
        {
            // Make sure we always start with a fully populated list of detectors
            base.PopulateQueue( KDetectors );
            base.Start( aSynchronicity );
        }

        protected override bool Process( CIProblemDetector aItem )
        {
            iEngine.Trace( "[CIProblemDetectorManager] Process() - START - detector: \'{0}\'", aItem.GetType().Name );
            //
            foreach ( CIContainer container in iEngine )
            {
                aItem.Check( container );
            }
            //
            iEngine.Trace( "[CIProblemDetectorManager] Process() - END - detector: \'{0}\'", aItem.GetType().Name );
            return true;
        }
        #endregion

        #region Data members
        private readonly CIEngine iEngine;
        private static PluginManager<CIProblemDetector> KDetectors = new PluginManager<CIProblemDetector>();
		#endregion
    }
}
