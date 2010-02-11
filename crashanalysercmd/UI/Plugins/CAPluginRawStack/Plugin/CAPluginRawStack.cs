/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using CrashAnalyserEngine.Plugins;
using CrashAnalyserEngine.Engine;
using CrashAnalyserEngine.Interfaces;
using SymbianUtils;
using SymbianUtils.FileSystem.Utilities;
using SymbianStackLib.Engine;

namespace CAPRawStack.Plugin
{
	public class CAPluginRawStack : CAPlugin
	{
		#region Constructors
        public CAPluginRawStack( CAEngine aEngine )
            : base( aEngine, KPluginName )
		{
            iStackEngine = new StackEngine( aEngine.DebugEngine );
		}
		#endregion

        #region Constants
        public const string KPluginName = "Call Stack Reconstructor";
        #endregion

		#region API
        #endregion

        #region From CAPlugin
        public override CAPlugin.TType Type
        {
            get { return CAPlugin.TType.ETypeEngine; }
        }
        #endregion

		#region Properties
        public StackEngine StackEngine
        {
            get { return iStackEngine; }
        }
		#endregion

        #region Event handlers
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private StackEngine iStackEngine;
        #endregion
    }
}
