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
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using CAPRawStack.Plugin;
using CrashAnalyserEngine.Tabs;
using CrashAnalyserEngine.Engine;
using CrashAnalyserEngine.Interfaces;
using SymbianStackLib.Engine;
using SymbianStackLib.Data.Source;
using SymbianStackLib.Data.Output;
using SymbianStackLib.Data.Output.Entry;
using SymbianStackLibUi.GUI;

namespace CAPluginRawStackUi.Tabs
{
    internal partial class CATabStackViewer : CATab
    {
        #region Constructors
        public CATabStackViewer( CAPluginRawStack aPlugin )
        {
            InitializeComponent();
            //
            iPlugin = aPlugin;
            base.Title = "Call Stack";
        }
        #endregion

        #region Properties
        #endregion

        #region Event handlers
        private void CATabStackViewer_Load( object sender, EventArgs e )
        {
            StackEngine stackEngine = iPlugin.StackEngine;

            try
            {
                StackEngineProgressDialog.ReconstructStack( stackEngine );
            }
            catch( Exception exception )
            {
                iLbl_ErrorMessage.Text = exception.Message;
                iLbl_ErrorMessage.Visible = true;
            }

            iStackViewer.StackData = stackEngine.DataOutput;

            // Work out if we have any ghosts in the data - this dictates whether
            // or not we show the "hide ghosts" checkbox.
            bool seenGhost = false;
            foreach ( StackOutputEntry entry in stackEngine.DataOutput )
            {
                if ( entry.IsGhost )
                {
                    seenGhost = true;
                    break;
                }
            }
            //
            iCB_HideGhosts.Checked = seenGhost;
            iCB_HideGhosts.Visible = seenGhost;
            iStackViewer.OnlyShowEntriesWithSymbols = iCB_MatchExactSymbols.Checked;
            iStackViewer.HideGhosts = iCB_HideGhosts.Checked;
        }

        private void iCB_HideGhosts_CheckedChanged( object sender, EventArgs e )
        {
            bool hideGhosts = iCB_HideGhosts.Checked;
            iStackViewer.HideGhosts = hideGhosts;
            iStackViewer.Refresh();
        }

        private void iCB_MatchExactSymbols_CheckedChanged( object sender, EventArgs e )
        {
            bool matchExact = iCB_MatchExactSymbols.Checked;
            iStackViewer.OnlyShowEntriesWithSymbols = matchExact;
            iStackViewer.Refresh();
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly CAPluginRawStack iPlugin;
        #endregion
    }
}
