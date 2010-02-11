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

namespace SymbianETMLib.Common.Packets
{
    public class ETMPcktCycleCount : ETMPcktFiveByteRunBase 
    {
        #region Constructors
        public ETMPcktCycleCount()
            : base( "00000100" )
        {
        }

        public ETMPcktCycleCount( byte aValue )
            : this()
        {
            base.RawByte = aValue;
        }
        #endregion

        #region API
        #endregion

        #region From PcktBase
        #endregion

        #region Properties
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        #endregion
    }
}
