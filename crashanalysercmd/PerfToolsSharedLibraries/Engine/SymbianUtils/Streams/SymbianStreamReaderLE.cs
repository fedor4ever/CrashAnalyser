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

namespace SymbianUtils.Streams
{
    public class SymbianStreamReaderLE : DisposableObject
    {
        #region Enumerations
        [Flags]
        public enum TCloseOperation
        {
            ENone = 0,
            ECloseStream = 1,
            EResetPosition = 2
        }
        #endregion

        #region Factory
        public static SymbianStreamReaderLE New( Stream aStream )
        {
            return new SymbianStreamReaderLE( aStream, TCloseOperation.ENone );
        }

        public static SymbianStreamReaderLE New( Stream aStream, TCloseOperation aFlags )
        {
            return new SymbianStreamReaderLE( aStream, aFlags );
        }
        #endregion

        #region Constructors
        private SymbianStreamReaderLE( Stream aStream, TCloseOperation aFlags )
        {
            iStream = aStream;
            iFlags = aFlags;
            iOriginalPosition = aStream.Position;
        }
        #endregion

        #region API
        public sbyte ReadInt8()
        {
            sbyte b = (sbyte) iStream.ReadByte();
            return b;
        }

        public short ReadInt16()
        {
            sbyte low = ReadInt8();
            sbyte high = ReadInt8();
            int ret = ( high << 8 ) + low;
            return (short) ret;
        }

        public int ReadInt32()
        {
            int low = ReadInt16();
            int high = ReadInt16();
            int ret = ( high << 16 ) + low;
            return ret;
        }

        public byte ReadUInt8()
        {
            return (byte) iStream.ReadByte();
        }

        public ushort ReadUInt16()
        {
            byte[] temp = new byte[ 2 ];
            iStream.Read( temp, 0, temp.Length );
            //
            ushort ret = 0;
            for ( int i = 1; i >= 0; i-- )
            {
                ret |= temp[ i ];
                if ( i > 0 )
                {
                    ret <<= 8;
                }
            }
            return ret;
        }

        public uint ReadUInt32()
        {
            uint low = ReadUInt16();
            uint high = ReadUInt16();
            uint ret = ( high << 16 ) + low;
            return ret;
        }

        public string ReadString( int aLengthInCharacters )
        {
            byte[] bytes = new byte[ aLengthInCharacters ];
            //
            string ret = string.Empty;
            if ( iStream.Read( bytes, 0, bytes.Length ) == bytes.Length )
            {
                ret = SymbianUtils.Strings.StringParsingUtils.BytesToString( bytes );
            }
            //
            return ret;
        }

        public string ReadStringUTF16( int aLengthInCharacters )
        {
            byte[] bytes = new byte[ aLengthInCharacters * 2 ];
            //
            string ret = string.Empty;
            if ( iStream.Read( bytes, 0, bytes.Length ) == bytes.Length )
            {
                ret = Encoding.Unicode.GetString( bytes );
            }
            //
            return ret;
        }

        public byte[] ReadBytes( int aCount )
        {
            byte[] bytes = new byte[ aCount ];
            int ret = iStream.Read( bytes, 0, bytes.Length );
            if ( ret != bytes.Length )
            {
                throw new Exception( "Unable to read byte data" );
            }
            return bytes;
        }

        public long Seek( long aPosition )
        {
            long ret = Seek( aPosition, SeekOrigin.Begin );
            return ret;
        }

        public long Seek( long aPosition, SeekOrigin aOrigin )
        {
            long ret = iStream.Seek( aPosition, aOrigin );
            return ret;
        }
        #endregion

        #region Properties
        public Stream BaseStream
        {
            get { return iStream; }
        }

        public long Position
        {
            get { return iStream.Position; }
            set
            {
                Seek( value );
            }
        }

        public long Offset
        {
            get
            {
                long ret = Position - iOriginalPosition;
                return ret;
            }
        }
        #endregion

        #region From DisposableObject
        protected override void CleanupManagedResources()
        {
            try
            {
                base.CleanupManagedResources();
            }
            finally
            {
                if ( iStream != null )
                {
                    if ( ( iFlags & TCloseOperation.EResetPosition ) != 0 )
                    {
                        iStream.Seek( iOriginalPosition, SeekOrigin.Begin );
                    }
                    //
                    if ( ( iFlags & TCloseOperation.ECloseStream ) != 0 )
                    {
                        iStream.Close();
                    }
                }
                iStream = null;
            }
        }
        #endregion

        #region Data members
        private readonly TCloseOperation iFlags;
        private readonly long iOriginalPosition;
        private Stream iStream = null;
        #endregion
    }
}
