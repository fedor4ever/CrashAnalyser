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
using SymbianTree;
using SymBuildParsingLib.Utils;
using SymBuildParsingLib.Tree;
using SymBuildParsingLib.Common.Objects;
using SymBuildParsingLib.Parser.Framework.Parser;

namespace SymBuildParsingLib.Parser.Framework.Parser
{
	public class SymParserWaiter
	{
		#region Constructors & destructor
		public SymParserWaiter( SymParserBase aParser )
		{
			iParser = aParser;
			//
			iParser.ParserObservers += new SymBuildParsingLib.Parser.Framework.Parser.SymParserBase.ParserObserver( HandleParserEvent );
		}
		#endregion

		#region API
		public void Wait()
		{
			iSemaphore.Wait();
		}
		#endregion

		#region Parser event handler
		private void HandleParserEvent(SymParserBase aParser, SymBuildParsingLib.Parser.Framework.Parser.SymParserBase.TEvent aEvent)
		{
			if	( aEvent == SymParserBase.TEvent.EEventParsingComplete )
			{
				iSemaphore.Signal();
			}
		}
		#endregion

		#region Data members
		private readonly SymParserBase iParser;
		private SymSemaphore iSemaphore = new SymSemaphore( 0, 1 );
		#endregion
	}
}
