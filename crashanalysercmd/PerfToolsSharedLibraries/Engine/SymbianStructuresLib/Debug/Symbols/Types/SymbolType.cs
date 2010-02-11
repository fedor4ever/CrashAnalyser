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
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using SymbianUtils.Range;

namespace SymbianStructuresLib.Debug.Symbols
{
    /// <summary>
    /// The 'type' associated with a symbol. Note that the types are
    /// ordered such that e.g. a RAM-loaded subobject will be reported
    /// as "sub object" even though it is also RAM-loaded. Therefore, 
    /// do not rely purely on "RAM loaded" to identify all RAM loaded
    /// types!
    /// </summary>
    public enum TSymbolType : sbyte 
	{
        // Do not change the order - these are priority based with
        // the most important symbol type appearing with a larger
        // value
		EUnknown = -1,
        EReadOnlySymbol = 0,
		EKernelGlobalVariable,
        ESubObject,
        ELabel,
		ERAMSymbol,
        EROMSymbol,
        ECode,
        EData,
        ENumber,
        ESection
	}
}