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
using System.Collections.Generic;
using System.Text;

namespace SymbianStructuresLib.Debug.Common.FileName
{
    public static class PlatformFileNameConstants
    {
        public static class Device
        {
            public const string KPathWildcardRoot = "?:";
            public const string KPathWildcardSysBin = @"?:\sys\bin\";
        }

        public static class Host
        {
            public const string KPathEpoc32ReleaseArmv5Urel = @"\epoc32\release\armv5\urel\";
        }
    }
}
