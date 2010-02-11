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
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.Container;
using SymbianUtils.Range;
using SymbianStructuresLib.Uids;
using SymbianUtils.DataBuffer;

namespace CrashItemLib.Crash.Messages
{
	public class CIMessageWarning : CIMessage
    {
        #region Constructors
        public CIMessageWarning( CIContainer aContainer )
            : this( aContainer, string.Empty )
		{
		}

        public CIMessageWarning( CIContainer aContainer, string aTitle )
            : base( aContainer, aTitle )
        {
            Type = TType.ETypeWarning;
            Color = System.Drawing.Color.DarkBlue;
        }
		#endregion

        #region API
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
