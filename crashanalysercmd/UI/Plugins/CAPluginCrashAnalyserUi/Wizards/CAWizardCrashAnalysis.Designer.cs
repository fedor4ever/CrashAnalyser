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
﻿namespace CAPluginCrashAnalysisUi.Wizards
{
    partial class CAWizardCrashAnalysis
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
            this.iWizard = new SymbianWizardLib.GUI.SymWizard();
            this.iPG_SourceFiles = new SymbianWizardLib.GUI.SymWizardPage();
            this.iPG_SourceFiles_FileList = new CrashItemUiLib.Controls.CIEngineSourceManager();
            this.iPG_SourceFiles_Header = new SymbianWizardLib.GUI.SymWizardHeaderSection();
            this.iPG_Processing = new SymbianWizardLib.GUI.SymWizardPage();
            this.iPG_Processing_Panel = new System.Windows.Forms.Panel();
            this.iPG_Processing_SplitCon = new System.Windows.Forms.SplitContainer();
            this.iPG_Processing_SourceList = new CrashItemUiLib.Controls.CIEngineSourceList();
            this.iPG_Processing_CrashList = new CrashItemUiLib.Controls.CIEngineCrashList();
            this.iPG_Processing_Header = new SymbianWizardLib.GUI.SymWizardHeaderSection();
            this.iPG_DebugEngine = new SymbianWizardLib.GUI.SymWizardPage();
            this.iPG_DebugEngine_Control = new SymbianDebugLibUi.Controls.DebugEngineControl();
            this.iPG_DebugEngine_Header = new SymbianWizardLib.GUI.SymWizardHeaderSection();
            this.iPG_SourceFiles.SuspendLayout();
            this.iPG_Processing.SuspendLayout();
            this.iPG_Processing_Panel.SuspendLayout();
            this.iPG_Processing_SplitCon.Panel1.SuspendLayout();
            this.iPG_Processing_SplitCon.Panel2.SuspendLayout();
            this.iPG_Processing_SplitCon.SuspendLayout();
            this.iPG_DebugEngine.SuspendLayout();
            this.SuspendLayout();
            // 
            // iWizard
            // 
            this.iWizard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iWizard.Location = new System.Drawing.Point( 0, 0 );
            this.iWizard.Margin = new System.Windows.Forms.Padding( 0 );
            this.iWizard.Name = "iWizard";
            this.iWizard.Pages.AddRange( new SymbianWizardLib.GUI.SymWizardPage[] {
            this.iPG_SourceFiles,
            this.iPG_DebugEngine,
            this.iPG_Processing} );
            this.iWizard.Size = new System.Drawing.Size( 892, 673 );
            this.iWizard.TabIndex = 0;
            this.iWizard.WizardClosedFromFinish += new SymbianWizardLib.GUI.SymWizard.WizardClosedFromFinishHandler( this.iWizard_WizardClosedFromFinish );
            // 
            // iPG_SourceFiles
            // 
            this.iPG_SourceFiles.Controls.Add( this.iPG_SourceFiles_FileList );
            this.iPG_SourceFiles.Controls.Add( this.iPG_SourceFiles_Header );
            this.iPG_SourceFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iPG_SourceFiles.Location = new System.Drawing.Point( 0, 0 );
            this.iPG_SourceFiles.Margin = new System.Windows.Forms.Padding( 0 );
            this.iPG_SourceFiles.Name = "iPG_SourceFiles";
            this.iPG_SourceFiles.Size = new System.Drawing.Size( 892, 617 );
            this.iPG_SourceFiles.TabIndex = 3;
            this.iPG_SourceFiles.Load += new System.EventHandler( this.iPG_SourceFiles_Load );
            this.iPG_SourceFiles.PageClosedFromButtonNext += new SymbianWizardLib.GUI.SymWizardPage.PageClosedFromButtonNextHandler( this.iPG_SourceFiles_PageClosedFromButtonNext );
            // 
            // iPG_SourceFiles_FileList
            // 
            this.iPG_SourceFiles_FileList.DialogFilter = "";
            this.iPG_SourceFiles_FileList.DialogMultiselect = true;
            this.iPG_SourceFiles_FileList.DialogTitle = "Select Crash Files";
            this.iPG_SourceFiles_FileList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iPG_SourceFiles_FileList.FileName = "";
            this.iPG_SourceFiles_FileList.Font = new System.Drawing.Font( "Tahoma", 8.25F );
            this.iPG_SourceFiles_FileList.Location = new System.Drawing.Point( 0, 74 );
            this.iPG_SourceFiles_FileList.Name = "iPG_SourceFiles_FileList";
            this.iPG_SourceFiles_FileList.Size = new System.Drawing.Size( 892, 543 );
            this.iPG_SourceFiles_FileList.TabIndex = 5;
            this.iPG_SourceFiles_FileList.SourceListChanged += new CrashItemUiLib.Controls.CIEngineSourceManager.SourceListChangeHandler( this.iPG_SourceFiles_FileList_SourceListChanged );
            // 
            // iPG_SourceFiles_Header
            // 
            this.iPG_SourceFiles_Header.BackColor = System.Drawing.SystemColors.Window;
            this.iPG_SourceFiles_Header.Description = "Select a file or directory to analyse for crashes. You can also drag and drop fil" +
                "es onto this page.";
            this.iPG_SourceFiles_Header.Dock = System.Windows.Forms.DockStyle.Top;
            this.iPG_SourceFiles_Header.Image = null;
            this.iPG_SourceFiles_Header.Location = new System.Drawing.Point( 0, 0 );
            this.iPG_SourceFiles_Header.Name = "iPG_SourceFiles_Header";
            this.iPG_SourceFiles_Header.Size = new System.Drawing.Size( 892, 74 );
            this.iPG_SourceFiles_Header.TabIndex = 4;
            this.iPG_SourceFiles_Header.Title = "Crash File Analysis";
            // 
            // iPG_Processing
            // 
            this.iPG_Processing.Controls.Add( this.iPG_Processing_Panel );
            this.iPG_Processing.Controls.Add( this.iPG_Processing_Header );
            this.iPG_Processing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iPG_Processing.IsFinishingPage = true;
            this.iPG_Processing.Location = new System.Drawing.Point( 0, 0 );
            this.iPG_Processing.Margin = new System.Windows.Forms.Padding( 0 );
            this.iPG_Processing.Name = "iPG_Processing";
            this.iPG_Processing.Size = new System.Drawing.Size( 892, 617 );
            this.iPG_Processing.TabIndex = 3;
            this.iPG_Processing.Load += new System.EventHandler( this.iPG_Processing_Load );
            this.iPG_Processing.PageShownFromButtonNext += new SymbianWizardLib.GUI.SymWizardPage.PageShownFromButtonNextHandler( this.iPG_Processing_PageShownFromButtonNext );
            this.iPG_Processing.Unload += new System.EventHandler( this.iPG_Processing_Unload );
            // 
            // iPG_Processing_Panel
            // 
            this.iPG_Processing_Panel.Controls.Add( this.iPG_Processing_SplitCon );
            this.iPG_Processing_Panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iPG_Processing_Panel.Location = new System.Drawing.Point( 0, 74 );
            this.iPG_Processing_Panel.Name = "iPG_Processing_Panel";
            this.iPG_Processing_Panel.Padding = new System.Windows.Forms.Padding( 3 );
            this.iPG_Processing_Panel.Size = new System.Drawing.Size( 892, 543 );
            this.iPG_Processing_Panel.TabIndex = 43;
            // 
            // iPG_Processing_SplitCon
            // 
            this.iPG_Processing_SplitCon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iPG_Processing_SplitCon.Location = new System.Drawing.Point( 3, 3 );
            this.iPG_Processing_SplitCon.Margin = new System.Windows.Forms.Padding( 0 );
            this.iPG_Processing_SplitCon.Name = "iPG_Processing_SplitCon";
            this.iPG_Processing_SplitCon.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // iPG_Processing_SplitCon.Panel1
            // 
            this.iPG_Processing_SplitCon.Panel1.Controls.Add( this.iPG_Processing_SourceList );
            // 
            // iPG_Processing_SplitCon.Panel2
            // 
            this.iPG_Processing_SplitCon.Panel2.Controls.Add( this.iPG_Processing_CrashList );
            this.iPG_Processing_SplitCon.Size = new System.Drawing.Size( 886, 537 );
            this.iPG_Processing_SplitCon.SplitterDistance = 293;
            this.iPG_Processing_SplitCon.TabIndex = 0;
            // 
            // iPG_Processing_SourceList
            // 
            this.iPG_Processing_SourceList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iPG_Processing_SourceList.Location = new System.Drawing.Point( 0, 0 );
            this.iPG_Processing_SourceList.Margin = new System.Windows.Forms.Padding( 0 );
            this.iPG_Processing_SourceList.Name = "iPG_Processing_SourceList";
            this.iPG_Processing_SourceList.Size = new System.Drawing.Size( 886, 293 );
            this.iPG_Processing_SourceList.TabIndex = 0;
            // 
            // iPG_Processing_CrashList
            // 
            this.iPG_Processing_CrashList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iPG_Processing_CrashList.Engine = null;
            this.iPG_Processing_CrashList.Font = new System.Drawing.Font( "Tahoma", 8.25F );
            this.iPG_Processing_CrashList.Location = new System.Drawing.Point( 0, 0 );
            this.iPG_Processing_CrashList.Margin = new System.Windows.Forms.Padding( 0 );
            this.iPG_Processing_CrashList.Name = "iPG_Processing_CrashList";
            this.iPG_Processing_CrashList.Size = new System.Drawing.Size( 886, 240 );
            this.iPG_Processing_CrashList.TabIndex = 0;
            // 
            // iPG_Processing_Header
            // 
            this.iPG_Processing_Header.BackColor = System.Drawing.SystemColors.Window;
            this.iPG_Processing_Header.Description = "Shows a list of all identified crash files, as well as their analysis status.";
            this.iPG_Processing_Header.Dock = System.Windows.Forms.DockStyle.Top;
            this.iPG_Processing_Header.Image = null;
            this.iPG_Processing_Header.Location = new System.Drawing.Point( 0, 0 );
            this.iPG_Processing_Header.Name = "iPG_Processing_Header";
            this.iPG_Processing_Header.Size = new System.Drawing.Size( 892, 74 );
            this.iPG_Processing_Header.TabIndex = 42;
            this.iPG_Processing_Header.Title = "Crash Data Summary";
            // 
            // iPG_DebugEngine
            // 
            this.iPG_DebugEngine.Controls.Add( this.iPG_DebugEngine_Control );
            this.iPG_DebugEngine.Controls.Add( this.iPG_DebugEngine_Header );
            this.iPG_DebugEngine.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iPG_DebugEngine.Location = new System.Drawing.Point( 0, 0 );
            this.iPG_DebugEngine.Margin = new System.Windows.Forms.Padding( 0 );
            this.iPG_DebugEngine.Name = "iPG_DebugEngine";
            this.iPG_DebugEngine.Size = new System.Drawing.Size( 892, 617 );
            this.iPG_DebugEngine.TabIndex = 3;
            this.iPG_DebugEngine.Load += new System.EventHandler( this.iPG_DebugEngine_Load );
            this.iPG_DebugEngine.PageClosedFromButtonNext += new SymbianWizardLib.GUI.SymWizardPage.PageClosedFromButtonNextHandler( this.iPG_DebugEngine_PageClosedFromButtonNext );
            // 
            // iPG_DebugEngine_Control
            // 
            this.iPG_DebugEngine_Control.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iPG_DebugEngine_Control.Location = new System.Drawing.Point( 0, 74 );
            this.iPG_DebugEngine_Control.Margin = new System.Windows.Forms.Padding( 0 );
            this.iPG_DebugEngine_Control.Name = "iPG_DebugEngine_Control";
            this.iPG_DebugEngine_Control.Size = new System.Drawing.Size( 892, 543 );
            this.iPG_DebugEngine_Control.TabIndex = 42;
            // 
            // iPG_DebugEngine_Header
            // 
            this.iPG_DebugEngine_Header.BackColor = System.Drawing.SystemColors.Window;
            this.iPG_DebugEngine_Header.Description = "";
            this.iPG_DebugEngine_Header.Dock = System.Windows.Forms.DockStyle.Top;
            this.iPG_DebugEngine_Header.Image = null;
            this.iPG_DebugEngine_Header.Location = new System.Drawing.Point( 0, 0 );
            this.iPG_DebugEngine_Header.Name = "iPG_DebugEngine_Header";
            this.iPG_DebugEngine_Header.Size = new System.Drawing.Size( 892, 74 );
            this.iPG_DebugEngine_Header.TabIndex = 41;
            this.iPG_DebugEngine_Header.Title = "Symbolic / Debug Meta-Data";
            // 
            // CAWizardCrashAnalysis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 892, 673 );
            this.Controls.Add( this.iWizard );
            this.Font = new System.Drawing.Font( "Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte) ( 0 ) ) );
            this.MinimumSize = new System.Drawing.Size( 900, 700 );
            this.Name = "CAWizardCrashAnalysis";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Crash File Wizard";
            this.Load += new System.EventHandler( this.CAWizardCrashFile_Load );
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler( this.CAWizardCrashFile_FormClosing );
            this.iPG_SourceFiles.ResumeLayout( false );
            this.iPG_Processing.ResumeLayout( false );
            this.iPG_Processing_Panel.ResumeLayout( false );
            this.iPG_Processing_SplitCon.Panel1.ResumeLayout( false );
            this.iPG_Processing_SplitCon.Panel2.ResumeLayout( false );
            this.iPG_Processing_SplitCon.ResumeLayout( false );
            this.iPG_DebugEngine.ResumeLayout( false );
            this.ResumeLayout( false );

        }

        #endregion

        private SymbianWizardLib.GUI.SymWizard iWizard;
        private SymbianWizardLib.GUI.SymWizardPage iPG_SourceFiles;
        private SymbianWizardLib.GUI.SymWizardHeaderSection iPG_SourceFiles_Header;
        private CrashItemUiLib.Controls.CIEngineSourceManager iPG_SourceFiles_FileList;
        private SymbianWizardLib.GUI.SymWizardPage iPG_DebugEngine;
        private SymbianDebugLibUi.Controls.DebugEngineControl iPG_DebugEngine_Control;
        private SymbianWizardLib.GUI.SymWizardHeaderSection iPG_DebugEngine_Header;
        private SymbianWizardLib.GUI.SymWizardPage iPG_Processing;
        private SymbianWizardLib.GUI.SymWizardHeaderSection iPG_Processing_Header;
        private System.Windows.Forms.Panel iPG_Processing_Panel;
        private System.Windows.Forms.SplitContainer iPG_Processing_SplitCon;
        private CrashItemUiLib.Controls.CIEngineSourceList iPG_Processing_SourceList;
        private CrashItemUiLib.Controls.CIEngineCrashList iPG_Processing_CrashList;
    }
}