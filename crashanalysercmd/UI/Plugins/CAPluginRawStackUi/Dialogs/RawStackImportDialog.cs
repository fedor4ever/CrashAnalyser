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
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SymbianUtils;
using SymbianUtils.Settings;

namespace CAPluginRawStackUi.Dialogs
{
    public partial class RawStackImportDialog : Form
    {
        #region Constructors
        public static DialogResult ShowDialog( XmlSettings aSettings, out string aFileName, out string aFilter )
        {
            RawStackImportDialog self = new RawStackImportDialog( aSettings );
            DialogResult result = self.ShowDialog();
            //
            if ( result == DialogResult.OK )
            {
                aFileName = self.iFileName;
                aFilter = self.iFilter;
            }
            else
            {
                aFileName = string.Empty;
                aFilter = string.Empty;
            }
            //
            return result;
        }
        #endregion

        #region Constructors
        public RawStackImportDialog( XmlSettings aSettings )
        {
            iSettings = aSettings;
            //
            InitializeComponent();
        }
        #endregion

        #region Event handlers
        private void iFBrowse_FileName_FileSelectionChanged( SymbianUtilsUi.Controls.SymbianFileControl aSelf, string aFileName )
        {

        }

        private void iBT_Cancel_Click( object sender, EventArgs e )
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void iBT_OK_Click( object sender, EventArgs e )
        {
            iFileName = iFBrowse_FileName.EntityName;
            iFilter = iTB_FilterText.Text;
            //
            DialogResult = DialogResult.OK;
            Close();
        }

        private void RawStackImportDialog_Load( object sender, EventArgs e )
        {
            iFBrowse_FileName.EntityName = iSettings[ "RawStackImportDialog", "iFBrowse_FileName" ];
            iSettings.Load( "RawStackImportDialog", iTB_FilterText );
        }

        private void RawStackImportDialog_FormClosed( object sender, FormClosedEventArgs e )
        {
            if ( DialogResult == DialogResult.OK )
            {
                iSettings[ "RawStackImportDialog", "iFBrowse_FileName" ] = iFBrowse_FileName.EntityName;
                iSettings.Save( "RawStackImportDialog", iTB_FilterText );
            }
        }
        #endregion

        #region Data members
        private readonly XmlSettings iSettings;
        private string iFilter = string.Empty;
        private string iFileName = string.Empty;
        #endregion
    }
}