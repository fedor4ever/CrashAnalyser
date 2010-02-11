/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace SLPluginMap.Utilities
{
    internal static class MapFileUtils
    {
        public static TMapFileType Type( string aFileName )
        {
            TMapFileType ret = TMapFileType.ETypeUnknown;
            //
            if ( File.Exists( aFileName ) )
            {
                ret = TMapFileType.ETypeGCCE;
                //
                using ( StreamReader reader = new StreamReader( aFileName ) )
                {
                    string firstLine = reader.ReadLine().Trim().ToUpper();
                    if ( firstLine.StartsWith( "ARM LINKER, RVCT" ) )
                    {
                        ret = TMapFileType.ETypeRVCT;
                    }
                }
            }
            //
            return ret;
        }
    }
}
