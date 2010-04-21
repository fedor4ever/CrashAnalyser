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

namespace CrashInfoFilePlugin.PluginImplementations.FileFormat
{
    internal class CrashInfoConsts
    {
        //General constants
        public const int KVersionNumber = 10;
        public const string KNewIdStart = "[";
        public const string KNewIdEnd = "]";
        public const string KCloseIdStart = "[/";
        public const string KCloseIdEnd = "]";
        public const string KSeparator = "|";
        public const string KEOL = "\r\n";        
        public const string KRegistrationMiscInfo = "MOBILECRASH_REGISTRATION";
        public const string KAliveTimeMiscInfo = "MOBILECRASH_ALIVETIME";

        //Data item identifiers
        public const string Kversion = "VERSION";
        public const string Ktimestamp = "TIMESTAMP";
        public const string Kromid = "ROMID";
        public const string Ksw_version = "SW_VERSION";
        public const string Kvariant_id = "VARIANT_ID";
        public const string Khw_version = "HW_VERSION";
        public const string Kpanic_id = "PANIC_ID";
        public const string Kpanic_category = "PANIC_CATEGORY";
        public const string Kpanic_description = "PANIC_DESCRIPTION";
        public const string Klanguage = "LANGUAGE";
        public const string Kpanicked_process = "PANICKED_PROCESS";
        public const string Kprogram_counter = "PROGRAM_COUNTER";
        public const string Kcrashed_module_name = "CRASHED_MODULE_NAME";
        public const string Kregister = "REGISTER";
        public const string Kcrashtime_loaded_dlls = "CRASHTIME_LOADED_DLLS";
        public const string Kavailable_memory = "AVAILABLE_MEMORY";
        public const string Kuser_comment = "USER_COMMENT";
        public const string Kmemory_info = "MEMORY_INFO";
        public const string Kmisc_info = "MISC_INFO";
        public const string Kreporter = "REPORTER";
        public const string Karchive = "ARCHIVE";
        public const string Kproduct_type = "PRODUCT_TYPE";
        public const string Kproduction_mode = "PRODUCTION_MODE";
        public const string Kcrash_source = "CRASH_SOURCE";
        public const string Kimei = "IMEI";
        public const string Knetwork_country_code = "NETWORK_COUNTRY_CODE";
        public const string Knetwork_identity = "NETWORK_IDENTITY";
        public const string Kresetreason = "RESETREASON";
        public const string Kuptime = "UPTIME";
        public const string Ktestset = "TESTSET";
        public const string Ksymbols = "SYMBOLS";
        public const string Kcall_stack = "CALL_STACK";
        public const string Kexcinfo = "EXCINFO";
        public const string Ksiminfo = "SIMINFO";
        public const string Klocinfo = "LOCINFO";
        public const string Kcellid = "CELLID";
        public const string Kpsninfo = "PSNINFO";
        public const string Ks60version = "S60VERSION";
        public const string Kdiskinfo = "DISKINFO";
        public const string Kmmcinfo = "MMCINFO";
        public const string Kuid = "UID";
        public const string Keventlog = "EVENTLOG";
        public const string Kproduct_code = "PRODUCT_CODE";
        public const string Kvariant_version = "VARIANT_VERSION";
        public const string Kfile_type = "FILE_TYPE";
        public const string Kreport_type = "REPORT_TYPE";
        public const string Kreport_category = "REPORT_CATEGORY";
        public const string Kreport_ok = "REPORT_OK";
        public const string Kreport_fail = "REPORT_FAIL";
        public const string Kreport_param_name1 = "REPORT_PARAM_NAME1";
        public const string Kreport_param_value1 = "REPORT_PARAM_VALUE1";
        public const string Kreport_param_name2 = "REPORT_PARAM_NAME2";
        public const string Kreport_param_value2 = "REPORT_PARAM_VALUE2";
        public const string Kreport_param_name3 = "REPORT_PARAM_NAME3";
        public const string Kreport_param_value3 = "REPORT_PARAM_VALUE3";
        public const string Kreport_comments = "REPORT_COMMENTS";
        public const string Ktrace_data = "TRACE_DATA";
        public const string Knum_datablocks = "NUM_DATABLOCKS";

        //CrashAnalyser implementation's own types (not used in selge output)

        public const string Kregister_extra = "REGISTER_EXTRA";
        public const string Kcall_stack_text = "CALL_STACK_TEXT";
        public const string Kcrash_hash = "DEFECT_HASH"; // New crash hash that DbMover used to create itself.
        public const string Kbinfile_name = "BIN_FILE_NAME";
        public const string Ksymbolfile_names = "SYMBOL_FILE_NAME";

        public const string Kproduction_mode_value = "Production mode";
        public const string Krnd_mode_value = "R&D mode";
        public const string Kcrash_source_user = "User";
        public const string Kcrash_source_kernel = "Kernel";

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
