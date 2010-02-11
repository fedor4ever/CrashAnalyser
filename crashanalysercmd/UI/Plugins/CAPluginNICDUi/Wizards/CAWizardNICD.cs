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
using CAPluginNICD.Plugin;

namespace CAPluginNICDUi.Wizards
{
    internal partial class CAWizardNICD : Form
    {
        #region Constructors
        public CAWizardNICD( CAPluginNICD.Plugin.CAPluginNICD aPlugin )
        {
            iPlugin = aPlugin;
            //
            InitializeComponent();
        }
        #endregion

        #region Properties
        #endregion

        #region Events
        private void iWizard_WizardClosedFromFinish( SymbianWizardLib.Engine.SymWizardClosureEvent aEventArgs )
        {
            string errorText = string.Empty;
            bool isReady = iPG_DebugEngine_Control.IsReadyToPrime( out errorText );
            if ( isReady )
            {
                // Prime debug engine
                iPG_DebugEngine_Control.Prime();

                // Store the settings at this point as we are largely past the
                // initial configuration
                iPlugin.Settings.Store();

                // Also save debug engine configuration
                iPG_DebugEngine_Control.XmlSettingsSave();
               
                // Read NICD trace file
                string fileName = iFB_NICD_Trace.EntityName;
                CrashDebuggerUiLib.Dialogs.ParserProgressDialog.Read( iPlugin.CrashDebuggerInfo, fileName );

                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show( errorText, "Error" );
                aEventArgs.CancelClosure = true;
            }
        }
        #endregion

        #region Page - source files
        private void iPG_SourceFile_Load( object sender, EventArgs e )
        {
            iPlugin.Settings.Load( this.Name, iFB_NICD_Trace );
        }

        private void iPG_SourceFile_PageClosedFromButtonNext( SymbianWizardLib.Engine.SymWizardPageTransitionEvent aEventArgs )
        {
            if ( iFB_NICD_Trace.IsValid )
            {
                iPlugin.Settings.Save( this.Name, iFB_NICD_Trace );
            }
            else
            {
                // Don't let a page transition occur if a valid file is not entered
                aEventArgs.SuggestedNewPage = aEventArgs.CurrentPage;
            }
        }
        #endregion

        #region Page - debug engine
        private void iPG_DebugEngine_Load( object aSender, EventArgs aArgs )
        {
            iPG_DebugEngine_Control.Engine = iPlugin.DebugEngine;
        }

        private void iPG_DebugEngine_PageClosedFromButtonNext( SymbianWizardLib.Engine.SymWizardPageTransitionEvent aEventArgs )
        {
            string errorText = string.Empty;
            bool isReady = iPG_DebugEngine_Control.IsReadyToPrime( out errorText );
            if ( isReady == false )
            {
                MessageBox.Show( errorText, "Error" );
                aEventArgs.SuggestedNewPage = aEventArgs.CurrentPage;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly CAPluginNICD.Plugin.CAPluginNICD iPlugin;
        #endregion
    }
}
