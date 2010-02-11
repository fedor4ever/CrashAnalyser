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
using SymbolLib.Generics;
using SymbianUtils.Range;

namespace SymbolLib.Utils
{
    public class SymbolAddressRange : AddressRangeCollection
	{
		#region Constructors
        public SymbolAddressRange( GenericSymbolCollection aCollection )
		{
            foreach ( GenericSymbol sym in aCollection )
            {
                base.Add( sym.AddressRange );
            }
		}
        #endregion

        #region API
        public bool IsWithinRange( long aValue )
        {
            return base.Contains( (uint) aValue );
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
