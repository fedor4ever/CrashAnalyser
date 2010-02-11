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
using SLPluginSymbol.Reader;

namespace SLPluginSymbol.Utilities
{
    internal static class SymbolFileUtils
    {
        public static bool IsRelocatable( string aFileName )
        {
            bool ret = false;
            //
            using ( StreamReader reader = new StreamReader( aFileName ) )
            {
                string line = reader.ReadLine();
                while ( line != null )
                {
                    SymbolCreator.BasicSymbol symbol = SymbolCreator.Parse( line );
                    if ( symbol != null )
                    {
                        ret = ( symbol.iAddress == 0 );
                        break;
                    }
                    //
                    line = reader.ReadLine();
                }
            }
            //
            return ret;
        }
    }
}
