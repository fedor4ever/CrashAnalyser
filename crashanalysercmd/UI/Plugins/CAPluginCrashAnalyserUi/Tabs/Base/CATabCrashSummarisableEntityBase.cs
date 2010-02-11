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
    internal partial class CATabCrashSummarisableEntityBase : CATabCrashBase
    {
        #region Constructors
        protected CATabCrashSummarisableEntityBase()
        {
            // For IDE designer only
            InitializeComponent();
        }

        protected CATabCrashSummarisableEntityBase( CAPluginCrashAnalysis aSubEngine, CISummarisableEntity aEntity )
            : base( aSubEngine )
        {
            iEntity = aEntity;
            //
            InitializeComponent();
            //
            base.Title = aEntity.Name;
            //
            RegisterMenuItems();
        }
        #endregion
        
        #region Properties
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        protected CISummarisableEntity CISummarisableEntity
        {
            get { return iEntity; }
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
        private CISummarisableEntity iEntity = null;
        #endregion
    }
}
