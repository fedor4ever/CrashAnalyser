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
using System.IO;

namespace SymbianImageLib.ROM.Structures
{
    internal class TRomLoaderHeader
    {
        #region Constructors
        public TRomLoaderHeader()
        {
            uint sizeOfRomLoad = TRomLoad.Size;
            iPadding = new byte[ KRomWrapperSize - sizeOfRomLoad ];
        }
        #endregion

        #region API
        public void Read( BinaryReader aReader )
        {
            iRomLoad.Read( aReader );
            aReader.BaseStream.Read( iPadding, 0, iPadding.Length );
        }
        #endregion

        #region Properties
        public uint Size
        {
            get
            {
                uint ret = TRomLoad.Size;
                ret += (uint) iPadding.Length;
                return ret;
            }
        }
        #endregion

        #region Internal constants
        private const int KRomWrapperSize = 0x100;
        private const int KRomLoadSize = 0xc8;
        #endregion

        #region Data members
        private TRomLoad iRomLoad = new TRomLoad();
        private byte[] iPadding = new byte[ 0 ];
        #endregion
    }
}
