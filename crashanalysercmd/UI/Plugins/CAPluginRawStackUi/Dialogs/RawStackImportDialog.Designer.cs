/*
* Copyright (c) 2009 Nokia Corporation and/or its subsidiary(-ies).
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
﻿namespace CAPluginRawStackUi.Dialogs
{
    partial class RawStackImportDialog
    {
        // <summary>
        // Required designer variable.
        // </summary>
        private System.ComponentModel.IContainer components = null;

        // <summary>
        // Clean up any resources being used.
        // </summary>
        // <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        // <summary>
        // Required method for Designer support - do not modify
        // the contents of this method with the code editor.
        // </summary>
        private void InitializeComponent()
        {
            this.iFBrowse_FileName = new SymbianUtilsUi.Controls.SymbianFileBrowserControl();
            this.label1 = new System.Windows.Forms.Label();
            this.iTB_FilterText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.iBT_Cancel = new System.Windows.Forms.Button();
            this.iBT_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // iFBrowse_FileName
            // 
            this.iFBrowse_FileName.DialogFilter = "";
            this.iFBrowse_FileName.DialogTitle = "";
            this.iFBrowse_FileName.EntityName = "";
            this.iFBrowse_FileName.Location = new System.Drawing.Point( 78, 11 );
            this.iFBrowse_FileName.Margin = new System.Windows.Forms.Padding( 0 );
            this.iFBrowse_FileName.MinimumSize = new System.Drawing.Size( 396, 21 );
            this.iFBrowse_FileName.Name = "iFBrowse_FileName";
            this.iFBrowse_FileName.Size = new System.Drawing.Size( 396, 21 );
            this.iFBrowse_FileName.TabIndex = 0;
            this.iFBrowse_FileName.FileSelectionChanged += new SymbianUtilsUi.Controls.SymbianFileControl.FileSelectionChangedHandler( this.iFBrowse_FileName_FileSelectionChanged );
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point( 12, 15 );
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size( 55, 13 );
            this.label1.TabIndex = 1;
            this.label1.Text = "File name:";
            // 
            // iTB_FilterText
            // 
            this.iTB_FilterText.Location = new System.Drawing.Point( 78, 41 );
            this.iTB_FilterText.Name = "iTB_FilterText";
            this.iTB_FilterText.Size = new System.Drawing.Size( 344, 20 );
            this.iTB_FilterText.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point( 12, 44 );
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size( 60, 13 );
            this.label2.TabIndex = 1;
            this.label2.Text = "Filter prefix:";
            // 
            // iBT_Cancel
            // 
            this.iBT_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.iBT_Cancel.Location = new System.Drawing.Point( 166, 77 );
            this.iBT_Cancel.Name = "iBT_Cancel";
            this.iBT_Cancel.Size = new System.Drawing.Size( 75, 23 );
            this.iBT_Cancel.TabIndex = 3;
            this.iBT_Cancel.Text = "Cancel";
            this.iBT_Cancel.UseVisualStyleBackColor = true;
            this.iBT_Cancel.Click += new System.EventHandler( this.iBT_Cancel_Click );
            // 
            // iBT_OK
            // 
            this.iBT_OK.Location = new System.Drawing.Point( 247, 77 );
            this.iBT_OK.Name = "iBT_OK";
            this.iBT_OK.Size = new System.Drawing.Size( 75, 23 );
            this.iBT_OK.TabIndex = 3;
            this.iBT_OK.Text = "OK";
            this.iBT_OK.UseVisualStyleBackColor = true;
            this.iBT_OK.Click += new System.EventHandler( this.iBT_OK_Click );
            // 
            // RawStackImportDialog
            // 
            this.AcceptButton = this.iBT_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.iBT_Cancel;
            this.ClientSize = new System.Drawing.Size( 489, 113 );
            this.ControlBox = false;
            this.Controls.Add( this.iBT_OK );
            this.Controls.Add( this.iBT_Cancel );
            this.Controls.Add( this.iTB_FilterText );
            this.Controls.Add( this.label2 );
            this.Controls.Add( this.label1 );
            this.Controls.Add( this.iFBrowse_FileName );
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "RawStackImportDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Import Text File...";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler( this.RawStackImportDialog_FormClosed );
            this.Load += new System.EventHandler( this.RawStackImportDialog_Load );
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private SymbianUtilsUi.Controls.SymbianFileBrowserControl iFBrowse_FileName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox iTB_FilterText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button iBT_Cancel;
        private System.Windows.Forms.Button iBT_OK;
    }
}