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
using SymBuildParsingLib.Parser.PreProcessor.Nodes;

namespace SymBuildParsingLib.Parser.PreProcessor.Workers
{
	public class SymPreProcessorWorkerElse : SymParserWorker
	{
		#region Constructors & destructor
		public SymPreProcessorWorkerElse( SymParserWorkerContext aContext )
			: base( aContext )
		{
			// When the else token is reached, we must work back up the tree
			// looking for the previous (i.e. most recent) conditional expression
			// node.
			SymNodeConditionalExpression condExpNode = SymParserWorkerConditionalExpression.FindMostRecentConditionalExpression( aContext.Document.CurrentNode );
			if	( condExpNode == null )
			{
				throw new Exception( "Unable to locate most recent condition expression during ELSE handling" );
			}

			// There must always be a positive condition node and some kind of condition
			if	( condExpNode.ChildTypeExists( typeof(SymNodeCondition) ) == false )
			{
				throw new Exception( "No child condition node found during ELSE handling" );
			}

			// Make the conditional expression the current node
			aContext.Document.CurrentNode = condExpNode;

			// Make a new condition worker. The condition worker creates a new condition node
			// which becomes the new current node.
			SymParserWorkerContext context = new SymParserWorkerContext( aContext.Document.Context, this );
			SymParserWorkerCondition conditionWorker = new SymParserWorkerCondition( context, new SymNodePreProcessorCondition( aContext.CurrentToken.Value ) );
			AddChild( conditionWorker );

			// If we didn't find any appropriately handled condition object, then
			// we make this the catch-all condition (with no expression to evaluate)
			if	( condExpNode.HasPositiveBranch == false )
			{
			}
		}
		#endregion

		#region From SymParserWorker
		public override void RemoveChild( SymParserWorker aWorker )
		{
			base.RemoveChild( aWorker );
			RemoveSelf();
		}
		#endregion
	}

	public class SymPreProcessorWorkerElseIf : SymParserWorker
	{
		#region Constructors & destructor
		public SymPreProcessorWorkerElseIf( SymParserWorkerContext aContext )
			: base( aContext )
		{
			// When the else token is reached, we must work back up the tree
			// looking for the previous (i.e. most recent) conditional expression
			// node.
			SymNodeConditionalExpression condExpNode = SymParserWorkerConditionalExpression.FindMostRecentConditionalExpression( aContext.Document.CurrentNode );
			if	( condExpNode == null )
			{
				throw new Exception( "Unable to locate most recent condition expression during ELSE IF handling" );
			}

			// There must always be a positive condition node and some kind of condition
			if	( condExpNode.ChildTypeExists( typeof(SymNodeCondition) ) == false )
			{
				throw new Exception( "No child condition node found during ELSE IF handling" );
			}

			// Make the conditional expression the current node
			aContext.Document.CurrentNode = condExpNode;

			// Make a new condition worker. The condition worker creates a new condition node
			// which becomes the new current node.
			SymParserWorkerContext context = new SymParserWorkerContext( aContext.Document.Context, this );
			SymParserWorkerCondition conditionWorker = new SymParserWorkerCondition( context, new SymNodePreProcessorCondition( aContext.CurrentToken.Value ) );
			AddChild( conditionWorker );
		}
		#endregion

		#region From SymParserWorker
		public override void RemoveChild( SymParserWorker aWorker )
		{
			base.RemoveChild( aWorker );
			RemoveSelf();
		}
		#endregion
	}
}
