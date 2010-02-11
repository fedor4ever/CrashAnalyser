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
    internal class StateTheCurrentProcess : State
    {
        #region Constructors
        public StateTheCurrentProcess( CrashDebuggerParser aParser )
            : base( aParser )
        {
        }
        #endregion

        #region API
        public override void Prepare()
        {
            // TODO:
            // TheCurrentProcess=c801c078
            // TheCurrentAddressSpace=c801c078

            iHelper.CreateMonitorProcess( ParserEngine, "TheCurrentProcess", CrashDebugger.TheCurrentProcess );
        }

        public override void Finalise()
        {
            DObjectCon container = CrashDebugger[ DObject.TObjectType.EProcess ];
            if ( container.Contains( CrashDebugger.TheCurrentProcess ) == false )
            {
                container.Add( CrashDebugger.TheCurrentProcess );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private HelperDProcess iHelper = new HelperDProcess();
        #endregion
    }
}
