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
* The class CXmlFileDocument is part of CrashAnalyser XmlFilePlugin plugin.
* Container and output implementation for data in XML File format.
* XML format is an intermediate file used in the MobileCrash server
* CXmlFileSink creates an instance of this class and uses it to output
* crash data to file in XML format. 
* 
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.Container;
using System.IO;
using CrashItemLib.Sink;

namespace XmlFilePlugin.PluginImplementations.FileFormat
{
    internal class CXmlFileDocument
    {
        #region Constructors
        public CXmlFileDocument()
           
        {
        }

        #endregion

        /** Creates a new datablock and inputs data from container to the datablock */
        public void ReadDataFromContainer(CISinkSerializationParameters aParams)
        {
            CIContainer container = aParams.Container;

            //Create a datablock for this container's contents
            CXmlDataBlock datablock = new CXmlDataBlock();
           
            //Read all interesting data from container to the datablock
            datablock.AddHeader(container);
            datablock.AddSWInfos(container);
            datablock.AddThreadAndExitInfo(container);
            datablock.AddPanicedProcess(container);
            datablock.AddRegisterLists(container);
            datablock.AddStacks(container);
            datablock.AddCodeSegments(container);
            datablock.AddMemoryInfo(container);
            datablock.AddHWInfo(container);
            datablock.AddTelephony(container);
            datablock.AddEnvInfo(container);
            datablock.AddReportParameters(container);
            datablock.AddMessages(container);
            datablock.AddCrashHash(container);

            string archivedFileName = (String)aParams.OperationData1;
            datablock.AddFileNames(container, archivedFileName);
            datablock.AddEventlog(container);

            datablock.AddOstTraces(container);

            //If all went well, we will add datablock to stored datablocks
            iDatablocks.Add(datablock);
        }

        public void WriteToXmlStream(XmlWriter aOutput)
        {
            System.Diagnostics.Debug.Assert(aOutput != null);

            WriteHeader(aOutput);

            if (iDatablocks.Count > 0)
            {
                WriteReport(aOutput);
            }
            
            WriteFooter(aOutput);
        }

        /**
         * Writes as plain text.
         */
        public void WriteToPlainTextStream(StreamWriter aOutput)
        {
            PlainTextOutput.Write(aOutput, iDatablocks);
        }

        private void WriteReport(XmlWriter aOutput)
        {
            CXmlDataBlock datablock = iDatablocks[0];
            
            aOutput.WriteStartElement(XmlConsts.Kxml_report);

            WriteDevice(aOutput, datablock);
            WriteReportDetails(aOutput, datablock);

            aOutput.WriteEndElement();
        }


        private static void WriteDevice(XmlWriter aOutput, CXmlDataBlock datablock)
        {
            // Device
            aOutput.WriteStartElement(XmlConsts.Kxml_device);

            if (datablock.Platform().Equals(XmlConsts.Kxml_sos))
            {
                aOutput.WriteAttributeString(XmlConsts.Kxml_platform, XmlConsts.Kxml_s60);
            }
            else
            {
                aOutput.WriteAttributeString(XmlConsts.Kxml_platform, datablock.Platform());
            }

            aOutput.WriteAttributeString(XmlConsts.Kxml_code, datablock.ProductType());
            aOutput.WriteAttributeString(XmlConsts.Kxml_version, datablock.SWVersion());

            if (datablock.RomId() != null)
            {
                aOutput.WriteAttributeString(XmlConsts.Kxml_romid, datablock.RomId().ToString());
            }

            // variant is not written.

            if (datablock.Imei() != string.Empty)
            {
                aOutput.WriteAttributeString(XmlConsts.Kxml_imei, datablock.Imei());
            }

            // Write additional device details here if exist.

            aOutput.WriteEndElement();
        }

        private static void WriteHeader(XmlWriter aOutput)
        {
            aOutput.WriteStartDocument();
            aOutput.WriteDocType(XmlConsts.Kxml_report, null, XmlConsts.Kxml_doctype, null);
        }

        private static void WriteFooter(XmlWriter aOutput)
        {
            aOutput.WriteEndDocument();
        }

        private static void WriteReportDetails(XmlWriter aOutput, CXmlDataBlock datablock)
        {
            // Device report details
            aOutput.WriteStartElement(XmlConsts.Kxml_report_details);
            aOutput.WriteAttributeString(XmlConsts.Kxml_timestamp, datablock.TimeStampText());

            string crashtypestr = "Not found";
            if(datablock.FileType() == XmlConsts.MobileCrashFileType.ETypeBasicCrash)
            {
                crashtypestr = XmlConsts.Kxml_type_crash;
            }
            else if (datablock.FileType() == XmlConsts.MobileCrashFileType.ETypeAliveMessage)
            {
                crashtypestr = XmlConsts.Kxml_type_alivemsg;
            }
            else if (datablock.FileType() == XmlConsts.MobileCrashFileType.ETypeCrashAPIReport)
            {
                crashtypestr = XmlConsts.Kxml_type_report;
            }
            else if (datablock.FileType() == XmlConsts.MobileCrashFileType.ETypeRegistrationMessage)
            {
                crashtypestr = XmlConsts.Kxml_type_regmsg;
            }

            aOutput.WriteAttributeString(XmlConsts.Kxml_type, crashtypestr);

            aOutput.WriteAttributeString(XmlConsts.Kxml_file_name, datablock.BinFilename());

            if (crashtypestr.Equals(XmlConsts.Kxml_type_report))
            {
                aOutput.WriteAttributeString(XmlConsts.Kxml_report_category, datablock.ReportCategory());
            }

            if (crashtypestr.Equals(XmlConsts.Kxml_type_report))
            {
                aOutput.WriteAttributeString(XmlConsts.Kxml_report_type, datablock.ReportType());
            }

            if (datablock.CrashedModuleName() != string.Empty)
            {
                aOutput.WriteAttributeString(XmlConsts.Kxml_crashed_module, datablock.CrashedModuleName());
            }

            if (datablock.Hash() != string.Empty)
            {
                aOutput.WriteAttributeString(XmlConsts.Kxml_defect_hash, datablock.Hash());
            }

            // Child elements for report details
            if (datablock.CodeSegs().Count > 0)
            {
                aOutput.WriteStartElement(XmlConsts.Kxml_loaded_dlls);
            }
            foreach (CXmlCodeSegItem codeseg in datablock.CodeSegs())
            {
                aOutput.WriteStartElement(XmlConsts.Kxml_loaded_dll);
                aOutput.WriteAttributeString(XmlConsts.Kxml_name, codeseg.Name);
                aOutput.WriteAttributeString(XmlConsts.Kxml_start_address, codeseg.Start.ToString());
                aOutput.WriteAttributeString(XmlConsts.Kxml_end_address, codeseg.End.ToString());
                aOutput.WriteEndElement(); // loaded dll
            }

            if (datablock.CodeSegs().Count > 0)
            {
                aOutput.WriteEndElement(); // loaded dlls
            }

            // Traces
            if(datablock.RegStorage().BasicRegs().Registers.Count > 0 ||
                datablock.CallStacks().Count > 0 ||
                datablock.OstTraces().Count > 0 ||
                datablock.Eventlog().Count > 0 ||
                datablock.RegStorage().OtherRegLists().Count > 0)
            {
                aOutput.WriteStartElement(XmlConsts.Kxml_traces);
            }

            WriteDetail(aOutput, XmlConsts.Kxml_register, datablock.RegStorage().BasicRegs().ToString());

            // Extra registers  
            System.Text.StringBuilder extraRegisters = new System.Text.StringBuilder();
            foreach (CXmlRegisterStorage.CCrashInfoRegisterList registerList in datablock.RegStorage().OtherRegLists())
            {
                extraRegisters.AppendLine(registerList.ToPrettyString());
            }
            WriteDetail(aOutput, XmlConsts.Kxml_register_extra, extraRegisters.ToString());

            // Call stack  
            foreach (XmlFilePlugin.FileFormat.CXmlCallStack stack in datablock.CallStacks())
            {
                WriteDetail(aOutput, XmlConsts.Kxml_call_stack_text, stack.ToString());
            }

            // OST traces  
            System.Text.StringBuilder ostTraces = new System.Text.StringBuilder();
            foreach (string line in datablock.OstTraces())
            {
                ostTraces.AppendLine(line);
            }
            WriteDetail(aOutput, XmlConsts.Kxml_trace_data, ostTraces.ToString());

            // Event log
            System.Text.StringBuilder eventLog = new System.Text.StringBuilder();
            foreach (string line in datablock.Eventlog())
            {
                eventLog.AppendLine(line);
            }
            WriteDetail(aOutput, XmlConsts.Kxml_eventlog, eventLog.ToString());

            if (datablock.RegStorage().BasicRegs().Registers.Count > 0 ||
                datablock.CallStacks().Count > 0 ||
                datablock.OstTraces().Count > 0 ||
                datablock.Eventlog().Count > 0 ||
                datablock.RegStorage().OtherRegLists().Count > 0)
            {
                aOutput.WriteEndElement(); // traces
            }
            WriteDictionaryValues(aOutput, datablock);
            WriteUniqueValues(aOutput, datablock);

            aOutput.WriteEndElement(); // report details
        }


        private static void WriteDictionaryValues(XmlWriter aOutput, CXmlDataBlock datablock)
        {
            // Dictionary values
            aOutput.WriteStartElement(XmlConsts.Kxml_dictionary_values);

            WriteDetail(aOutput, XmlConsts.Kxml_panic_id, datablock.PanicId());
            WriteDetail(aOutput, XmlConsts.Kxml_panic_category, datablock.PanicCategory());
            WriteDetail(aOutput, XmlConsts.Kxml_panic_description, datablock.PanicDescription());
            WriteDetail(aOutput, XmlConsts.Kxml_language, datablock.Language());
            
            if (!datablock.Process().Equals(XmlConsts.Kxml_unknown_process))
            {
                WriteDetail(aOutput, XmlConsts.Kxml_panicked_process, datablock.Process());
            }

            WriteDetail(aOutput, XmlConsts.Kxml_network_country_code, datablock.NetworkCountry());
            WriteDetail(aOutput, XmlConsts.Kxml_network_identity, datablock.NetworkIdentity());
            WriteDetail(aOutput, XmlConsts.Kxml_s60version, datablock.S60Version());
            WriteDetail(aOutput, XmlConsts.Kxml_product_code, datablock.ProductCode());
            WriteDetail(aOutput, XmlConsts.Kxml_variant_version, datablock.VariantVersion());

            aOutput.WriteStartElement(XmlConsts.Kxml_detail);
            aOutput.WriteAttributeString(XmlConsts.Kxml_name, XmlConsts.Kxml_file_type);
            if (datablock.ReportType() != string.Empty)
            {
                aOutput.WriteAttributeString(XmlConsts.Kxml_value, "1");
            }
            else
            {
                aOutput.WriteAttributeString(XmlConsts.Kxml_value, "0");
            }
            aOutput.WriteEndElement();

            // Production mode
            if (datablock.ProductionMode() != -1)
            {
                aOutput.WriteStartElement(XmlConsts.Kxml_detail);
                aOutput.WriteAttributeString(XmlConsts.Kxml_name, XmlConsts.Kxml_production_mode);
                if (datablock.ProductionMode() == 1)
                {
                    aOutput.WriteAttributeString(XmlConsts.Kxml_value, XmlConsts.Kxml_production_mode_value);
                }
                else
                {
                    aOutput.WriteAttributeString(XmlConsts.Kxml_value, XmlConsts.Kxml_rnd_mode_value);
                }
                aOutput.WriteEndElement();
            }

            // Crash source
            if (datablock.CrashSource() != -1)
            {
                aOutput.WriteStartElement(XmlConsts.Kxml_detail);
                aOutput.WriteAttributeString(XmlConsts.Kxml_name, XmlConsts.Kxml_crash_source);
                if (datablock.CrashSource() == 1)
                {
                    aOutput.WriteAttributeString(XmlConsts.Kxml_value, XmlConsts.Kxml_crash_source_user);
                }
                else
                {
                    aOutput.WriteAttributeString(XmlConsts.Kxml_value, XmlConsts.Kxml_crash_source_kernel);
                }
                aOutput.WriteEndElement();
            }

            WriteSymbolFileNames(aOutput, datablock);


            aOutput.WriteEndElement(); // dictionary values
        }


        private static void WriteUniqueValues(XmlWriter aOutput, CXmlDataBlock datablock)
        {
            // Unique values
            aOutput.WriteStartElement(XmlConsts.Kxml_unique_values);

            // Hw version is not added because it doesn't exist.

            WriteDetail(aOutput, XmlConsts.Kxml_available_memory, datablock.FreeMemory());

            if (datablock.RegStorage() != null && datablock.RegStorage().ProgramCounter() != null)
            {
                WriteDetail(aOutput, XmlConsts.Kxml_program_counter, datablock.RegStorage().ProgramCounter().Value);
                WriteDetail(aOutput, XmlConsts.Kxml_program_counter_symbol, datablock.RegStorage().ProgramCounter().Symbol);
            }

            WriteDetail(aOutput, XmlConsts.Kxml_misc_info, datablock.GetMiscInfo());

            // Reporter not written.
            // Resetreason not written.

            WriteDetail(aOutput, XmlConsts.Kxml_uptime, datablock.Uptime());
            WriteDetail(aOutput, XmlConsts.Kxml_siminfo, datablock.Imsi());
            WriteDetail(aOutput, XmlConsts.Kxml_locinfo, datablock.LocInfo());
            WriteDetail(aOutput, XmlConsts.Kxml_cellid, datablock.NetworkCell());
            WriteDetail(aOutput, XmlConsts.Kxml_psninfo, datablock.SerialNumber());
            WriteDetail(aOutput, XmlConsts.Kxml_uid, datablock.UID());
            WriteDetail(aOutput, XmlConsts.Kxml_testset, datablock.TestSet());
            WriteDetail(aOutput, XmlConsts.Kxml_diskinfo, datablock.DiskInfo());
            WriteDetail(aOutput, XmlConsts.Kxml_phone_number, datablock.PhoneNumber());
            
            WriteDetail(aOutput, XmlConsts.Kxml_report_ok, datablock.ReportOK());
            WriteDetail(aOutput, XmlConsts.Kxml_report_fail, datablock.ReportFail());
            WriteDetail(aOutput, XmlConsts.Kxml_report_param_name1, datablock.ReportParamName1());
            WriteDetail(aOutput, XmlConsts.Kxml_report_param_value1, datablock.ReportParamValue1());
            WriteDetail(aOutput, XmlConsts.Kxml_report_param_name2, datablock.ReportParamName2());
            WriteDetail(aOutput, XmlConsts.Kxml_report_param_value2, datablock.ReportParamValue2());
            WriteDetail(aOutput, XmlConsts.Kxml_report_param_name3, datablock.ReportParamName3());
            WriteDetail(aOutput, XmlConsts.Kxml_report_param_value3, datablock.ReportParamValue3());
            WriteDetail(aOutput, XmlConsts.Kxml_report_comments, datablock.ReportComments());

            aOutput.WriteEndElement(); // unique values
        }


        private static void WriteSymbolFileNames(XmlWriter aOutput, CXmlDataBlock datablock)
        {
            List<string> symbolFiles = datablock.SymbolFiles();

            string symbolfilenames = string.Empty;
            foreach (string fileName in symbolFiles)
            {
                if (symbolfilenames != string.Empty)
                {
                    symbolfilenames = symbolfilenames + ", ";
                }
                symbolfilenames = symbolfilenames + Path.GetFileName(fileName);
            }

            WriteDetail(aOutput, XmlConsts.Kxml_symbol_file_name, symbolfilenames);
        }

        private static void WriteDetail(XmlWriter aOutput, string name, ulong? value)
        {
            if (value != null)
            {
                aOutput.WriteStartElement(XmlConsts.Kxml_detail);
                aOutput.WriteAttributeString(XmlConsts.Kxml_name, name);
                aOutput.WriteAttributeString(XmlConsts.Kxml_value, value.ToString());
                aOutput.WriteEndElement();
            }
        }

        private static void WriteDetail(XmlWriter aOutput, string name, double? value)
        {
            if (value != null)
            {
                aOutput.WriteStartElement(XmlConsts.Kxml_detail);
                aOutput.WriteAttributeString(XmlConsts.Kxml_name, name);
                aOutput.WriteAttributeString(XmlConsts.Kxml_value, value.ToString());
                aOutput.WriteEndElement();
            }
        }

        private static void WriteDetail(XmlWriter aOutput, string name, string value)
        {
            if (value != string.Empty)
            {
                aOutput.WriteStartElement(XmlConsts.Kxml_detail);
                aOutput.WriteAttributeString(XmlConsts.Kxml_name, name);
                aOutput.WriteAttributeString(XmlConsts.Kxml_value, value);
                aOutput.WriteEndElement();
            }
        }

        private static void WriteDetail(XmlWriter aOutput, string name, int? value)
        {
            if (value != null)
            {
                aOutput.WriteStartElement(XmlConsts.Kxml_detail);
                aOutput.WriteAttributeString(XmlConsts.Kxml_name, name);
                aOutput.WriteAttributeString(XmlConsts.Kxml_value, value.ToString());
                aOutput.WriteEndElement();
            }
        }

        #region Data members
        private List<CXmlDataBlock> iDatablocks = new List<CXmlDataBlock>();
        #endregion
    }
}
