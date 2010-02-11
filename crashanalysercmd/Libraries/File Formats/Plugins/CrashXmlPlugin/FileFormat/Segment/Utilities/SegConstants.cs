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
using System.Text;
using System.Collections.Generic;

namespace CrashXmlPlugin.FileFormat.Segment.Entries
{
    internal static class SegConstants
    {
        // Common
        public const string CmnAddress = "address";
        public const string CmnSize = "size";
        public const string CmnName = "name";
        public const string CmnAttributes = "attributes";
        public const string CmnXIP = "xip";
        public const string CmnRAM = "ram";
        public const string CmnRange = "range";
        public const string CmnValue = "value";
        public const string CmnText = "text";
        public const string CmnBase = "base";
        public const string CmnType = "type";
        public const string CmnPriority = "priority";
        public const string CmnChecksum = "checksum";
        public const string CmnDate = "date";
        public const string CmnTime = "time";
        public const string CmnIndex = "index";
        public const string CmnStart = "start";
        public const string CmnEnd = "end";

        // Id related
        public const string CmnId = "id";
        public const string CmnId_Explicit = "explicit";

        // Header
        public const string Header = "seg_header";
        public const string Header_Uptime = "uptime";

        // Symbols
        public const string Symbols = "seg_symbols";
        public const string Symbols_SymbolSet = "symbol_set";
        public const string Symbols_SymbolSet_Source = "source";
        public const string Symbols_SymbolSet_Symbol = "symbol";
        public const string Symbols_SymbolSet_Symbol_Object = "object";
        public const string Symbols_SymbolSet_Symbol_Attribute_Map = "map";
        public const string Symbols_SymbolSet_Symbol_Attribute_Symbol = "symbol";

        // Stacks
        public const string Stacks = "seg_stacks";
        public const string Stacks_Stack = "stack";
        public const string Stacks_Stack_Data = "stack_data";
        public const string Stacks_Stack_Attributes_Accurate = "accurate";
        public const string Stacks_Stack_Attributes_Heuristic = "heuristic";
        public const string Stacks_Stack_Data_Entry = "stack_entry";
        public const string Stacks_Stack_Data_Offset = "offset";
        public const string Stacks_Stack_Data_Entry_Attributes_FromRegister = "from_register";
        public const string Stacks_Stack_Data_Entry_Attributes_CurrentStackPointer = "current_stack_pointer";
        public const string Stacks_Stack_Data_Entry_Attributes_Accurate = "accurate";
        public const string Stacks_Stack_Data_Entry_Attributes_OutsideBounds = "outside_current_stack_pointer_range";
        
        // Registers
        public const string Registers = "seg_registers";
        public const string Registers_RegisterSet = "register_set";
        public const string Registers_RegisterSet_CurrentBank = "current_bank";
        public const string Registers_RegisterSet_Register = "register";
        public const string Registers_RegisterSet_Register_Extra = "extra";

        // Threads
        public const string Threads = "seg_threads";
        public const string Threads_Thread = "thread";
        public const string Threads_Thread_FullName = "fullname";

        // Processes
        public const string Processes = "seg_processes";
        public const string Processes_Process = "process";
        public const string Processes_Process_UID1 = "uid1";
        public const string Processes_Process_UID2 = "uid2";
        public const string Processes_Process_UID3 = "uid3";
        public const string Processes_Process_SID = "sid";
        public const string Processes_Process_Generation = "generation";

        // Messages
        public const string Messages = "seg_messages";
        public const string Messages_Message = "message";
        public const string Messages_Message_Title = "title";
        public const string Messages_Message_Line = "line";

        // Exit info
        public const string ExitInfo = "exit_info";
        public const string ExitInfo_Type = "exit_type";
        public const string ExitInfo_Type_Kill = "Kill";
        public const string ExitInfo_Type_Exception = "Exception";
        public const string ExitInfo_Type_Terminate = "Terminate";
        public const string ExitInfo_Type_Panic = "Panic";
        public const string ExitInfo_Type_Pending = "Pending";
        public const string ExitInfo_Reason = "exit_reason";
        public const string ExitInfo_Category = "exit_category";

        // Code Segs
        public const string CodeSegs = "seg_codesegs";
        public const string CodeSegs_CodeSeg = "codeseg";
        public const string CodeSegs_CodeSeg_Attributes_NoSymbols = "nosymbols";
        public const string CodeSegs_CodeSeg_Attributes_Speculative = "speculative";
        public const string CodeSegs_CodeSeg_Attributes_Mismatch = "mismatch";

        // HW Info
        public const string HWInfo = "seg_hw_info";
        public const string HWInfo_ProductType = "product_type";
        public const string HWInfo_ProductCode = "product_code";
        public const string HWInfo_SerialNumber = "serial_number";

        // SW Info
        public const string SWInfo = "seg_sw_info";
        public const string SWInfo_Platform = "platform";
        public const string SWInfo_Language = "language";

        // Event log
        public const string EventLog = "seg_event_log";
        public const string EventLog_Event = "event";

        // Telephony
        public const string Telephony = "seg_telephony";
        public const string Telephony_PhoneNumber = "phone_number";
        public const string Telephony_Imsi = "imsi";
        public const string Telephony_Imei = "imei";
        public const string Telephony_Network = "network";
        public const string Telephony_Network_Country = "country";
        public const string Telephony_Network_Identity = "identity";
        public const string Telephony_Network_Cell = "cell";
        public const string Telephony_Network_Registration = "registration";

        // Memory Info
        public const string MemoryInfo = "seg_memory_info";
        public const string MemoryInfo_Free = "free";
        public const string MemoryInfo_Capacity = "capacity";
        public const string MemoryInfo_UID = "uid";
        public const string MemoryInfo_Drive = "drive";
        public const string MemoryInfo_Drive_Path = "path";
        public const string MemoryInfo_Drive_Vendor = "vendor";
        public const string MemoryInfo_RAM = "ram";

        // Binary data
        public const string BinaryData = "seg_binary_data";
        public const string BinaryData_Blob = "blob";
        public const string BinaryData_Blob_Payload = "payload";
        public const string BinaryData_Blob_Payload_Data = "data";

        // (Register) Value Interpretations
        public const string ValueInterpretation = "seg_value_interpretations";
        public const string ValueInterpretation_Entry = "vi_entry";
        public const string ValueInterpretation_Entry_Hex = "hex";
        public const string ValueInterpretation_Entry_Binary = "binary";
        public const string ValueInterpretation_Entry_Endianness = "endian";
        public const string ValueInterpretation_Entry_Endianness_Bit0 = "bit0";
        public const string ValueInterpretation_Entry_Endianness_Bit0_Right = "right";
        public const string ValueInterpretation_Entry_Endianness_Bit0_Left = "left";
        public const string ValueInterpretation_Entry_Endianness_Big = "big";
        public const string ValueInterpretation_Entry_Endianness_Little = "little";
        public const string ValueInterpretation_Entry_Description = "description";
        public const string ValueInterpretation_Entry_Category = "category";
        public const string ValueInterpretation_Entry_Interpretation = "interpretation";
        public const string ValueInterpretation_Entry_Reserved = "reserved";
        public const string ValueInterpretation_Entry_Interpretation_BitRange = "bit_range";
        public const string ValueInterpretation_Entry_Interpretation_BitGroup = "bit_group";
        public const string ValueInterpretation_Entry_Interpretation_Bit = "bit";
        public const string ValueInterpretation_Entry_Interpretation_Bit_Char = "char";

        // Trace
        public const string Traces = "seg_traces";
        public const string Traces_Line = "line";
        public const string Traces_Type_Binary = "bin";
        public const string Traces_Type_Raw = "raw";
        public const string Traces_Type_Unknown = "unknown";
        public const string Traces_Type_Text = "";
        public const string Traces_ContextId = "context_id";
        public const string Traces_TimeStamp = "timestamp";
        public const string Traces_Prefix = "prefix";
        public const string Traces_Suffix = "suffix";
        public const string Traces_File = "file";
        public const string Traces_LineNumber = "line_number";
        public const string Traces_ComponentId = "component";
        public const string Traces_GroupId = "group";
        public const string Traces_InstanceId = "id";
    }
}
