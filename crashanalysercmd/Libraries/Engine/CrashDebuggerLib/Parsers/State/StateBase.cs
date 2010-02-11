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
using CrashDebuggerLib.Structures;

namespace CrashDebuggerLib.Parsers.State
{
    internal abstract class State
    {
        #region Constructors
        protected State( CrashDebuggerParser aParser )
        {
            iParser = aParser;
        }
        #endregion

        #region API
        public abstract void Prepare();

        public virtual void Finalise()
        {
        }
        #endregion

        #region Properties
        public ParserEngine ParserEngine
        {
            get { return iParserEngine; }
            set { iParserEngine = value; }
        }

        public CrashDebuggerParser Parser
        {
            get { return iParser; }
        }

        public CrashDebuggerInfo CrashDebugger
        {
            get { return Parser.CrashDebugger; }
        }
        #endregion

        #region Data members
        private ParserEngine iParserEngine = new ParserEngine();
        private readonly CrashDebuggerParser iParser;
        #endregion
    }
}
