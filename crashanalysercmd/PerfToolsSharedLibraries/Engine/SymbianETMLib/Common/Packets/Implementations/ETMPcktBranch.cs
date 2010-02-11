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
using SymbianETMLib.Common.Utilities;

namespace SymbianETMLib.Common.Packets
{
    public class ETMPcktBranch : ETMPcktFiveByteRunBase 
    {
        #region Constructors
        public ETMPcktBranch()
            : base( "#######1" )
        {
            // In the first packet, bits 6:1 contain the payload.
            // Bit 7 indicates whether another packet follows
            // Bit 0 is forms the "branch packet" signature.
            iExtractionMaskFirst = CreateMask( "01111110" );
        }

        public ETMPcktBranch( byte aValue )
            : this()
        {
            base.RawByte = aValue;
        }
        #endregion

        #region API
        #endregion

        #region From PcktBase
        public override int Priority
        {
            get { return int.MaxValue; }
        }
        #endregion

        #region Properties
        public string ExtractedBitsFirstByte
        {
            get
            {
                byte raw = (byte) ( base.RawByte & iExtractionMaskFirst );
                raw >>= 1;
                string ret = SymBitUtils.GetBits( raw );

                // Only the first 6 bits are valid
                return ret.Substring( 2, 6 );
            }
        }
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        private readonly byte iExtractionMaskFirst;
        #endregion
    }
}
