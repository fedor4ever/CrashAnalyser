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

namespace SymbianUtils.BasicTypes
{
    public class SymAddress
    {
        #region Constructors
        public SymAddress()
            : this( 0 )
        {
        }

        public SymAddress( uint aValue )
        {
            iAddress = aValue;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint Address
        {
            get { return iAddress; }
            set { iAddress = value; }
        }

        public virtual string AddressBinary
        {
            get
            {
                string ret = SymBitUtils.BeautifyBits( iAddress, 32 );
                return ret;
            }
        }

        public virtual string AddressHex
        {
            get
            {
                string ret = this.Address.ToString( "x8" );
                return ret;
            }
        }
        #endregion

        #region Operators
        public static implicit operator uint( SymAddress aAddress )
        {
            return aAddress.Address;
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            string ret = AddressHex;
            return ret;
        }
        #endregion

        #region Data members
        private uint iAddress = 0;
        #endregion
    }
}
