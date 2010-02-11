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
using System.Runtime.InteropServices;
using SymbianStructuresLib.Compression.Common;

namespace SymbianStructuresLib.Compression.Inflate
{
    public class SymbianDecompressorInflate : SymbianDecompressor
    {
        #region Constructors
        public SymbianDecompressorInflate()
        {
        }
        #endregion

        #region From SymbianDecompressor
        public override TSymbianCompressionType Type
        {
            get { return TSymbianCompressionType.EDeflate; }
        }

        protected override int DoDecompressRaw( IntPtr aSource, int aSourceLength, IntPtr aDestination, int aDestinationLength )
        {
            throw new NotSupportedException();
        }

        protected override int DoDecompressImage( IntPtr aSource, int aSourceLength, IntPtr aDestination, int aDestinationLength, out int aAmountOfSourceRead )
        {
            int ret = SymbianInflateImage( aSource, aSourceLength, aDestination, aDestinationLength );
            aAmountOfSourceRead = aSourceLength;
            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        [DllImport("SymbianNativeTools.dll")]
        public static extern int SymbianInflateImage( IntPtr aSource, int aSourceSize, IntPtr aDest, int aDestSize );
        #endregion

        #region Data members
        #endregion
    }
}
