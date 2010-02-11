
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
using SymbianImageLib.E32Image.Image;
using SymbianImageLib.Common.Image;
using SymbianImageLib.Common.Streams;
using SymbianImageLib.Common.Content;
using SymbianImageLib.ROFS.Image;

namespace SymbianImageLib.ROFS.Content
{
    internal static class SIContentFactoryROFS
    {
        public static SIContent New( SIROFS aImage, string aName, uint aSize, long aPosition, TCheckedUid aUids )
        {
            SIContent ret = null;
            //
            bool isImage = SymbianImageE32.IsImageFile( (Stream) aImage.Stream, aPosition );
            if ( !isImage )
            {
                // We create either a code file (binary) or data file depending on the type of file at the specified location.
                ret = new SIContentROFSData( aImage, aName, aSize, aPosition, aUids );
            }
            else
            {
                ret = new SIContentROFSCode( aImage, aName, aSize, aPosition );
            }
            //
            return ret;
        }
    }
}
