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
using SymbianStructuresLib.CodeSegments;

namespace SymbianStructuresLib.Debug.Symbols.Interfaces
{
    public interface ISymbolCodeSegmentResolver
    {
        bool IsMatchingCodeSegment( SymbolCollection aCollection, CodeSegDefinition aCodeSegment );
    }
}
