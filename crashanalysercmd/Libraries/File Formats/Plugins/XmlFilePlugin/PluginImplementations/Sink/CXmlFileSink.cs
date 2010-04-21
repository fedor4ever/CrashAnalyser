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
using System.Xml;

using CrashItemLib.Sink;

//using CrashItemLib.Crash.Symbols;
//using CrashItemLib.Crash.CodeSegs;
//using CrashItemLib.Crash.Registers;
//using CrashItemLib.Crash.Threads;


using XmlFilePlugin.PluginImplementations.FileFormat;
using System.IO;

namespace XmlFilePlugin.PluginImplementations.Sink
{
    public class CXmlFileSink : CISink
    {
        #region Constants
        public const string KCrashInfoSinkName = "XML Crash File";
        #endregion

        #region Constructors
        public CXmlFileSink(CISinkManager aManager)
            : base(KCrashInfoSinkName, aManager)
        {
        }
        #endregion

        #region From CISink

        public override object Serialize(CISinkSerializationParameters aParams)
        {
            CXmlFileDocument document = new CXmlFileDocument();

            //Read information relevant to crash info file from container to internal variables
            document.ReadDataFromContainer(aParams);
            string newFileName = "";
            if (aParams.PlainTextOutput)
            {
                //Override default file extension
                aParams.FileExtensionFailed = ".corrupt_txt";
                aParams.FileExtensionSuccess = ".txt";

                //Write document's internal data to file
                newFileName = string.Empty;
                using (Stream output = aParams.CreateFile(out newFileName))
                {
                    using (StreamWriter sw = new StreamWriter(output, Encoding.ASCII))
                    {
                        document.WriteToPlainTextStream(sw);
                    }
                }
            }
            else // XML output
            {
                //Override default file extension
                aParams.FileExtensionFailed = ".corrupt_xml";
                aParams.FileExtensionSuccess = ".xml";

                //Write document's internal data to file
                newFileName = string.Empty;
                using (Stream output = aParams.CreateFile(out newFileName))
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.NewLineChars = XmlConsts.Kxml_EOL;
                    settings.NewLineHandling = NewLineHandling.None;
                    using (XmlWriter sw = XmlWriter.Create(output, settings))
                    {
                        document.WriteToXmlStream(sw);
                    }
                }
            }
            return newFileName;
        }

        #endregion
    }
}
