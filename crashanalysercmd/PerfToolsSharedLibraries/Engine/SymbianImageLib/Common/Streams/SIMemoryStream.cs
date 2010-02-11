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
using SymbianUtils;
using SymbianImageLib.Common.Header;
using SymbianStructuresLib.Compression.Common;

namespace SymbianImageLib.Common.Streams
{
    internal class SIMemoryStream : SIStream
    {
        #region Constructors
        public SIMemoryStream( uint aSize )
        {
            iData = new byte[ aSize ];
            base.SwitchStream( new MemoryStream( iData ), TOwnershipType.EOwned );
        }
        #endregion

        #region From SymbianImageStream
        #endregion

        #region API
        #endregion

        #region Properties
        public byte[] Data
        {
            get { return iData; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly byte[] iData;
        #endregion
    }
}
