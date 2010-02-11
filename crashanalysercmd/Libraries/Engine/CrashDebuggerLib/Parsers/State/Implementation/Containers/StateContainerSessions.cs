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
using SymbianParserLib.Engine;
using CrashDebuggerLib.Structures.Session;
using CrashDebuggerLib.Structures.KernelObjects;
using SymbianParserLib.Elements;
using SymbianParserLib.Enums;

namespace CrashDebuggerLib.Parsers.State.Implementation
{
    internal class StateContainerSessions : StateContainerBase
    {
        #region Constructors
        public StateContainerSessions( CrashDebuggerParser aParser )
            : base( aParser )
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        #endregion

        #region From StateContainerBase
        protected override DObject CreateNewObject()
        {
            return new DSession( CrashDebugger );
        }
        #endregion

        #region Internal methods
        #endregion

        #region Event handlers
        #endregion

        #region Data members
        #endregion
    }
}
