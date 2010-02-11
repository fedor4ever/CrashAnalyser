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
using CrashAnalyserEngine.Interfaces;
using CrashAnalyserEngine.Plugins;
using CrashAnalyserEngine.Tabs;
using CrashAnalyser.Menu;
using CrashAnalyserEngine.Engine;
using SymbianTabStripLib.Manager;

namespace CrashAnalyser.UI
{
    internal partial class CAGraphicalUI : Form, IEngineUIManager
    {
        #region Constructors
        public CAGraphicalUI( CAEngine aEngine )
        {
            iEngine = aEngine;

            // Must call this to ensure that UI components are created before
            // we load the crash analyser plugins.
            InitializeComponent();
            
            // Create tab manager to oversee all tab items
            iTabManager = new TabStripManager( iTabStrip, this );
            iTabManager.AutoHide = true;
            
            // Listen for tab change events
            iMenuManager = new CAMenuManager( iEngine.Settings, iMenu, iTabManager );

            // Now it's safe to do this - the menu items that each plugin hangs off of
            // will have been created
            iEngine.UIManager = this;

            // Restore settings needed to position & size form
            iEngine.Settings.Load( "GraphicalUI", this );
        }
        #endregion

        #region API
        #endregion

        #region Event handlers
        private void iMenu_File_DropDownOpening( object sender, EventArgs e )
        {
            ShowOrHideMenu( iMenu_File, iMenu_File_SaveAs );
            ShowOrHideMenu( iMenu_File, iMenu_File_New );
            
            // Hide the separator if no save as or new menu item
            iMenu_File_ExitSep.Visible = ( iMenu_File_SaveAs.DropDownItems.Count > 0 || iMenu_File_New.DropDownItems.Count > 0 );
        }
        
        private void iMenu_File_Exit_Click( object sender, EventArgs e )
        {
            this.Close();
        }

        private void CAGraphicalUI_Load( object sender, EventArgs e )
        {
        }

        private void CAGraphicalUI_FormClosing( object sender, FormClosingEventArgs e )
        {
            iEngine.Settings.Save( "GraphicalUI", this );
        }

        private void TabStrip_TabCloseRequestReceiver( SymbianTabStripLib.Tabs.TabStripTab aTab )
        {
            if ( aTab.Page != null && aTab.Page.Body != null )
            {
                CATab body = aTab.Page.Body as CATab;
                if ( body != null )
                {
                    UIManagerContentClose( body );
                }
            }
        }

        private void CAGraphicalUI_LocationChanged( object sender, EventArgs e )
        {

        }

        private void CAGraphicalUI_Resize( object sender, EventArgs e )
        {

        }
        #endregion

        #region Internal methods
        private void SetLabelVisibility( bool aVisible )
        {
            iLbl_Title.Visible = aVisible;
            iLbl_Copyright.Visible = aVisible;
        }

        private void ShowOrHideMenu( ToolStripMenuItem aParent, ToolStripMenuItem aMenu )
        {
            int count = aMenu.DropDownItems.Count;
            bool added = aParent.DropDownItems.Contains( aMenu );
            aMenu.Visible = ( count > 0 );
        } 
        #endregion

        #region IEngineUIManager Members
        public void UIManagerContentAdd( CATab aTab )
        {
            iTabManager.Add( aTab );
            SetLabelVisibility( iTabManager.TabCount > 0 );
        }

        public void UIManagerContentClose( CATab aTab )
        {
            iTabManager.Remove( aTab );
            SetLabelVisibility( iTabManager.TabCount == 0 );
        }

        public void UIManagerMenuItemAdd( TEngineUIMenuPane aPane, string aCaption, UIMenuItemClickHandler aClickHandler, object aTag )
        {
            UIManagerMenuItemAdd( aPane, aCaption, aClickHandler, aTag, null );
        }

        public void UIManagerMenuItemAdd( TEngineUIMenuPane aPane, string aCaption, UIMenuItemClickHandler aClickHandler, object aTag, CATab aHost )
        {
            ToolStripMenuItem parent = null;
            //
            switch ( aPane )
            {
            case TEngineUIMenuPane.EFileNew:
                parent = iMenu_File_New;
                break;
            case TEngineUIMenuPane.EFileSaveAs:
                parent = iMenu_File_SaveAs;
                break;
            }
            //
            if ( parent != null )
            {
                iMenuManager.Add( parent, aCaption, aClickHandler, aTag, aHost );
            } 
        }

        public Version UIVersion
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public string UICommandLineArguments
        {
            get { return Environment.CommandLine; }
        }

        public bool UIIsSilent
        {
            get { return false; }
        }

        public void UITrace( string aMessage )
        {
            if ( iDebug )
            {
                StringBuilder text = new StringBuilder( aMessage );
                text.Insert( 0, string.Format( "[{0:x6}] ", System.Threading.Thread.CurrentThread.ManagedThreadId ) );
                //
                System.Diagnostics.Debug.WriteLine( text.ToString() );
            }
        }

        public void UITrace( string aFormat, params object[] aParams )
        {
            string msg = string.Format( aFormat, aParams );
            UITrace( msg );
        }
        #endregion

        #region Data members
        private bool iDebug = true;
        private readonly CAEngine iEngine;
        private readonly CAMenuManager iMenuManager;
        private readonly TabStripManager iTabManager;
        #endregion
    }
}
