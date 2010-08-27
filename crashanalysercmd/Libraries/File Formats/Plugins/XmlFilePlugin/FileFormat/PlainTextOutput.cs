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
* The class PlainTextOutput is part of CrashAnalyser XmlFilePlugin plugin.
* This class creates plain text output.
*
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using XmlFilePlugin.PluginImplementations.FileFormat;
using System.Text.RegularExpressions;

namespace XmlFilePlugin.PluginImplementations.FileFormat
{
    class PlainTextOutput
    {
        private static int KColumnWidth = 30;
        private static string KTitleCrashFile    = "Crash File";
        private static string KTitleCodesegments = "Crash Time Loaded Code Segments";
        private static string KTitleRegisters    = "Registers";
        private static string KTitleCallStacks   = "Call Stacks";
        private static string KTitleOstTraces    = "OST Traces";
        private static string KTitleEventLog     = "Event Log";
        private static string KTitleSymbolFileNames = "Symbol File Names";
        private static string KTitleUniqueValues = "Unique Values";

        public static void Write(StreamWriter writer, List<CXmlDataBlock> datablocks)
        {
            foreach (CXmlDataBlock datablock in datablocks)
            {
                WriteDataBlock(writer, datablock);
                WriteUniqueValues(writer, datablock);
            }
        }

        /**
         * Writes one datablock as plain text.
         */
        private static void WriteDataBlock(StreamWriter writer, CXmlDataBlock datablock)
        {
            writer.Write(PrettyTitle(KTitleCrashFile));

            // Device
            if (datablock.Platform().Equals(XmlConsts.Kxml_sos))
            {
                writer.Write(PrettyString(XmlConsts.Kxml_platform, XmlConsts.Kxml_s60));
            }
            else
            {
                writer.Write(PrettyString(XmlConsts.Kxml_platform, datablock.Platform()));
            }

            writer.Write(PrettyString(XmlConsts.Kxml_code, datablock.ProductType()));
            writer.Write(PrettyString(XmlConsts.Kxml_version, datablock.SWVersion()));

            if (datablock.RomId() != null)
            {
                writer.Write(PrettyString(XmlConsts.Kxml_romid, "0x" + datablock.RomId().Value.ToString("X8")));
            }

            // variant is not written.

            if (datablock.Imei() != string.Empty)
            {
                writer.Write(PrettyString(XmlConsts.Kxml_imei, datablock.Imei()));
            }

            writer.Write(PrettyString(XmlConsts.Kxml_timestamp, datablock.TimeStampText()));

            string crashtypestr = "Not found";
            if (datablock.FileType() == XmlConsts.MobileCrashFileType.ETypeBasicCrash)
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

            writer.Write(PrettyString(XmlConsts.Kxml_type, crashtypestr));

            writer.Write(PrettyString(XmlConsts.Kxml_file_name, datablock.BinFilename()));

            if (crashtypestr.Equals(XmlConsts.Kxml_type_report))
            {
                writer.Write(PrettyString(XmlConsts.Kxml_report_category, datablock.ReportCategory()));
            }

            if (crashtypestr.Equals(XmlConsts.Kxml_type_report))
            {
                writer.Write(PrettyString(XmlConsts.Kxml_report_type, datablock.ReportType()));
            }

            if (datablock.CrashedModuleName() != string.Empty)
            {
                writer.Write(PrettyString(XmlConsts.Kxml_crashed_module, datablock.CrashedModuleName()));
            }

            if (datablock.Hash() != string.Empty)
            {
                writer.Write(PrettyString(XmlConsts.Kxml_defect_hash, datablock.Hash()));
            }

            if (datablock.DetailedHash() != string.Empty)
            {
                writer.Write(PrettyString(XmlConsts.Kxml_detailed_defect_hash, datablock.DetailedHash()));
            }

            if (datablock.TestSet() != string.Empty)
            {
                writer.Write(PrettyString(XmlConsts.Kxml_testset, datablock.TestSet()));
            }

            WriteDictionaryValues(writer, datablock);

            // Codesegments
            if (datablock.CodeSegs().Count > 0)
                writer.Write(PrettyTitle(KTitleCodesegments));
    
            foreach (CXmlCodeSegItem codeseg in datablock.CodeSegs())
            {
                string range = "0x" + codeseg.Start.ToString("X8") + " - " + "0x" + codeseg.End.ToString("x8");
                writer.WriteLine(range + FillSpaces(range) + codeseg.Name);
            }

            // Registers
            if (datablock.RegStorage().BasicRegs().ToPrettyString().Length > 0 ||
                datablock.RegStorage().OtherRegLists().Count > 0)
            {
                writer.Write(PrettyTitle(KTitleRegisters));
            }
            writer.WriteLine(datablock.RegStorage().BasicRegs().ToPrettyString());

            // Extra registers
            foreach (CXmlRegisterStorage.CCrashInfoRegisterList registerList in datablock.RegStorage().OtherRegLists())
            {
                writer.WriteLine(registerList.ToPrettyString());
            }

            // Call stacks
            if (datablock.CallStacks().Count > 0)
                writer.Write(PrettyTitle(KTitleCallStacks));
    
            foreach (XmlFilePlugin.FileFormat.CXmlCallStack stack in datablock.CallStacks())
            {
                writer.WriteLine(stack.ToString());
            }

            // OST traces
            if (datablock.OstTraces().Count > 0)
                writer.Write(PrettyTitle(KTitleOstTraces));
 
            foreach (string line in datablock.OstTraces())
            {
                writer.WriteLine(line);
            }

            // Event log
            if (datablock.Eventlog().Count > 0)
                writer.Write(PrettyTitle(KTitleEventLog));
            
            foreach (string line in datablock.Eventlog())
            {
                writer.WriteLine(line);
            }
        }


        /**
         * Writes dictionary values.
         */
        private static void WriteDictionaryValues(StreamWriter writer, CXmlDataBlock datablock)
        {
            writer.Write(PrettyString(XmlConsts.Kxml_panic_id, datablock.PanicId()));
            writer.Write(PrettyString(XmlConsts.Kxml_panic_category, datablock.PanicCategory()));
            writer.Write(PrettyString(XmlConsts.Kxml_panic_description, Regex.Replace(datablock.PanicDescription(), "<(.|\n)*?>", "")));
            writer.Write(PrettyString(XmlConsts.Kxml_language, datablock.Language()));
            
            if (!datablock.Process().Equals(XmlConsts.Kxml_unknown_process))
            {
                writer.Write(PrettyString(XmlConsts.Kxml_panicked_process, datablock.Process()));
            }

            writer.Write(PrettyString(XmlConsts.Kxml_network_country_code, datablock.NetworkCountry()));
            writer.Write(PrettyString(XmlConsts.Kxml_network_identity, datablock.NetworkIdentity()));
            writer.Write(PrettyString(XmlConsts.Kxml_s60version, datablock.S60Version()));
            writer.Write(PrettyString(XmlConsts.Kxml_product_code, datablock.ProductCode()));
            writer.Write(PrettyString(XmlConsts.Kxml_variant_version, datablock.VariantVersion()));

            if (datablock.ReportType() != string.Empty)
            {
                writer.Write(PrettyString(XmlConsts.Kxml_file_type, "1"));
            }
            else
            {
                writer.Write(PrettyString(XmlConsts.Kxml_file_type, "0"));
            }

            // Production mode
            if (datablock.ProductionMode() != -1)
            {
                if (datablock.ProductionMode() == 1)
                {
                    writer.Write(PrettyString(XmlConsts.Kxml_production_mode, XmlConsts.Kxml_production_mode_value));
                }
                else
                {
                    writer.Write(PrettyString(XmlConsts.Kxml_production_mode, XmlConsts.Kxml_rnd_mode_value));
                }
            }

            // Crash source
            if (datablock.CrashSource() != -1)
            {
                if (datablock.CrashSource() == 1)
                {
                    writer.Write(PrettyString(XmlConsts.Kxml_crash_source, XmlConsts.Kxml_crash_source_user));
                }
                else
                {
                    writer.Write(PrettyString(XmlConsts.Kxml_crash_source, XmlConsts.Kxml_crash_source_kernel));
                }
            }

            // Symbol file names
            if (datablock.SymbolFiles().Count > 0)
                writer.Write(PrettyTitle(KTitleSymbolFileNames));
    
            foreach (string fileName in datablock.SymbolFiles())
            {
                writer.WriteLine(fileName);
            }
        
        }


        /**
         * Writes unique values.
         */
        private static void WriteUniqueValues(StreamWriter writer, CXmlDataBlock datablock)
        {
            // Unique values
            writer.Write(PrettyTitle(KTitleUniqueValues));

            // Available memory
            writer.Write(PrettyString(XmlConsts.Kxml_available_memory, datablock.FreeMemory()));

            if (datablock.RegStorage() != null && datablock.RegStorage().ProgramCounter() != null)
            {
                writer.Write(PrettyString(XmlConsts.Kxml_program_counter, "0x" + datablock.RegStorage().ProgramCounter().Value.ToString("X8")));
                writer.Write(PrettyString(XmlConsts.Kxml_program_counter_symbol, datablock.RegStorage().ProgramCounter().Symbol));
            }

            writer.Write(PrettyString(XmlConsts.Kxml_misc_info, datablock.GetMiscInfo()));

            // Reporter not written.
            // Resetreason not written.

            writer.Write(PrettyString(XmlConsts.Kxml_uptime, datablock.Uptime()));
            writer.Write(PrettyString(XmlConsts.Kxml_siminfo, datablock.Imsi()));
            writer.Write(PrettyString(XmlConsts.Kxml_locinfo, datablock.LocInfo()));
            writer.Write(PrettyString(XmlConsts.Kxml_cellid, datablock.NetworkCell()));
            writer.Write(PrettyString(XmlConsts.Kxml_psninfo, datablock.SerialNumber()));

            if (datablock.UID() != null)
               writer.Write(PrettyString(XmlConsts.Kxml_uid, "0x" + datablock.UID().Value.ToString("X8")));
            
            writer.Write(PrettyString(XmlConsts.Kxml_diskinfo, datablock.DiskInfo()));
            writer.Write(PrettyString(XmlConsts.Kxml_phone_number, datablock.PhoneNumber()));

            writer.Write(PrettyString(XmlConsts.Kxml_report_ok, datablock.ReportOK()));
            writer.Write(PrettyString(XmlConsts.Kxml_report_fail, datablock.ReportFail()));
            writer.Write(PrettyString(XmlConsts.Kxml_report_param_name1, datablock.ReportParamName1()));
            writer.Write(PrettyString(XmlConsts.Kxml_report_param_value1, datablock.ReportParamValue1()));
            writer.Write(PrettyString(XmlConsts.Kxml_report_param_name2, datablock.ReportParamName2()));
            writer.Write(PrettyString(XmlConsts.Kxml_report_param_value2, datablock.ReportParamValue2()));
            writer.Write(PrettyString(XmlConsts.Kxml_report_param_name3, datablock.ReportParamName3()));
            writer.Write(PrettyString(XmlConsts.Kxml_report_param_value3, datablock.ReportParamValue3()));
            writer.Write(PrettyString(XmlConsts.Kxml_report_comments, datablock.ReportComments()));
        }


        /**
         * Returns string in the pretty format (i.e. two columns).
         * Converts int param to string.
         */
        private static string PrettyString(string key, int? value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return PrettyString(key, value.ToString());
        }

        /**
         * Returns string in the pretty format (i.e. two columns).
         * Converts ulong param to string.
         */
        private static string PrettyString(string key, ulong? value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return PrettyString(key, value.ToString());
        }

        /**
         * Returns string in the pretty format (i.e. two columns).
         * Converts uint param to string.
         */
        private static string PrettyString(string key, uint? value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return PrettyString(key, value.ToString());
        }

        /**
         * Returns string in the pretty format (i.e. two columns).
         * Converts double param to string.
         */
        private static string PrettyString(string key, double? value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return PrettyString(key, value.ToString());
        }

        /**
         * Returns string in the pretty format (i.e. two columns)
         */
        private static string PrettyString(string key, string value)
        {
            if (value == string.Empty || value.Length == 0)
                return string.Empty;

            return key.ToUpper() + ":" + FillSpaces(key) + value + "\n";
        }

        /**
         * Fill spaces so that the output looks nice.
         */
        private static string FillSpaces(string key)
        {
            string spaceString = "";
            
            for (int i = key.Length; i < KColumnWidth; i++)
            {
                spaceString += " ";
            }

            return spaceString;
        }

        /**
         * Writes title in the pretty format
         */
        private static string PrettyTitle(string titleString)
        {
            return "\n === " + titleString + " ===\n";
        }
    }
}
