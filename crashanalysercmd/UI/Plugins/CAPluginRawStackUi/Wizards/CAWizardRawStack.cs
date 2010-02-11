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
using CAPRawStack.Plugin;
using SymbianUtilsUi.Dialogs;
using CAPluginRawStackUi.Dialogs;
using SymbianStackLibUi.GUI;
using SymbianUtils;

namespace CAPluginRawStackUi.Wizards
{
    internal partial class CAWizardRawStack : Form
    {
        #region Constructors
        public CAWizardRawStack( CAPluginRawStack aEngine )
        {
            iEngine = aEngine;
            //
            InitializeComponent();
        }
        #endregion

        #region Events
        private void CAWizardRawStack_Load( object sender, EventArgs e )
        {
        }

        private void CAWizardRawStack_FormClosing( object sender, FormClosingEventArgs e )
        {
        }

        private void iWizard_WizardClosedFromFinish( SymbianWizardLib.Engine.SymWizardClosureEvent aEventArgs )
        {
            string errorText = string.Empty;
            bool isReady = iPG_DebugEngine_Control.IsReadyToPrime( out errorText );
            if ( isReady )
            {
                // Create new engine with (optionally) any new prefix values
                iEngine.StackEngine.Prefixes.SetCustomPointer( iTB_PrefixCurrentStackPointer.Text );
                iEngine.StackEngine.Prefixes.SetCustomCodeSegment( iTB_PrefixCodeSegment.Text );
                iEngine.StackEngine.AddressInfo.Pointer = 0;

                // Check current stack pointer address is valid...
                long currentStackAddress = 0;
                string currentStackAddressText = iTB_CurrentStackPointerAddress.Text.Trim();
                NumberBaseUtils.TNumberBase numberBase;
                if ( NumberBaseUtils.TextToDecimalNumber( ref currentStackAddressText, out currentStackAddress, out numberBase ) )
                {
                    iEngine.StackEngine.AddressInfo.Pointer = System.Convert.ToUInt32( currentStackAddress );
                }

                // Prime stack engine content
                iEngine.StackEngine.Primer.Prime( iTB_RawStackData.Lines );

                // If we still haven't obtained the stack pointer, then use the base address
                if ( iEngine.StackEngine.AddressInfo.Pointer == 0 )
                {
                    iEngine.StackEngine.AddressInfo.Pointer = iEngine.StackEngine.AddressInfo.Top;
                }

                // Prime debug engine
                iPG_DebugEngine_Control.Prime();

                // Store the settings at this point as we are largely past the
                // initial configuration
                iEngine.Settings.Store();

                // Also save debug engine configuration
                iPG_DebugEngine_Control.XmlSettingsSave();

                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show( errorText, "Error" );
                aEventArgs.CancelClosure = true;
            }
        }
        #endregion

        #region Page - source data
        private void iPG_SourceData_Load( object aSender, EventArgs aArgs )
        {
            iEngine.Settings.Load( this.Name, iTB_PrefixCodeSegment );
            iEngine.Settings.Load( this.Name, iTB_PrefixCurrentStackPointer );
            iEngine.Settings.Load( this.Name, iCB_MatchExactSymbols );
        }

        private void iPG_SourceData_PageClosedFromButtonNext( SymbianWizardLib.Engine.SymWizardPageTransitionEvent aEventArgs )
        {
            iEngine.Settings.Save( this.Name, iTB_PrefixCodeSegment );
            iEngine.Settings.Save( this.Name, iTB_PrefixCurrentStackPointer );
            iEngine.Settings.Save( this.Name, iCB_MatchExactSymbols );
        }

        private void iBT_LoadTextDataFromFile_Click( object sender, EventArgs e )
        {
            string fileName;
            string filter;
            //
            DialogResult result = RawStackImportDialog.ShowDialog( iEngine.Settings, out fileName, out filter );
            //
            if ( result == DialogResult.OK )
            {
                FileToTextBoxProgressDialog.Read( fileName, filter, iTB_RawStackData );
                iTB_RawStackData.Select();
                iTB_RawStackData.Focus();
            }
        }

        private void iBT_LoadBinaryDataFromFile_Click( object sender, EventArgs e )
        {
            OpenFileDialog dialog = new OpenFileDialog();
            //
            dialog.Filter = "All Files|*.*";
            dialog.Title = "Select a File";
            //
            if ( dialog.ShowDialog() == DialogResult.OK )
            {
                string fileName = dialog.FileName;
                //
                string data = StackFileToTextConverterDialog.Convert( fileName );
                iTB_RawStackData.Text = data;
                iTB_RawStackData.Select();
                iTB_RawStackData.Focus();
            }
        }

        private void iTB_RawStackData_KeyDown( object sender, KeyEventArgs e )
        {
            if ( e.KeyCode == Keys.A && e.Control )
            {
                iTB_RawStackData.SelectAll();
                e.Handled = true;
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
        private readonly CAPluginRawStack iEngine;
        #endregion

    }
}
