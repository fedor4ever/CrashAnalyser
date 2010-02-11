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
using SymbolLib.Sources.Symbol.File;
using SymbolLib.Sources.Map.File;

namespace SymbolLib.Sources.Map.Comparison
{
    class MapFileAddressRangeComparer : IComparer<MapFile>
    {
        #region IComparer Members
        int IComparer<MapFile>.Compare( MapFile aLeft, MapFile aRight )
        {
            int ret = -1;
            //
            if ( aLeft == null || aRight == null )
            {
                if ( aRight == null )
                {
                    ret = 1;
                }
            }
            else
            {
                ret = aLeft.AddressRangeStart.CompareTo( aRight.AddressRangeStart );
            }
            //
            return ret;
        }
        #endregion
    }
}
