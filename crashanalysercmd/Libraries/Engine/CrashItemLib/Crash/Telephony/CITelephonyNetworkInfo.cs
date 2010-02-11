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
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Base.DataBinding;
using SymbianUtils.Range;
using SymbianStructuresLib.Uids;
using SymbianUtils.DataBuffer;

namespace CrashItemLib.Crash.Telephony
{
	public class CITelephonyNetworkInfo : CIElement
    {
        #region Enumerations
        public enum TRegistrationMode
        {
            [Description("Unknown")]
            ERegModeUnknown = 0,

            [Description( "Offline" )]
            ERegModeOffline,

            [Description( "2G" )]
            ERegMode2g,

            [Description( "3G" )]
            ERegMode3g,

            [Description( "HSDPA" )]
            ERegModeHSDPA
        }
        #endregion

        #region Constructors
        internal CITelephonyNetworkInfo( CITelephony aParent )
            : base( aParent.Container )
		{
            iParent = aParent;
		}
		#endregion

        #region API
        #endregion

        #region Properties
        [CIDBAttributeCell( "Country Code", 100, "", "" )]
        public string Country
        {
            get { return iCountry; }
            set { iCountry = value; }
        }

        [CIDBAttributeCell( "Identity", 101, "", "" )]
        public string Identity
        {
            get { return iIdentity; }
            set { iIdentity = value; }
        }

        [CIDBAttributeCell( "Cell ID", 102, "", "" )]
        public string CellId
        {
            get { return iCellId; }
            set { iCellId = value; }
        }

        [CIDBAttributeCell( "Registration", 103, "", "" )]
        public TRegistrationMode RegistrationMode
        {
            get { return iRegistrationMode; }
            set { iRegistrationMode = value; }
        }

        [CIDBAttributeCell("CGI", 104, "", "")]
        public string CGI
        {
            get { return iCGI; }
            set { iCGI = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly CITelephony iParent;
        private string iCountry = string.Empty;
        private string iIdentity = string.Empty;
        private string iCellId = string.Empty;
        private string iCGI = string.Empty; //Cell Global Identity
        private TRegistrationMode iRegistrationMode = TRegistrationMode.ERegModeUnknown;
        #endregion
    }
}
