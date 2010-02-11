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
using SymBuildParsingLib.Token;
using SymBuildParsingLib.Utils;

namespace SymBuildParsingLib.Lexer
{
	public class SymLexerWorkerWord : SymLexerWorker
	{
		#region Constructors & destructor
		public SymLexerWorkerWord( SymLexer aLexer )
			: base( aLexer )
		{
		}
		#endregion

		#region API
		#endregion

		#region SymLexerWorker Members
		public override bool ProcessCharacter( char aCharacter )
		{
			bool consumed = true;
			//
			if	( iCurrentWord.Length == 0 )
			{
				// First letter of new word. Just take the character
				AddToWord( aCharacter, CharacterClassType( aCharacter ) );
			}
			else if	( char.IsWhiteSpace( aCharacter ) )
			{
				// Was the last character also white space?
				bool lastCharWasWhiteSpace = char.IsWhiteSpace( LastCharacter );
				if	( lastCharWasWhiteSpace )
				{
					// We don't want to make a new word for each white space
					// character, so begin to group them here.
				}
				else
				{
					// Last character wasn't whitespace, so we must have ended
					// a word
					MakeWord();
				}

				AddToWord( aCharacter, SymToken.TClass.EClassWhiteSpace );
			}
			else if ( iCurrentWord.Length > 0 )
			{
				// We're already processing a word.
				bool thisCharIsLetterOrDigit = char.IsLetterOrDigit( aCharacter );
				bool lastCharWasLetterOrDigit = char.IsLetterOrDigit( LastCharacter );
				//
				if	( thisCharIsLetterOrDigit && lastCharWasLetterOrDigit )
				{
					// In the middle of an ascii word, keep going...
					AddToWord( aCharacter, SymToken.TClass.EClassAlphaNumeric );
				}
				else
				{
					// This char is text, but the last wasn't - make a new word
					// from what we have and use this character as the basis
					// for the next word.
					MakeWord();
					AddToWord( aCharacter, CharacterClassType( aCharacter ) );
				}
			}
			//
			return consumed;
		}

		public override void StartedNewLine( SymTextPosition aEOLPosition )
		{
			MakeWord();
		}
		#endregion

		#region Internal methods
		private static SymToken.TClass CharacterClassType( char aCharacter )
		{
			SymToken.TClass ret = SymToken.TClass.EClassSymbol;
			//
			if	( char.IsWhiteSpace( aCharacter ) )
			{
				ret = SymToken.TClass.EClassWhiteSpace;
			}
			else if ( char.IsLetterOrDigit( aCharacter ) )
			{
				ret = SymToken.TClass.EClassAlphaNumeric;
			}
			//
			return ret;
		}

		private void AddToWord( char aCharacter, SymToken.TClass aClassType )
		{
			iCurrentClass = aClassType;
			iCurrentWord.Append( aCharacter );
		}

		private void MakeWord()
		{
			if	( iCurrentWord.Length > 0 )
			{
				// Finished a word
				SymToken token = new SymToken( iCurrentWord.ToString(), iCurrentClass, Lexer.CurrentPosition );
				Lexer.FlushToken( token );

				// Reset 
				iCurrentWord.Remove( 0, iCurrentWord.Length );
				iCurrentClass = SymToken.TClass.EClassWhiteSpace;
			}
		}
		#endregion

		#region Internal properties
		private char LastCharacter
		{
			get
			{
				char ret = '\0';
				int length = iCurrentWord.Length;
				//
				if	( length > 0 )
				{
					ret = iCurrentWord[ length - 1 ];
				}
				//
				return ret;
			}
		}
		#endregion

		#region Internal enumerations
		private enum TCharClass
		{
			ECharClassWhiteSpace = 0,
			ECharClassText,
			ECharClassDigit,
			ECharClassComma,
			ECharClassPeriod,
			ECharClassHash,
			ECharClassSlash,
			ECharClassColon,
			ECharClassSemiColon,
			ECharClassTilde,
			ECharClassMathematic
		};
		#endregion

		#region Data members
		private StringBuilder iCurrentWord = new StringBuilder( 200 );
		private SymToken.TClass iCurrentClass = SymToken.TClass.EClassWhiteSpace;
		#endregion
	}
}
