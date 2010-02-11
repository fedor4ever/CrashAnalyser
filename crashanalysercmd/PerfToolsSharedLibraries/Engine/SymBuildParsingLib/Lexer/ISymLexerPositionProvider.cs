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
	public interface ISymLexerPositionObserver
	{
		#region ISymLexerPositionProvider definition
		void HandleEndOfLineDetected( SymTextPosition aEOLPosition );
		#endregion
	}

	public interface ISymLexerPositionProvider
	{
		#region ISymLexerPositionProvider definition
		void SetObserver( ISymLexerPositionObserver aObserver );
		SymTextPosition CurrentPosition { get; }
		#endregion
	}
}
