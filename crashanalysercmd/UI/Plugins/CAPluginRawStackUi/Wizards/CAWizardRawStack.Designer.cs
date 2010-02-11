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
﻿namespace CAPluginRawStackUi.Wizards
{
    partial class CAWizardRawStack
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
            this.iPG_SourceData = new SymbianWizardLib.GUI.SymWizardPage();
            this.iTabControl_Options = new System.Windows.Forms.TabControl();
            this.iTabCtrl_Page_Options = new System.Windows.Forms.TabPage();
            this.iTB_CurrentStackPointerAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.iCB_MatchExactSymbols = new System.Windows.Forms.CheckBox();
            this.iTabCtrl_Page_Prefixes = new System.Windows.Forms.TabPage();
            this.iTB_PrefixCurrentStackPointer = new System.Windows.Forms.TextBox();
            this.label136 = new System.Windows.Forms.Label();
            this.iTB_PrefixCodeSegment = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.iTabCtrl_Page_Import = new System.Windows.Forms.TabPage();
            this.iBT_LoadTextDataFromFile = new System.Windows.Forms.Button();
            this.iStackDataWriteOutputToFileButton = new System.Windows.Forms.Button();
            this.iStackDataSaveOutputAsButton = new System.Windows.Forms.Button();
            this.iBT_LoadBinaryDataFromFile = new System.Windows.Forms.Button();
            this.iTB_RawStackData = new System.Windows.Forms.TextBox();
            this.iPG_SourceFiles_Header = new SymbianWizardLib.GUI.SymWizardHeaderSection();
            this.iPG_DebugEngine = new SymbianWizardLib.GUI.SymWizardPage();
            this.iPG_DebugEngine_Control = new SymbianDebugLibUi.Controls.DebugEngineControl();
            this.iPG_DebugEngine_Header = new SymbianWizardLib.GUI.SymWizardHeaderSection();
            this.iPG_SourceData.SuspendLayout();
            this.iTabControl_Options.SuspendLayout();
            this.iTabCtrl_Page_Options.SuspendLayout();
            this.iTabCtrl_Page_Prefixes.SuspendLayout();
            this.iTabCtrl_Page_Import.SuspendLayout();
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
            this.iPG_SourceData,
            this.iPG_DebugEngine} );
            this.iWizard.Size = new System.Drawing.Size( 892, 673 );
            this.iWizard.TabIndex = 0;
            this.iWizard.WizardClosedFromFinish += new SymbianWizardLib.GUI.SymWizard.WizardClosedFromFinishHandler( this.iWizard_WizardClosedFromFinish );
            // 
            // iPG_SourceData
            // 
            this.iPG_SourceData.Controls.Add( this.iTabControl_Options );
            this.iPG_SourceData.Controls.Add( this.iTB_RawStackData );
            this.iPG_SourceData.Controls.Add( this.iPG_SourceFiles_Header );
            this.iPG_SourceData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iPG_SourceData.IsFinishingPage = false;
            this.iPG_SourceData.Location = new System.Drawing.Point( 0, 0 );
            this.iPG_SourceData.Margin = new System.Windows.Forms.Padding( 0 );
            this.iPG_SourceData.Name = "iPG_SourceData";
            this.iPG_SourceData.Size = new System.Drawing.Size( 892, 617 );
            this.iPG_SourceData.TabIndex = 3;
            this.iPG_SourceData.Wizard = this.iWizard;
            this.iPG_SourceData.Load += new System.EventHandler( this.iPG_SourceData_Load );
            this.iPG_SourceData.PageClosedFromButtonNext += new SymbianWizardLib.GUI.SymWizardPage.PageClosedFromButtonNextHandler( this.iPG_SourceData_PageClosedFromButtonNext );
            // 
            // iTabControl_Options
            // 
            this.iTabControl_Options.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.iTabControl_Options.Controls.Add( this.iTabCtrl_Page_Options );
            this.iTabControl_Options.Controls.Add( this.iTabCtrl_Page_Prefixes );
            this.iTabControl_Options.Controls.Add( this.iTabCtrl_Page_Import );
            this.iTabControl_Options.Location = new System.Drawing.Point( 12, 502 );
            this.iTabControl_Options.Name = "iTabControl_Options";
            this.iTabControl_Options.SelectedIndex = 0;
            this.iTabControl_Options.Size = new System.Drawing.Size( 864, 112 );
            this.iTabControl_Options.TabIndex = 39;
            // 
            // iTabCtrl_Page_Options
            // 
            this.iTabCtrl_Page_Options.Controls.Add( this.iTB_CurrentStackPointerAddress );
            this.iTabCtrl_Page_Options.Controls.Add( this.label1 );
            this.iTabCtrl_Page_Options.Controls.Add( this.label13 );
            this.iTabCtrl_Page_Options.Controls.Add( this.iCB_MatchExactSymbols );
            this.iTabCtrl_Page_Options.Location = new System.Drawing.Point( 4, 22 );
            this.iTabCtrl_Page_Options.Name = "iTabCtrl_Page_Options";
            this.iTabCtrl_Page_Options.Size = new System.Drawing.Size( 856, 86 );
            this.iTabCtrl_Page_Options.TabIndex = 2;
            this.iTabCtrl_Page_Options.Text = "Options";
            // 
            // iTB_CurrentStackPointerAddress
            // 
            this.iTB_CurrentStackPointerAddress.Location = new System.Drawing.Point( 144, 12 );
            this.iTB_CurrentStackPointerAddress.Name = "iTB_CurrentStackPointerAddress";
            this.iTB_CurrentStackPointerAddress.Size = new System.Drawing.Size( 200, 21 );
            this.iTB_CurrentStackPointerAddress.TabIndex = 23;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point( 8, 16 );
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size( 104, 32 );
            this.label1.TabIndex = 22;
            this.label1.Text = "Current stack pointer address:";
            // 
            // label13
            // 
            this.label13.Location = new System.Drawing.Point( 8, 53 );
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size( 120, 13 );
            this.label13.TabIndex = 21;
            this.label13.Text = "Match exact symbols:";
            // 
            // iCB_MatchExactSymbols
            // 
            this.iCB_MatchExactSymbols.Location = new System.Drawing.Point( 144, 48 );
            this.iCB_MatchExactSymbols.Name = "iCB_MatchExactSymbols";
            this.iCB_MatchExactSymbols.Size = new System.Drawing.Size( 14, 22 );
            this.iCB_MatchExactSymbols.TabIndex = 20;
            // 
            // iTabCtrl_Page_Prefixes
            // 
            this.iTabCtrl_Page_Prefixes.Controls.Add( this.iTB_PrefixCurrentStackPointer );
            this.iTabCtrl_Page_Prefixes.Controls.Add( this.label136 );
            this.iTabCtrl_Page_Prefixes.Controls.Add( this.iTB_PrefixCodeSegment );
            this.iTabCtrl_Page_Prefixes.Controls.Add( this.label3 );
            this.iTabCtrl_Page_Prefixes.Location = new System.Drawing.Point( 4, 22 );
            this.iTabCtrl_Page_Prefixes.Name = "iTabCtrl_Page_Prefixes";
            this.iTabCtrl_Page_Prefixes.Size = new System.Drawing.Size( 856, 86 );
            this.iTabCtrl_Page_Prefixes.TabIndex = 3;
            this.iTabCtrl_Page_Prefixes.Text = "Prefixes";
            this.iTabCtrl_Page_Prefixes.Visible = false;
            // 
            // iTB_PrefixCurrentStackPointer
            // 
            this.iTB_PrefixCurrentStackPointer.Location = new System.Drawing.Point( 144, 48 );
            this.iTB_PrefixCurrentStackPointer.Name = "iTB_PrefixCurrentStackPointer";
            this.iTB_PrefixCurrentStackPointer.Size = new System.Drawing.Size( 200, 21 );
            this.iTB_PrefixCurrentStackPointer.TabIndex = 42;
            this.iTB_PrefixCurrentStackPointer.Text = "CurrentSP - ";
            // 
            // label136
            // 
            this.label136.Location = new System.Drawing.Point( 8, 52 );
            this.label136.Name = "label136";
            this.label136.Size = new System.Drawing.Size( 120, 13 );
            this.label136.TabIndex = 41;
            this.label136.Text = "Current stack pointer:";
            // 
            // iTB_PrefixCodeSegment
            // 
            this.iTB_PrefixCodeSegment.Location = new System.Drawing.Point( 144, 12 );
            this.iTB_PrefixCodeSegment.Name = "iTB_PrefixCodeSegment";
            this.iTB_PrefixCodeSegment.Size = new System.Drawing.Size( 200, 21 );
            this.iTB_PrefixCodeSegment.TabIndex = 42;
            this.iTB_PrefixCodeSegment.Text = "CodeSeg - ";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point( 8, 16 );
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size( 120, 13 );
            this.label3.TabIndex = 41;
            this.label3.Text = "Code segment name:";
            // 
            // iTabCtrl_Page_Import
            // 
            this.iTabCtrl_Page_Import.Controls.Add( this.iBT_LoadTextDataFromFile );
            this.iTabCtrl_Page_Import.Controls.Add( this.iStackDataWriteOutputToFileButton );
            this.iTabCtrl_Page_Import.Controls.Add( this.iStackDataSaveOutputAsButton );
            this.iTabCtrl_Page_Import.Controls.Add( this.iBT_LoadBinaryDataFromFile );
            this.iTabCtrl_Page_Import.Location = new System.Drawing.Point( 4, 22 );
            this.iTabCtrl_Page_Import.Name = "iTabCtrl_Page_Import";
            this.iTabCtrl_Page_Import.Size = new System.Drawing.Size( 856, 86 );
            this.iTabCtrl_Page_Import.TabIndex = 1;
            this.iTabCtrl_Page_Import.Text = "Import...";
            this.iTabCtrl_Page_Import.Visible = false;
            // 
            // iBT_LoadTextDataFromFile
            // 
            this.iBT_LoadTextDataFromFile.Location = new System.Drawing.Point( 8, 13 );
            this.iBT_LoadTextDataFromFile.Name = "iBT_LoadTextDataFromFile";
            this.iBT_LoadTextDataFromFile.Size = new System.Drawing.Size( 232, 24 );
            this.iBT_LoadTextDataFromFile.TabIndex = 48;
            this.iBT_LoadTextDataFromFile.Text = "Load text data from file...";
            this.iBT_LoadTextDataFromFile.Click += new System.EventHandler( this.iBT_LoadTextDataFromFile_Click );
            // 
            // iStackDataWriteOutputToFileButton
            // 
            this.iStackDataWriteOutputToFileButton.Location = new System.Drawing.Point( 8, 88 );
            this.iStackDataWriteOutputToFileButton.Name = "iStackDataWriteOutputToFileButton";
            this.iStackDataWriteOutputToFileButton.Size = new System.Drawing.Size( 912, 32 );
            this.iStackDataWriteOutputToFileButton.TabIndex = 47;
            this.iStackDataWriteOutputToFileButton.Text = "Write Call Stack to File...";
            // 
            // iStackDataSaveOutputAsButton
            // 
            this.iStackDataSaveOutputAsButton.Location = new System.Drawing.Point( 895, 44 );
            this.iStackDataSaveOutputAsButton.Name = "iStackDataSaveOutputAsButton";
            this.iStackDataSaveOutputAsButton.Size = new System.Drawing.Size( 25, 20 );
            this.iStackDataSaveOutputAsButton.TabIndex = 44;
            this.iStackDataSaveOutputAsButton.Text = "...";
            // 
            // iBT_LoadBinaryDataFromFile
            // 
            this.iBT_LoadBinaryDataFromFile.Location = new System.Drawing.Point( 8, 47 );
            this.iBT_LoadBinaryDataFromFile.Name = "iBT_LoadBinaryDataFromFile";
            this.iBT_LoadBinaryDataFromFile.Size = new System.Drawing.Size( 232, 24 );
            this.iBT_LoadBinaryDataFromFile.TabIndex = 48;
            this.iBT_LoadBinaryDataFromFile.Text = "Load binary data from file...";
            this.iBT_LoadBinaryDataFromFile.Click += new System.EventHandler( this.iBT_LoadBinaryDataFromFile_Click );
            // 
            // iTB_RawStackData
            // 
            this.iTB_RawStackData.AcceptsReturn = true;
            this.iTB_RawStackData.AcceptsTab = true;
            this.iTB_RawStackData.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.iTB_RawStackData.Font = new System.Drawing.Font( "Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte) ( 0 ) ) );
            this.iTB_RawStackData.Location = new System.Drawing.Point( 12, 80 );
            this.iTB_RawStackData.MaxLength = 0;
            this.iTB_RawStackData.Multiline = true;
            this.iTB_RawStackData.Name = "iTB_RawStackData";
            this.iTB_RawStackData.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.iTB_RawStackData.Size = new System.Drawing.Size( 864, 413 );
            this.iTB_RawStackData.TabIndex = 38;
            this.iTB_RawStackData.WordWrap = false;
            this.iTB_RawStackData.KeyDown += new System.Windows.Forms.KeyEventHandler( this.iTB_RawStackData_KeyDown );
            // 
            // iPG_SourceFiles_Header
            // 
            this.iPG_SourceFiles_Header.BackColor = System.Drawing.SystemColors.Window;
            this.iPG_SourceFiles_Header.Description = "Paste in the raw stack data to analyse or import from a file";
            this.iPG_SourceFiles_Header.Dock = System.Windows.Forms.DockStyle.Top;
            this.iPG_SourceFiles_Header.Image = null;
            this.iPG_SourceFiles_Header.Location = new System.Drawing.Point( 0, 0 );
            this.iPG_SourceFiles_Header.Name = "iPG_SourceFiles_Header";
            this.iPG_SourceFiles_Header.Size = new System.Drawing.Size( 892, 74 );
            this.iPG_SourceFiles_Header.TabIndex = 4;
            this.iPG_SourceFiles_Header.Title = "Call Stack Reconstructor Data";
            // 
            // iPG_DebugEngine
            // 
            this.iPG_DebugEngine.Controls.Add( this.iPG_DebugEngine_Control );
            this.iPG_DebugEngine.Controls.Add( this.iPG_DebugEngine_Header );
            this.iPG_DebugEngine.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iPG_DebugEngine.IsFinishingPage = true;
            this.iPG_DebugEngine.Location = new System.Drawing.Point( 0, 0 );
            this.iPG_DebugEngine.Margin = new System.Windows.Forms.Padding( 0 );
            this.iPG_DebugEngine.Name = "iPG_DebugEngine";
            this.iPG_DebugEngine.Size = new System.Drawing.Size( 892, 617 );
            this.iPG_DebugEngine.TabIndex = 3;
            this.iPG_DebugEngine.Wizard = this.iWizard;
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
            // CAWizardRawStack
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 892, 673 );
            this.Controls.Add( this.iWizard );
            this.Font = new System.Drawing.Font( "Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte) ( 0 ) ) );
            this.MinimumSize = new System.Drawing.Size( 900, 700 );
            this.Name = "CAWizardRawStack";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Call Stack Reconstructor Wizard";
            this.Load += new System.EventHandler( this.CAWizardRawStack_Load );
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler( this.CAWizardRawStack_FormClosing );
            this.iPG_SourceData.ResumeLayout( false );
            this.iPG_SourceData.PerformLayout();
            this.iTabControl_Options.ResumeLayout( false );
            this.iTabCtrl_Page_Options.ResumeLayout( false );
            this.iTabCtrl_Page_Options.PerformLayout();
            this.iTabCtrl_Page_Prefixes.ResumeLayout( false );
            this.iTabCtrl_Page_Prefixes.PerformLayout();
            this.iTabCtrl_Page_Import.ResumeLayout( false );
            this.iPG_DebugEngine.ResumeLayout( false );
            this.ResumeLayout( false );

        }

        #endregion

        private SymbianWizardLib.GUI.SymWizard iWizard;
        private SymbianWizardLib.GUI.SymWizardPage iPG_SourceData;
        private SymbianWizardLib.GUI.SymWizardHeaderSection iPG_SourceFiles_Header;
        private SymbianWizardLib.GUI.SymWizardPage iPG_DebugEngine;
        private SymbianDebugLibUi.Controls.DebugEngineControl iPG_DebugEngine_Control;
        private SymbianWizardLib.GUI.SymWizardHeaderSection iPG_DebugEngine_Header;
        private System.Windows.Forms.TabControl iTabControl_Options;
        private System.Windows.Forms.TabPage iTabCtrl_Page_Options;
        private System.Windows.Forms.TextBox iTB_CurrentStackPointerAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox iCB_MatchExactSymbols;
        private System.Windows.Forms.TabPage iTabCtrl_Page_Prefixes;
        private System.Windows.Forms.TextBox iTB_PrefixCurrentStackPointer;
        private System.Windows.Forms.Label label136;
        private System.Windows.Forms.TextBox iTB_PrefixCodeSegment;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage iTabCtrl_Page_Import;
        private System.Windows.Forms.Button iBT_LoadTextDataFromFile;
        private System.Windows.Forms.Button iStackDataWriteOutputToFileButton;
        private System.Windows.Forms.Button iStackDataSaveOutputAsButton;
        private System.Windows.Forms.Button iBT_LoadBinaryDataFromFile;
        private System.Windows.Forms.TextBox iTB_RawStackData;
    }
}