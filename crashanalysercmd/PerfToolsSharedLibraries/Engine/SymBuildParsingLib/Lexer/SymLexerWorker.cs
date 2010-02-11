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
using SymBuildParsingLib.Utils;

namespace SymBuildParsingLib.Lexer
{
	public abstract class SymLexerWorker
	{
		#region Constructors & destructor
		public SymLexerWorker( SymLexer aLexer )
		{
			iLexer = aLexer;
		}
		#endregion

		#region SymLexerWorker abstract interface
		public abstract bool ProcessCharacter( char aCharacter );
		public abstract void StartedNewLine( SymTextPosition aEOLPosition );
		#endregion

		#region Properties
		internal SymLexer Lexer
		{
			get { return iLexer; }
		}
		#endregion

		#region Data members
		private readonly SymLexer iLexer;
		#endregion
	}
}
