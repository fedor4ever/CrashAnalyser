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
using System.Text;
using System.Threading;
using System.Collections;
using SymBuildParsingLib.Utils;
using SymBuildParsingLib.Lexer;
using SymBuildParsingLib.Token;

namespace SymBuildParsingLib.Grouper
{
	public class SymGrouper
	{
		#region Enumerations
		public enum TEvent
		{
			EEventGroupingStarted = 0,
			EEventGroupingPaused,
			EEventGroupingTokenReady,
			EEventGroupingComplete
		};
		#endregion

		#region Observer interface
		public delegate void GrouperObserver( SymGrouper aGrouper, TEvent aEvent, SymToken aToken );
		#endregion

		#region Events
		public event GrouperObserver GrouperObservers;
		#endregion

		#region Constructors & destructor
		public SymGrouper( SymLexer aLexer )
		{
			// The mastermind implements all the logic for grouping token runs into
			// more sophisticated/logical token groupings.
			iMastermind.MastermindObservers += new SymGrouperMastermind.MastermindObserver( MastermindObserver );

			// Observe the lexer for tokens
			aLexer.LexerObservers += new SymLexer.LexerObserver( LexerTokenHandler );

			// Prepare worker thread
			ThreadStart threadStart = new ThreadStart( DoGrouping );
			iWorkerThread = new Thread( threadStart );
			iWorkerThread.Name = "SymGrouperWorkerThread";
			iWorkerThread.IsBackground = true;
			iWorkerThread.Start();
		}
		#endregion

		#region Internal methods
		private void ReportEvent( TEvent aEvent, SymToken aToken )
		{
			if	( GrouperObservers != null )
			{
				GrouperObservers( this, aEvent, aToken );
			}
		}
		#endregion

		#region Internal threading related
		private void DoGrouping()
		{
			ReportEvent( TEvent.EEventGroupingStarted, SymToken.NullToken() );

			bool lexerFinished = false;
			do
			{
				// Count how many tokens we have...
				lock( this )
				{
					iMastermind.PerformGrouping();
					lexerFinished = iLexerFinished;
				}

				// Wait until there are more items to process
				if	( lexerFinished == false )
				{
					ReportEvent( TEvent.EEventGroupingPaused, SymToken.NullToken() );
					iSemaphore.Wait();
					ReportEvent( TEvent.EEventGroupingStarted, SymToken.NullToken() );
				}
			} 
			while ( lexerFinished == false );

			ReportEvent( TEvent.EEventGroupingComplete, SymToken.NullToken() );
		}
		#endregion

		#region Event handlers
		private void LexerTokenHandler( SymLexer aLexer, SymLexer.TEvent aEvent, SymToken aToken )
		{
			if	( aEvent == SymLexer.TEvent.EEventLexingToken )
			{
				// Store the token
				lock( this )
				{
					iMastermind.EnqueueLexedToken( aToken );
				}

				// and signal the worker thread if it is waiting...
				if	( iSemaphore.Count == 0 )
				{
					iSemaphore.Signal();
				}
			}
			else if ( aEvent == SymLexer.TEvent.EEventLexingComplete )
			{
				lock( this )
				{
					iLexerFinished = true;
				}

				// and signal the worker thread if it is waiting...
				if	( iSemaphore.Count == 0 )
				{
					iSemaphore.Signal();
				}
			}
		}

		private void MastermindObserver( SymGrouperMastermind.TEvent aEvent, SymToken aGroupedToken )
		{
			// We've received a token from the grouper. Pass it on to our observer to handle.
			if	( aEvent == SymGrouperMastermind.TEvent.EEventGroupTokenReady )
			{
				ReportEvent( TEvent.EEventGroupingTokenReady, aGroupedToken );
			}
		}
		#endregion

		#region Data members
		private Thread iWorkerThread;
		private SymGrouperMastermind iMastermind = new SymGrouperMastermind();
		private SymSemaphore iSemaphore = new SymSemaphore( 0, 1 );
		private bool iLexerFinished = false;
		#endregion
	}
}
