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
using System.Threading;
using System.Windows.Forms;
using CrashAnalyserEngine.Plugins;
using CrashItemLib.Engine;
using CrashItemLib.Engine.Interfaces;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Container;
using CrashItemLib.PluginAPI;
using CrashAnalyserEngine.Engine;
using CrashAnalyserEngine.Interfaces;
using CAPluginCrashAnalysisUi.Wizards;
using CAPluginCrashAnalysisUi.Tabs;
using CAPCrashAnalysis.Plugin;
using SymbianUtils;

namespace CAPluginCrashAnalysisUi.Plugin
{
    public class CAPluginCrashAnalysisUi : CAPluginCrashAnalysis
	{
		#region Constructors
        public CAPluginCrashAnalysisUi( CAEngine aEngine )
            : base( aEngine )
		{
            // Switch debug engine to UI mode
            aEngine.DebugEngine.UiMode = SymbianDebugLib.TDbgUiMode.EUiEnabled;
            RegisterMenuItems();
        }
		#endregion

		#region API
        #endregion

        #region From CAPlugin
        public override CAPlugin.TType Type
        {
            get { return CAPlugin.TType.ETypeUi; }
        }
        #endregion

		#region Properties
		#endregion

        #region Event handlers
        private void Menu_File_New_CrashAnalyser_Click( object aTag, string aCaption )
        {
            CAWizardCrashAnalysis wizard = new CAWizardCrashAnalysis( this );
            DialogResult result = wizard.ShowDialog();
            if ( result == DialogResult.OK )
            {
                // Wizard was closed without error so prepare main summary form
                // and register with UI. Only show the summary form if we have 
                // multiple crash items, otherwise just show the container explorer.
                int count = CrashItemEngine.Count;
                if ( count > 1 )
                {
                    CATabCrashContainerSummary control = new CATabCrashContainerSummary( this );
                    base.UIManager.UIManagerContentAdd( control );
                }
                else if ( count == 1 )
                {
                    CIContainer container = (CIContainer) CrashItemEngine[ 0 ];
                    //
                    CATabCrashContainerExplorer control = new CATabCrashContainerExplorer( this, container );
                    base.UIManager.UIManagerContentAdd( control );
                }
            }
        }
        #endregion

        #region Internal constants
        #endregion

        #region Internal methods
        private void RegisterMenuItems()
        {
            // Register "new" menu item
            base.UIManager.UIManagerMenuItemAdd( TEngineUIMenuPane.EFileNew, base.Name, new UIMenuItemClickHandler( Menu_File_New_CrashAnalyser_Click ), null );
        }
        #endregion

        #region Data members
        #endregion
    }
}
