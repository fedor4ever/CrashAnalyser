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
using System.Xml;

namespace CrashXmlPlugin.FileFormat
{
    internal static class Constants
    {
        #region Node names

        // Root node name
        public const string RootNode = "crash_root";

        // For <crash_analyser>
        public const string CrashAnalyser = "crash_analyser";
        public const string CrashAnalyser_FileFormat = "file_format";
        public const string CrashAnalyser_Runtime = "runtime";
        public const string CrashAnalyser_Runtime_AnalysisType = "analysis_type";
        public const string CrashAnalyser_Runtime_CommandLine = "command_line";
        public const string CrashAnalyser_Runtime_InputFiles = "sources";
        public const string CrashAnalyser_Runtime_InputFiles_File = "file";

        // For <source_info>
        public const string SourceInfo = "source_info";
        public const string SourceInfo_FileType = "type";
        public const string SourceInfo_MasterFile = "source";
        public const string SourceInfo_LineNumber = "line";
        public const string SourceInfo_RawData = "raw_data";
        public const string SourceInfo_RawData_Item = "data";

        // For <segment_dictionary>
        public const string SegmentDictionary = "segment_dictionary";
        public const string SegmentDictionary_Segment = "segment";

        // For <segment_table>
        public const string SegmentTable = "segment_table";

        // For versions
        public const string Version = "version";
        public const string Version_Extended = "version_extended";
        public const string Version_Extended_Major = "major";
        public const string Version_Extended_Minor = "minor";
        public const string Version_Text = "version_text";
        public const string Version_Text_List = "version_text_list";

        // Common
        public const string CmnLink = "link";
        public const string CmnLink_Seg = "seg";
        public const string CmnLinkList = "linklist";
        #endregion

        #region Versions
        public const int MasterFileFormatVersionMajor = 1;
        public const int MasterFileFormatVersionMinor = 0;
        #endregion

        #region Misc
        public const int KBinaryDataMaxLineLength = 60;
        #endregion
    }
}
