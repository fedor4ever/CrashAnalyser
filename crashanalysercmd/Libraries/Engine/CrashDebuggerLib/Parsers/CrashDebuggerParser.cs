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
using SymbianUtils;
using SymbianParserLib.Engine;
using SymbianParserLib.Elements;
using CrashDebuggerLib.Parsers.State;
using CrashDebuggerLib.Structures;

namespace CrashDebuggerLib.Parsers
{
    public class CrashDebuggerParser : AsyncTextFileReader
    {
        #region Constructors
        public CrashDebuggerParser( CrashDebuggerInfo aCrashDebugger, string aFileName )
            : base( aFileName )
        {
            iCrashDebugger = aCrashDebugger;
            //
            iCommandParser.ParagraphComplete += new SymbianParserLib.BaseStructures.ParserElementBase.ElementCompleteHandler( CommandParser_ParagraphComplete );
            PrepareCommandParser();

            // We need to preserve white space
            TrimLine = false;
        }
        #endregion

        #region API
        public void ParseCrashData()
        {
            base.AsyncRead();
        }
        #endregion

        #region Properties
        public CrashDebuggerInfo CrashDebugger
        {
            get { return iCrashDebugger; }
        }
        #endregion

        #region Internal methods
        private void PrepareCommandParser()
        {
            StateFactory.RegisterCommands( iCommandParser );
        }
        #endregion

        #region Internal constants
        #endregion

        #region Event handlers
        void CommandParser_ParagraphComplete( SymbianParserLib.BaseStructures.ParserElementBase aElement )
        {
            TState state = (TState) aElement.Tag;
            State.State nextState = StateFactory.Create( state, this );
            //
            if ( iStateObject != null )
            {
                iStateObject.Finalise();
                iStateObject = null;
            } 
            //
            if ( nextState == null )
            {
                throw new ArgumentException( "Invalid state: " + state );
            }
            //
            iStateObject = nextState;
            iCurrentState = state;
            //
#if DEBUG
            System.Diagnostics.Debug.WriteLine( "SWITCHING STATE: => " + iStateObject.GetType().ToString() );
#endif
        }
        #endregion

        #region From AsyncTextFileReader
        protected override void HandleReadStarted()
        {
            iCrashDebugger.Clear();
            base.HandleReadStarted();
        }

        protected override void HandleReadCompleted()
        {
            try
            {
                if ( iStateObject != null )
                {
                    iStateObject.Finalise();
                    iStateObject = null;
                }

                // Start the async op queue
                iCrashDebugger.AsyncOperationManager.Start();
            }
            finally
            {
                base.HandleReadCompleted();
            }

            System.Diagnostics.Debug.WriteLine( "TOTAL REGEX TICKS - " + SymbianParserLib.RegExTranslators.TheTickCounter.TickCount.ToString( "d12" ) );
        }

        protected override void HandleFilteredLine( string aLine )
        {
            string line = aLine;
            bool consumed = iCommandParser.OfferLine( ref line );
            if ( !consumed && iStateObject != null )
            {
                ParserEngine stateParser = iStateObject.ParserEngine;
                stateParser.OfferLine( ref line );
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Internal LUT
        #endregion

        #region Data members
        private readonly CrashDebuggerInfo iCrashDebugger;
        private ParserEngine iCommandParser = new ParserEngine();
        private State.State iStateObject = null;
        private TState iCurrentState = TState.EStateIdle;
        #endregion
    }
}
