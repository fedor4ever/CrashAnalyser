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
﻿namespace CAPluginCrashAnalysisUi.Tabs
{
    partial class CATabCrashContainerExplorer
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
            this.iCtrl_Container = new CrashItemUiLib.Crash.Container.CICtrlContainer();
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
            // iCtrl_Container
            // 
            this.iCtrl_Container.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iCtrl_Container.Location = new System.Drawing.Point( 0, 0 );
            this.iCtrl_Container.Name = "iCtrl_Container";
            this.iCtrl_Container.Size = new System.Drawing.Size( 907, 679 );
            this.iCtrl_Container.TabIndex = 0;
            // 
            // CATabCrashContainerExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.iCtrl_Container );
            this.Name = "CATabCrashContainerExplorer";
            this.Size = new System.Drawing.Size( 907, 679 );
            this.Load += new System.EventHandler( this.CATabCrashContainerExplorer_Load );
            this.ResumeLayout( false );

        }

        #endregion

        private XPTable.Models.TextColumn iCol_FileName;
        private XPTable.Models.TextColumn iCol_CrashCount;
        private XPTable.Models.TextColumn iCol_SubRow_Line;
        private CrashItemUiLib.Crash.Container.CICtrlContainer iCtrl_Container;
    }
}
