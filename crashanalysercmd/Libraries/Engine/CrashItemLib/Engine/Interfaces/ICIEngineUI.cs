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
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using SymbianDebugLib.Engine;
using SymbianUtils;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Container;
using CrashItemLib.PluginAPI;
using CrashItemLib.Engine.Primer;
using CrashItemLib.Engine.Sources;
using CrashItemLib.Engine.Operations;
using CrashItemLib.Engine.ProblemDetectors;
using CrashItemLib.Sink;

namespace CrashItemLib.Engine.Interfaces
{
    public interface ICIEngineUI
    {
        void CITrace( string aMessage );

        void CITrace( string aFormat, params object[] aParameters );
    }
}
