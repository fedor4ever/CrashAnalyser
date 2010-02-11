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
using CrashItemLib.Crash.Summarisable;
using CAPCrashAnalysis.Plugin;

namespace CAPluginCrashAnalysisUi.Tabs
{
    internal partial class CATabCrashViewer : CATabCrashSummarisableEntityBase
    {
        #region Constructors
        public CATabCrashViewer( CAPluginCrashAnalysis aSubEngine, CISummarisableEntity aEntity )
            : base( aSubEngine, aEntity )
        {
            InitializeComponent();
            //
            base.Title = "Crash Details - " + aEntity.Name;
        }
        #endregion

        #region Properties
        #endregion

        #region Event handlers
        private void CATabCrashViewer_Load( object sender, EventArgs e )
        {
            iCtrl_Summarisable.CISummarisableEntity = base.CISummarisableEntity;
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
