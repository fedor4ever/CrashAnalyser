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
using CrashItemLib.Crash.Utils;
using CrashItemLib.Crash.Container;

namespace CrashItemLib.Crash.Header
{
    [CIDBAttributeColumn( "Name", 0 )]
    [CIDBAttributeColumn( "Value", 1, true )]
    public class CIHeader : CIElement
    {
        #region Constructors
        [CIElementAttributeMandatory()]
        public CIHeader( CIContainer aContainer )
            : base( aContainer )
		{
		}
		#endregion

        #region API
        #endregion

        #region Properties
        [CIDBAttributeCell( "Time", 0 )]
        public DateTime CrashTime
        {
            get { return iCrashTime; }
            set { iCrashTime = value; }
        }

        [CIDBAttributeCell( "Up Time", 1, "", "00:00:00" )]
        public TimeSpan UpTime
        {
            get { return iUpTime; }
            set { iUpTime = value; }
        }

        [CIDBAttributeCell( "File Format", 2, "", "" )]
        public CIVersionInfo FileFormatVersion
        {
            get { return iFileFormatVersion; }
            set { iFileFormatVersion = value; }
        }

        [CIDBAttributeCell("Crash Source", 3)]
        public int CrashSource
        {
            get { return iCrashSource; }
            set { iCrashSource = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private DateTime iCrashTime = new DateTime();
        private TimeSpan iUpTime = new TimeSpan();
        private CIVersionInfo iFileFormatVersion = new CIVersionInfo();
        private int iCrashSource = -1; // user side = 1, kernel side = 0
        #endregion
    }
}
