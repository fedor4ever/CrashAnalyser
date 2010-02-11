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
using System.Threading;
using SymbianUtils;
using CrashAnalyserEngine.Engine;
using CrashAnalyserEngine.Plugins;
using CrashItemLib.Engine;
using CrashItemLib.Engine.Interfaces;
using CAPCrashAnalysis.CommandLine;

namespace CAPCrashAnalysis.Plugin
{
	public class CAPluginCrashAnalysis : CAPlugin, ICIEngineUI
	{
		#region Constructors
        public CAPluginCrashAnalysis( CAEngine aEngine )
            : base( aEngine, KPluginName )
		{
            iCommandLineEngine = new CACmdLineEngine( this );
            iCrashItemEngine = new CIEngine( aEngine.DebugEngine, this );
		}
		#endregion

        #region Constants
        public const string KPluginName = "Crash File Analysis";
        #endregion

		#region API
        public string GetSupportedCrashFileTypes()
        {
            return CrashItemEngine.PluginRegistry.GetSupportedCrashFileTypes();
        }

        public void PrimeSources( string[] aFileNames )
        {
            // Seed up the crash item engine with all our file names
            CrashItemEngine.ClearAll();
            foreach ( string file in aFileNames )
            {
                CrashItemEngine.Prime( new System.IO.FileInfo( file ) );
            }
        }

        public void IdentifyCrashes( TSynchronicity aSynchronicity )
        {
            CrashItemEngine.IdentifyCrashes( aSynchronicity );
        }
        #endregion

        #region From CAPlugin
        public override bool IsCommandLineHandler( string aName )
        {
            bool ret = iCommandLineEngine.IsCommandLineHandler( aName );
            return ret;
        }

        public override int RunCommandLineOperations()
        {
            int error = iCommandLineEngine.RunCommandLineOperations();
            return error;
        }

        public override CAPlugin.TType Type
        {
            get { return CAPlugin.TType.ETypeEngine; }
        }
        #endregion

		#region Properties
        public CIEngine CrashItemEngine
        {
            get { return iCrashItemEngine; }
        }
		#endregion

        #region Event handlers
        #endregion

        #region Internal methods
        #endregion

        #region From ICIEngineUI
        void ICIEngineUI.CITrace( string aMessage )
        {
            base.UIEngine.UIManager.UITrace( aMessage );
        }

        void ICIEngineUI.CITrace( string aFormat, params object[] aParameters )
        {
            base.UIEngine.UIManager.UITrace( aFormat, aParameters );
        }
        #endregion

        #region Operators
        public static implicit operator CIEngine( CAPluginCrashAnalysis aCrashFileEngine )
        {
            return aCrashFileEngine.CrashItemEngine;
        }
        #endregion

        #region Data members
        private readonly CIEngine iCrashItemEngine;
        private readonly CACmdLineEngine iCommandLineEngine;
        #endregion
    }
}
