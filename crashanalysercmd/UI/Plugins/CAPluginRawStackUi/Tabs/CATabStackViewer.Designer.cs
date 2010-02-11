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
﻿namespace CAPluginRawStackUi.Tabs
{
    partial class CATabStackViewer
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

        #region Component Designer generated code

        // <summary> 
        // Required method for Designer support - do not modify 
        // the contents of this method with the code editor.
        // </summary>
        private void InitializeComponent()
        {
            this.iCol_FileName = new XPTable.Models.TextColumn();
            this.iCol_SubRow_Line = new XPTable.Models.TextColumn();
            this.iCol_CrashCount = new XPTable.Models.TextColumn();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.iCB_HideGhosts = new System.Windows.Forms.CheckBox();
            this.iCB_MatchExactSymbols = new System.Windows.Forms.CheckBox();
            this.iStackViewer = new SymbianStackLibUi.GUI.StackViewerControl();
            this.panel1 = new System.Windows.Forms.Panel();
            this.iLbl_ErrorMessage = new System.Windows.Forms.Label();
            this.eventLog1 = new System.Diagnostics.EventLog();
            this.groupBox10.SuspendLayout();
            this.panel1.SuspendLayout();
            ( (System.ComponentModel.ISupportInitialize) ( this.eventLog1 ) ).BeginInit();
            this.SuspendLayout();
            // 
            // iCol_FileName
            // 
            this.iCol_FileName.ContentWidth = 0;
            this.iCol_FileName.Width = 24;
            // 
            // iCol_SubRow_Line
            // 
            this.iCol_SubRow_Line.ContentWidth = 0;
            this.iCol_SubRow_Line.Text = "Line";
            this.iCol_SubRow_Line.Width = 40;
            // 
            // iCol_CrashCount
            // 
            this.iCol_CrashCount.ContentWidth = 0;
            this.iCol_CrashCount.Text = "Count";
            this.iCol_CrashCount.Width = 40;
            // 
            // groupBox10
            // 
            this.groupBox10.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.groupBox10.Controls.Add( this.iCB_HideGhosts );
            this.groupBox10.Controls.Add( this.iCB_MatchExactSymbols );
            this.groupBox10.Location = new System.Drawing.Point( 3, 479 );
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size( 297, 72 );
            this.groupBox10.TabIndex = 10;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Options";
            // 
            // iCB_HideGhosts
            // 
            this.iCB_HideGhosts.Location = new System.Drawing.Point( 143, 25 );
            this.iCB_HideGhosts.Name = "iCB_HideGhosts";
            this.iCB_HideGhosts.Size = new System.Drawing.Size( 136, 32 );
            this.iCB_HideGhosts.TabIndex = 0;
            this.iCB_HideGhosts.Text = "Hide irrelevant function calls";
            this.iCB_HideGhosts.CheckedChanged += new System.EventHandler( this.iCB_HideGhosts_CheckedChanged );
            // 
            // iCB_MatchExactSymbols
            // 
            this.iCB_MatchExactSymbols.Location = new System.Drawing.Point( 16, 25 );
            this.iCB_MatchExactSymbols.Name = "iCB_MatchExactSymbols";
            this.iCB_MatchExactSymbols.Size = new System.Drawing.Size( 136, 32 );
            this.iCB_MatchExactSymbols.TabIndex = 0;
            this.iCB_MatchExactSymbols.Text = "Show only exact Symbolic matches";
            this.iCB_MatchExactSymbols.CheckedChanged += new System.EventHandler( this.iCB_MatchExactSymbols_CheckedChanged );
            // 
            // iStackViewer
            // 
            this.iStackViewer.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.iStackViewer.BorderWidth = 1;
            this.iStackViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iStackViewer.Font = new System.Drawing.Font( "Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte) ( 0 ) ) );
            this.iStackViewer.HideGhosts = false;
            this.iStackViewer.Location = new System.Drawing.Point( 0, 0 );
            this.iStackViewer.Name = "iStackViewer";
            this.iStackViewer.OnlyShowEntriesWithSymbols = false;
            this.iStackViewer.Size = new System.Drawing.Size( 966, 477 );
            this.iStackViewer.TabIndex = 11;
            // 
            // panel1
            // 
            this.panel1.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                        | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.panel1.Controls.Add( this.iStackViewer );
            this.panel1.Location = new System.Drawing.Point( 3, 3 );
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size( 966, 477 );
            this.panel1.TabIndex = 12;
            // 
            // iLbl_ErrorMessage
            // 
            this.iLbl_ErrorMessage.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.iLbl_ErrorMessage.Font = new System.Drawing.Font( "Tahoma", 8.25F, System.Drawing.FontStyle.Bold );
            this.iLbl_ErrorMessage.ForeColor = System.Drawing.Color.Red;
            this.iLbl_ErrorMessage.Location = new System.Drawing.Point( 304, 489 );
            this.iLbl_ErrorMessage.Name = "iLbl_ErrorMessage";
            this.iLbl_ErrorMessage.Size = new System.Drawing.Size( 663, 59 );
            this.iLbl_ErrorMessage.TabIndex = 13;
            this.iLbl_ErrorMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.iLbl_ErrorMessage.Visible = false;
            // 
            // eventLog1
            // 
            this.eventLog1.SynchronizingObject = this;
            // 
            // CATabStackViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.iLbl_ErrorMessage );
            this.Controls.Add( this.panel1 );
            this.Controls.Add( this.groupBox10 );
            this.Font = new System.Drawing.Font( "Tahoma", 8.25F );
            this.Name = "CATabStackViewer";
            this.Size = new System.Drawing.Size( 972, 554 );
            this.Load += new System.EventHandler( this.CATabStackViewer_Load );
            this.groupBox10.ResumeLayout( false );
            this.panel1.ResumeLayout( false );
            ( (System.ComponentModel.ISupportInitialize) ( this.eventLog1 ) ).EndInit();
            this.ResumeLayout( false );

        }

        #endregion

        private XPTable.Models.TextColumn iCol_FileName;
        private XPTable.Models.TextColumn iCol_CrashCount;
        private XPTable.Models.TextColumn iCol_SubRow_Line;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.CheckBox iCB_HideGhosts;
        private System.Windows.Forms.CheckBox iCB_MatchExactSymbols;
        private SymbianStackLibUi.GUI.StackViewerControl iStackViewer;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label iLbl_ErrorMessage;
        private System.Diagnostics.EventLog eventLog1;
    }
}
