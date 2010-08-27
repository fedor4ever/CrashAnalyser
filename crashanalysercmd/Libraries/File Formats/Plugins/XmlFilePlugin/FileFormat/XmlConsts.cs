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
* The class CrashInfoConsts is part of CrashAnalyser CrashInfoFile plugin.
* Defines constant strings used in the crash info file format.
* 
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace XmlFilePlugin.PluginImplementations.FileFormat
{
    internal class XmlConsts
    {
        //General constants
        public const string KRegistrationMiscInfo = "MOBILECRASH_REGISTRATION";
        public const string KAliveTimeMiscInfo = "MOBILECRASH_ALIVETIME";
        
        public const string Kxml_doctype = "MobileCrashCISchema.dtd";
        public const string Kxml_unknown_process = "UnknownProcess.exe";
        public const string Kxml_report = "report";
        public const string Kxml_device = "device";
        public const string Kxml_report_details = "report_details";
        public const string Kxml_platform = "platform";
        public const string Kxml_sos = "SOS";
        public const string Kxml_s60 = "S60";
        public const string Kxml_code = "code";
        public const string Kxml_version = "version";
        public const string Kxml_romid = "romid";
        public const string Kxml_variant = "variant";
        public const string Kxml_imei = "imei";
        public const string Kxml_timestamp = "timestamp";
        public const string Kxml_type = "type";
        public const string Kxml_file_name = "file_name";
        public const string Kxml_report_category = "report_category";
        public const string Kxml_report_type = "report_type";
        public const string Kxml_crashed_module = "crashed_module";
        public const string Kxml_defect_hash = "defect_hash";
        public const string Kxml_detailed_defect_hash = "detailed_defect_hash";
        public const string Kxml_testset = "testset";
        public const string Kxml_loaded_dlls = "loaded_dlls";
        public const string Kxml_loaded_dll = "loaded_dll";
        public const string Kxml_name = "name";
        public const string Kxml_value = "value";
        public const string Kxml_start_address = "start_address";
        public const string Kxml_end_address = "end_address";
        public const string Kxml_traces = "traces";
        public const string Kxml_detail = "detail";
        public const string Kxml_dictionary_values = "dictionary_values";
        public const string Kxml_unique_values = "unique_values";
        public const string Kxml_panic_id = "PANIC_ID";
        public const string Kxml_panic_category = "PANIC_CATEGORY";
        public const string Kxml_panic_description = "PANIC_DESCRIPTION";
        public const string Kxml_language = "LANGUAGE";
        public const string Kxml_panicked_process = "PANICKED_PROCESS";
        public const string Kxml_network_country_code = "NETWORK_COUNTRY_CODE";
        public const string Kxml_network_identity = "NETWORK_IDENTITY";
        public const string Kxml_s60version = "S60VERSION";
        public const string Kxml_production_mode = "PRODUCTION_MODE";
        public const string Kxml_crash_source = "CRASH_SOURCE";

        public const string Kxml_product_code = "P_CODE";
        public const string Kxml_variant_version = "VARIANT_VERSION";
        public const string Kxml_file_type = "FILE_TYPE";
        public const string Kxml_available_memory = "AVAILABLE_MEMORY";
        public const string Kxml_program_counter = "PROGRAM_COUNTER";
        public const string Kxml_program_counter_symbol = "PC_SYMBOL";
        public const string Kxml_misc_info = "MISC_INFO";
        public const string Kxml_reporter = "REPORTER";
        public const string Kxml_resetreason = "RESETREASON";
        public const string Kxml_uptime = "UPTIME";
        public const string Kxml_siminfo = "SIMINFO";
        public const string Kxml_locinfo = "LOCINFO";
        public const string Kxml_cellid = "CELLID";
        public const string Kxml_psninfo = "PSNINFO";
        public const string Kxml_uid = "UID";
        public const string Kxml_diskinfo = "DISKINFO";
        public const string Kxml_phone_number = "PHONE_NUMBER";
        public const string Kxml_register = "REGISTER";
        public const string Kxml_register_extra = "REGISTER_EXTRA";
        public const string Kxml_call_stack_text = "CALL_STACK";
        public const string Kxml_trace_data = "TRACE_DATA";
        public const string Kxml_eventlog = "EVENTLOG";
        public const string Kxml_symbol_file_name = "SYMBOL_FILE_NAME";

        public const string Kxml_report_ok = "REPORT_OK";
        public const string Kxml_report_fail = "REPORT_FAIL";
        public const string Kxml_report_param_name1 = "REPORT_PARAM_NAME1";
        public const string Kxml_report_param_value1 = "REPORT_PARAM_VALUE1";
        public const string Kxml_report_param_name2 = "REPORT_PARAM_NAME2";
        public const string Kxml_report_param_value2 = "REPORT_PARAM_VALUE2";
        public const string Kxml_report_param_name3 = "REPORT_PARAM_NAME3";
        public const string Kxml_report_param_value3 = "REPORT_PARAM_VALUE3";
        public const string Kxml_report_comments = "REPORT_COMMENTS";

        public const string Kxml_separator = "|";
        public const string Kxml_symbol_separator = ":";
        public const string Kxml_EOL = "\r\n";        

        public const string Kxml_type_crash = "crash";
        public const string Kxml_type_report = "report";
        public const string Kxml_type_regmsg = "rgmsg";
        public const string Kxml_type_alivemsg = "alivemsg";

        public const string Kxml_production_mode_value = "Production mode";
        public const string Kxml_rnd_mode_value = "R&D mode";
        public const string Kxml_crash_source_user = "User";
        public const string Kxml_crash_source_kernel = "Kernel";

        public const int KMaxStackSize = 65000; //max length of call stack output in bytes
        public const int KMaxItemAboveSP = 7; //How many items are taken above stack pointer, should never be less than 2 to keep PC and LR 
        public const int KNonSymbolItemsAfterSP = 20; //How many items are always taken below stack pointer (rest items are taken if they have symbols)
        
        //Internally used constants

        public enum MobileCrashFileType
        {            
            ETypeBasicCrash = 0,
            ETypeCrashAPIReport,
            ETypeRegistrationMessage,
            ETypeAliveMessage
        }
    
    }
}
