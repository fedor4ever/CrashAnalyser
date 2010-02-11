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



namespace CrashItemLib.Crash.InfoEnvironment
{
    [CIDBAttributeColumn( "Name", 0 )]
    [CIDBAttributeColumn( "Value", 1, true )]
    public class CIInfoEnvironment : CIElement
    {
        #region Constructors
        public CIInfoEnvironment(CIContainer aContainer)
            : base( aContainer )
		{
		}
		#endregion

        #region API
    
        #endregion

        #region Properties

        [CIDBAttributeCell("Test Set", 1)]
        public string TestSet
        {
            get { return iTestSet; }
            set { iTestSet = value; }
        }

        #endregion

        #region Internal methods
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return iTestSet;
        }
        #endregion

    
        #region Data members
        private string iTestSet = string.Empty;
        private string iProductCode = string.Empty;
        private string iSerialNumber = string.Empty;
        private List<CIVersionInfo> iVersions = new List<CIVersionInfo>();
        #endregion
    }
}
