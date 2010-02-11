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
﻿namespace CAPluginNICDUi.Wizards
{
    partial class CAWizardNICD
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
            this.iPG_SourceFile = new SymbianWizardLib.GUI.SymWizardPage();
            this.label2 = new System.Windows.Forms.Label();
            this.iFB_NICD_Trace = new SymbianUtilsUi.Controls.SymbianFileBrowserControl();
            this.iPG_SourceFiles_Header = new SymbianWizardLib.GUI.SymWizardHeaderSection();
            this.iPG_DebugEngine = new SymbianWizardLib.GUI.SymWizardPage();
            this.iPG_DebugEngine_Control = new SymbianDebugLibUi.Controls.DebugEngineControl();
            this.iPG_DebugEngine_Header = new SymbianWizardLib.GUI.SymWizardHeaderSection();
            this.iPG_SourceFile.SuspendLayout();
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
            this.iPG_SourceFile,
            this.iPG_DebugEngine} );
            this.iWizard.Size = new System.Drawing.Size( 892, 673 );
            this.iWizard.TabIndex = 0;
            this.iWizard.WizardClosedFromFinish += new SymbianWizardLib.GUI.SymWizard.WizardClosedFromFinishHandler( this.iWizard_WizardClosedFromFinish );
            // 
            // iPG_SourceFile
            // 
            this.iPG_SourceFile.Controls.Add( this.label2 );
            this.iPG_SourceFile.Controls.Add( this.iFB_NICD_Trace );
            this.iPG_SourceFile.Controls.Add( this.iPG_SourceFiles_Header );
            this.iPG_SourceFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iPG_SourceFile.IsFinishingPage = false;
            this.iPG_SourceFile.Location = new System.Drawing.Point( 0, 0 );
            this.iPG_SourceFile.Margin = new System.Windows.Forms.Padding( 0 );
            this.iPG_SourceFile.Name = "iPG_SourceFile";
            this.iPG_SourceFile.Size = new System.Drawing.Size( 892, 617 );
            this.iPG_SourceFile.TabIndex = 3;
            this.iPG_SourceFile.Wizard = this.iWizard;
            this.iPG_SourceFile.Load += new System.EventHandler( this.iPG_SourceFile_Load );
            this.iPG_SourceFile.PageClosedFromButtonNext += new SymbianWizardLib.GUI.SymWizardPage.PageClosedFromButtonNextHandler( this.iPG_SourceFile_PageClosedFromButtonNext );
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point( 12, 101 );
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size( 67, 13 );
            this.label2.TabIndex = 6;
            this.label2.Text = "NICD Trace:";
            // 
            // iFB_NICD_Trace
            // 
            this.iFB_NICD_Trace.DialogFilter = "";
            this.iFB_NICD_Trace.DialogTitle = "";
            this.iFB_NICD_Trace.EntityMustExist = true;
            this.iFB_NICD_Trace.EntityName = "";
            this.iFB_NICD_Trace.Location = new System.Drawing.Point( 88, 97 );
            this.iFB_NICD_Trace.Margin = new System.Windows.Forms.Padding( 0 );
            this.iFB_NICD_Trace.MinimumSize = new System.Drawing.Size( 396, 21 );
            this.iFB_NICD_Trace.Name = "iFB_NICD_Trace";
            this.iFB_NICD_Trace.ShowDriveLetter = true;
            this.iFB_NICD_Trace.Size = new System.Drawing.Size( 781, 21 );
            this.iFB_NICD_Trace.TabIndex = 5;
            this.iFB_NICD_Trace.TextInputEnabled = true;
            // 
            // iPG_SourceFiles_Header
            // 
            this.iPG_SourceFiles_Header.BackColor = System.Drawing.SystemColors.Window;
            this.iPG_SourceFiles_Header.Description = "Select an NICD trace file for analysis. You can also drag and drop a file onto th" +
                "e edit box.";
            this.iPG_SourceFiles_Header.Dock = System.Windows.Forms.DockStyle.Top;
            this.iPG_SourceFiles_Header.Image = null;
            this.iPG_SourceFiles_Header.Location = new System.Drawing.Point( 0, 0 );
            this.iPG_SourceFiles_Header.Name = "iPG_SourceFiles_Header";
            this.iPG_SourceFiles_Header.Size = new System.Drawing.Size( 892, 74 );
            this.iPG_SourceFiles_Header.TabIndex = 4;
            this.iPG_SourceFiles_Header.Title = "NICD Analysis";
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
            // CAWizardNICD
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 892, 673 );
            this.Controls.Add( this.iWizard );
            this.Font = new System.Drawing.Font( "Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte) ( 0 ) ) );
            this.MinimumSize = new System.Drawing.Size( 900, 700 );
            this.Name = "CAWizardNICD";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Non Interactive Crash Debugger Analysis Wizard";
            this.iPG_SourceFile.ResumeLayout( false );
            this.iPG_SourceFile.PerformLayout();
            this.iPG_DebugEngine.ResumeLayout( false );
            this.ResumeLayout( false );

        }

        #endregion

        private SymbianWizardLib.GUI.SymWizard iWizard;
        private SymbianWizardLib.GUI.SymWizardPage iPG_SourceFile;
        private SymbianWizardLib.GUI.SymWizardHeaderSection iPG_SourceFiles_Header;
        private SymbianWizardLib.GUI.SymWizardPage iPG_DebugEngine;
        private SymbianDebugLibUi.Controls.DebugEngineControl iPG_DebugEngine_Control;
        private SymbianWizardLib.GUI.SymWizardHeaderSection iPG_DebugEngine_Header;
        private SymbianUtilsUi.Controls.SymbianFileBrowserControl iFB_NICD_Trace;
        private System.Windows.Forms.Label label2;
    }
}