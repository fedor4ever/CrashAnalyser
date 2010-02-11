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
using SymbianETMLib.Common.Types;

namespace SymbianETMLib.Common.Packets
{
    public abstract class ETMPcktFiveByteRunBase : ETMPcktBase 
    {
        #region Constructors
        static ETMPcktFiveByteRunBase()
        {
            // In subsequent packets, 7 bits are used and the top
            // bit indicates whether or not another byte follows.
            iExtractionMaskSubsequentOriginalScheme = new SymMask( "01111111" );
            iExtractionMaskSubsequentAlternativeScheme = new SymMask( "00111111" );
        }

        protected ETMPcktFiveByteRunBase( string aMask )
        {
            base.SetMask( aMask );

        }
        #endregion

        #region API
        internal string ExtractedBitsSubsequentBytes( TETMBranchCompressionScheme aScheme )
        {
            byte raw = (byte) iExtractionMaskSubsequentOriginalScheme.Apply( base.RawByte );
            string ret = System.Convert.ToString( raw, 2 );
            return ret;
        }

        public bool MoreToCome
        {
            get
            {
                bool ret = ( ( base.RawByte & 0x80 ) == 0x80 );
                return ret;
            }
        }
        #endregion

        #region From PcktBase
        public override int Priority
        {
            get { return int.MaxValue; }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        private readonly static SymMask iExtractionMaskSubsequentOriginalScheme;
        private readonly static SymMask iExtractionMaskSubsequentAlternativeScheme;
        #endregion
    }
}
