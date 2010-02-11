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
using System.Data;
using System.Text;
using System.Windows.Forms;
using SymbianUtils;
using CrashItemLib.Engine.Sources;
using CAPCrashAnalysis.Plugin;

namespace CAPluginCrashAnalysisUi.Wizards
{
    internal partial class CAWizardCrashAnalysis : Form
    {
        #region Constructors
        public CAWizardCrashAnalysis( CAPluginCrashAnalysis aEngine )
        {
            iEngine = aEngine;
            //
            InitializeComponent();
        }
        #endregion

        #region Events
        private void CAWizardCrashFile_Load( object sender, EventArgs e )
        {
        }

        private void CAWizardCrashFile_FormClosing( object sender, FormClosingEventArgs e )
        {
        }

        private void iWizard_WizardClosedFromFinish( SymbianWizardLib.Engine.SymWizardClosureEvent aEventArgs )
        {
            DialogResult = DialogResult.OK;
        }
        #endregion

        #region Page - source files
        private void PG_SourceFiles_UpdateWizardButtonState()
        {
            iWizard.ButtonNext.Enabled = ( iPG_SourceFiles_FileList.FileCount > 0 );
        }

        private void iPG_SourceFiles_Load( object aSender, EventArgs aArgs )
        {
            // Seed file list dialog with file types.
            string types = iEngine.GetSupportedCrashFileTypes();
            iPG_SourceFiles_FileList.DialogFilter = types;
 
            iEngine.Settings.Load( "CrashFileWizard_SourceFiles", iPG_SourceFiles_FileList );
            PG_SourceFiles_UpdateWizardButtonState();
        }

        private void iPG_SourceFiles_FileList_SourceListChanged( CrashItemUiLib.Controls.CIEngineSourceManager aControl, CrashItemUiLib.Controls.CIEngineSourceManager.TEvent aEvent )
        {
            PG_SourceFiles_UpdateWizardButtonState();
        }

        private void iPG_SourceFiles_PageClosedFromButtonNext( SymbianWizardLib.Engine.SymWizardPageTransitionEvent aEventArgs )
        {
            if ( iPG_SourceFiles_FileList.FileCount == 0 )
            {
                aEventArgs.SuggestedNewPage = aEventArgs.CurrentPage;
            }
            else
            {
                List<string> files = iPG_SourceFiles_FileList.FileNames;
                iEngine.PrimeSources( files.ToArray() );

                // Save settings
                iEngine.Settings.Save( "CrashFileWizard_SourceFiles", iPG_SourceFiles_FileList );
            }
        }
        #endregion

        #region Page - debug engine
        private void iPG_DebugEngine_Load( object aSender, EventArgs aArgs )
        {
            iPG_DebugEngine_Control.Engine = iEngine.DebugEngine;
        }

        private void iPG_DebugEngine_PageClosedFromButtonNext( SymbianWizardLib.Engine.SymWizardPageTransitionEvent aEventArgs )
        {
            string errorText = string.Empty;
            bool isReady = iPG_DebugEngine_Control.IsReadyToPrime( out errorText );
            if ( isReady )
            {
                // Store the settings at this point as we are largely past the
                // initial configuration
                iEngine.Settings.Store();

                // Also save debug engine configuration
                iPG_DebugEngine_Control.XmlSettingsSave();

                // Begin priming
                iPG_DebugEngine_Control.Prime();
            }
            else
            {
                MessageBox.Show( errorText, "Error" );
                aEventArgs.SuggestedNewPage = aEventArgs.CurrentPage;
            }
        }
        #endregion

        #region Page - processing summary
        private void iPG_Processing_Load( object aSender, EventArgs aArgs )
        {
            // Observe crash item engine events
            iPG_Processing_CrashList.Engine = iEngine.CrashItemEngine;
            iEngine.CrashItemEngine.StateChanged += new CrashItemLib.Engine.CIEngine.CIEngineStateHandler( CrashItemEngine_StateHandler );
        }

        private void iPG_Processing_Unload( object sender, EventArgs e )
        {
            // Triggers event unsubscriptions
            iPG_Processing_SourceList.Sources = null;
            iPG_Processing_CrashList.Engine = null;
            iEngine.CrashItemEngine.StateChanged -= new CrashItemLib.Engine.CIEngine.CIEngineStateHandler( CrashItemEngine_StateHandler );
        }

        private void iPG_Processing_PageShownFromButtonNext( SymbianWizardLib.GUI.SymWizardPage aSender )
        {
            // Prevent navigation
            iWizard.ButtonNext.Enabled = false;
            iWizard.ButtonBack.Enabled = false;

            // Set up controls to interact with crash engine
            iPG_Processing_SourceList.Sources = iEngine.CrashItemEngine.Sources;
            iPG_Processing_CrashList.Engine = iEngine.CrashItemEngine;

            // Start reading the crash files asynchronously. We'll allow
            // the user to continue once the "processing complete" event
            // is signalled.
            iEngine.IdentifyCrashes( TSynchronicity.EAsynchronous );
        }

        private void CrashItemEngine_StateHandler( CrashItemLib.Engine.CIEngine.TState aState )
        {
            if ( InvokeRequired )
            {
                CrashItemLib.Engine.CIEngine.CIEngineStateHandler observer = new CrashItemLib.Engine.CIEngine.CIEngineStateHandler( CrashItemEngine_StateHandler );
                this.BeginInvoke( observer, new object[] { aState } );
            }
            else
            {
                switch ( aState )
                {
                case CrashItemLib.Engine.CIEngine.TState.EStateProcessingStarted:
                    break;
                case CrashItemLib.Engine.CIEngine.TState.EStateProcessingComplete:
                    iWizard.ButtonNext.Enabled = true;
                    iWizard.ButtonBack.Enabled = true;
                    break;
                }
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly CAPluginCrashAnalysis iEngine;
        #endregion
    }
}
