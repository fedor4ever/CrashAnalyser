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
using CrashItemLib.Crash.Container;
using CrashItemLib.Sink;
using CrashAnalyserEngine.Interfaces;
using CAPCrashAnalysis.Plugin;

namespace CAPluginCrashAnalysisUi.Tabs
{
    internal partial class CATabCrashContainerBase : CATabCrashBase
    {
        #region Constructors
        protected CATabCrashContainerBase()
        {
            // For IDE designer only
            InitializeComponent();
        }

        protected CATabCrashContainerBase( CAPluginCrashAnalysis aSubEngine, CIContainer aContainer )
            : base( aSubEngine )
        {
            iContainer = aContainer;
            //
            InitializeComponent();
            //
            RegisterMenuItems();
        }
        #endregion

        #region API
        #endregion

        #region Properties
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        protected CIContainer CIContainer
        {
            get { return iContainer; }
            set { iContainer = value; }
        }
        #endregion

        #region From CATab
        protected override void RegisterMenuItems()
        {
            base.RegisterMenuItems();
            //
            CISinkManager sinkManager = base.CrashItemEngine.SinkManager;
            foreach ( CISink sink in sinkManager )
            {
                base.UIManager.UIManagerMenuItemAdd( CrashAnalyserEngine.Interfaces.TEngineUIMenuPane.EFileSaveAs, sink.Name, new UIMenuItemClickHandler( Menu_File_SaveAs_SinkFormat_Click ), sink, this );
            }
        }
        #endregion

        #region Event handlers
        private void Menu_File_SaveAs_SinkFormat_Click( object aTag, string aCaption )
        {
            if ( aTag is CISink )
            {
                CISink sink = (CISink) aTag;
                //
                CISinkSerializationParameters parameters = new CISinkSerializationParameters( CIContainer, base.UIManager.UIVersion, base.UIManager.UICommandLineArguments );
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.Description = "Save Location";
                //
                if ( dialog.ShowDialog() == DialogResult.OK )
                {
                    parameters.OutputDirectory = new System.IO.DirectoryInfo( dialog.SelectedPath );
                    //
                    sink.Serialize( parameters );
                }
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private CIContainer iContainer;
        #endregion
    }
}
