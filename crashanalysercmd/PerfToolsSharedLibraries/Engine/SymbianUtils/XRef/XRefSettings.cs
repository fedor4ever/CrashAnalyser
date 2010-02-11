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
using System;
using System.Collections.Generic;
using System.Text;
using SymbianUtils.Settings;

namespace SymbianUtils.XRef
{
    public class XRefSettings : DisposableObject
    {
        #region Constructors
        public XRefSettings()
        {
            iSettings = new XmlSettings( SettingsFileName );
            iSettings.Restore();
            //
            ValidateSettings();
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public static string SettingsFileName
        {
            get { return "XRefSettings.xml"; }
        }
        
        public string ServerRootPath
        {
            get
            {
                string ret = iSettings[ "XRef", "ServerRootPath" ];
                if ( string.IsNullOrEmpty( ret ) )
                {
                    ret = KDefaultXRefURL;
                }
                return ret;
            }
            set { iSettings[ "XRef", "ServerRootPath" ] = value; }
        }
        #endregion

        #region Internal methods
        private void ValidateSettings()
        {
            if ( !iSettings.Exists( "XRef", "ServerRootPath" ) )
            {
                iSettings[ "XRef", "ServerRootPath" ] = KDefaultXRefURL;
            }
        }
        #endregion

        #region From DisposableObject - Cleanup Framework
        protected override void CleanupUnmanagedResources()
        {
            if ( iSettings != null )
            {
                iSettings.Store();
                iSettings = null;
            }
        }
        #endregion

        #region Internal constants
        private const string KDefaultXRefURL = "http://your-xref-server-name-goes-here/";
        #endregion

        #region Data members
        private XmlSettings iSettings;
        #endregion
    }
}
