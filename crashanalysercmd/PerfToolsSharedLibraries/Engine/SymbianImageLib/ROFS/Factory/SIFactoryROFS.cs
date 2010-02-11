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
using SymbianImageLib.Common.Image;
using SymbianImageLib.Common.Factory;
using SymbianImageLib.ROFS.Image;

namespace SymbianImageLib.ROFS.Factory
{
    internal class SIFactoryROFS : SIFactory
    {
        #region Constructors
        public SIFactoryROFS()
        {
        }
        #endregion

        #region From SymbianImageFactory
        public override SIImage CreateImage( ITracer aTracer, Stream aStream, string aName )
        {
            SIImage ret = null;
            //
            bool isSupported = SIROFS.IsROFS( aStream );
            if ( isSupported )
            {
                ret = new SIROFS( aTracer, aStream, aName );
            }
            //
            return ret;
        }
        #endregion
    }
}
