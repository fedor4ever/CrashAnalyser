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
using SymbianDebugLib.Engine;
using SymbianStackLib.Engine;

namespace SymbianStackLib.Exceptions
{
    public class StackAddressException : Exception
    {
        #region Enumerations
        public enum TType
        {
            ETypeBaseAddressBeforeTopAddress = 0,
            ETypeTopAddressAfterBaseAddress,
            ETypePointerIsNull,
            ETypePointerOutOfBounds
        }
        #endregion

        #region Constructors
        internal StackAddressException( TType aType )
        {
            iType = aType;
        }
        #endregion

        #region Properties
        public TType Type
        {
            get { return iType; }
        }
        #endregion

        #region Data members
        private readonly TType iType;
        #endregion
    }
}
