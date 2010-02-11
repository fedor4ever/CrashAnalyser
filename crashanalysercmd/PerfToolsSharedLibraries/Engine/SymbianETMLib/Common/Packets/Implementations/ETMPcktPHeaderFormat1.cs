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
using SymbianETMLib.Common.Exception;

namespace SymbianETMLib.Common.Packets
{
    public class ETMPcktPHeaderFormat1 : ETMPcktPHeaderBase 
    {
        #region Constructors
        public ETMPcktPHeaderFormat1()
        {
            // Format 1 P-header
            //
            // b1NEEEE00 None 0-15 x E, 0-1 x N
            // Bits [5:2], shown as EEEE, are the count of E atoms.
            //
            base.SetMask( "1 # #### 00" );
        }
        #endregion

        #region API
        public int ConditionCount( ETMPcktPHeaderBase.TAtomType aAtomType )
        {
            int ret = 0;
            //
            switch ( aAtomType )
            {
            case ETMPcktPHeaderBase.TAtomType.EAtomE_Passed:
                ret = ConditionCountFailed;
                break;
            case ETMPcktPHeaderBase.TAtomType.EAtomN_Failed:
                ret = ConditionCountPassed;
                break;
            default:
            case ETMPcktPHeaderBase.TAtomType.EAtomW_CycleBoundary:
                throw new ETMException( "ERROR: cycle accurate mode is not supported" );
            }
            //
            return ret;
        }
        #endregion

        #region From PcktBase
        #endregion

        #region Properties
        public int ConditionCountFailed
        {
            get
            {
                int ret = 0;
                //
                SymMask mask = new SymMask( "#1 #### ##" );
                if ( mask.IsMatch( base.RawByte ) )
                {
                    ret = 1;
                }
                //
                return ret;
            }
        }

        public int ConditionCountPassed
        {
            get
            {
                SymMask mask = new SymMask( "## 1111 ##" );
                byte val = (byte) mask.Apply( RawByte );
                val >>= 2;
                int ret = System.Convert.ToInt32( val );
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
