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

namespace SymbianStructuresLib.Compression.BytePair
{
    public class SymbianDecompressorBytePair : SymbianDecompressor
    {
        #region Constructors
        public SymbianDecompressorBytePair()
        {
        }
        #endregion

        #region From SymbianDecompressor
        public override TSymbianCompressionType Type
        {
            get { return TSymbianCompressionType.EBytePair; }
        }

        protected override int DoDecompressRaw( IntPtr aSource, int aSourceLength, IntPtr aDestination, int aDestinationLength )
 	    {
            int ret = SymbianBytePairUnpackRaw( aSource, aSourceLength, aDestination, aDestinationLength );
            return ret;
        }

        protected override int DoDecompressImage( IntPtr aSource, int aSourceLength, IntPtr aDestination, int aDestinationLength, out int aAmountOfSourceRead )
        {
            int ret = 0;
            aAmountOfSourceRead = 0;
            IntPtr pAmountSrcRead = Marshal.AllocHGlobal( 4 );
            //
            try
            {
                ret = SymbianBytePairUnpackImage( aSource, aSourceLength, aDestination, aDestinationLength, pAmountSrcRead );
                aAmountOfSourceRead = (int) Marshal.PtrToStructure( pAmountSrcRead, typeof( Int32 ) );
            }
            finally
            {
                Marshal.FreeHGlobal( pAmountSrcRead );
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region Native methods
        [DllImport("SymbianNativeTools.dll")]
        public static extern int SymbianBytePairUnpackRaw( IntPtr aSource, int aSourceSize, IntPtr aDest, int aDestSize );
        [DllImport("SymbianNativeTools.dll")]
        public static extern int SymbianBytePairUnpackImage( IntPtr aSource, 
                                                             int aSourceSize, 
                                                             IntPtr aDest, 
                                                             int aDestSize, 
                                                             IntPtr aAmountOfSourceRead );
        #endregion

        #region Data members
        #endregion
    }
}
