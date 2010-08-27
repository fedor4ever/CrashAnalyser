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
using CrashInfoFilePlugin.FileFormat;
using CrashItemLib.Crash.Source;
using CrashItemLib.Crash.Events;
using CrashItemLib.Crash.Messages;
using System.IO;
using CrashItemLib.Crash.Traces;
using SymbianStructuresLib.Debug.Trace;
using CrashItemLib.Crash.InfoEnvironment;
using ErrorLibrary;
using MobileCrashLib;

namespace CrashInfoFilePlugin.PluginImplementations.FileFormat
{
    internal class CCrashInfoDataBlock
    {
        #region Constructors
        public CCrashInfoDataBlock()
           
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
                System.Console.WriteLine("Warning: CrashInfoFilePlugin found multiple threads. CI file output can handle only one thread!");
            }
            foreach (CIThread thread in threads)
            {
                iPanicCategory = thread.ExitInfo.Category;
                iPanicID = thread.ExitInfo.Reason;

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
                CCrashInfoCallStack callStack = new CCrashInfoCallStack();
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

                CCrashInfoCodeSegItem ciCodeSeg = new CCrashInfoCodeSegItem(start, end, name);
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
                    iFileType = CrashInfoConsts.MobileCrashFileType.ETypeCrashAPIReport;
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

                    if (message.Description.Trim() == CrashInfoConsts.KRegistrationMiscInfo)
                    {
                        iFileType = CrashInfoConsts.MobileCrashFileType.ETypeRegistrationMessage;
                    }
                    if (message.Description.Trim() == CrashInfoConsts.KAliveTimeMiscInfo)
                    {
                        iFileType = CrashInfoConsts.MobileCrashFileType.ETypeAliveMessage;
                    }
                }
            }
        }

        internal void AddCrashHash(CIContainer aContainer)
        {
            //hash is only calculated for normal crashes - registrations and reports are omitted
            if (iFileType == CrashInfoConsts.MobileCrashFileType.ETypeBasicCrash)
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

                        // Get detailed hash
                        config = MobileCrashHashBuilder.TConfiguration.EDetailed;
                        builder = MobileCrashHashBuilder.New(config, primarySummary, MobileCrashHashBuilder.KDetailedNumberOfStackEntriesToCheckForSymbols);
                        
                        if (builder != null)
                            iDetailedHash = builder.GetHash();
                    }
                    catch (Exception e)
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

        #region Data writers

        internal void WriteTimeStamp(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iTimeStampText, CrashInfoConsts.Ktimestamp, aOutput);          
        }        

        internal void WriteRomID(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iRomId, CrashInfoConsts.Kromid, aOutput); 
        }

        internal void WriteSWVersion(System.IO.StreamWriter aOutput)
        {
            string version = iPlatform + CrashInfoConsts.KSeparator + iSWVersion;
            CCrashInfoFileUtilities.WriteOutputTags(version, CrashInfoConsts.Ksw_version, aOutput);
        }

        internal void WriteVariantID(System.IO.StreamWriter aOutput)
        {
            //variant id is not really used - dummy value needs to be written for dbmover        
            CCrashInfoFileUtilities.WriteOutputTags("12345678", CrashInfoConsts.Kvariant_id, aOutput); 
        }

        internal void WriteHWVersion(System.IO.StreamWriter aOutput)
        {
            //HW version is not really used - dummy value needs to be written for dbmover
           CCrashInfoFileUtilities.WriteOutputTags("NotFound", CrashInfoConsts.Khw_version, aOutput);           
        }

        internal void WritePanicID(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iPanicID, CrashInfoConsts.Kpanic_id, aOutput);
        }

        internal void WritePanicCategory(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iPanicCategory, CrashInfoConsts.Kpanic_category, aOutput);
        }

        internal void WritePanicDescription(System.IO.StreamWriter aOutput)
        {
            string panicDescription = XmlErrorLibrary.GetPanicDescription(iPanicCategory, iPanicID.ToString());
            CCrashInfoFileUtilities.WriteOutputTags(panicDescription, CrashInfoConsts.Kpanic_description, aOutput);
        }

        internal void WriteLanguage(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iLanguage, CrashInfoConsts.Klanguage, aOutput);
        }
        
        internal void WritePanicedProcess(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iProcess, CrashInfoConsts.Kpanicked_process, aOutput);
        }

        internal void WriteProgramCounter(System.IO.StreamWriter aOutput)
        {
            iRegStorage.WriteProgramCounter(aOutput);
        }

        internal void WriteRegisterList(System.IO.StreamWriter aOutput)
        {
            iRegStorage.WriteBasicRegisters(aOutput);           
        }
      
        internal void WriteModuleName(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iCrashedModuleName, CrashInfoConsts.Kcrashed_module_name, aOutput);
        }

        internal void WriteLoadedDLLs(System.IO.StreamWriter aOutput)
        {
            aOutput.Write(CCrashInfoFileUtilities.BlockStartMarker(CrashInfoConsts.Kcrashtime_loaded_dlls));
            bool first = true;
            foreach(CCrashInfoCodeSegItem codeseg in iCodeSegs)
            {
                if (first) //all but first item start with separator - special handling needed
                {
                    first = false;
                }
                else
                {
                    aOutput.Write(CrashInfoConsts.KSeparator);
                }
                aOutput.Write(codeseg.Start);
                aOutput.Write(CrashInfoConsts.KSeparator);
                aOutput.Write(codeseg.End);
                aOutput.Write(CrashInfoConsts.KSeparator);
                aOutput.Write(codeseg.Name);
            }
            aOutput.Write(CCrashInfoFileUtilities.BlockEndMarker(CrashInfoConsts.Kcrashtime_loaded_dlls));
        }

        internal void WriteAvailableMemory(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iFreeMomery, CrashInfoConsts.Kavailable_memory, aOutput);
        }

        internal void WriteUserComment(System.IO.StreamWriter aOutput)
        {
            //Dummy value needs to be written for dbmover
            CCrashInfoFileUtilities.WriteOutputTags("NotFound", CrashInfoConsts.Kuser_comment, aOutput);
        }

        internal void WriteMemoryInfo(System.IO.StreamWriter aOutput)
        {
            //Dummy value needs to be written for dbmover - for memory info just the tags
            aOutput.Write(CCrashInfoFileUtilities.BlockStartMarker(CrashInfoConsts.Kmemory_info));            
            aOutput.Write(CCrashInfoFileUtilities.BlockEndMarker(CrashInfoConsts.Kmemory_info));
        }

        internal void WriteMiscInfo(System.IO.StreamWriter aOutput)
        {
            //Dummy value needs to be written for dbmover
            string mInfo = "NotFound";
            if (iFileType == CrashInfoConsts.MobileCrashFileType.ETypeRegistrationMessage)
            {
                mInfo = CrashInfoConsts.KRegistrationMiscInfo;
            }
            if (iFileType == CrashInfoConsts.MobileCrashFileType.ETypeAliveMessage)
            {
                mInfo = CrashInfoConsts.KAliveTimeMiscInfo;
            }

            CCrashInfoFileUtilities.WriteOutputTags(mInfo, CrashInfoConsts.Kmisc_info, aOutput);
        }

        //This is the phone number
        internal void WriteReporter(System.IO.StreamWriter aOutput)
        {
            //Dummy value needs to be written for first part
            aOutput.Write(CCrashInfoFileUtilities.BlockStartMarker(CrashInfoConsts.Kreporter));      
            aOutput.Write("NotFound");
            aOutput.Write(CrashInfoConsts.KSeparator);
            aOutput.Write(iPhoneNumber);
            aOutput.Write(CCrashInfoFileUtilities.BlockEndMarker(CrashInfoConsts.Kreporter));     
        }

        internal void WriteArchive(System.IO.StreamWriter aOutput)
        {
            //Dummy value needs to be written for dbmover
            CCrashInfoFileUtilities.WriteOutputTags("0", CrashInfoConsts.Karchive, aOutput);
        }

        internal void WriteProductType(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iProductType, CrashInfoConsts.Kproduct_type, aOutput);   
        }

        internal void WriteProductionMode(System.IO.StreamWriter aOutput)
        {
            if (iProductionMode == 1)
            {
                CCrashInfoFileUtilities.WriteOutputTags(CrashInfoConsts.Kproduction_mode_value, CrashInfoConsts.Kproduction_mode, aOutput);
            }
            else if (iProductionMode == 0)
            {
                CCrashInfoFileUtilities.WriteOutputTags(CrashInfoConsts.Krnd_mode_value, CrashInfoConsts.Kproduction_mode, aOutput);
            }
        }

        internal void WriteCrashSource(System.IO.StreamWriter aOutput)
        {
            if (iCrashSource == 1)
            {
                CCrashInfoFileUtilities.WriteOutputTags(CrashInfoConsts.Kcrash_source_user, CrashInfoConsts.Kcrash_source, aOutput);
            }
            else if (iCrashSource == 0)
            {
                CCrashInfoFileUtilities.WriteOutputTags(CrashInfoConsts.Kcrash_source_kernel, CrashInfoConsts.Kcrash_source, aOutput);
            }
        }

        internal void WriteImei(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iImei, CrashInfoConsts.Kimei, aOutput);
        }

        internal void WriteResetreason(System.IO.StreamWriter aOutput)
        {
            //Dummy value needs to be written for dbmover
            CCrashInfoFileUtilities.WriteOutputTags("", CrashInfoConsts.Kresetreason, aOutput);
        }

        internal void WriteUptime(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iUptime.ToString(), CrashInfoConsts.Kuptime, aOutput);
        }
          
        internal void WriteIMSI(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iImsi, CrashInfoConsts.Ksiminfo, aOutput);
        }

        internal void WriteNetworkCountry(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iNetworkCountry, CrashInfoConsts.Knetwork_country_code, aOutput);
        }

        internal void WriteNetworkIdentity(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iNetworkIdentity, CrashInfoConsts.Knetwork_identity, aOutput);
        }      
      
        internal void WriteLocInfo(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iLocInfo, CrashInfoConsts.Klocinfo, aOutput);
        }
       
        internal void WriteNetworkCell(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iNetworkCell, CrashInfoConsts.Kcellid, aOutput);
        }

        internal void WriteTestset(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iTestSet, CrashInfoConsts.Ktestset, aOutput);
        }

        //Serial number known also as PSN
        internal void WriteSerialNumber(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iSerialNumber, CrashInfoConsts.Kpsninfo, aOutput);
        }

        internal void WriteS60Version(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iS60Version, CrashInfoConsts.Ks60version, aOutput);
        }

        internal void WriteProductCode(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iProductCode, CrashInfoConsts.Kproduct_code, aOutput);
        }

        internal void WriteVariantVersion(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iVariantVersion, CrashInfoConsts.Kvariant_version, aOutput);
        }
        internal void WriteCrashHash(System.IO.StreamWriter aOutput)
        {
            if ( string.IsNullOrEmpty( iHash ) == false )
            {
                aOutput.Write( CCrashInfoFileUtilities.MakeOutputTags( iHash, CrashInfoConsts.Kcrash_hash ) );
            }
        }
        
        internal void WriteDetailedCrashHash(System.IO.StreamWriter aOutput)
        {
            if (string.IsNullOrEmpty(iDetailedHash) == false)
            {
                aOutput.Write(CCrashInfoFileUtilities.MakeOutputTags(iDetailedHash, CrashInfoConsts.Kcrash_detailedhash));
            }
        }

        internal void WriteMMCInfo(System.IO.StreamWriter aOutput)
        {
            //Dummy value needs to be written for dbmover
            CCrashInfoFileUtilities.WriteOutputTags("", CrashInfoConsts.Kmmcinfo, aOutput);
        }
        internal void WriteUID(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iUID, CrashInfoConsts.Kuid, aOutput); 
        }

        internal void WriteDiskInfo(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iDiskInfo.ToString(), CrashInfoConsts.Kdiskinfo, aOutput);
        }

        internal void WriteFileType(System.IO.StreamWriter aOutput)
        {
            int type = 0; //default type 0
            if (iReportType != string.Empty)
            {
                type = 1; //for reports, type 1
            }
            CCrashInfoFileUtilities.WriteOutputTags(type, CrashInfoConsts.Kfile_type, aOutput);
        }
        
        internal void WriteReportType(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iReportType, CrashInfoConsts.Kreport_type, aOutput);
        }

        internal void WriteReportCategory(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iReportCategory, CrashInfoConsts.Kreport_category, aOutput);
        }

        internal void WriteReportOK(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iReportOK, CrashInfoConsts.Kreport_ok, aOutput); 
        }

        internal void WriteReportFail(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iReportFail, CrashInfoConsts.Kreport_fail, aOutput); 
        }

        internal void WriteReportParam1(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iReportParamName1, CrashInfoConsts.Kreport_param_name1, aOutput);      
            CCrashInfoFileUtilities.WriteOutputTags(iReportParamValue1, CrashInfoConsts.Kreport_param_value1, aOutput);

        }

        internal void WriteReportParam2(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iReportParamName2, CrashInfoConsts.Kreport_param_name2, aOutput);
            CCrashInfoFileUtilities.WriteOutputTags(iReportParamValue2, CrashInfoConsts.Kreport_param_value2, aOutput);
        }

        internal void WriteReportParam3(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iReportParamName3, CrashInfoConsts.Kreport_param_name3, aOutput);
            CCrashInfoFileUtilities.WriteOutputTags(iReportParamValue3, CrashInfoConsts.Kreport_param_value3, aOutput);
        }

        internal void WriteReportComments(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iReportComments, CrashInfoConsts.Kreport_comments, aOutput);
        }

        internal void WriteRegisterExtraList(System.IO.StreamWriter aOutput)
        {            
            iRegStorage.WriteOtherRegisters(aOutput);
        }

        internal void WriteCallstacks(System.IO.StreamWriter aOutput)
        {
            foreach (CCrashInfoCallStack stack in iCallStacks)
            {
                stack.WriteToStream(aOutput);
            }
        }

        internal void WriteBinFileName(System.IO.StreamWriter aOutput)
        {
            CCrashInfoFileUtilities.WriteOutputTags(iBinFilename, CrashInfoConsts.Kbinfile_name, aOutput);
        }
        internal void WriteSymbolFileNames(System.IO.StreamWriter aOutput)
        {
            string symbolfilenames = string.Empty;
            foreach (string fileName in iSymbolFiles)
            {
                if (symbolfilenames != string.Empty)
                {
                    symbolfilenames = symbolfilenames + ", ";
                }
                symbolfilenames = symbolfilenames + Path.GetFileName(fileName);
            }
            CCrashInfoFileUtilities.WriteOutputTags(symbolfilenames, CrashInfoConsts.Ksymbolfile_names, aOutput);
        }

         internal void WriteEventlog(System.IO.StreamWriter aOutput)
        {
            if (iEventlog.Count > 0)
            {
                aOutput.Write(CCrashInfoFileUtilities.BlockStartMarker(CrashInfoConsts.Keventlog));
                foreach (string line in iEventlog)
                {
                    aOutput.Write(line + CrashInfoConsts.KEOL);
                }
                aOutput.Write(CCrashInfoFileUtilities.BlockEndMarker(CrashInfoConsts.Keventlog));
            }
        }


         internal void WriteOstTraces(System.IO.StreamWriter aOutput)
         {
             if (iOstTraces.Count > 0)
             {
                 aOutput.Write(CCrashInfoFileUtilities.BlockStartMarker(CrashInfoConsts.Ktrace_data));
                 foreach (string line in iOstTraces)
                 {
                     aOutput.Write(line + CrashInfoConsts.KEOL);
                 }
                 aOutput.Write(CCrashInfoFileUtilities.BlockEndMarker(CrashInfoConsts.Ktrace_data));
             }
         }

        #endregion

        

        #region Data members
         private CrashInfoConsts.MobileCrashFileType iFileType = CrashInfoConsts.MobileCrashFileType.ETypeBasicCrash;
        
        private string iTimeStampText = string.Empty; //YearMonthDayHourMinSec
        private uint? iRomId = null; //aka rom's checksum word
        private string iPlatform = string.Empty; //usually SOS
        private string iSWVersion = string.Empty; //The "main" version number
        private string iS60Version = string.Empty;
        private string iVariantVersion = string.Empty;

        private int? iPanicID = null;
        private string iPanicCategory = string.Empty;

        private string iLanguage = string.Empty; //english, finnish etc
        
        private string iProcess = string.Empty;

        private CCrashInfoRegisterStorage iRegStorage = new CCrashInfoRegisterStorage(); //registers

        private string iCrashedModuleName = string.Empty; //thread name

        private List<CCrashInfoCodeSegItem> iCodeSegs = new List<CCrashInfoCodeSegItem>(); //crash time loaded dlls

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

        private List<CCrashInfoCallStack> iCallStacks = new List<CCrashInfoCallStack>(); //Call stacks

        private string iBinFilename = string.Empty;
        List<string> iSymbolFiles = new List<string>();
        List<string> iEventlog = new List<string>();
        List<string> iOstTraces = new List<string>();

        #endregion



    }
}
