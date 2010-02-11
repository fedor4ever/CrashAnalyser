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
using SymbianUtils.Tracer;
using SymbianUtils.Streams;
using SymbianStructuresLib.Uids;
using SymbianStructuresLib.Compression.Common;
using SymbianImageLib.Common.Content;
using SymbianImageLib.Common.Image;
using SymbianImageLib.Common.Streams;
using SymbianImageLib.ROFS.Image;

namespace SymbianImageLib.ROFS.Content
{
    public class SIContentROFSData : SIContent
    {
        #region Constructors
        internal SIContentROFSData( SIROFS aImage, string aName, uint aSize, long aPosition, TCheckedUid aUids )
            : base( aImage )
        {
            iFileName = aName;
            iUids = aUids;
            iFileSize = aSize;
            iPosition = aPosition;
        }
        #endregion

        #region From SymbianImageContentFile
        public override TSymbianCompressionType CompressionType
        {
            get
            {
                return TSymbianCompressionType.ENone;
            }
        }

        public override string FileName
        {
            get { return iFileName; }
        }

        public override uint FileSize
        {
            get { return iFileSize; }
        }

        public override uint ContentSize
        {
            get { return FileSize; }
        }

        public override TCheckedUid Uid
        {
            get { return iUids; }
        }

        public override byte[] GetAllData()
        {
            byte[] ret = null;
            //
            using ( SymbianStreamReaderLE reader = SymbianStreamReaderLE.New( (Stream) base.ImageStream, SymbianStreamReaderLE.TCloseOperation.EResetPosition ) )
            {
                reader.Position = iPosition;
                ret = reader.ReadBytes( (int) iFileSize );
            }
            //
            return ret;
        }

        public override bool IsCode
        {
            get { return false; }
        }

        protected override uint DoProvideDataUInt32( uint aTranslatedAddress )
        {
            using ( SymbianStreamReaderLE reader = Image.Stream.CreateReader( SymbianStreamReaderLE.TCloseOperation.ENone ) )
            {
                reader.Seek( aTranslatedAddress );
                uint ret = reader.ReadUInt32();
                return ret;
            }
        }

        protected override ushort DoProvideDataUInt16( uint aTranslatedAddress )
        {
            using ( SymbianStreamReaderLE reader = Image.Stream.CreateReader( SymbianStreamReaderLE.TCloseOperation.ENone ) )
            {
                reader.Seek( aTranslatedAddress );
                ushort ret = reader.ReadUInt16();
                return ret;
            }
        }

        protected override bool GetIsContentPrepared()
        {
            // We just read straight from the file
            return true;
        }
        #endregion

        #region Properties
        public long Position
        {
            get { return iPosition; }
        }
        #endregion

        #region Data members
        private readonly string iFileName;
        private readonly uint iFileSize;
        private readonly long iPosition;
        private readonly TCheckedUid iUids;
        #endregion
    }
}
