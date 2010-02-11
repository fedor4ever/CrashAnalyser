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
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using SymbianUtils.Settings;
using SymbianUtilsUi.Dialogs;
using CrashAnalyserEngine.Engine;
using CrashAnalyserEngine.Plugins;

namespace CrashAnalyser.UI
{
    public class CAGraphicalUIManager : ApplicationContext
	{
		#region Constructors
        public CAGraphicalUIManager()
		{
            Application.ApplicationExit += new EventHandler( Application_ApplicationExit );
            
            // Create engine
            iEngine = new CAEngine( new string[] {} );
        }
		#endregion

        #region API
        public void Run()
        {
            // Enable visual styles if supported
            if ( OSFeature.Feature.IsPresent( OSFeature.Themes ) )
            {
                Application.EnableVisualStyles();
                Application.DoEvents();
            }

            CAGraphicalUI graphicalUi = new CAGraphicalUI( iEngine );

            // Run the UI asynchronously
            base.MainForm = graphicalUi;
            Application.Run( this );
        }
		#endregion

        #region Properties
        #endregion

        #region Event handlers
        protected override void OnMainFormClosed( object aSender, EventArgs aArgs )
        {
            if ( aSender is CAGraphicalUI )
            {
            }

            base.OnMainFormClosed( aSender, aArgs );
            Application.Exit();
        }

        private void Application_ApplicationExit( object aSender, EventArgs aArgs )
        {
            try
            {
                if ( iEngine.Settings != null )
                {
                    iEngine.Settings.Store();
                }
            }
            catch ( Exception )
            {
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From ApplicationContext
        protected override void Dispose( bool disposing )
        {
            try
            {
                base.Dispose( disposing );
            }
            finally
            {
                if ( disposing )
                {
                    iEngine.Dispose();
                }
            }
        }
        #endregion

        #region Data members
        private readonly CAEngine iEngine;
        #endregion
    }
}
