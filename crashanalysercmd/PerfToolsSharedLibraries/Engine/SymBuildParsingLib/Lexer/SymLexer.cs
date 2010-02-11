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
using SymBuildParsingLib.Token;
using SymBuildParsingLib.Lexer;
using SymBuildParsingLib.Utils;

namespace SymBuildParsingLib.Lexer
{
	public class SymLexer : ISymLexerPositionObserver
	{
		#region Enumerations
		public enum TEvent
		{
			EEventLexingStarted = 0,
			EEventLexingNewLine,
			EEventLexingToken,
			EEventLexingComplete
		};
		#endregion

		#region Observer interface
		public delegate void LexerObserver( SymLexer aLexer, TEvent aEvent, SymToken aToken );
		#endregion

		#region Events
		public event LexerObserver LexerObservers;
		#endregion

		#region Constructors & destructor
		public SymLexer( string aFileName )
		{
			iStream = new SymStream( aFileName );
			//
			PrepareDefaultWorkers();
		}
		#endregion

		#region API
		public void Lex()
		{
			StartWorkerThread();
		}

		internal void RegisterWorker( SymLexerWorker aWorker )
		{
			iWorkers.Add( aWorker );
		}

		internal void FlushToken( SymToken aToken )
		{
			ReportEvent( TEvent.EEventLexingToken, aToken );
		}
		#endregion

		#region Properties
		public string FileName
		{
			get { return iStream.FileName; }
		}

		internal SymStream Stream
		{
			get { return iStream; }
		}

		internal SymTextPosition CurrentPosition
		{
			get { return iPositionProvider.CurrentPosition; }
		}
		#endregion

		#region Internal threading related
		private void DoLexing()
		{
			ReportEvent( TEvent.EEventLexingStarted, SymToken.NullToken() );

			while( !Stream.EOF )
			{
				char character = Stream.ReadChar();
				ProcessCharacter( character );
			}

			ReportEvent( TEvent.EEventLexingComplete, SymToken.NullToken() );
		}

		private void StartWorkerThread()
		{
			lock( this )
			{
				if	( iWorkerThread == null )
				{
					ThreadStart threadStart = new ThreadStart( DoLexing );
					iWorkerThread = new Thread( threadStart );
					iWorkerThread.Name = "SymLexerWorkerThread";
					iWorkerThread.IsBackground = true;
					iWorkerThread.Priority = ThreadPriority.BelowNormal;
					iWorkerThread.Start();
				}
			}
		}
		#endregion

		#region Internal methods
		private void PrepareDefaultWorkers()
		{
			SymLexerWorkerLine workerLine = new SymLexerWorkerLine( this );
			RegisterWorker( workerLine);
			iPositionProvider = workerLine;
			//
			SymLexerWorkerWord workerWord = new SymLexerWorkerWord( this );
			RegisterWorker( workerWord );
		}

		private void ProcessCharacter( char aCharacter )
		{
			int count = iWorkers.Count;
			//
			for( int i=0; i<count; i++ )
			{
				SymLexerWorker worker = (SymLexerWorker) iWorkers[ i ];
				bool consumed = worker.ProcessCharacter( aCharacter );
				//
				if	( consumed )
				{
					break;
				}
			}
		}

		private void ReportEvent( TEvent aEvent, SymToken aToken )
		{
			if	( LexerObservers != null )
			{
				LexerObservers( this, aEvent, aToken );
			}
		}
		#endregion

		#region ISymLexerPositionObserver Members
		public void HandleEndOfLineDetected( SymTextPosition aEOLPosition )
		{
			// Report to children
			int count = iWorkers.Count;
			for( int i=0; i<count; i++ )
			{
				SymLexerWorker worker = (SymLexerWorker) iWorkers[ i ];
				worker.StartedNewLine( aEOLPosition );
			}

			// Add a token for the new line - do this after reporting the
			// event to the children so that they can flush any data
			// they have before we append the new line.
			SymToken token = new SymToken( "", SymToken.TClass.EClassNewLine, aEOLPosition );
			FlushToken( token );

			// Report new line event
			ReportEvent( TEvent.EEventLexingNewLine, token );
		}
		#endregion

		#region Data members
		private SymStream iStream;
		private Thread iWorkerThread;
		private ISymLexerPositionProvider iPositionProvider;
		private ArrayList iWorkers = new ArrayList( 4 );
		#endregion
	}
}
