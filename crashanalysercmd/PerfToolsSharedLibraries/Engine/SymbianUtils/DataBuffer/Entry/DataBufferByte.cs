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
    public class DataBufferByte
    {
        #region Constructors
        internal DataBufferByte( byte aByte, uint aAddress )
        {
            iAddress = aAddress;
            iByte = aByte;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint Address
        {
            get
            {
                uint ret = iAddress;
                //
                if ( Buffer != null )
                {
                    ret += Buffer.AddressOffset;
                }
                //
                return ret;
            }
        }

        public byte Byte
        {
            get { return iByte; }
        }
        #endregion

        #region Operators
        public static implicit operator byte( DataBufferByte aEntry )
        {
            return aEntry.Byte;
        }
        #endregion

        #region Internal properties
        internal DataBuffer Buffer
        {
            get { return iBuffer; }
            set { iBuffer = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly uint iAddress;
        private readonly byte iByte;
        private DataBuffer iBuffer = null;
        #endregion
    }
}
