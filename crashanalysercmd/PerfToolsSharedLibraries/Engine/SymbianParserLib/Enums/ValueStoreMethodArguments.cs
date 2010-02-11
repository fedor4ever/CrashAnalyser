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

namespace SymbianParserLib.Enums
{
    public enum TValueStoreMethodArguments
    {
        /// <summary>
        /// Special value which means that no argument specification is
        /// supplied at compile time. The parser library runtime will guess
        /// which arguments it should supply (in the correct order) based
        /// upon the method signature
        /// </summary>
        EValueStoreMethodArgumentCalculateAtRuntime = -1,
        
        /// <summary>
        /// Supply field name as string
        /// </summary>
        EValueStoreMethodArgumentNameAsString = 0,

        /// <summary>
        /// Supply field name as object
        /// </summary>
        EValueStoreMethodArgumentNameAsObject,

        /// <summary>
        /// Supply field value
        /// </summary>
        EValueStoreMethodArgumentValue,

        /// <summary>
        /// Supply paragraph for associated field
        /// </summary>
        EValueStoreMethodArgumentParagraph,

        /// <summary>
        /// Supply line for associated field
        /// </summary>
        EValueStoreMethodArgumentLine,

        /// <summary>
        /// Supply field
        /// </summary>
        EValueStoreMethodArgumentField,
    }
}
