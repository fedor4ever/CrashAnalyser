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
using CrashItemLib.Crash;
using CrashItemLib.Crash.Container;
using CAPCrashAnalysis.Plugin;

namespace CAPluginCrashAnalysisUi.Tabs
{
    internal partial class CATabCrashContainerExplorer : CATabCrashContainerBase
    {
        #region Constructors
        public CATabCrashContainerExplorer( CAPluginCrashAnalysis aSubEngine, CIContainer aContainer )
            : base( aSubEngine, aContainer )
        {
            InitializeComponent();
            //
            base.Title = "Crash Details - " + System.IO.Path.GetFileName( aContainer.Source.MasterFileName ); 
        }
        #endregion

        #region Event handlers
        #endregion

        #region Event handlers
        private void CATabCrashContainerExplorer_Load( object sender, EventArgs e )
        {
            // Get the primary summary...
            iCtrl_Container.CIContainer = base.CIContainer;
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
