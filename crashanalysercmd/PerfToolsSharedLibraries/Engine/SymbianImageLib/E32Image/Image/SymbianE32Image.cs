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
using SymbianUtils.Tracer;
using SymbianUtils.Streams;
using SymbianStructuresLib.Uids;
using SymbianStructuresLib.Compression.Common;
using SymbianImageLib.Common.Header;
using SymbianImageLib.Common.Image;
using SymbianImageLib.Common.Streams;
using SymbianImageLib.E32Image.Content;
using SymbianImageLib.E32Image.Header;
using SymbianImageLib.E32Image.Exceptions;

namespace SymbianImageLib.E32Image.Image
{
    public class SymbianImageE32 : SIImage
    {
        #region Constructors
        public SymbianImageE32( FileInfo aFileInfo, ITracer aTracer )
            : this( aFileInfo.FullName, (uint) aFileInfo.Length, 0, new SIStream( aFileInfo.OpenRead() ), aTracer )
        {
        }

        internal SymbianImageE32( string aImageName, uint aImageContentSize, long aImageContentOffset, SIStream aStream, ITracer aTracer )
            : base( aTracer, aStream, aImageName )
        {
            iContentOffsetWithinDataStream = aImageContentOffset;
            //
            using ( SymbianStreamReaderLE reader = base.Stream.CreateReader() )
            {
                reader.Seek( iContentOffsetWithinDataStream );
                iHeader = new SIHeaderE32Image( this, reader );
            }

            // Since we are a single e32 image, we have a single e32 Image file descriptor
            SIContentE32Image file = new SIContentE32Image( this, aImageName, aImageContentSize, aImageContentOffset );
            base.RegisterFile( file );
        }
        #endregion

        #region From SIImage
        public override SIHeader Header
        {
            get { return iHeader; }
        }
        #endregion

        #region API
        public static bool IsImageFile( Stream aStream )
        {
            return IsImageFile( aStream, aStream.Position );
        }
        
        public static bool IsImageFile( Stream aStream, long aPosition )
        {
            bool ret = false;
            //
            try
            {
                using ( SymbianStreamReaderLE reader = SymbianStreamReaderLE.New( aStream, SymbianStreamReaderLE.TCloseOperation.EResetPosition ) )
                {
                    aStream.Seek( aPosition, SeekOrigin.Begin );
                    //
                    byte[] bytes = reader.ReadBytes( SIHeaderE32Image.KMinimumSize );
                    ret = SIHeaderE32Image.IsSymbianImageHeader( bytes );
                }
            }
            catch ( Exception )
            {
            }
            return ret;
        }
        #endregion

        #region Properties
        internal TCheckedUid Uid
        {
            get
            {
                return iHeader.Uid;
            }
        }

        internal long ContentOffsetWithinDataStream
        {
            get { return iContentOffsetWithinDataStream; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly SIHeaderE32Image iHeader;
        private readonly long iContentOffsetWithinDataStream;
        #endregion
    }
}
