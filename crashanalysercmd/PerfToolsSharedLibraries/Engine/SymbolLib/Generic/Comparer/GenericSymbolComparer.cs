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
using System.IO;
using System.Collections.Generic;

namespace SymbolLib.Generics
{
	internal class GenericSymbolComparer : IComparer<GenericSymbol>
	{
        #region IComparer<GenericSymbol> Members
        int IComparer<GenericSymbol>.Compare( GenericSymbol aLeft, GenericSymbol aRight )
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
                int ret2 = aLeft.AddressRange.CompareTo( aRight.AddressRange );
                GenericSymbol left = aLeft;
                System.Diagnostics.Debug.Assert( left.EndAddress >= left.Address );
                GenericSymbol right = aRight;
                System.Diagnostics.Debug.Assert( right.EndAddress >= right.Address );
                //
                if ( left.Address == right.Address && left.EndAddress == right.EndAddress )
                {
                    ret = 0;
                }
                else if ( left.EndAddress == right.Address )
                {
                    System.Diagnostics.Debug.Assert( left.Address < right.Address );
                    System.Diagnostics.Debug.Assert( right.EndAddress >= left.EndAddress );
                    //
                    ret = -1;
                }
                else if ( left.Address == right.EndAddress )
                {
                    System.Diagnostics.Debug.Assert( right.Address < left.Address );
                    System.Diagnostics.Debug.Assert( left.EndAddress >= right.EndAddress );
                    //
                    ret = 1;
                }
                else if ( left.Address > right.EndAddress )
                {
                    System.Diagnostics.Debug.Assert( left.EndAddress > right.EndAddress );
                    System.Diagnostics.Debug.Assert( left.EndAddress > right.Address );
                    ret = 1;
                }
                else if ( left.EndAddress < right.Address )
                {
                    System.Diagnostics.Debug.Assert( left.Address < right.EndAddress );
                    System.Diagnostics.Debug.Assert( right.EndAddress > left.EndAddress );
                    ret = -1;
                }
                System.Diagnostics.Debug.Assert( ret2 == ret );
            }
            //
            return ret;
          }
        #endregion
    }
}
