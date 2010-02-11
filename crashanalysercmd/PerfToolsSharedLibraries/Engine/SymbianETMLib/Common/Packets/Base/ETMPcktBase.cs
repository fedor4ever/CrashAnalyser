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
using SymbianETMLib.Common.Utilities;

namespace SymbianETMLib.Common.Packets
{
    public abstract class ETMPcktBase
    {
        #region Constructors
        protected ETMPcktBase()
            : this( 0 )
        {
        }

        protected ETMPcktBase( SymByte aValue )
        {
            iRawByte = aValue;
        }
        #endregion

        #region API
        public bool IsBitSet( int aBitNumber )
        {
            bool ret = RawByte[ aBitNumber ];
            return ret;
        }

        protected SymByte CreateMask( string aBinary )
        {
            return (byte) SymBitUtils.CreateMask( aBinary );
        }

        protected void SetMask( string aHigh, string aLow )
        {
            SetMask( aHigh + aLow );
        }

        protected void SetMask( string aSignificantBitValues )
        {
            iBitMask = SymBitUtils.CreateMask( aSignificantBitValues, out iBitValue );
        }
        #endregion

        #region API - framework
        public virtual bool Matches( SymByte aOpCode )
        {
            uint masked = (byte) ( aOpCode & BitMask );
            if ( masked == BitValue )
            {
                return true;
            }

            return false;
        }

        public virtual int Priority
        {
            get { return 0; }
        }
        #endregion

        #region Properties
        public string Binary
        {
            get
            {
                StringBuilder ret = new StringBuilder();
                ret.Append( Convert.ToString( iRawByte, 2 ).PadLeft( 8, '0' ) );
                return ret.ToString();
            }
        }

        public SymByte RawByte
        {
            get { return iRawByte; }
            set { iRawByte = value; }
        }

        protected SymByte BitMask
        {
            get { return iBitMask; }
        }

        protected SymByte BitValue
        {
            get { return iBitValue; }
        }
        #endregion

        #region Data members
        private SymByte iBitMask = 0;
        private SymByte iBitValue = 0;
        private SymByte iRawByte = 0;
        #endregion
    }
}
