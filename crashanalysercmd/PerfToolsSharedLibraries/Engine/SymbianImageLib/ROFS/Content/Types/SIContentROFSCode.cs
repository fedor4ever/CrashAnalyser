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
using SymbianImageLib.E32Image.Image;
using SymbianImageLib.E32Image.Content;
using SymbianImageLib.E32Image.Header;
using SymbianImageLib.ROFS.Image;

namespace SymbianImageLib.ROFS.Content
{
    public class SIContentROFSCode : SIContent
    {
        #region Constructors
        internal SIContentROFSCode( SIROFS aImage, string aName, uint aSize, long aPosition )
            : base( aImage )
        {
            // We need to give the E32Image access to the underlying ROFS image stream, but we don't
            // want to transfer ownership - ownership remains entirely with the SymbianImageROFS object.
            SIStream e32ImageStream = new SIStream( (Stream) ImageStream, SIStream.TOwnershipType.EOwnedExternally );
            try
            {
                iE32Image = new SymbianImageE32( aName, aSize, aPosition, e32ImageStream, aImage );
                if ( iE32Image.Count == 0 )
                {
                    throw new Exception( "Invalid E32Image file" );
                }
            }
            catch ( Exception e )
            {
                e32ImageStream.Close();
                throw e;
            }
        }
        #endregion

        #region From SymbianImageContentFile
        public override TSymbianCompressionType CompressionType
        {
            get { return iE32Image.CompressionType; }
        }

        public override uint ProvideDataUInt32( uint aAddress )
        {
            uint ret = E32ImageContent.ProvideDataUInt32( aAddress );
            return ret;
        }

        public override ushort ProvideDataUInt16( uint aAddress )
        {
            ushort ret = E32ImageContent.ProvideDataUInt16( aAddress );
            return ret;
        }

        public override string FileName
        {
            get { return E32ImageContent.FileName; }
        }

        public override uint FileSize
        {
            get { return E32ImageContent.FileSize; }
        }

        public override uint ContentSize
        {
            get
            {
                uint ret = E32ImageContent.ContentSize;
                return ret;
            }
        }

        public override TCheckedUid Uid
        {
            get { return E32ImageContent.Uid; }
        }

        public override byte[] GetAllData()
        {
            return E32ImageContent.GetAllData();
        }

        protected override void DoDecompress()
        {
            // We can prepare the content synchronously because we're either running
            // in a separate thread (async) or supposed to be performing the operation
            // synchronously in any case.
            SIContentE32Image content = E32ImageContent;
            content.PrepareContent( TSynchronicity.ESynchronous );
        }

        protected override void OnRelocationAddressChanged( uint aOld, uint aNew )
        {
            // Cascade the relocation change to the E32Image
            E32ImageContent.RelocationAddress = aNew;
        }

        protected override bool GetIsContentPrepared()
        {
            return E32ImageContent.IsContentPrepared;
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
                iE32Image.Dispose();
            }
        }
        #endregion

        #region Properties
        internal SIContentE32Image E32ImageContent
        {
            get { return iE32Image[ 0 ] as SIContentE32Image; }
        }
        #endregion

        #region Data members
        private readonly SymbianImageE32 iE32Image;
        #endregion
    }
}
