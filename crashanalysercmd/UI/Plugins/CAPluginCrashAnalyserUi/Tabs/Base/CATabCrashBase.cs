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
using System.Data;
using System.Text;
using System.Windows.Forms;
using CrashAnalyserEngine.Tabs;
using CrashAnalyserEngine.Engine;
using CrashAnalyserEngine.Interfaces;
using CrashItemLib.Engine;
using CAPCrashAnalysis.Plugin;

namespace CAPluginCrashAnalysisUi.Tabs
{
    internal partial class CATabCrashBase : CATab
    {
        #region Constructors
        protected CATabCrashBase()
        {
            // For IDE designer only
            InitializeComponent();
        }

        protected CATabCrashBase( CAPluginCrashAnalysis aSubEngine )
        {
            iSubEngine = aSubEngine;
            //
            InitializeComponent();
        }
        #endregion
        
        #region Properties
        protected CAPluginCrashAnalysis SubEngine
        {
            get { return iSubEngine; }
        }

        protected CIEngine CrashItemEngine
        {
            get { return SubEngine.CrashItemEngine; }
        }

        protected IEngineUIManager UIManager
        {
            get { return SubEngine.UIEngine.UIManager; }
        }
        #endregion

        #region From CATab
        protected override void RegisterMenuItems()
        {
            base.RegisterMenuItems();
        }
        #endregion

        #region Event handlers
        #endregion

        #region Event handlers
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private CAPluginCrashAnalysis iSubEngine = null;
        #endregion
    }
}
