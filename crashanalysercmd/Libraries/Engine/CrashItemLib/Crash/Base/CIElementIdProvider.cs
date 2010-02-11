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
using SymbianDebugLib.Engine;
using CrashItemLib.Crash.Messages;

namespace CrashItemLib.Crash.Base
{
	internal class CIElementIdProvider
	{
		#region Constructors
        public CIElementIdProvider()
            : this( KInitialStartingValue )
        {
        }

        public CIElementIdProvider( int aInitialValue )
		{
            iNextValue = aInitialValue;
		}
		#endregion

        #region Internal constants
        // Assumption: Symbian OS process and thread ids should be less than this value
        internal const int KInitialStartingValue = 50000; 
        #endregion

        #region API
        public int GetNextId()
        {
            return ++iNextValue;
        }
        #endregion

        #region Properties
        #endregion

        #region Constants
        #endregion

        #region Operators
        #endregion

        #region Data members
        private int iNextValue = KInitialStartingValue;
		#endregion
	}
}
