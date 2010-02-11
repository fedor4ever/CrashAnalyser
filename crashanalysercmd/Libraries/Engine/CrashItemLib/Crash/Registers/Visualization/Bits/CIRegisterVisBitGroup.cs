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
using CrashItemLib.Crash.Registers;
using SymbianUtils.Range;

namespace CrashItemLib.Crash.Registers.Visualization.Bits
{
    public class CIRegisterVisBitGroup : CIRegisterVisBitList
    {
        #region Constructors
        public CIRegisterVisBitGroup( CIContainer aContainer )
            : base( aContainer )
        {
        }
        
        public CIRegisterVisBitGroup( CIContainer aContainer, string aCategory )
            : base( aContainer )
        {
            base.Category = aCategory;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region Data members
        #endregion
    }
}
