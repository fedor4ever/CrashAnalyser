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
using SymbianStructuresLib.Compression.Common;
using SymbianImageLib.Common.Image;
using SymbianImageLib.Common.Header;
using SymbianImageLib.Common.Streams;
using SymbianImageLib.ROM.Content;
using SymbianImageLib.ROM.Header;
using SymbianImageLib.ROM.Structures;

namespace SymbianImageLib.ROM.Image
{
    public class SIROM : SIImage
    {
        #region Constructors
        public SIROM( ITracer aTracer, Stream aStream, string aName )
            : base( aTracer, new SIStream( aStream ), aName )
        {
            base.Trace( "[SymbianImageROM] Ctor() - START" );

            // Read the header and set base class relocation address
            iHeader = new SIHeaderROM( this, aStream );

            // We treat ourselves as one giant blob
            SIContentROM file = new SIContentROM( this );
            base.RegisterFile( file );

            base.Trace( "[SymbianImageROM] Ctor() - END" );
        }
        #endregion

        #region API
        public static bool IsROM( Stream aStream )
        {
            bool ret = false;
            //
            try
            {
                ret = SIHeaderROM.IsROM( aStream );
            }
            catch ( Exception )
            {
            }
            //
            return ret;
        }
        #endregion

        #region From SIImage
        public override SIHeader Header
        {
            get { return iHeader; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly SIHeaderROM iHeader;
        #endregion
    }
}
