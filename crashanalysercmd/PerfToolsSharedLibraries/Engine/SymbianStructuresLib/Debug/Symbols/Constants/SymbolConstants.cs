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
using SymbianStructuresLib.Debug.Common.FileName;

namespace SymbianStructuresLib.Debug.Symbols.Constants
{
    public static class SymbolConstants
    {
        public const int KNullEntryAddress = 0;
        
        public const string KNonMatchingObjectName = "Unknown Object";
        public const string KNonMatchingInternedName = "Unknown Symbol";
        public const string KUnknownOffset = "[+ ??????]";
 
        public static readonly string[] KVTableOrTypeInfoPrefixes = new string[] { "vtable for ", "typeinfo for ", "typeinfo name for " };

        public const string KPrefixReadonly = "Image$$ER_RO$$";
        public const string KPrefixSubObject = "__sub_object()";
    }
}
