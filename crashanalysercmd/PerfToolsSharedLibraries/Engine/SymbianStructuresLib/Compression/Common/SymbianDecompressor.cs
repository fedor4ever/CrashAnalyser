
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
using SymbianUtils;
using SymbianStructuresLib.Compression.BytePair;
using SymbianStructuresLib.Compression.Inflate;

namespace SymbianStructuresLib.Compression.Common
{
    public abstract class SymbianDecompressor : DisposableObject
    {
        #region Factory
        public static SymbianDecompressor NewByType( TSymbianCompressionType aType )
        {
            SymbianDecompressor ret = null;
            //
            switch ( aType )
            {
            case TSymbianCompressionType.EBytePair:
                ret = new SymbianDecompressorBytePair();
                break;
            case TSymbianCompressionType.EDeflate:
                ret = new SymbianDecompressorInflate();
                break;
            }
            //
            return ret;
        }
        #endregion

        #region Constructors
        protected SymbianDecompressor()
        {
        }
        #endregion

        #region Framework API
        public abstract TSymbianCompressionType Type
        {
            get;
        }

        protected abstract int DoDecompressRaw( IntPtr aSource, int aSourceLength, IntPtr aDestination, int aDestinationLength );

        protected virtual int DoDecompressImage( IntPtr aSource, int aSourceLength, IntPtr aDestination, int aDestinationLength, out int aAmountOfSourceRead )
        {
            throw new NotImplementedException();
        }
        #endregion

        #region API
        public int DecompressRaw( byte[] aSource, byte[] aDestination )
        {
            if ( aSource == null || aDestination == null )
            {
                throw new ArgumentException( "Must supply data buffers" );
            }
            //
            int ret = 0;
            //
            unsafe
            {
                fixed ( byte* source = aSource )
                {
                    fixed ( byte* dest = aDestination )
                    {
                        IntPtr pSource = new IntPtr( source );
                        IntPtr pDest = new IntPtr( dest );
                        //
                        ret = DoDecompressRaw( pSource, aSource.Length, pDest, aDestination.Length );
                    }
                }
            }
            //
            return ret;
        }

        public int DecompressImage( byte[] aSource, byte[] aDestination, out int aAmountOfSourceRead )
        {
            if ( aSource == null || aDestination == null )
            {
                throw new ArgumentException( "Must supply data buffers" );
            }
            //
            int ret = 0;
            //
            unsafe
            {
                fixed ( byte* source = aSource )
                {
                    fixed ( byte* dest = aDestination )
                    {
                        IntPtr pSource = new IntPtr( source );
                        IntPtr pDest = new IntPtr( dest );
                        //
                        ret = DoDecompressImage( pSource, aSource.Length, pDest, aDestination.Length, out aAmountOfSourceRead );
                    }
                }
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
