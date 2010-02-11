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
using SymbianImageLib.ROM.Image;

namespace SymbianImageLib.ROM.Factory
{
    internal class SIFactoryROM : SIFactory
    {
        #region Constructors
        public SIFactoryROM()
        {
        }
        #endregion

        #region From SymbianImageFactory
        public override SIImage CreateImage( ITracer aTracer, Stream aStream, string aName )
        {
            SIImage ret = null;
            //
            bool isSupported = SIROM.IsROM( aStream );
            if ( isSupported )
            {
                ret = new SIROM( aTracer, aStream, aName );
            }
            //
            return ret;
        }
        #endregion
    }
}
