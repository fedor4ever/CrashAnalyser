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
﻿using System;
using System.Collections.Generic;
using System.Text;
using CrashItemLib.Crash.Symbols;
using SymbianStructuresLib.Debug.Symbols;

namespace XmlFilePlugin.PluginImplementations.FileFormat
{
    static class CXmlFileUtilities
    {
        public static bool IsSymbolSerializable(CISymbol aSymbol)
        {
            bool ret = true;
            //
            if (aSymbol.IsNull)
            {
                ret = false;
            }
            else
            {
                TSymbolType type = aSymbol.Symbol.Type;
                ret = ( type != TSymbolType.EUnknown );
            }           
            return ret;
        }
    }

}
