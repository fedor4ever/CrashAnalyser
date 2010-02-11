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
using SymbianUtils.FileSystem;
using SymbianUtils.FileSystem.Utilities;
using CrashAnalyserServerExe.Engine;

namespace CrashAnalyserServerExe
{
    class EntryPoint
    {
        static int Main( string[] aArguments )
        {
            int error = CACmdLineException.KErrNone;
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
                    if ( error != CACmdLineException.KErrNone )
                    {
                        SymbianUtils.SymDebug.SymDebugger.Break();
                    }
                }
                catch ( Exception e )
                {
                    log.TraceAlways( "CrashAnalyserServerExe.Main() - EXCEPTION: " + e.Message );
                    log.TraceAlways( "CrashAnalyserServerExe.Main() - STACK:     " + e.StackTrace );
                    SymbianUtils.SymDebug.SymDebugger.Break();
                    error = -1;
                }
                //
                if ( error != 0 )
                {
                    log.TraceAlways( "CA completed with error: " + error );
                }
            }
            //
            return error;
        }
    }
}
