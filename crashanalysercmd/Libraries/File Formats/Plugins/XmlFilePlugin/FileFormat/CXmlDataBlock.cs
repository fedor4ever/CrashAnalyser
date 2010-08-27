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
* The class CCrashInfoDataBlock is part of CrashAnalyser CrashInfoFile plugin.
* Provides reading methods, container and output methods for all data in a single
* datablock of crashinfo file, corresponding to one crash. Complete crashinfo file 
* may contain one or more datablock.
* 
*/
using System;
using System.Collections.Generic;
using System.Text;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.InfoSW;
using CrashItemLib.Crash.Utils;
using System.Globalization;
using CrashItemLib.Crash.Threads;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Symbols;
using CrashItemLib.Crash.CodeSegs;
using CrashItemLib.Crash.Memory;
using CrashItemLib.Crash.Summarisable;
using CrashItemLib.Crash.InfoHW;
using CrashItemLib.Crash.Telephony;
using CrashItemLib.Crash.Header;
using CrashItemLib.Crash.Reports;
using CrashItemLib.Crash.Stacks;
using XmlFilePlugin.FileFormat;
using CrashItemLib.Crash.Source;
using CrashItemLib.Crash.Events;
using CrashItemLib.Crash.Messages;
using System.IO;
using CrashItemLib.Crash.Traces;
using SymbianStructuresLib.Debug.Trace;
using CrashItemLib.Crash.InfoEnvironment;
using ErrorLibrary;
using MobileCrashLib;

namespace XmlFilePlugin.PluginImplementations.FileFormat
{
    internal class CXmlDataBlock
    {
        #region Constructors
        public CXmlDataBlock()
           
        {
        }

        #endregion

        #region Adding data content
        /** Add timestamp and uptime */
        internal void AddHeader(CIContainer aContainer)
        {
            CIHeader header = (CIHeader) aContainer.ChildByType( typeof( CIHeader ) );
            if (header != null)
            {
                //Timestamp
                DateTime timeStamp = header.CrashTime;
                String date = timeStamp.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo);
                int hour = timeStamp.Hour;
                int minute = timeStamp.Minute;
                int second = timeStamp.Second;
                iTimeStampText = date + hour.ToString().PadLeft(2, '0') + minute.ToString().PadLeft(2, '0') + second.ToString().PadLeft(2, '0');

                //UpTime
                iUptime = header.UpTime.TotalSeconds;
                
                // Crash source
                iCrashSource = header.CrashSource;
            }
        }
        /** Add romid, timestamp, platform, language and sw version */
        internal void AddSWInfos(CIContainer aContainer)
        {
            CIInfoSW info = (CIInfoSW) aContainer.ChildByType( typeof( CIInfoSW ) );
            if (info != null)
            {
                //RomID
                if (info.ImageCheckSum != 0)
                {
                    iRomId = info.ImageCheckSum;                    
                }
                //Platform
                iPlatform = info.Platform;
                
                //Language
                iLanguage = info.Language;             

                //Version                
                const string KInfoSW_Version_Runtime    = "Runtime Version";
                const string KInfoSW_Version_Variant    = "Variant Version";
                const string KInfoSW_Version_S60        = "S60 Version";      
                foreach ( CIVersionInfo version in info )
                {
                    if (version.IsValid && version.Name == KInfoSW_Version_Runtime)
                    {                        
                        iSWVersion = version.Value;                            
                    }
                    if (version.IsValid && version.Name == KInfoSW_Version_Variant)
                    {                        
                        iVariantVersion =  version.Value;                            
                    }
                    if (version.IsValid && version.Name == KInfoSW_Version_S60)
                    {                        
                        iS60Version =  version.Value;                            
                    }
                }          
 
                //Timestamp
                DateTime timeStamp = info.ImageTimeStamp;
                String date = timeStamp.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo);
                int hour = timeStamp.Hour;
                int minute = timeStamp.Minute;
                int second = timeStamp.Second;
                iTimeStampText = date + hour.ToString().PadLeft(2, '0') + minute.ToString().PadLeft(2, '0') + second.ToString().PadLeft(2, '0');
                           
            }
        }

          

        internal void AddThreadAndExitInfo(CIContainer aContainer)
        {

            CIElementList<CIThread> threads = aContainer.ChildrenByType<CIThread>( CIElement.TChildSearchType.EEntireHierarchy );
            if (threads.Count > 1)
            {
                System.Console.WriteLine("Warning: XmlFilePlugin found multiple threads. XML file output can handle only one thread!");
            }
            foreach (CIThread thread in threads)
            {
                iPanicCategory = thread.ExitInfo.Category;
                iPanicID = thread.ExitInfo.Reason;
                iPanicDescription = XmlErrorLibrary.GetPanicDescription(iPanicCategory, iPanicID.ToString());
                iCrashedModuleName = thread.FullName;
            }

        }

        internal void AddPanicedProcess(CIContainer aContainer)
        {
            CIElementList<CIProcess> processes = aContainer.ChildrenByType<CIProcess>(CIElement.TChildSearchType.EEntireHierarchy);
            if (processes.Count > 1)
            {
                System.Console.WriteLine("Warning: CrashInfoFilePlugin found multiple processes. CI file output can handle only one process!");
            }
            foreach (CIProcess process in processes)
            {
                 iProcess = process.Name;
                 iUID = process.Uids.MostSignificant;
            }

        }

        internal void AddRegisterLists(CIContainer aContainer)
        {
            CIElementList<CIRegisterListCollection> regListCols = aContainer.ChildrenByType<CIRegisterListCollection>(CIElement.TChildSearchType.EEntireHierarchy);
            foreach (CIRegisterListCollection regListCol in regListCols)
            {
                foreach (CIRegisterList regList in regListCol)
                {                                      
                    iRegStorage.ReadRegisterData(regList);     
                }
            }

        }

        internal void AddStacks(CIContainer aContainer)
        {
            CIElementList<CIStack> stacks = aContainer.ChildrenByType<CIStack>(CIElement.TChildSearchType.EEntireHierarchy);
            foreach (CIStack stack in stacks)
            {
                CXmlCallStack callStack = new CXmlCallStack();
                callStack.Read(stack);
                callStack.CleanStack();
                iCallStacks.Add(callStack);

            }
        }

        internal void AddCodeSegments(CIContainer aContainer)
        {
            // Get the code segments
            CIElementList<CICodeSeg> codeSegs = aContainer.ChildrenByType<CICodeSeg>(CIElement.TChildSearchType.EEntireHierarchy);

            // Sort them
            Comparison<CICodeSeg> comparer = delegate(CICodeSeg aLeft, CICodeSeg aRight)
            {
                return string.Compare(aLeft.Name, aRight.Name, true);
            };
            codeSegs.Sort(comparer);

            // List them
            foreach (CICodeSeg codeSeg in codeSegs)
            {
                uint start = codeSeg.Range.Min;
                uint end = codeSeg.Range.Max;
                string name = codeSeg.Name;

                CXmlCodeSegItem ciCodeSeg = new CXmlCodeSegItem(start, end, name);
                iCodeSegs.Add(ciCodeSeg);
            }
        }

        internal void AddMemoryInfo(CIContainer aContainer)
        {
            CIElementList<CIMemoryInfo> list = aContainer.ChildrenByType<CIMemoryInfo>(CIElement.TChildSearchType.EEntireHierarchy);
            foreach ( CIMemoryInfo info in list )
            {                
                if ( info.Type == CIMemoryInfo.TType.ETypeRAM )
                {
                    iFreeMomery = info.Free;        
                }
                if (info.Type == CIMemoryInfo.TType.ETypeDrive)
                {
                    iDiskInfo = info.Free;
                }
            }
        }

        internal void AddHWInfo(CIContainer aContainer)
        {
            CIInfoHW info = (CIInfoHW)aContainer.ChildByType(typeof(CIInfoHW));
            if (info != null)
            {
                iProductType = info.ProductType; 
                iProductCode = info.ProductCode.Trim();
                iSerialNumber = info.SerialNumber.Trim();
                iProductionMode = info.ProductionMode;
            }           
        }

        internal void AddTelephony(CIContainer aContainer)
        {
            CITelephony info = (CITelephony)aContainer.ChildByType(typeof(CITelephony));
            if (info != null)
            {
                iPhoneNumber = info.PhoneNumber;
                iImei = info.IMEI;
                iImsi = info.IMSI;

                CITelephonyNetworkInfo networkInfo = info.NetworkInfo;

                iNetworkCountry = networkInfo.Country;
                iNetworkIdentity = networkInfo.Identity;
                iNetworkCell = networkInfo.CellId;
                iLocInfo = networkInfo.CGI;

            }

        }

        internal void AddEnvInfo(CIContainer aContainer)
        {
            CIInfoEnvironment info = (CIInfoEnvironment)aContainer.ChildByType(typeof(CIInfoEnvironment));
            if (info != null)
            {
                iTestSet = info.TestSet;
            }

        }

        internal void AddReportParameters(CIContainer aContainer)
        {
            CIReportInfo report = (CIReportInfo)aContainer.ChildByType(typeof(CIReportInfo));
            if (report != null)
            {
                iReportType = report.Type;
                if (iReportType != string.Empty)
                {
                    iFileType = XmlConsts.MobileCrashFileType.ETypeCrashAPIReport;
                }
                

                iReportCategory = report.Category;
                iReportOK = report.CountSuccess;
                iReportFail = report.CountFail;
                IEnumerator<CIReportParameter> parameters = report.GetEnumerator();
                if (parameters.MoveNext()) //has first parameter
                {
                    iReportParamName1 = parameters.Current.Name;
                    iReportParamValue1 = parameters.Current.Value;

                    if (parameters.MoveNext()) //has second parameter
                    {
                        iReportParamName2 = parameters.Current.Name;
                        iReportParamValue2 = parameters.Current.Value;
                        if (parameters.MoveNext())
                        {
                            iReportParamName3 = parameters.Current.Name;
                            iReportParamValue3 = parameters.Current.Value;
                        }
                    }
                }
                   
                iReportComments = report.Comments;
                
            }
            
        }

        internal void AddMessages(CIContainer container)
        {
            foreach (CIMessage message in container.Messages)
            {
                if (message.Title == "Miscellaneous Information")
                {

                    if (message.Description.Trim() == XmlConsts.KRegistrationMiscInfo)
                    {
                        iFileType = XmlConsts.MobileCrashFileType.ETypeRegistrationMessage;
                    }
                    if (message.Description.Trim() == XmlConsts.KAliveTimeMiscInfo)
                    {
                        iFileType = XmlConsts.MobileCrashFileType.ETypeAliveMessage;
                    }
                }
            }
        }

        internal void AddCrashHash(CIContainer aContainer)
        {
            //hash is only calculated for normal crashes - registrations and reports are omitted
            if (iFileType == XmlConsts.MobileCrashFileType.ETypeBasicCrash)
            {
                CISummarisableEntity primarySummary = aContainer.PrimarySummary;
                if (primarySummary != null)
                {
                    MobileCrashHashBuilder.TConfiguration config = MobileCrashHashBuilder.TConfiguration.EDefault;
                    try //CCrashInfoHashBuilder.New throws an exception if there's not enough data for hash creation
                    {
                        MobileCrashHashBuilder builder = MobileCrashHashBuilder.New(config, primarySummary);
                        
                        if (builder != null)
                            iHash = builder.GetHash();

                        // Detailed hash
                        config = MobileCrashHashBuilder.TConfiguration.EDetailed;
                        builder = MobileCrashHashBuilder.New(config, primarySummary, MobileCrashHashBuilder.KDetailedNumberOfStackEntriesToCheckForSymbols);
                        
                        if (builder != null)
                            iDetailedHash = builder.GetHash();
                    }
                    catch (Exception /* e */)
                    {
                        //Not enough data -> no hash and no grouping
                    }
                }
            }
        }

        internal void AddFileNames(CIContainer aContainer, string aArchivedFileName)
     
        {
            iBinFilename = aArchivedFileName;

            CISource source = aContainer.Source;
            string binFileOriginalName = source.MasterFileName;
                        
            foreach (string filename in aContainer.FileNames)
            {
                if (filename != binFileOriginalName) //Since bin file name is stored separately, remove it from this list
                {
                    iSymbolFiles.Add(filename);
                }
            }          

        }
        internal void AddEventlog(CIContainer aContainer)
        {
            CIEventList events = aContainer.Events;
            foreach (CIEvent ev in events)
            {

                iEventlog.Add(ev.Value.ToString());
            }
        }


        internal void AddOstTraces(CIContainer aContainer)
        {
            CITraceData traceData = aContainer.Traces;
            //
            if (traceData != null && traceData.Lines.Length > 0)
            {
                foreach (CITrace ciTrace in traceData)
                {
                    System.Text.StringBuilder line = new System.Text.StringBuilder();                
                   
                    TraceLine trace = ciTrace;

                    // Type
                    string type = string.Empty;
                    switch (trace.Type)
                    {
                        case TraceLine.TType.ETypeBinary:
                            type = "Bin";
                            break;
                        case TraceLine.TType.ETypeRaw:
                            type = "Raw";
                            break;
                        case TraceLine.TType.ETypeText:
                            type = "Text";
                            break;
                        default:
                            type = "Unknown";
                            break;
                    }
                    if (string.IsNullOrEmpty(type) == false)
                    {
                        line.Append(type);
                    }

                    // Context id
                    if (trace.ContextId != 0)
                    {
                        line.Append(" " + "0x" + trace.ContextId.ToString("x8"));
                    }

                    // Time stamp
                    line.Append(" " + trace.TimeStamp.ToString());

                    // Prefix
                    string prefix = trace.Prefix;
                    if (string.IsNullOrEmpty(prefix) == false)
                    {
                        line.Append(" " + prefix);
                    }

                    // Suffix
                    string suffix = trace.Suffix;
                    if (string.IsNullOrEmpty(suffix) == false)
                    {
                        line.Append(" " + suffix);
                    }

                    if (trace.HasIdentifier)
                    {
                        // Component/group/id triplet
                        TraceIdentifier identifier = trace.Identifier;
                        line.Append(" C:" + "0x" + identifier.Component.ToString("x8"));
                        line.Append(" G:" + identifier.Group.ToString());
                        line.Append(" I:" + identifier.Id.ToString());
                        // File & line
                        TraceLocation location = identifier.Location;
                        //
                        string file = location.File;
                        string lineNumber = location.Line.ToString();
                        //
                        if (string.IsNullOrEmpty(file) == false && string.IsNullOrEmpty(lineNumber) == false)
                        {
                            line.Append(" " +file);
                            line.Append(":" + lineNumber);
                        }
                    }

                    // Payload
                    string payload = trace.Payload;
                    line.Append(" " + payload);
                    iOstTraces.Add(line.ToString());
                }
            }
        }

        #endregion

        #region Getters

        internal XmlConsts.MobileCrashFileType FileType()
        {
            return iFileType;
        }

        internal string GetMiscInfo()
        {
            string mInfo = "NotFound";
            if (iFileType == XmlConsts.MobileCrashFileType.ETypeRegistrationMessage)
            {
                mInfo = XmlConsts.KRegistrationMiscInfo;
            }
            if (iFileType == XmlConsts.MobileCrashFileType.ETypeAliveMessage)
            {
                mInfo = XmlConsts.KAliveTimeMiscInfo;
            }
            return mInfo;
        }

         internal string TimeStampText()
         {
             return iTimeStampText;
         }

         internal uint? RomId()
         {
             return iRomId;
         }

         internal string Platform()
         {
             return iPlatform;
         }

         internal string SWVersion()
         {
             return iSWVersion;
         }

         internal string S60Version()
         {
             return iS60Version;
         }

         internal string VariantVersion()
         {
             return iVariantVersion;
         }

         internal int? PanicId()
         {
             return iPanicID;
         }

         internal string PanicCategory()
         {
             return iPanicCategory;
         }

         internal string PanicDescription()
         {
             return iPanicDescription;
         }

         internal string Language()
         {
             return iLanguage;
         }

         internal string Process()
         {
             return iProcess;
         }

         internal CXmlRegisterStorage RegStorage()
         {
             return iRegStorage;
         }

         internal string CrashedModuleName()
         {
             return iCrashedModuleName;
         }

         internal List<CXmlCodeSegItem> CodeSegs()
         {
             return iCodeSegs;
         }

         internal ulong? FreeMemory() 
         {
             return iFreeMomery;
         }

         internal string ProductType()
         {
             return iProductType;
         }

         internal int? ProductionMode()
         {
             return iProductionMode;
         }

         internal int? CrashSource()
         {
             return iCrashSource;
         }

         internal string ProductCode() 
         {
             return iProductCode;
         }
         
         internal string SerialNumber() 
         {
             return iSerialNumber;
         }

         internal string PhoneNumber()
         {
             return iPhoneNumber;
         }
        
         internal string Imei() 
         {
             return iImei;
         }

         internal string Imsi() 
         {
             return iImsi;
         }

         internal string NetworkCountry() 
         {
             return iNetworkCountry;
         }

         internal string NetworkIdentity() 
         {
             return iNetworkIdentity;
         }

         internal string NetworkCell() 
         {
             return iNetworkCell;
         }

         internal string LocInfo() 
         {
             return iLocInfo;
         }

         internal string TestSet() 
         {
             return iTestSet;
         }

         internal double? Uptime() 
         {
             return iUptime;
         }

         internal uint? UID() 
         {
             return iUID;
         }

         internal ulong? DiskInfo() 
         {
             return iDiskInfo;
         }

         internal string ReportType() 
         {
             return iReportType;
         }

         internal string ReportCategory() 
         {
             return iReportCategory;
         }

         internal uint? ReportOK() 
         {
             return iReportOK;
         }

         internal uint? ReportFail() 
         {
             return iReportFail;
         }

         internal string ReportParamName1() 
         {
             return iReportParamName1;
         }

         internal uint? ReportParamValue1() 
         {
             return iReportParamValue1;
         }

         internal string ReportParamName2() 
         {
             return iReportParamName2;
         }

         internal uint? ReportParamValue2() 
         {
             return iReportParamValue2;
         }

         internal string ReportParamName3() 
         {
             return iReportParamName3;
         }

         internal uint? ReportParamValue3()
         {
             return iReportParamValue3;
         }
                 
        internal string ReportComments() 
         {
             return iReportComments;
         }
        
        internal string Hash()
         {
             return iHash;
         }

        internal string DetailedHash()
        {
            return iDetailedHash;
        }

        internal List<CXmlCallStack> CallStacks()
        {
             return iCallStacks;
        }

        internal string BinFilename()
        {
            return iBinFilename;
        }

        internal List<string> SymbolFiles()
        {
            return iSymbolFiles;
        }

        internal List<string> Eventlog()
        {
            return iEventlog;
        }

        internal List<string> OstTraces()
        {
            return iOstTraces;
        }
        
        #endregion

         #region Data members
         private XmlConsts.MobileCrashFileType iFileType = XmlConsts.MobileCrashFileType.ETypeBasicCrash;
        
        private string iTimeStampText = string.Empty; //YearMonthDayHourMinSec
        private uint? iRomId = null; //aka rom's checksum word
        private string iPlatform = string.Empty; //usually SOS
        private string iSWVersion = string.Empty; //The "main" version number
        private string iS60Version = string.Empty;
        private string iVariantVersion = string.Empty;

        private int? iPanicID = null;
        private string iPanicCategory = string.Empty;
        private string iPanicDescription = string.Empty;

        private string iLanguage = string.Empty; //english, finnish etc
        
        private string iProcess = string.Empty;

        private CXmlRegisterStorage iRegStorage = new CXmlRegisterStorage(); //registers

        private string iCrashedModuleName = string.Empty; //thread name

        private List<CXmlCodeSegItem> iCodeSegs = new List<CXmlCodeSegItem>(); //crash time loaded dlls

        private ulong? iFreeMomery = null; //free ram

        private string iProductType = string.Empty; //aka RM-code
        private string iProductCode = string.Empty; //7-digit HW variant code 
        private string iSerialNumber = string.Empty; //aka PSN
        private int? iProductionMode = null; // 1: production mode phone, 0: RnD phone
        private int? iCrashSource = null; // 1: crash from user side, 0: from kernel side.

        private string iPhoneNumber = "NotFound";
        private string iImei = string.Empty;
        private string iImsi = string.Empty;
        private string iNetworkCountry = string.Empty;
        private string iNetworkIdentity = string.Empty;
        private string iNetworkCell = string.Empty;
        private string iLocInfo = string.Empty;
        private string iTestSet = string.Empty;
        private double? iUptime = null;
        private uint? iUID = null;
        private ulong? iDiskInfo = null;
        private string iReportType = string.Empty;
        private string iReportCategory = string.Empty;
        private uint? iReportOK = null;
        private uint? iReportFail = null;
        private string iReportParamName1 = string.Empty;
        private uint? iReportParamValue1 = null;
        private string iReportParamName2 = string.Empty;
        private uint? iReportParamValue2 = null;
        private string iReportParamName3 = string.Empty;
        private uint? iReportParamValue3 = null;
        private string iReportComments = string.Empty;
        private string iHash = string.Empty;
        private string iDetailedHash = string.Empty;

        private List<CXmlCallStack> iCallStacks = new List<CXmlCallStack>(); //Call stacks

        private string iBinFilename = string.Empty;
        List<string> iSymbolFiles = new List<string>();
        List<string> iEventlog = new List<string>();
        List<string> iOstTraces = new List<string>();

        #endregion



    }
}
