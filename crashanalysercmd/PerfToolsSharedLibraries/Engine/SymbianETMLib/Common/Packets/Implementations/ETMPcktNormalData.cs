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
using SymbianUtils.BasicTypes;
using SymbianETMLib.Common.Types;

namespace SymbianETMLib.Common.Packets
{
    public class ETMPcktNormalData : ETMPcktBase
    {
        #region Constructors
        public ETMPcktNormalData()
        {
            base.SetMask( "00#0##10" );
        }

        public ETMPcktNormalData( SymByte aByte )
            : base( aByte )
        {
        }
        #endregion

        #region API
        #endregion

        #region From PcktBase
        public override int Priority
        {
            get
            {
                return int.MinValue + 3;
            }
        }
        #endregion

        #region Properties
        public int Size
        {
            get
            {
                SymMask mask = new SymMask( "1100", SymMask.TShiftDirection.ERight, 2 );
                int ret = (int) mask.Apply( base.RawByte );
                return ret;
            }
        }
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        #endregion
    }
}
