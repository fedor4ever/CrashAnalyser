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
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace DExcPlugin.ExpressionManager
{
    internal static class DExcExpressionManager
    {
        // For start of log
        public static readonly Regex LogStart = new Regex( "(?:.*)EKA2 USER CRASH LOG(?:.*)", RegexOptions.CultureInvariant | RegexOptions.Compiled );

        // For thread
        public static readonly Regex ThreadName = new Regex( "(?:.*)Thread Name: (.+)(?:.*)", RegexOptions.CultureInvariant | RegexOptions.Compiled );
        public static readonly Regex ThreadId = new Regex( "(?:.*)Thread ID: ([0-9]{1,5})(?:.*)", RegexOptions.CultureInvariant | RegexOptions.Compiled );
        public static readonly Regex ThreadStackRange = new Regex( "(?:.*)User Stack(?:\\:|) ([a-fA-F0-9]{8})\\-([a-fA-F0-9]{8})(?:.*)", RegexOptions.CultureInvariant | RegexOptions.Compiled );
        public static readonly Regex ThreadPanicDetails = new Regex( "(?:.*)Panic: (.*)-(\\d+)(?:.*)", RegexOptions.CultureInvariant | RegexOptions.Compiled );

        // For stack
        public static readonly Regex StackDataEntry = new Regex( "(?:.*)\r\n([a-fA-F0-9]{8})\r\n\r\n\\:\\s{1}\r\n\r\n((?:[a-fA-F0-9]{2})\\s{1}){1,16}\r\n(?:.*)", RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled );
 
        // For registers (exception)
        public static readonly Regex RegistersExceptionStart = new Regex( "(?:.*)UNHANDLED EXCEPTION(?:\\:|)(?:.*)", RegexOptions.CultureInvariant | RegexOptions.Compiled );
        public static readonly Regex RegistersExceptionSet1 = new Regex( "(?:.*)code=(\\d*) PC=([a-fA-F0-9]{8}) FAR=([a-fA-F0-9]{8}) FSR=([a-fA-F0-9]{8})(?:.*)", RegexOptions.CultureInvariant | RegexOptions.Compiled );
        public static readonly Regex RegistersExceptionSet2 = new Regex( "(?:.*)R13svc=([a-fA-F0-9]{8}) R14svc=([a-fA-F0-9]{8}) SPSRsvc=([a-fA-F0-9]{8})(?:.*)", RegexOptions.CultureInvariant | RegexOptions.Compiled );

        // For registers (user)
        public static readonly Regex RegistersUserStart = new Regex( "(?:.*)USER REGISTERS(?:\\:|)(?:.*)", RegexOptions.CultureInvariant | RegexOptions.Compiled );
        public static readonly Regex RegistersUserCPSR = new Regex( "(?:.*)CPSR=([a-fA-F0-9]{8})(?:.*)", RegexOptions.CultureInvariant | RegexOptions.Compiled );
        public static readonly Regex RegistersUserSet = new Regex( "(?:.*)r(\\d{2})=([a-fA-F0-9]{8}) ([a-fA-F0-9]{8}) ([a-fA-F0-9]{8}) ([a-fA-F0-9]{8})(?:.*)", RegexOptions.CultureInvariant | RegexOptions.Compiled );

        // For code segments
        public static readonly Regex CodeSegmentsStart = new Regex( "(?:.*)CODE SEGMENTS(?:\\:|)(?:.*)", RegexOptions.CultureInvariant | RegexOptions.Compiled );
        public static readonly Regex CodeSegmentsEntry = new Regex( "(?:.*)([a-fA-F0-9]{8})\\-([a-fA-F0-9]{8}) (.+)(?:.*)", RegexOptions.CultureInvariant | RegexOptions.Compiled );
    }
}
