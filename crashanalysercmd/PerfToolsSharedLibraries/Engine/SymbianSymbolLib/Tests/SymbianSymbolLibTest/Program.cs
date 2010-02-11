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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SymbianUtils.Range;
using SymbianUtils.Tracer;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Symbols;
using SymbianDebugLib.Engine;
using SymbianDebugLib.Entity;
using SymbianDebugLib.PluginAPI;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.PluginAPI.Types.Symbols;


namespace SymbianSymbolLibTest
{
    class EntryPoint
    {
        static void Main( string[] args )
        {
            using ( TestCode tests = new TestCode() )
            {
                tests.RunTests();
            }
        }
    }
}
