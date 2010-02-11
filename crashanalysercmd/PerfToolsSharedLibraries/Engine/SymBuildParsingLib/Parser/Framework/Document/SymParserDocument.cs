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
using System.Collections;
using SymbianTree;

namespace SymBuildParsingLib.Parser.Framework.Document
{
	public abstract class SymParserDocument : SymDocument
	{
		#region Constructors & destructor
		public SymParserDocument( SymParserDocumentContext aContext )
		{
			aContext.Document = this;
			iContextStack.Push( aContext );
		}
		#endregion

		#region API
		public void PushContext( SymParserDocumentContext aContext )
		{
			iContextStack.Push( aContext );
		}

		public SymParserDocumentContext PopContext()
		{
			SymParserDocumentContext top = (SymParserDocumentContext) iContextStack.Peek();
			return PopContext( top );
		}

		public SymParserDocumentContext PopContext( SymParserDocumentContext aExpected )
		{
			// Can't pop off the last context
			System.Diagnostics.Debug.Assert( iContextStack.Count > 1 );
			SymParserDocumentContext top = (SymParserDocumentContext) iContextStack.Peek();
			//
			if	( aExpected.Equals( aExpected ) == false )
			{
				throw new ArgumentException( "Cannot pop context - expectations not met during pop operation" );
			}
			//
			iContextStack.Pop();
			return top;
		}
		#endregion

		#region Properties
		public SymParserDocumentContext Context
		{
			get
			{
				System.Diagnostics.Debug.Assert( iContextStack.Count > 0 );
				SymParserDocumentContext ret = (SymParserDocumentContext) iContextStack.Peek();
				return ret;
			}
		}
		#endregion

		#region Data members
		private Stack iContextStack = new Stack();
		#endregion
	}
}
