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
using SymbianStructuresLib.Compression.Common;

namespace SymbianImageLib.Common.Header
{
    public abstract class SIHeader : ITracer
    {
        #region Constructors
        protected SIHeader( SIImage aImage )
        {
            iImage = aImage;
        }
        #endregion

        #region Framework API
        public abstract TSymbianCompressionType CompressionType
        {
            get;
        }

        public abstract uint HeaderSize
        {
            get;
        }
        #endregion

        #region Properties
        public SIImage Image
        {
            get { return iImage; }
        }
        #endregion

        #region From ITracer
        public void Trace( string aMessage )
        {
            iImage.Trace( aMessage );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            iImage.Trace( aFormat, aParams );
        }
        #endregion

        #region Data members
        private readonly SIImage iImage;
        #endregion
    }
}
