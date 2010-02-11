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
using SymbianStructuresLib.Version;

namespace SymbianImageLib.ROM.Structures
{
    internal class SRomPageInfo
    {
        #region Enumerations
        public enum TSymbianCompressionType
        {
            ENoCompression = 0,
            EBytePair,
        }
        #endregion

        #region Static constructors
        public static SRomPageInfo New( Stream aStream, uint aOffsetToStartOfRomData )
        {
            byte[] data = new byte[ Size ];
            aStream.Read( data, 0, data.Length );
            return new SRomPageInfo( data, aOffsetToStartOfRomData );
        }
        #endregion

        #region Constructors
        private SRomPageInfo( byte[] aArray, uint aOffsetToStartOfRomData )
        {
            uint delta = 0;
            //
            iDataStart = aOffsetToStartOfRomData + ReadUint( aArray, ref delta );
            iDataSize = ReadUshort( aArray, ref delta );
            iCompressionType = (TSymbianCompressionType) aArray[ delta++ ];
            iPagingAttributes = (TAttributes) aArray[ delta++ ];
        }
        #endregion

        #region Constants
        public const int KPageSize = 0x1000;
        #endregion

        #region Properties
        public static uint Size
        {
            get { return 8; }
        }

        public bool IsPageable
        {
            get { return ( iPagingAttributes & TAttributes.EPageable ) == TAttributes.EPageable; }
        }

        public TSymbianCompressionType CompressionType
        {
            get
            { 
                // Rather frustratingly, even unpageable areas of the ROM still report their
                // compression type as "byte pair". We must zero this out, since they are
                // definitely not compressed.
                if ( !IsPageable )
                {
                    return TSymbianCompressionType.ENoCompression;
                }
                //
                return iCompressionType;
            }
        }

        public uint DataStart
        {
            get { return iDataStart; }
        }

        public uint DataSize
        {
            get { return iDataSize; }
        }
        #endregion

        #region Internal enumerations
        [Flags]
        private enum TAttributes
        {
            EPageable = 1 << 0
        }
        #endregion

        #region Internal methods
        private static uint ReadUint( byte[] aArray, ref uint aOffset )
        {
            uint ret = 0;
            //
            ret += (uint) ( aArray[ aOffset + 0 ] << 00 );
            ret += (uint) ( aArray[ aOffset + 1 ] << 08 );
            ret += (uint) ( aArray[ aOffset + 2 ] << 16 );
            ret += (uint) ( aArray[ aOffset + 3 ] << 24 );
            //
            aOffset += 4;
            //
            return ret;
        }

        private static ushort ReadUshort( byte[] aArray, ref uint aOffset )
        {
            ushort ret = 0;
            //
            ret += (ushort) ( aArray[ aOffset + 0 ] << 00 );
            ret += (ushort) ( aArray[ aOffset + 1 ] << 08 );
            //
            aOffset += 2;
            //
            return ret;
        }
        #endregion

        #region Data members
        private uint iDataStart;
        private ushort iDataSize;
        private TSymbianCompressionType iCompressionType;
        private TAttributes iPagingAttributes;
        #endregion
    }
}
