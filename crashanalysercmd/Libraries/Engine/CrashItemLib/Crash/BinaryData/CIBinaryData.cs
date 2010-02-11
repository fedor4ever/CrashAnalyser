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
using CrashItemLib.Crash.Container;
using SymbianUtils.DataBuffer;

namespace CrashItemLib.Crash.BinaryData
{
	public class CIBinaryData : CIElement
    {
        #region Constructors
        public CIBinaryData( CIContainer aContainer )
            : base( aContainer )
		{
		}
		#endregion

        #region API
        #endregion

        #region Properties
        public override string Name
        {
            get { return iName; }
            set { iName = value; }
        }

        public DataBuffer DataBuffer
        {
            get { return iData; }
            set { iData = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private string iName = string.Empty;
        private DataBuffer iData = new DataBuffer();
       #endregion
    }
}
