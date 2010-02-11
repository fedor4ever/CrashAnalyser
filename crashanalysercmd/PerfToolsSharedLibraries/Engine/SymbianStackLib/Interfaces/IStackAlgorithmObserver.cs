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
using System.Collections.Generic;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianStackLib.Data.Output.Entry;
using SymbianStackLib.Algorithms;

namespace SymbianStackLib.Interfaces
{
    public interface IStackAlgorithmObserver
    {
        void StackBuildingStarted( StackAlgorithm aAlg );
        void StackBuldingProgress( StackAlgorithm aAlg, int aPercent );
        void StackBuildingComplete( StackAlgorithm aAlg );
        void StackBuildingException( StackAlgorithm aAlg, Exception aException );
        void StackBuildingElementConstructed( StackAlgorithm aAlg, StackOutputEntry aEntry );
    }
}
