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

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SymbianStackAlgorithmAccurate.Prologue
{
    internal class APESymbolNotFound : Exception
    {
        #region Constructors
        public APESymbolNotFound( uint aAddress )
            : this( aAddress, string.Empty )
        {
        }

        public APESymbolNotFound( uint aAddress, string aMessage )
            : base( aMessage )
        {
            iAddress = aAddress;
        }
        #endregion

        #region Properties
        public uint Address
        {
            get { return iAddress; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.AppendFormat( "{0} [0x{1:x8}]", base.Message, iAddress );
            return ret.ToString();
        }
        #endregion

        #region Data members
        private readonly uint iAddress;
        #endregion
    }

    internal class APESymbolNotFoundCodeSegmentUnavailable : APESymbolNotFound
    {
        #region Constructors
        public APESymbolNotFoundCodeSegmentUnavailable( uint aAddress )
            : base( aAddress )
        {
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.AppendFormat( "No code segment exists that describes the specified symbol address [0x{0:x8}]", base.Address );
            return ret.ToString();
        }
        #endregion
    }
}
