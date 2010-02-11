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
﻿using System;
using System.Collections.Generic;
using System.Text;
using CrashItemLib.Crash.Symbols;
using SymbianStructuresLib.Debug.Symbols;

namespace CrashInfoFilePlugin.PluginImplementations.FileFormat
{
    static class CCrashInfoFileUtilities
    {
        public static bool IsSymbolSerializable(CISymbol aSymbol)
        {
            bool ret = true;
            //
            if (aSymbol.IsNull)
            {
                ret = false;
            }
            else
            {
                TSymbolType type = aSymbol.Symbol.Type;
                ret = ( type != TSymbolType.EUnknown );
            }           
            return ret;
        }

        /** Write given value, if it exists, in tags to stream */
        public static void WriteOutputTags(uint? aContent, string aTagText, System.IO.StreamWriter aOutput)
        {
            if (aContent.HasValue)
            {
                aOutput.Write(MakeOutputTags(aContent.Value.ToString(), aTagText));
            }          
        }

        /** Write given value, if it exists, in tags to stream */
        public static void WriteOutputTags(int? aContent, string aTagText, System.IO.StreamWriter aOutput)
        {
            if (aContent.HasValue)
            {
                aOutput.Write(MakeOutputTags(aContent.Value.ToString(), aTagText));
            }
        }

        /** Write given value, if it exists, in tags to stream */
        public static void WriteOutputTags(ulong? aContent, string aTagText, System.IO.StreamWriter aOutput)
        {
            if (aContent.HasValue)
            {
                aOutput.Write(MakeOutputTags(aContent.Value.ToString(), aTagText));
            }
        }


        /** Write given string, if not empty, in tags to stream */
        public static void WriteOutputTags(string aContent, string aTagText, System.IO.StreamWriter aOutput)
        {
            if (aContent != string.Empty)
            {
                aOutput.Write(MakeOutputTags(aContent, aTagText));
            }
        }
        /** Return the parameter string enclosed in crashinfofile identifier tags */
        public static string MakeOutputTags(string aContent, string aTagText)
        {
            string output = CCrashInfoFileUtilities.BlockStartMarker(aTagText);
            output += aContent;
            output += CCrashInfoFileUtilities.BlockEndMarker(aTagText);
            return output;
        }

        /** Return the parameter integer enclosed in crashinfofile identifier tags */
        public static string MakeOutputTags(uint aContent, string aTagText)
        {
            return MakeOutputTags(aContent.ToString(), aTagText);
        }

        public static string BlockEndMarker(string aId)
        {
            return CrashInfoConsts.KCloseIdStart + aId + CrashInfoConsts.KCloseIdEnd + CrashInfoConsts.KEOL;
        }

        public static string BlockStartMarker(string aId)
        {
            return CrashInfoConsts.KNewIdStart + aId + CrashInfoConsts.KNewIdEnd;
        }
    }
}
