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
﻿namespace CAPluginNICDUi.Tabs
{
    partial class CATabNICDViewer
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
            this.iCtrl_CrashDebugger = new CrashDebuggerUiLib.CrashDebuggerControl();
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
            // iCtrl_CrashDebugger
            // 
            this.iCtrl_CrashDebugger.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iCtrl_CrashDebugger.Location = new System.Drawing.Point( 0, 0 );
            this.iCtrl_CrashDebugger.Name = "iCtrl_CrashDebugger";
            this.iCtrl_CrashDebugger.Size = new System.Drawing.Size( 907, 679 );
            this.iCtrl_CrashDebugger.TabIndex = 0;
            // 
            // CATabNICDViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.iCtrl_CrashDebugger );
            this.Name = "CATabNICDViewer";
            this.Size = new System.Drawing.Size( 907, 679 );
            this.Load += new System.EventHandler( this.CATabNICDViewer_Load );
            this.ResumeLayout( false );

        }

        #endregion

        private XPTable.Models.TextColumn iCol_FileName;
        private XPTable.Models.TextColumn iCol_CrashCount;
        private XPTable.Models.TextColumn iCol_SubRow_Line;
        private CrashDebuggerUiLib.CrashDebuggerControl iCtrl_CrashDebugger;
    }
}
