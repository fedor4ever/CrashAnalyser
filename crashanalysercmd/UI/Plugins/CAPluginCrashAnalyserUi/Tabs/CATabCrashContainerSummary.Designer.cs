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
    partial class CATabCrashContainerSummary
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
            XPTable.Models.DataSourceColumnBinder dataSourceColumnBinder1 = new XPTable.Models.DataSourceColumnBinder();
            this.iTLP = new System.Windows.Forms.TableLayoutPanel();
            this.iTable = new XPTable.Models.Table();
            this.iColModel = new XPTable.Models.ColumnModel();
            this.iCol_FileName = new XPTable.Models.TextColumn();
            this.iCol_SubRow_LineOrOtherThreadCount = new XPTable.Models.TextColumn();
            this.iCol_SubRow_ThreadName = new XPTable.Models.TextColumn();
            this.iCol_SubRow_ExitInfo = new XPTable.Models.TextColumn();
            this.iTableModel = new XPTable.Models.TableModel();
            this.iTLP_Buttons = new System.Windows.Forms.TableLayoutPanel();
            this.iBT_Open = new System.Windows.Forms.Button();
            this.iBT_Open_All = new System.Windows.Forms.Button();
            this.iBT_Close = new System.Windows.Forms.Button();
            this.iTLP.SuspendLayout();
            ( (System.ComponentModel.ISupportInitialize) ( this.iTable ) ).BeginInit();
            this.iTLP_Buttons.SuspendLayout();
            this.SuspendLayout();
            // 
            // iTLP
            // 
            this.iTLP.ColumnCount = 1;
            this.iTLP.ColumnStyles.Add( new System.Windows.Forms.ColumnStyle( System.Windows.Forms.SizeType.Percent, 100F ) );
            this.iTLP.Controls.Add( this.iTable, 0, 0 );
            this.iTLP.Controls.Add( this.iTLP_Buttons, 0, 1 );
            this.iTLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iTLP.Location = new System.Drawing.Point( 0, 0 );
            this.iTLP.Name = "iTLP";
            this.iTLP.RowCount = 2;
            this.iTLP.RowStyles.Add( new System.Windows.Forms.RowStyle( System.Windows.Forms.SizeType.Percent, 100F ) );
            this.iTLP.RowStyles.Add( new System.Windows.Forms.RowStyle() );
            this.iTLP.Size = new System.Drawing.Size( 837, 483 );
            this.iTLP.TabIndex = 0;
            // 
            // iTable
            // 
            this.iTable.BorderColor = System.Drawing.Color.Black;
            this.iTable.ColumnModel = this.iColModel;
            this.iTable.DataMember = null;
            this.iTable.DataSourceColumnBinder = dataSourceColumnBinder1;
            this.iTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iTable.FullRowSelect = true;
            this.iTable.HeaderFont = new System.Drawing.Font( "Tahoma", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte) ( 0 ) ) );
            this.iTable.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.iTable.Location = new System.Drawing.Point( 3, 3 );
            this.iTable.Name = "iTable";
            this.iTable.NoItemsText = "No Crash Data available";
            this.iTable.NoItemsTextColor = System.Drawing.SystemColors.ControlText;
            this.iTable.NoItemsTextFont = new System.Drawing.Font( "Tahoma", 8.25F );
            this.iTable.Size = new System.Drawing.Size( 831, 441 );
            this.iTable.TabIndex = 0;
            this.iTable.TableModel = this.iTableModel;
            this.iTable.Text = "iTable";
            this.iTable.UnfocusedBorderColor = System.Drawing.Color.Black;
            this.iTable.SelectionChanged += new XPTable.Events.SelectionEventHandler( this.iTable_SelectionChanged );
            // 
            // iColModel
            // 
            this.iColModel.Columns.AddRange( new XPTable.Models.Column[] {
            this.iCol_FileName,
            this.iCol_SubRow_LineOrOtherThreadCount,
            this.iCol_SubRow_ThreadName,
            this.iCol_SubRow_ExitInfo} );
            // 
            // iCol_FileName
            // 
            this.iCol_FileName.ContentWidth = 0;
            this.iCol_FileName.Text = "File Name";
            this.iCol_FileName.Width = 30;
            // 
            // iCol_SubRow_LineOrOtherThreadCount
            // 
            this.iCol_SubRow_LineOrOtherThreadCount.ContentWidth = 0;
            this.iCol_SubRow_LineOrOtherThreadCount.Text = "Line";
            this.iCol_SubRow_LineOrOtherThreadCount.Width = 40;
            // 
            // iCol_SubRow_ThreadName
            // 
            this.iCol_SubRow_ThreadName.ContentWidth = 0;
            this.iCol_SubRow_ThreadName.TakesUpSlack = true;
            this.iCol_SubRow_ThreadName.Text = "Name";
            this.iCol_SubRow_ThreadName.Width = 607;
            // 
            // iCol_SubRow_ExitInfo
            // 
            this.iCol_SubRow_ExitInfo.ContentWidth = 0;
            this.iCol_SubRow_ExitInfo.Text = "Exit Info";
            this.iCol_SubRow_ExitInfo.Width = 150;
            // 
            // iTLP_Buttons
            // 
            this.iTLP_Buttons.ColumnCount = 4;
            this.iTLP_Buttons.ColumnStyles.Add( new System.Windows.Forms.ColumnStyle() );
            this.iTLP_Buttons.ColumnStyles.Add( new System.Windows.Forms.ColumnStyle() );
            this.iTLP_Buttons.ColumnStyles.Add( new System.Windows.Forms.ColumnStyle( System.Windows.Forms.SizeType.Percent, 100F ) );
            this.iTLP_Buttons.ColumnStyles.Add( new System.Windows.Forms.ColumnStyle() );
            this.iTLP_Buttons.Controls.Add( this.iBT_Open, 0, 0 );
            this.iTLP_Buttons.Controls.Add( this.iBT_Open_All, 1, 0 );
            this.iTLP_Buttons.Controls.Add( this.iBT_Close, 3, 0 );
            this.iTLP_Buttons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iTLP_Buttons.Location = new System.Drawing.Point( 0, 447 );
            this.iTLP_Buttons.Margin = new System.Windows.Forms.Padding( 0 );
            this.iTLP_Buttons.Name = "iTLP_Buttons";
            this.iTLP_Buttons.RowCount = 1;
            this.iTLP_Buttons.RowStyles.Add( new System.Windows.Forms.RowStyle() );
            this.iTLP_Buttons.Size = new System.Drawing.Size( 837, 36 );
            this.iTLP_Buttons.TabIndex = 1;
            // 
            // iBT_Open
            // 
            this.iBT_Open.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iBT_Open.Location = new System.Drawing.Point( 3, 3 );
            this.iBT_Open.Name = "iBT_Open";
            this.iBT_Open.Size = new System.Drawing.Size( 74, 30 );
            this.iBT_Open.TabIndex = 0;
            this.iBT_Open.Text = "Open";
            this.iBT_Open.UseVisualStyleBackColor = true;
            this.iBT_Open.Click += new System.EventHandler( this.iBT_Open_Click );
            // 
            // iBT_Open_All
            // 
            this.iBT_Open_All.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iBT_Open_All.Location = new System.Drawing.Point( 83, 3 );
            this.iBT_Open_All.Name = "iBT_Open_All";
            this.iBT_Open_All.Size = new System.Drawing.Size( 89, 30 );
            this.iBT_Open_All.TabIndex = 1;
            this.iBT_Open_All.Text = "Open All";
            this.iBT_Open_All.UseVisualStyleBackColor = true;
            this.iBT_Open_All.Click += new System.EventHandler( this.iBT_Open_All_Click );
            // 
            // iBT_Close
            // 
            this.iBT_Close.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iBT_Close.Location = new System.Drawing.Point( 784, 3 );
            this.iBT_Close.Name = "iBT_Close";
            this.iBT_Close.Size = new System.Drawing.Size( 50, 30 );
            this.iBT_Close.TabIndex = 2;
            this.iBT_Close.Text = "Close";
            this.iBT_Close.UseVisualStyleBackColor = true;
            this.iBT_Close.Click += new System.EventHandler( this.iBT_Close_Click );
            // 
            // CATabCrashContainerSummary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.iTLP );
            this.Font = new System.Drawing.Font( "Tahoma", 8.25F );
            this.Name = "CATabCrashContainerSummary";
            this.Size = new System.Drawing.Size( 837, 483 );
            this.Title = "Summary";
            this.Load += new System.EventHandler( this.CASubControlCrashSummary_Load );
            this.iTLP.ResumeLayout( false );
            ( (System.ComponentModel.ISupportInitialize) ( this.iTable ) ).EndInit();
            this.iTLP_Buttons.ResumeLayout( false );
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel iTLP;
        private XPTable.Models.Table iTable;
        private XPTable.Models.ColumnModel iColModel;
        private XPTable.Models.TableModel iTableModel;
        private System.Windows.Forms.TableLayoutPanel iTLP_Buttons;
        private System.Windows.Forms.Button iBT_Open;
        private System.Windows.Forms.Button iBT_Open_All;
        private XPTable.Models.TextColumn iCol_FileName;
        private XPTable.Models.TextColumn iCol_SubRow_ThreadName;
        private XPTable.Models.TextColumn iCol_SubRow_LineOrOtherThreadCount;
        private XPTable.Models.TextColumn iCol_SubRow_ExitInfo;
        private System.Windows.Forms.Button iBT_Close;
    }
}
