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
using System.Collections.Generic;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Base.DataBinding;
using SymbianUtils.Range;
using SymbianStructuresLib.Uids;
using SymbianUtils.DataBuffer;
using CrashItemLib.Crash.Container;

namespace CrashItemLib.Crash.Telephony
{
    [CIDBAttributeColumn( "Name", 0 )]
    [CIDBAttributeColumn( "Value", 1, true )]
    public class CITelephony : CIElement
    {
        #region Constructors
        public CITelephony( CIContainer aContainer )
            : base( aContainer )
		{
            iNetworkInfo = new CITelephonyNetworkInfo( this );
		}
		#endregion

        #region API
        #endregion

        #region Properties
        [CIDBAttributeCell( "Phone Number", 1, "", "" )]
        public string PhoneNumber
        {
            get { return iPhoneNumber; }
            set { iPhoneNumber = value; }
        }

        [CIDBAttributeCell( "IMEI", 2, "", "" )]
        public string IMEI
        {
            get { return iIMEI; }
            set { iIMEI = value; }
        }

        [CIDBAttributeCell( "IMSI", 3, "", "" )]
        public string IMSI
        {
            get { return iIMSI; }
            set { iIMSI = value; }
        }

        [CIDBAttributeCell( CIDBAttributeCell.TOptions.EAutoExpand )]
        public CITelephonyNetworkInfo NetworkInfo
        {
            get { return iNetworkInfo; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly CITelephonyNetworkInfo iNetworkInfo;
        private string iPhoneNumber = string.Empty;
        private string iIMEI = string.Empty;
        private string iIMSI = string.Empty;
        #endregion
    }
}
