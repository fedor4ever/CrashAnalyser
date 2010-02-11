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

namespace SymBuildParsingLib.Lexer
{
	public class SymLexedTokens
	{
		#region Observer interface
		public delegate void TokenHandler( SymToken aNewToken );
		#endregion

		#region Events
		public event TokenHandler iTokenHandlers;
		#endregion

		#region Constructors & destructor
		public SymLexedTokens()
		{
		}
		#endregion

		#region API
		public void Enqueue( SymToken aToken )
		{
			iTokens.Enqueue( aToken );
			NotifyNewToken( aToken );
		}

		public SymToken Dequeue()
		{
			SymToken ret = (SymToken) iTokens.Dequeue();
			return ret;
		}
		#endregion

		#region Properties
		public int Count
		{
			get { return iTokens.Count; }
		}
		#endregion

		#region Internal methods
		private void NotifyNewToken( SymToken aToken )
		{
			if	( iTokenHandlers != null )
			{
				iTokenHandlers( aToken );
			}
		}
		#endregion

		#region Data members
		private Queue iTokens = new Queue( 250 );
		#endregion
	}
}
