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
using SymBuildParsingLib.Tree;
using SymBuildParsingLib.Token;
using SymBuildParsingLib.Parser.Framework.Document;
using SymBuildParsingLib.Common.Objects;

namespace SymBuildParsingLib.Parser.Framework.Workers
{
	public class SymParserWorkerContext : SymParserDocumentContext
	{
		#region Constructors & destructor
		public SymParserWorkerContext( SymParserDocumentContext aDocumentContext )
			: base( aDocumentContext )
		{
		}

		public SymParserWorkerContext( SymParserDocumentContext aDocumentContext, SymParserWorker aParent )
			: this( aDocumentContext, aParent, SymToken.NullToken() )
		{
		}

		public SymParserWorkerContext( SymParserDocumentContext aDocumentContext, SymParserWorker aParent, SymToken aCurrentToken )
			: base( aDocumentContext )
		{
			iParent = aParent;
			iCurrentToken = aCurrentToken;
		}
		#endregion

		#region Properties
		public SymToken CurrentToken
		{
			get { return iCurrentToken; }
		}

		public SymParserWorker Parent
		{
			get { return iParent; }
		}
		#endregion

		#region Data members
		private readonly SymToken iCurrentToken;
		private readonly SymParserWorker iParent;
		#endregion
	}
}
