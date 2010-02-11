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
using CrashDebuggerLib.Structures.Process;
using CrashDebuggerLib.Structures.KernelObjects;
using SymbianParserLib.Elements;
using CrashDebuggerLib.Parsers.State.Implementation.Helpers;

namespace CrashDebuggerLib.Parsers.State.Implementation
{
    internal class StateTheCurrentThread : State
    {
        #region Constructors
        public StateTheCurrentThread( CrashDebuggerParser aParser )
            : base( aParser )
        {
        }
        #endregion

        #region API
        public override void Prepare()
        {
            // TODO:
            // TheCurrentThread=c8041a00

            iHelper.CreateMonitorThread( ParserEngine, "TheCurrentThread", CrashDebugger.TheCurrentThread );
        }

        public override void Finalise()
        {
            DObjectCon container = CrashDebugger[ DObject.TObjectType.EThread ];
            if ( container.Contains( CrashDebugger.TheCurrentThread ) == false )
            {
                container.Add( CrashDebugger.TheCurrentThread );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private HelperDThread iHelper = new HelperDThread();
        #endregion
    }
}
