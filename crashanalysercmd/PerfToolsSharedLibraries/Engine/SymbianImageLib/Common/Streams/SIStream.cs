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
using SymbianUtils.Streams;
using SymbianImageLib.Common.Header;
using SymbianStructuresLib.Compression.Common;

namespace SymbianImageLib.Common.Streams
{
    internal class SIStream : DisposableObject
    {
        #region Enumerations
        public enum TOwnershipType
        {
            EOwned = 0,
            EOwnedExternally
        }
        #endregion

        #region Constructors
        public SIStream()
            : this( null, TOwnershipType.EOwned )
        {
        }

        public SIStream( Stream aStream )
            : this( aStream, TOwnershipType.EOwned )
        {
        }

        public SIStream( Stream aStream, TOwnershipType aType )
        {
            SwitchStream( aStream, aType );
        }
        #endregion

        #region Framework API
        public virtual bool InRange( long aPosition )
        {
            bool ret = ( aPosition >= 0 && aPosition < this.Length );
            return ret;
        }

        public virtual void Seek( long aPosition, SeekOrigin aOrigin )
        {
            iStream.Seek( aPosition, aOrigin );
        }

        public virtual int Read( byte[] aBuffer, int aOffset, int aCount )
        {
            int ret = iStream.Read( aBuffer, aOffset, aCount );
            return ret;
        }

        public virtual void Write( byte[] aBuffer )
        {
            Write( aBuffer, 0, aBuffer.Length );
        }

        public virtual void Write( SIStream aFrom, int aCount )
        {
            Write( (Stream) aFrom, aCount );
        }

        public virtual void Write( Stream aFrom, int aCount )
        {
            byte[] temp = new byte[ aCount ];
            int ret = aFrom.Read( temp, 0, aCount );
            if ( ret != aCount )
            {
                throw new Exception( "Unable to read required number of bytes from stream" );
            }
            Write( temp, 0, aCount );
        }

        public virtual void Write( byte[] aBuffer, int aOffset, int aCount )
        {
            iStream.Write( aBuffer, aOffset, aCount );
        }
        #endregion

        #region API
        public void SwitchStream( Stream aStream, TOwnershipType aType )
        {
            if ( iStream != null )
            {
                if ( iOwnership == TOwnershipType.EOwned )
                {
                    iStream.Dispose();
                    iStream = null;
                    iLength = 0;
                }
            }
            //
            iStream = aStream;
            iOwnership = aType;
            //
            if ( iStream != null )
            {
                // Cache this because calling it is very expensive.
                iLength = iStream.Length;
            }
        }

        public void Close()
        {
            base.Dispose();
        }

        public void Seek( long aPosition )
        {
            Seek( aPosition, SeekOrigin.Begin );
        }

        public SymbianStreamReaderLE CreateReader()
        {
            return SymbianStreamReaderLE.New( iStream );
        }

        public SymbianStreamReaderLE CreateReader( SymbianStreamReaderLE.TCloseOperation aCloseOperation )
        {
            return SymbianStreamReaderLE.New( iStream, aCloseOperation );
        }
        #endregion

        #region Properties
        public long Length
        {
            get { return iLength; }
        }
        #endregion

        #region Operators
        public static explicit operator Stream( SIStream aStream )
        {
            return aStream.InternalStream;
        }
        #endregion

        #region Internal methods
        protected Stream InternalStream
        {
            get { return iStream; }
        }
        #endregion

        #region Data members
        private Stream iStream = null;
        private TOwnershipType iOwnership = TOwnershipType.EOwned;
        private long iLength = 0;
        #endregion
    }
}
