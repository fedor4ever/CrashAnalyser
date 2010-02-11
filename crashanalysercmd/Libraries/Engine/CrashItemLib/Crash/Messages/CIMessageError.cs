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
	public class CIMessageError : CIMessage
    {
        #region Constructors
        public CIMessageError( CIContainer aContainer )
            : this( aContainer, string.Empty )
		{
		}
        
        public CIMessageError( CIContainer aContainer, string aTitle )
            : base( aContainer, aTitle )
        {
            Type = TType.ETypeError;
            Color = System.Drawing.Color.Red;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region From CIMessage
        #endregion

        #region Data members
        #endregion
    }
}
