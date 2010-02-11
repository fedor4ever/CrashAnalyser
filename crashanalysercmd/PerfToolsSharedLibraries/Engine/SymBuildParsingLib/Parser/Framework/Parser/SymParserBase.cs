#define SHOW_TOKENS/*
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
using System.Collections;
using System.Threading;
using System.Reflection;
using SymBuildParsingLib.Tree;
using SymBuildParsingLib.Utils;
using SymBuildParsingLib.Grouper;
using SymBuildParsingLib.Lexer;
using SymBuildParsingLib.Token;
using SymBuildParsingLib.Parser.Framework.Workers;
using SymBuildParsingLib.Parser.Framework.Document;
using SymbianTree;

namespace SymBuildParsingLib.Parser.Framework.Parser
{
	public abstract class SymParserBase
	{
		#region Event enumerations
		public enum TEvent
		{
			EEventStaringLexing = 0,
			EEventStaringGrouping,
			EEventParsingComplete
		};
		#endregion

		#region Observer interface
		public delegate void ParserObserver( SymParserBase aParser, TEvent aEvent );
		#endregion

		#region Events
		public event ParserObserver ParserObservers;
		#endregion

		#region Constructors and destructor
		public SymParserBase( SymParserDocument aDocument )
		{
			// Store the document and initialise the document's 
			// parser reference
			iDocument = aDocument;
			iDocument.Context.Parser = this;

			// The lexer reads the tokens from the stream and performs minimal
			// (very basic) grouping.
			iLexer = new SymLexer( iDocument.Context.FileName );

			// We observe the lexer in order to know when all the input data
			// has been consumed from the source file.
			iLexer.LexerObservers += new SymLexer.LexerObserver( HandleLexerEvent );

			// The grouper performs more sophisticated grouping based upon
			// simple C++/C rules. It observes the lexer for tokens.
			iGrouper = new SymGrouper( iLexer );

			// We observe the grouper in order to obtain the grouped tokens
			// which we will parse to form the tree.
			iGrouper.GrouperObservers += new SymGrouper.GrouperObserver( HandleGrouperEvent );

			// Create the thread for the parser
			iWorkerThread = new Thread( new System.Threading.ThreadStart( BuildParserTree ) );
			PrepareWorkerThread();

			// Create any initial required workers
			PrepareInitialWorkers();
		}
		#endregion

		#region API
		public void Parse()
		{
			// Starting the lexer will implicitly start the grouper
			// as the grouper is already awaiting the lexer's output
			// tokens. Tokens will arrive into the iGroupedTokens data
			// member. This call is asynchronous.
			iLexer.Lex();
		}
		#endregion

		#region Clone API
		public SymParserBase CarbonCopy( object[] aArguments )
		{
			// NB. Should use object.Clone with custom IClonable charactersitics?
			System.Type type = this.GetType();
			object objectInstance = Activator.CreateInstance( type, aArguments );
			//
			if	( !(objectInstance is SymParserBase) )
			{
				throw new Exception( "Unexpected object type during Carbon Copy operation" );
			}
			//
			SymParserBase parser = (SymParserBase) objectInstance;
			return parser;
		}
		#endregion

		#region Abstract API
		protected virtual void PrepareInitialWorkers()
		{
		}

		protected virtual void HandleParserToken( SymToken aToken )
		{
			// Try to dish the token out to the workers. Give
			// the token to the highest priority worker initially.
			// Keep trying to offer it until one of the workers
			// consumes it.
			int count = iWorkers.Count;
			for( int i=0; i<count; i++ )
			{
				SymParserWorker worker = (SymParserWorker) iWorkers[ i ];
				SymParserWorker.TTokenConsumptionType consumptionType = worker.OfferToken( aToken );
				//
				if	( consumptionType == SymParserWorker.TTokenConsumptionType.ETokenConsumed )
				{
					break;
				}
			}
		}
		#endregion

		#region Abstract properties
		protected abstract string ParserName { get; }
		#endregion

		#region Properties
		public string FileName
		{
			get { return iLexer.FileName; }
		}

		public SymParserDocument Document
		{
			get { return iDocument; }
		}
		#endregion

		#region Worker related
		public int WorkerCount
		{
			get { return iWorkers.Count; }
		}

		public void QueueWorker( SymParserWorker aWorker )
		{
			// Insert into sorted order
			int count = iWorkers.Count;
			for( int i=0; i<count; i++ )
			{
				SymParserWorker worker = (SymParserWorker) iWorkers[ i ];
				int existingPriority = worker.Priority;
				//
				if	( aWorker.Priority >= existingPriority )
				{
					iWorkers.Insert( i, aWorker );
					return;
				}
			}

			// First entry - just append it...
			iWorkers.Add( aWorker );
		}
		#endregion

		#region Internal Properties
		protected SymTokenContainer GroupedTokens
		{
			get { return iGroupedTokens; }
		}
		#endregion

		#region Internal methods
		private void BuildParserTree()
		{
			// Pull a token from the source buffer
			do
			{
				int tokenCount = 0;
				//
				do
				{
					SymToken token = null;

					// Try to get the head token (if there is one)
					lock( iGroupedTokens )
					{
						tokenCount = iGroupedTokens.Count;
						if	( tokenCount > 0 )
						{
							token = iGroupedTokens.PopHead();
						}
					}

					// Then process it
					if	( token != null )
					{
#if SHOW_TOKENS
						System.Diagnostics.Debug.WriteLine( "BuildParserTree() - next token is: [" + token.Value + "]");
#endif
						HandleParserToken( token );
					}
				} 
				while ( tokenCount > 0 );

				// We've processed all the tokens we had so far... put us to sleep
				//System.Diagnostics.Debug.WriteLine( "Parser.BuildParserTree() - no more tokens - waiting on semaphore..." );
				iSemaphore.Wait();
				//System.Diagnostics.Debug.WriteLine( "Parser.BuildParserTree() - AWAKE!" );
			}
			while( true );

		}

		private void PrepareWorkerThread()
		{
			System.Random randomNumberGen = new Random( System.Environment.TickCount );
			int uniquePostfix = randomNumberGen.Next();
			//
			string workerThreadName = ParserName + uniquePostfix.ToString("d8");
			iWorkerThread.Name = workerThreadName;
			iWorkerThread.IsBackground = true;
			iWorkerThread.Start();
		}
		private void ReportEvent( TEvent aEvent )
		{
			if	( ParserObservers != null )
			{
				ParserObservers( this, aEvent );
			}
		}
		private void NewGroupedTokenArrived( SymToken aToken )
		{
			lock( this )
			{
				iGroupedTokens.Append( aToken );
				//
				if	( iSemaphore.Count == 0 )
				{
					iSemaphore.Signal();
				}
			}
		}
		#endregion

		#region Event handlers
		private void HandleLexerEvent( SymLexer aLexer, SymLexer.TEvent aEvent, SymToken aToken )
		{
			switch( aEvent )
			{
				case SymLexer.TEvent.EEventLexingStarted: 
					ReportEvent( TEvent.EEventStaringLexing );
					break;
				case SymLexer.TEvent.EEventLexingNewLine: 
					break;
				case SymLexer.TEvent.EEventLexingComplete: 
					break;
				default:
					break;
			}
		}

		private void HandleGrouperEvent( SymGrouper aGrouper, SymGrouper.TEvent aEvent, SymToken aGroupedToken )
		{
			switch( aEvent )
			{
				case SymGrouper.TEvent.EEventGroupingStarted: 
					ReportEvent( TEvent.EEventStaringGrouping );
					break;
				case SymGrouper.TEvent.EEventGroupingPaused:
					break;
				case SymGrouper.TEvent.EEventGroupingComplete: 
					ReportEvent( TEvent.EEventParsingComplete );
					break;
				case SymGrouper.TEvent.EEventGroupingTokenReady:
					NewGroupedTokenArrived( aGroupedToken );
					break;
				default:
					break;
			}
		}
		#endregion

		#region Data members
		private readonly Thread iWorkerThread;
		private readonly SymGrouper iGrouper;
		private readonly SymLexer iLexer;
		private readonly SymParserDocument iDocument;
		private SymTokenContainer iGroupedTokens = new SymTokenContainer();
		private ArrayList iWorkers = new ArrayList( 4 );
		private SymSemaphore iSemaphore = new SymSemaphore( 0, 1 );
		#endregion
	}
}
