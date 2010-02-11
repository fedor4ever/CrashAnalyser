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
    public class ETMPcktPHeaderFormat2 : ETMPcktPHeaderBase 
    {
        #region Constructors
        public ETMPcktPHeaderFormat2()
        {
            // Format 2 P-header:
            // 
            //   b1000 FF 10
            //   1 x (N/nE), 1 x (N/nE)
            //   Bit [3] represents the first instruction and bit [2] represents the second instruction.
            // 
            // Important Note:
            //
            //   This is the opposite bit encoding to the PHeader1 !!! - I.e. a '1' indicates fail (N) and
            //   a 0 indicates pass (E)!!!
            //
            // Example:
            //
            //   A header of value b10001010 is encountered in the trace when cycle-accurate mode is disabled. This is a
            //   format 2 P-header representing the atoms NE. It indicates that one instruction was executed that failed its
            //   condition codes, followed by one instruction that passed its condition codes.
            //
            base.SetMask( "1000 ## 10" );
        }

        static ETMPcktPHeaderFormat2()
        {
            iAtomMask1 = new SymMask( "1###", SymMask.TShiftDirection.ERight, 3 );
            iAtomMask2 = new SymMask( "#1##", SymMask.TShiftDirection.ERight, 2 );
        }
        #endregion

        #region API
        #endregion

        #region From PcktBase
        #endregion

        #region Properties
        public ETMPcktPHeaderBase.TAtomType Atom1Type
        {
            get
            {
                uint val = iAtomMask1.Apply( base.RawByte );
                ETMPcktPHeaderBase.TAtomType ret = TAtomType.EAtomN_Failed;
                if ( val == 0 )
                {
                    ret = ETMPcktPHeaderBase.TAtomType.EAtomE_Passed;
                }
                return ret;
            }
        }

        public ETMPcktPHeaderBase.TAtomType Atom2Type
        {
            get
            {
                uint val = iAtomMask2.Apply( base.RawByte );
                ETMPcktPHeaderBase.TAtomType ret = TAtomType.EAtomN_Failed;
                if ( val == 0 )
                {
                    ret = ETMPcktPHeaderBase.TAtomType.EAtomE_Passed;
                }
                return ret;
            }
        }
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        private readonly static SymMask iAtomMask1;
        private readonly static SymMask iAtomMask2;
        #endregion
    }
}
