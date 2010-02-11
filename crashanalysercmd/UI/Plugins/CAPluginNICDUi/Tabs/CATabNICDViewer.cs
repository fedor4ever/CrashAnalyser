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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using CAPluginNICD.Plugin;
using CrashAnalyserEngine.Tabs;
using CrashAnalyserEngine.Engine;
using CrashAnalyserEngine.Interfaces;

namespace CAPluginNICDUi.Tabs
{
    internal partial class CATabNICDViewer : CATab
    {
        #region Constructors
        public CATabNICDViewer( CAPluginNICD.Plugin.CAPluginNICD aPlugin )
        {
            InitializeComponent();
            //
            iPlugin = aPlugin;
            base.Title = "NICD";
        }
        #endregion

        #region Properties
        #endregion

        #region Event handlers
        private void CATabNICDViewer_Load( object sender, EventArgs e )
        {
            iCtrl_CrashDebugger.Info = iPlugin.CrashDebuggerInfo;
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly CAPluginNICD.Plugin.CAPluginNICD iPlugin;
        #endregion
    }
}
