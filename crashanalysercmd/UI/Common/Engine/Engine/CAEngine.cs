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
using System.Reflection;
using SymbianUtils;
using SymbianUtils.Settings;
using SymbianUtils.Tracer;
using SymbianUtils.PluginManager;
using SymbianDebugLib.Engine;
using CrashAnalyserEngine.Plugins;
using CrashAnalyserEngine.Interfaces;

namespace CrashAnalyserEngine.Engine
{
	public class CAEngine : DisposableObject, IEnumerable<CAPlugin>, ITracer
	{
		#region Constructors
        public CAEngine( string[] aCommandLineArgs )
		{
            iDebugEngine = new DbgEngine( this );
            iCommandLineArgs = aCommandLineArgs;

            // Create settings
            iSettings = new XmlSettings( "CASettings.xml" );
            iSettings.Restore();
		}
		#endregion

		#region API
        #endregion

		#region Properties
        public XmlSettings Settings
        {
            get { return iSettings; }
        }

        public string[] CommandLineArguments
        {
            get { return iCommandLineArgs; }
        }

        /// <summary>
        /// The main debug engine instance is owned by the engine and is shared
        /// amongst all that need it.
        /// </summary>
        public DbgEngine DebugEngine
        {
            get { return iDebugEngine; }
        }

        public IEngineUIManager UIManager
        {
            get { return iUIManager; }
            set
            {
                iUIManager = value;

                // Now we can create the sub-engines
                LoadPlugins();
            }
        }

        public CAPlugin this[ string aName ]
        {
            get
            {
                CAPlugin ret = null;
                //
                foreach ( CAPlugin plugin in iPlugins )
                {
                    string name = plugin.Name.ToUpper();
                    if ( name == aName.ToUpper() )
                    {
                        ret = plugin;
                        break;
                    }
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        private void LoadPlugins()
        {
            object[] parameters = new object[ 1 ];
            parameters[ 0 ] = this;
            //
            iPlugins.Load( parameters );
            //
            NotifyAllLoaded();
        }

        private void NotifyAllLoaded()
        {
            foreach ( CAPlugin plugin in iPlugins )
            {
                plugin.AllPluginsLoaded();
            }
        }
        #endregion

        #region From IEnumerable<CAPlugin> 
        public IEnumerator<CAPlugin> GetEnumerator()
        {
            return iPlugins.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return iPlugins.GetEnumerator();
        }
        #endregion

        #region From ITracer
        public void Trace( string aMessage )
        {
            if ( iUIManager != null )
            {
                iUIManager.UITrace( aMessage );
            }
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            Trace( string.Format( aFormat, aParams ) );
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
                if ( iPlugins != null )
                {
                    iPlugins.Unload();
                    iPlugins = null;
                }
                //
                iDebugEngine.Dispose();
            }
        }
        #endregion

        #region Data members
        private readonly XmlSettings iSettings;
        private readonly string[] iCommandLineArgs;
        private readonly DbgEngine iDebugEngine;
        private IEngineUIManager iUIManager = null;
        private PluginManager<CAPlugin> iPlugins = new PluginManager<CAPlugin>(1);
		#endregion
    }
}
