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
using SymbianUtils;
using SymbianParserLib.Engine;
using SymbianParserLib.Elements;

namespace CrashDebuggerLib.Parsers.State
{
    internal enum TState
    {
        EStateIdle = 0,
        EStateInfoFault,
        EStateInfoCpu,
        EStateInfoDebugMask,
        EStateInfoScheduler,
        EStateInfoUserContextTable,
        EStateTheCurrentProcess,
        EStateTheCurrentThread,
        EStateContainerCodeSegs,
        EStateContainerThread,
        EStateContainerProcess,
        EStateContainerChunk,
        EStateContainerLibrary,
        EStateContainerSemaphore,
        EStateContainerMutex,
        EStateContainerTimer,
        EStateContainerServer,
        EStateContainerSession,
        EStateContainerLogicalDevice,
        EStateContainerPhysicalDevice,
        EStateContainerLogicalChannel,
        EStateContainerChangeNotifier,
        EStateContainerUndertaker,
        EStateContainerMessageQueue,
        EStateContainerPropertyRef,
        EStateContainerConditionalVariable,
        EStateThreadStacks,
        EStateDone
    }
}
