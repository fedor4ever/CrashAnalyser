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
using SymbianUtils.Strings;
using SymbianUtils.Streams;
using SymbianImageLib.Common.Header;
using SymbianImageLib.Common.Image;
using SymbianStructuresLib.Compression.Common;
using SymbianImageLib.ROFS.Structures;
using SymbianImageLib.ROFS.Header.Types;

namespace SymbianImageLib.ROFS.Header
{
    internal abstract class SIHeaderROF : SIHeader
    {
        #region Static constructor
        public static SIHeaderROF New( SIImage aImage, Stream aStream )
        {
            byte[] signature = new byte[ 4 ];
            int readResult = aStream.Read( signature, 0, signature.Length );
            if ( readResult != 4 )
            {
                throw new Exception( "Unable to read ROF signature" );
            }
            
            // Put us back where we were
            aStream.Seek( -signature.Length, SeekOrigin.Current );

            // Convert signature to string and compare against known types.
            string headerText = StringParsingUtils.BytesToString( signature );
            aImage.Trace( "[SymbianImageHeaderROF] New() - headerText: {0}", headerText );
            //
            SIHeaderROF ret = null;
            switch ( headerText )
            {
            case "ROFX":
                ret = new SIHeaderROFS( aImage, aStream );
                break;
            case "ROFS":
                ret = new SIHeaderROFX( aImage, aStream );
                break;
            default:
                throw new NotSupportedException( "Unsupported ROF type" );
            }
            //
            return ret;
        }
        #endregion

        #region Constructors
        protected SIHeaderROF( SIImage aImage, Stream aStream )
            : base( aImage )
        {
            // Skip over identifier
            aStream.Seek( 4, SeekOrigin.Begin );

            // Read header size and then put us back at the start of the stream
            // ready to read the entire header.
            int headerSize = aStream.ReadByte();
            aStream.Seek( 0, SeekOrigin.Begin );
            //
            iHeaderData = new byte[ headerSize ];
            aStream.Read( iHeaderData, 0, iHeaderData.Length );
            //
            ReadHeaderData( iHeaderData );
        }
        #endregion

        #region API
        public static bool IsROFS( Stream aStream )
        {
            using ( SymbianStreamReaderLE reader = SymbianStreamReaderLE.New( aStream, SymbianStreamReaderLE.TCloseOperation.EResetPosition ) )
            {
                string signature = reader.ReadString( 4 );
                bool ret = ( signature == KImageROFS || signature == KImageROFX );
                return ret;
            }
        }
        #endregion

        #region Constants
        public const string KImageROFS = "ROFS";
        public const string KImageROFX = "ROFX";
        #endregion

        #region From SymbianImageHeader
        public override TSymbianCompressionType CompressionType
        {
            get
            {
                // ROFS image itself is not compressed - however, the individual ROFS entries 
                // themselves are likely compressed.
                return TSymbianCompressionType.ENone;
            }
        }

        public override uint HeaderSize
        {
            get { return iRofsHdr.Size; }
        }
        #endregion

        #region API
        #endregion

        #region Properties
        internal TRofsHeader InternalHeader
        {
            get { return iRofsHdr; }
        }
        #endregion

        #region Internal methods
        private void ReadHeaderData( byte[] aBuffer )
        {
            using ( MemoryStream stream = new MemoryStream( aBuffer ) )
            {
                using ( BinaryReader reader = new BinaryReader( stream ) )
                {
                    iRofsHdr.Read( reader );
                }
            }
        }
        #endregion

        #region Data members
        private TRofsHeader iRofsHdr = new TRofsHeader();
        private readonly byte[] iHeaderData;
        #endregion
    }
}
