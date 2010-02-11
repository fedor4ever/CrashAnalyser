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
using System.Text;
using System.Collections.Generic;
using System.Data;
using CrashAnalyserEngine.Tabs;
using CrashAnalyserEngine.Engine;
using CrashAnalyserEngine.Interfaces;
using CrashAnalyserEngine.Plugins;
using SymbianXmlInputLib.Elements;
using SymbianXmlInputLib.Parser;
using SymbianXmlInputLib.Parser.Nodes;
using SymbianUtils.FileSystem;

namespace CrashAnalyserConsole
{
	internal class CACommandLineUI : IEngineUIManager
	{
		#region Constructors & destructor
        public CACommandLineUI( string[] aArguments, FSLog aLog )
		{
            iLog = aLog;

            // Create engine
            iEngine = new CAEngine( aArguments );

            // Work out if we are in verbose mode
            CheckArgsForVerbose();

            // Associate engine and UI with one another - this causes
            // plugins to be loaded
            iEngine.UIManager = this;
        }
		#endregion

        #region API
        public int Run()
        {
            iLog.TraceAlways( "[CmdExe] Run() - START" );
            int error = CAPlugin.KErrCommandLineNone;
            //
            CAPlugin plugin = LocatePlugin();
            iLog.TraceAlways( "[CmdExe] Run() - plugin: " + plugin );
            if ( plugin != null )
            {
                iLog.TraceAlways( "[CmdExe] Run() - executing plugin command line operations..." );
                error = plugin.RunCommandLineOperations();
            }
            else
            {
                iLog.TraceAlways( "[CmdExe] Run() - plugin not found!" );
                error = CAPlugin.KErrCommandLinePluginNotFound;
            }
            //
            iLog.TraceAlways( "[CmdExe] Run() - END - error: " + error );
            return error;
        }
        #endregion

        #region Properties
        public bool Verbose
        {
            get { return iLog.Verbose; }
            private set
            { 
                iLog.Verbose = value;
                iLog.TraceAlways( "[CmdExe] Verbose Mode: " + value.ToString() );
            }
        }
        #endregion

        #region IEngineUIManager Members
        public void UIManagerMenuItemAdd( TEngineUIMenuPane aPane, string aCaption, UIMenuItemClickHandler aClickHandler, object aTag )
        {
        }

        public void UIManagerMenuItemAdd( TEngineUIMenuPane aPane, string aCaption, UIMenuItemClickHandler aClickHandler, object aTag, CATab aHost )
        {
        }

        public void UIManagerContentAdd( CATab aTab )
        {
        }

        public void UIManagerContentClose( CATab aTab )
        {
        }

        public Version UIVersion
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public string UICommandLineArguments
        {
            get { return Environment.CommandLine; }
        }

        public bool UIIsSilent
        {
            get { return true; }
        }

        public void UITrace( string aMessage )
        {
            iLog.Trace( aMessage );
        }

        public void UITrace( string aFormat, params object[] aParams )
        {
            string msg = string.Format( aFormat, aParams );
            UITrace( msg );
        }
        #endregion

        #region Internal constants
        private const string KParamVerbose = "-V";
        private const string KParamPlugin = "-PLUGIN";
        #endregion

        #region Internal methods
        private void CheckArgsForVerbose()
        {
            string[] args = iEngine.CommandLineArguments;
            for ( int i = 0; i < args.Length; i++ )
            {
                string cmd = args[ i ].Trim().ToUpper();
                string nextArg = ( i < args.Length - 1 ? args[ i + 1 ].Trim().ToUpper() : string.Empty );
                //
                try
                {
                    if ( cmd == KParamVerbose )
                    {
                        Verbose = true;
                        break;
                    }
                }
                catch ( Exception )
                {
                }
            }
        }

        private CAPlugin LocatePlugin()
        {
            // -nogui -plugin CRASH_ANALYSIS -input d:\ca_fullsummary.xml
            CAPlugin ret = null;
            //
            string[] args = iEngine.CommandLineArguments;
            for( int i=0; i<args.Length; i++ )
            {
                string cmd = args[ i ].Trim().ToUpper();
                string nextArg = ( i < args.Length - 1 ? args[ i + 1 ].Trim().ToUpper() : string.Empty );
                //
                try
                {
                    if ( cmd == KParamPlugin && nextArg != string.Empty )
                    {
                        ret = LocatePluginByName( nextArg );
                        break;
                    }
                }
                catch ( Exception )
                {
                }
            }
            //
            return ret;
        }

        private CAPlugin LocatePluginByName( string aName )
        {
            CAPlugin ret = null;
            //
            foreach ( CAPlugin plugin in iEngine )
            {
                bool isHandler = plugin.IsCommandLineHandler( aName );
                if ( isHandler )
                {
                    ret = plugin;
                    break;
                }
            }
            //
            return ret;
        }
        #endregion

        #region Data members
        private readonly CAEngine iEngine;
        private readonly FSLog iLog;
        #endregion
    }
}
