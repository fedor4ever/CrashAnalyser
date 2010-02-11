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

namespace SymbianStructuresLib.Debug.Code.Interfaces
{
    public interface ICodeCollectionRelocationHandler
    {
        void PrepareForRelocation( CodeCollection aCollection, uint aOldBase, uint aNewBase );
    }
}
