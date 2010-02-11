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
using CrashAnalyserEngine.Engine;
using CrashAnalyserEngine.Plugins;
using SymbianUtils.FileSystem;
using SymbianUtils.FileSystem.Utilities;

namespace CrashAnalyserConsole
{
    class EntryPoint
    {
        static int Main( string[] aArguments )
        {
            int error = 0;
            //
            using ( FSLog log = new FSLog( true ) )
            {
                try
                {
                    FSUtilities.ClearTempPath();
                    //
                    CACommandLineUI commandLineUi = new CACommandLineUI( aArguments, log );
                    error = commandLineUi.Run();
                    //
                    if ( error != CrashAnalyserEngine.Plugins.CAPlugin.KErrCommandLineNone )
                    {
                        SymbianUtils.SymDebug.SymDebugger.Break();
                    }
                }
                catch ( Exception e )
                {
                    log.TraceAlways( "CrashAnalyserConsole.Main() - EXCEPTION: " + e.Message );
                    log.TraceAlways( "CrashAnalyserConsole.Main() - STACK:     " + e.StackTrace );
                    SymbianUtils.SymDebug.SymDebugger.Break();
                    error = -1;
                }
                //
                if ( error != 0 )
                {
                    log.TraceAlways( "CA completed with error: " + error );
                }
            }

            if ( System.Diagnostics.Debugger.IsAttached )
            {
                System.Console.Write( "PRESS ANY KEY" );
                System.Console.ReadKey();
            }
            //
            return error;
        }
    }
}
