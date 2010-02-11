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

namespace SymbianParserLib.BaseStructures
{
    internal class ParserResponse
    {
        #region Enumerations
        public enum TResponseType
        {
            EResponseTypeUnhandled = 0,
            EResponseTypeHandled,
            EResponseTypeHandledByRequiresReProcessing
        }
        #endregion

        #region Constructors
        public ParserResponse()
            : this( TResponseType.EResponseTypeUnhandled )
        {
        }

        public ParserResponse( TResponseType aType )
        {
            iType = aType;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public TResponseType Type
        {
            get { return iType; }
        }

        public bool WasHandled
        {
            get { return Type != TResponseType.EResponseTypeUnhandled; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        private readonly TResponseType iType;
        #endregion
    }
}
