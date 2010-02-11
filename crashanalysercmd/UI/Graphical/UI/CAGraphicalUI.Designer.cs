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
﻿namespace CrashAnalyser.UI
{
    partial class CAGraphicalUI
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
            this.iMenu = new System.Windows.Forms.MenuStrip();
            this.iMenu_File = new System.Windows.Forms.ToolStripMenuItem();
            this.iMenu_File_New = new System.Windows.Forms.ToolStripMenuItem();
            this.iMenu_File_SaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.iMenu_File_ExitSep = new System.Windows.Forms.ToolStripSeparator();
            this.iMenu_File_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.iMenu_Help = new System.Windows.Forms.ToolStripMenuItem();
            this.iMenu_Help_About = new System.Windows.Forms.ToolStripMenuItem();
            this.iLbl_Title = new System.Windows.Forms.Label();
            this.iLbl_Copyright = new System.Windows.Forms.Label();
            this.iTabStrip = new SymbianTabStripLib.Tabs.TabStrip();
            this.iTabStripPageManager = new SymbianTabStripLib.Pages.TabStripPageManager();
            this.iMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // iMenu
            // 
            this.iMenu.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.iMenu_File,
            this.iMenu_Help} );
            this.iMenu.Location = new System.Drawing.Point( 0, 0 );
            this.iMenu.Name = "iMenu";
            this.iMenu.Size = new System.Drawing.Size( 792, 24 );
            this.iMenu.TabIndex = 0;
            this.iMenu.Text = "Main Menu";
            // 
            // iMenu_File
            // 
            this.iMenu_File.DropDownItems.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.iMenu_File_New,
            this.iMenu_File_SaveAs,
            this.iMenu_File_ExitSep,
            this.iMenu_File_Exit} );
            this.iMenu_File.Name = "iMenu_File";
            this.iMenu_File.Size = new System.Drawing.Size( 35, 20 );
            this.iMenu_File.Text = "&File";
            this.iMenu_File.DropDownOpening += new System.EventHandler( this.iMenu_File_DropDownOpening );
            // 
            // iMenu_File_New
            // 
            this.iMenu_File_New.Name = "iMenu_File_New";
            this.iMenu_File_New.ShortcutKeyDisplayString = "";
            this.iMenu_File_New.Size = new System.Drawing.Size( 136, 22 );
            this.iMenu_File_New.Text = "&New...";
            // 
            // iMenu_File_SaveAs
            // 
            this.iMenu_File_SaveAs.Name = "iMenu_File_SaveAs";
            this.iMenu_File_SaveAs.Size = new System.Drawing.Size( 136, 22 );
            this.iMenu_File_SaveAs.Text = "Save &As...";
            // 
            // iMenu_File_ExitSep
            // 
            this.iMenu_File_ExitSep.Name = "iMenu_File_ExitSep";
            this.iMenu_File_ExitSep.Size = new System.Drawing.Size( 133, 6 );
            // 
            // iMenu_File_Exit
            // 
            this.iMenu_File_Exit.Name = "iMenu_File_Exit";
            this.iMenu_File_Exit.Size = new System.Drawing.Size( 136, 22 );
            this.iMenu_File_Exit.Text = "E&xit";
            this.iMenu_File_Exit.Click += new System.EventHandler( this.iMenu_File_Exit_Click );
            // 
            // iMenu_Help
            // 
            this.iMenu_Help.DropDownItems.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.iMenu_Help_About} );
            this.iMenu_Help.Name = "iMenu_Help";
            this.iMenu_Help.Size = new System.Drawing.Size( 40, 20 );
            this.iMenu_Help.Text = "&Help";
            // 
            // iMenu_Help_About
            // 
            this.iMenu_Help_About.Name = "iMenu_Help_About";
            this.iMenu_Help_About.Size = new System.Drawing.Size( 114, 22 );
            this.iMenu_Help_About.Text = "About";
            // 
            // iLbl_Title
            // 
            this.iLbl_Title.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.iLbl_Title.AutoSize = true;
            this.iLbl_Title.BackColor = System.Drawing.Color.Transparent;
            this.iLbl_Title.Font = new System.Drawing.Font( "Tahoma", 8.25F, System.Drawing.FontStyle.Bold );
            this.iLbl_Title.ForeColor = System.Drawing.Color.White;
            this.iLbl_Title.Location = new System.Drawing.Point( 645, 535 );
            this.iLbl_Title.Name = "iLbl_Title";
            this.iLbl_Title.Size = new System.Drawing.Size( 92, 13 );
            this.iLbl_Title.TabIndex = 2;
            this.iLbl_Title.Text = "Crash Analyser";
            // 
            // iLbl_Copyright
            // 
            this.iLbl_Copyright.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.iLbl_Copyright.AutoSize = true;
            this.iLbl_Copyright.BackColor = System.Drawing.Color.Transparent;
            this.iLbl_Copyright.Font = new System.Drawing.Font( "Tahoma", 8.25F );
            this.iLbl_Copyright.ForeColor = System.Drawing.Color.White;
            this.iLbl_Copyright.Location = new System.Drawing.Point( 645, 551 );
            this.iLbl_Copyright.Name = "iLbl_Copyright";
            this.iLbl_Copyright.Size = new System.Drawing.Size( 136, 13 );
            this.iLbl_Copyright.TabIndex = 2;
            this.iLbl_Copyright.Text = "(c) Nokia Corporation 2009";
            // 
            // iTabStrip
            // 
            this.iTabStrip.Location = new System.Drawing.Point( 0, 24 );
            this.iTabStrip.Name = "iTabStrip";
            this.iTabStrip.PageManager = this.iTabStripPageManager;
            this.iTabStrip.Size = new System.Drawing.Size( 792, 19 );
            this.iTabStrip.TabIndex = 8;
            this.iTabStrip.Text = "iTabStrip";
            this.iTabStrip.TabCloseRequestReceiver += new SymbianTabStripLib.Tabs.TabStrip.TabHandler( this.TabStrip_TabCloseRequestReceiver );
            // 
            // iTabStripPageManager
            // 
            this.iTabStripPageManager.Anchor = ( (System.Windows.Forms.AnchorStyles) ( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                        | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.iTabStripPageManager.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.iTabStripPageManager.Location = new System.Drawing.Point( 0, 43 );
            this.iTabStripPageManager.Name = "iTabStripPageManager";
            this.iTabStripPageManager.Size = new System.Drawing.Size( 792, 533 );
            this.iTabStripPageManager.TabIndex = 9;
            this.iTabStripPageManager.TabStrip = this.iTabStrip;
            this.iTabStripPageManager.Visible = false;
            // 
            // CAGraphicalUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 792, 573 );
            this.Controls.Add( this.iTabStrip );
            this.Controls.Add( this.iTabStripPageManager );
            this.Controls.Add( this.iLbl_Copyright );
            this.Controls.Add( this.iLbl_Title );
            this.Controls.Add( this.iMenu );
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.iMenu;
            this.MinimumSize = new System.Drawing.Size( 800, 600 );
            this.Name = "CAGraphicalUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Crash Analyser";
            this.Load += new System.EventHandler( this.CAGraphicalUI_Load );
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler( this.CAGraphicalUI_FormClosing );
            this.Resize += new System.EventHandler( this.CAGraphicalUI_Resize );
            this.LocationChanged += new System.EventHandler( this.CAGraphicalUI_LocationChanged );
            this.iMenu.ResumeLayout( false );
            this.iMenu.PerformLayout();
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip iMenu;
        private System.Windows.Forms.ToolStripMenuItem iMenu_File;
        private System.Windows.Forms.ToolStripMenuItem iMenu_File_Exit;
        private System.Windows.Forms.ToolStripMenuItem iMenu_File_New;
        private System.Windows.Forms.ToolStripMenuItem iMenu_Help;
        private System.Windows.Forms.ToolStripMenuItem iMenu_Help_About;
        private System.Windows.Forms.Label iLbl_Title;
        private System.Windows.Forms.Label iLbl_Copyright;
        private SymbianTabStripLib.Tabs.TabStrip iTabStrip;
        private SymbianTabStripLib.Pages.TabStripPageManager iTabStripPageManager;
        private System.Windows.Forms.ToolStripMenuItem iMenu_File_SaveAs;
        private System.Windows.Forms.ToolStripSeparator iMenu_File_ExitSep;
    }
}