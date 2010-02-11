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
using System.Text.RegularExpressions;
using System.IO;

namespace SymbianUtils.DataBuffer.Entry
{
    public class DataBufferUint
    {
        #region Constructors
        internal DataBufferUint( uint aUint, uint aAddress )
        {
            iAddress = aAddress;
            iUint = aUint;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint Address
        {
            get { return iAddress; }
        }

        public uint Uint
        {
            get { return iUint; }
        }
        #endregion

        #region Operators
        public static implicit operator uint( DataBufferUint aEntry )
        {
            return aEntry.Uint;
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly uint iAddress;
        private readonly uint iUint;
        #endregion
    }
}
