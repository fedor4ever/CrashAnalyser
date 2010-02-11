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
using System.Reflection;
using System.Windows.Forms;
using SymbianDebugLib.Engine;
using CrashAnalyserEngine.Engine;
using CrashAnalyserEngine.Plugins;
using CrashDebuggerLib.Structures;

namespace CAPluginNICD.Plugin
{
    public class CAPluginNICD : CAPlugin
	{
		#region Constructors
        public CAPluginNICD( CAEngine aEngine )
            : base( aEngine, "Non-interactive Crash Debugger" )
		{
            iCrashDebuggerInfo = new CrashDebuggerInfo( DebugEngine );
        }
		#endregion

        #region From CAPlugin
        public override CAPlugin.TType Type
        {
            get { return TType.ETypeUi; }
        }
        #endregion

		#region API
        #endregion

		#region Properties
        public CrashDebuggerInfo CrashDebuggerInfo
        {
            get { return iCrashDebuggerInfo; }
        }
        #endregion

		#region Data members
        private readonly CrashDebuggerInfo iCrashDebuggerInfo;
        #endregion
	}
}
