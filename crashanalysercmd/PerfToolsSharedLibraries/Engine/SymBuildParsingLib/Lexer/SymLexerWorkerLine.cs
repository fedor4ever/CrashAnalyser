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
using System.Collections;
using SymBuildParsingLib.Lexer;
using SymBuildParsingLib.Utils;

namespace SymBuildParsingLib.Lexer
{
	public class SymLexerWorkerLine : SymLexerWorker, ISymLexerPositionProvider
	{
		#region Constructors & destructor
		public SymLexerWorkerLine( SymLexer aLexer )
			: base( aLexer )
		{
		}
		#endregion

		#region Properties
		public char LastCharacter
		{
			get { return iLastCharacter; }
			set { iLastCharacter = value; }
		}
		#endregion

		#region SymLexerWorker Members
		public override bool ProcessCharacter( char aCharacter )
		{
			bool consumed = false;

			// At the end of a line if we see 0x0A or then 0x0D and the next character is not 0x0A
			if	( aCharacter == KSymLineFeed || aCharacter == KSymCarriageReturn )
			{
				if	( aCharacter == KSymLineFeed )
				{
					// This is definitely the end of a line. If the last
					// character was a carriage return, then we mark
					// the start of the end-of-line as the previous
					// character. Otherwise, its this one.
					AddNewEndOfLinePosition();
				}
				else
				{
					// Its a CR (0x0D) but it might be followed by a LF
					// so don't do anything yet.
					LastCharacter = aCharacter;
				}

				consumed = true;
			}
			else if ( LastCharacter == KSymCarriageReturn )
			{
				// The current character is not a line feed, but the last
				// char was a carriage return => implicitly, the last
				// character was an end of line.
				AddNewEndOfLinePosition();
			}
			else
			{
				iCurrentPosition.Inc();
				LastCharacter = aCharacter;
			}

			return consumed;
		}

		public override void StartedNewLine( SymTextPosition aEOLPosition )
		{
		}
		#endregion

		#region ISymLexerPositionProvider Members
		public SymTextPosition CurrentPosition
		{
			get
			{
				return new SymTextPosition( iCurrentPosition );
			}
		}

		public void SetObserver( ISymLexerPositionObserver aObserver )
		{
		}
		#endregion

		#region Internal methods
		private void AddNewEndOfLinePosition()
		{
			iLineEndingPositions.Add( iCurrentPosition );
			Lexer.HandleEndOfLineDetected( iCurrentPosition );

			iCurrentPosition.NewLine();
			iLastCharacter = '\0';
		}
		#endregion

		#region Internal constants
		const int KSymLineFeed = 0xA;
		const int KSymCarriageReturn = 0xD;
		#endregion

		#region Data members
		private char iLastCharacter = '\0';
		private ArrayList iLineEndingPositions = new ArrayList( 1024 );
		private SymTextPosition iCurrentPosition = new SymTextPosition();
		#endregion
	}
}
