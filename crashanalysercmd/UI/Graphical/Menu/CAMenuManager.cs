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
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using SymbianUtils.Settings;
using SymbianTabStripLib.Tabs;
using SymbianTabStripLib.Pages;
using SymbianTabStripLib.Manager;
using CrashAnalyserEngine.Interfaces;
using CrashAnalyserEngine.Tabs;

namespace CrashAnalyser.Menu
{
	internal class CAMenuManager
	{
		#region Constructors
        public CAMenuManager( XmlSettings aSettings, MenuStrip aMenu, TabStripManager aTabStripManager )
		{
            iSettings = aSettings;
            iMenuBar = aMenu;
            iTabStripManager = aTabStripManager;

            // Listen to tab changes
            iTabStripManager.TabStrip.SelectedTabChanged += new TabStrip.TabHandler( TabStrip_SelectedTabChanged );
		}
		#endregion

        #region API
        public void Add( ToolStripMenuItem aParent, string aCaption, UIMenuItemClickHandler aClickHandler, object aTag, CATab aHost )
        {
            CAMenuItem item = new CAMenuItem( this, aCaption, aClickHandler, aTag );

            if ( aHost != null )
            {
                // Find a menu list for the corresponding tab
                CAMenuItemList list = this[ aHost ];
                if ( list == null )
                {
                    list = new CAMenuItemList( this );
                    iDictionary.Add( aHost, list );
                }

                // Add the item to the list
                list.Add( item );
            }
            else
            {
                // Not associated with a specific tab, so most likely a top-level
                // plugin menu item that is always visible
            }

            aParent.DropDownItems.Add( item );
        }
        #endregion

		#region Properties
        public XmlSettings Settings
        {
            get { return iSettings; }
        }

        public MenuStrip MenuBar
        {
            get { return iMenuBar; }
        }
		#endregion

        #region Event handlers
        private void TabStrip_SelectedTabChanged( TabStripTab aTab )
        {
            TabStripPage page = aTab.Page;
            if ( page != null )
            {
                // Get the corresponding tab host
                Control mainControl = aTab.Page.Body;
                CATab host = mainControl as CATab;
                if ( host != null )
                {
                    // Map tab host back to our tab type
                    CAMenuItemList menuItems = this[ host ];
                    if ( menuItems != null )
                    {
                        Activate( menuItems );
                    }
                }
            }
        }
        #endregion

        #region Internal methods
        private CAMenuItemList this[ CATab aHost ]
        {
            get
            {
                CAMenuItemList ret = null;
                //
                if ( iDictionary.ContainsKey( aHost ) )
                {
                    ret = iDictionary[ aHost ];
                }
                //
                return ret;
            }
        }

        private void Activate( CAMenuItemList aList )
        {
            if ( iActiveList != null )
            {
                iActiveList.Deactivate();
                iActiveList = null;
            }
            //
            iActiveList = aList;
            //
            if ( iActiveList != null )
            {
                iActiveList.Activate();
            }
        }
        #endregion

        #region Data members
        private readonly XmlSettings iSettings;
        private readonly MenuStrip iMenuBar;
        private readonly TabStripManager iTabStripManager;
        private CAMenuItemList iActiveList = null;
        private Dictionary<CATab, CAMenuItemList> iDictionary = new Dictionary<CATab, CAMenuItemList>();
		#endregion
    }
}
