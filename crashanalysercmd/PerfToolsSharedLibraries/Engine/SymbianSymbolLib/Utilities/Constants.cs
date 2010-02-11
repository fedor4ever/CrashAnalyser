/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using System.Collections.Generic;
using System.Text;

namespace SymbianSymbolLib
{
    public static class Constants
    {
        public static class Device
        {
            public const string KPathSysBin = @"\sys\bin\";
            public const string KPathSysBinROM = "z:" + KPathSysBin;
        }

        public static class Host
        {
            public const string KPathEpoc32ReleaseArmv5Urel = @"\epoc32\release\armv5\urel\";
            public const string KPathEpoc32ReleaseArmv5Udeb = @"\epoc32\release\armv5\urel\";
        }
    }
}
