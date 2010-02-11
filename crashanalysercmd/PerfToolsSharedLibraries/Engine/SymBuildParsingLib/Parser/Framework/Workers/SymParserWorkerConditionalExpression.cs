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
using SymBuildParsingLib.Tree;
using SymBuildParsingLib.Parser.Framework.Workers;
using SymBuildParsingLib.Parser.Framework.Nodes;
using SymbianTree;

namespace SymBuildParsingLib.Parser.Framework.Workers
{
	// NB: Used only for 'if', 'ifdef', 'ifndef' expressions.
	public class SymParserWorkerConditionalExpression : SymParserWorker
	{
		#region Constructors & destructor
		public SymParserWorkerConditionalExpression( SymParserWorkerContext aContext )
			: base( aContext )
		{
			SymNode node = new SymNodeConditionalExpression( aContext.CurrentToken );
			aContext.Document.CurrentNode.Add( node );
			
			// Make this child node the new parent
			aContext.Document.CurrentNode = node;
		}
		#endregion

		#region From SymParserWorker
		public override void RemoveChild( SymParserWorker aWorker )
		{
			base.RemoveChild( aWorker );
			RemoveSelf();
		}
		#endregion

		#region Utility functions
		public static SymNodeConditionalExpression FindMostRecentConditionalExpression( SymNode aCurrentNode )
		{
			SymNodeConditionalExpression ret = null;
			//
			SymNodeEnumeratorUpTreeSiblingsFirst iterator = new SymNodeEnumeratorUpTreeSiblingsFirst( aCurrentNode );
			//
			foreach( SymNode node in iterator )
			{
				if	( node is SymNodeConditionalExpression )
				{
					ret = (SymNodeConditionalExpression) node;
					break;
				}
			}
			//
			return ret;
		}
		#endregion
	}
}
