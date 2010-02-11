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
* The class CCrashInfoFileSink is part of CrashAnalyser CrashInfoFile plugin.
* Sink output plugin for Crash Info File format.
* Implements CISink which is automatically discovered by CISinkManager.
* Serialize function produces a Crash Info file formatted text output
* 
*/

using System;
using System.Collections.Generic;
using System.Text;

using CrashItemLib.Sink;

//using CrashItemLib.Crash.Symbols;
//using CrashItemLib.Crash.CodeSegs;
//using CrashItemLib.Crash.Registers;
//using CrashItemLib.Crash.Threads;


using CrashInfoFilePlugin.PluginImplementations.FileFormat;
using System.IO;

namespace CrashInfoFilePlugin.PluginImplementations.Sink
{
    public class CCrashInfoFileSink : CISink
    {
        #region Constants
        public const string KCrashInfoSinkName = "Crash Info File";
        #endregion

        #region Constructors
        public CCrashInfoFileSink(CISinkManager aManager)
            : base(KCrashInfoSinkName, aManager)
        {
        }
        #endregion

        #region From CISink
        public override object Serialize(CISinkSerializationParameters aParams)
        {            
            CCrashInfoFileDocument document = new CCrashInfoFileDocument();

            //Read information relevant to crash info file from container to internal variables
            document.ReadDataFromContainer(aParams);

            //Override default file extension
            aParams.FileExtensionFailed = ".corrupt_ci";
            aParams.FileExtensionSuccess = ".ci";

            //Write document's internal data to file
            string newFileName = string.Empty;
            using ( Stream output = aParams.CreateFile( out newFileName ) )
            {
                using ( StreamWriter sw = new StreamWriter( output, Encoding.ASCII ) )
                {
                    document.WriteToStream( sw );
                }
            }
            return newFileName;
        }
        #endregion
    }
}
