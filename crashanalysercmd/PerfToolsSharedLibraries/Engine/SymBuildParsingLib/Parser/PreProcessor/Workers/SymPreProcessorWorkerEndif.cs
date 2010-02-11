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
using SymBuildParsingLib.Parser.Framework;
using SymBuildParsingLib.Parser.Framework.Nodes;
using SymBuildParsingLib.Parser.Framework.Workers;
using SymBuildParsingLib.Parser.PreProcessor.Parser;

namespace SymBuildParsingLib.Parser.PreProcessor.Workers
{
	public class SymPreProcessorWorkerEndif : SymParserWorker
	{
		#region Constructors & destructor
		public SymPreProcessorWorkerEndif( SymParserWorkerContext aContext )
			: base( aContext )
		{
			// When the endif token is reached, we must work back up the tree
			// looking for the previous (i.e. most recent) conditional expression
			// node.
			SymNodeConditionalExpression condExpNode = SymParserWorkerConditionalExpression.FindMostRecentConditionalExpression( aContext.Document.CurrentNode );
			if	( condExpNode == null )
			{
				throw new Exception( "Unable to locate most recent condition expression during ENDIF handling" );
			}

			// There must always be a positive condition node and some kind of condition
			if	( condExpNode.ChildTypeExists( typeof(SymNodeCondition) ) == false )
			{
				throw new Exception( "No child condition node found during ENDIF handling" );
			}

			// We change the current node to be the parent of the conditional expression. 
			// Any new tokens will appear as siblings
			aContext.Document.CurrentNode = condExpNode.Parent;

			// Make sure we tell the parser that it can pop off this conditional expression node.
			SymPreProcessorParser parser = (SymPreProcessorParser) aContext.Parser;
			SymNodeConditionalExpression poppedExpression = parser.ConditionalExpressionPop();
			System.Diagnostics.Debug.Assert( poppedExpression == condExpNode );

			// Job done - dequeue
			RemoveSelf();
		}
		#endregion
	}
}
