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
using CrashAnalyserEngine.Engine;
using CrashAnalyserEngine.Interfaces;
using SymbianUtils;
using CAPRawStack.Plugin;
using CAPluginRawStackUi.Tabs;
using CAPluginRawStackUi.Wizards;

namespace CAPluginRawStackUi.Plugin
{
    public class CAPluginRawStackUi : CAPluginRawStack
	{
		#region Constructors
        public CAPluginRawStackUi( CAEngine aEngine )
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
        private void Menu_File_New_RawStack_Click( object aTag, string aCaption )
        {
            CAWizardRawStack wizard = new CAWizardRawStack( this );
            DialogResult result = wizard.ShowDialog();
            if ( result == DialogResult.OK )
            {
                CATabStackViewer control = new CATabStackViewer( this );
                base.UIManager.UIManagerContentAdd( control );
            }
        }
        #endregion

        #region Internal constants
        #endregion

        #region Internal methods
        private void RegisterMenuItems()
        {
            // Register "new" menu item
            base.UIManager.UIManagerMenuItemAdd( TEngineUIMenuPane.EFileNew, base.Name, new UIMenuItemClickHandler( Menu_File_New_RawStack_Click ), null );
        }
        #endregion

        #region Data members
        #endregion
    }
}
