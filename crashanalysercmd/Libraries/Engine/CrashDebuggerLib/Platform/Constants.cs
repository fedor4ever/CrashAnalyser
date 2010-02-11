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

namespace CrashDebuggerLib.Platform
{
    public static class ProcessNames
    {
        public const string KKernel = "ekern.exe";
    }

    public static class NKernSizes
    {
        public const int KSizeOf_Pointer = 4;
        public const int KSizeOf_Byte = 1;
        public const int KSizeOf_TInt = 4;
        public const int KSizeOf_SDblQueLink = KSizeOf_Pointer + KSizeOf_Pointer; // sizeof( SDblQueLink.iPrev ) + sizeof( SDblQueLink.iNext )
        public const int KSizeOf_TPriListLink = KSizeOf_SDblQueLink + ( 4 * KSizeOf_Byte ); // iPriority, iSpare1, iSpare2, iSpare3
    }

    public static class NKernOffsets
    {
        public const int KOffsetOf_iRequestSemaphore_In_NThread = NKernSizes.KSizeOf_TPriListLink +
                                                                  NKernSizes.KSizeOf_Pointer + // NFastMutex* iHeldFastMutex
                                                                  NKernSizes.KSizeOf_Pointer + // NFastMutex* iWaitFastMutex
                                                                  NKernSizes.KSizeOf_Pointer + // TAny* iAddressSpace
                                                                  NKernSizes.KSizeOf_TInt + // TInt iTime
                                                                  NKernSizes.KSizeOf_TInt  // TInt iTimeslice
            ;
    }
}
