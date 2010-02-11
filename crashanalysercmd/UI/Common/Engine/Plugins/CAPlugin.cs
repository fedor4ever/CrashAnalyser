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
using SymbianDebugLib.Engine;
using SymbianUtils.Settings;
using SymbianUtils.PluginManager;
using CrashAnalyserEngine.Engine;
using CrashAnalyserEngine.Interfaces;

namespace CrashAnalyserEngine.Plugins
{
	public abstract class CAPlugin
    {
        #region Enumerations
        public enum TType
        {
            ETypeEngine = 0,
            ETypeUi
        }
        #endregion

        #region Constructors
        protected CAPlugin( CAEngine aEngine, string aName )
		{
            iEngine = aEngine;
            iName = aName;
		}
		#endregion

        #region Constants
        public const int KErrCommandLineNone = 0;
        public const int KErrCommandLineGeneral = -1;
        public const int KErrCommandLinePluginNotFound = -2;
        public const int KErrCommandLinePluginArgumentsMissing = -3;
        public const int KErrCommandLinePluginArgumentsInvalid = -4;
        public const int KErrCommandLinePluginArgumentsFileNotFound = -5;
        public const int KErrCommandLinePluginArgumentsFileInvalid = -6;
        public const int KErrCommandLinePluginSinkNotAvailable = -7;
        #endregion

        #region Framework API
        public virtual void AllPluginsLoaded()
        {
        }

        public virtual bool IsCommandLineHandler( string aName )
        {
            return false;
        }

        public virtual int RunCommandLineOperations()
        {
            return 0;
        }

        public abstract TType Type
        {
            get;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public string Name
        {
            get { return iName; }
        }

        public XmlSettings Settings
        {
            get { return iEngine.Settings; }
        }

        public CAEngine UIEngine
        {
            get { return iEngine; }
        }

        public IEngineUIManager UIManager
        {
            get { return UIEngine.UIManager; }
        }

        public DbgEngine DebugEngine
        {
            get { return iEngine.DebugEngine; }
        }
		#endregion

        #region Internal methods
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return Name;
        }
        #endregion

		#region Data members
        private readonly CAEngine iEngine;
        private readonly string iName;
		#endregion
	}
}
