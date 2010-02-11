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
using SymBuildParsingLib.Token;
using SymBuildParsingLib.Parser.Framework.Nodes;
using SymBuildParsingLib.Utils;
using SymbianTree;

namespace SymBuildParsingLib.Parser.Framework.Workers
{
	public class SymParserWorkerCondition : SymParserWorkerConsumer
	{
		#region Constructors & destructor
		public SymParserWorkerCondition( SymParserWorkerContext aContext )
			: this( aContext, new SymNodeCondition( aContext.CurrentToken.Value ) )
		{
		}

		public SymParserWorkerCondition( SymParserWorkerContext aContext, SymNodeCondition aNodeCondition )
			: base( aContext, SymToken.TClass.EClassNewLine )
		{
			System.Diagnostics.Debug.Assert( aContext.Document.CurrentNode is SymNodeConditionalExpression );

			// Make a new child node for the conditions. 
			iConditionNode = aNodeCondition;
			aContext.Document.CurrentNode.Add( iConditionNode );
			aContext.Document.CurrentNode = iConditionNode;

			// Set up the token balancer event handler
			iTokenBalancer.EventLevelFinished += new SymBuildParsingLib.Token.SymTokenBalancer.LevelChangeEventHandler( TokenBalancer_EventLevelFinished );
			iTokenBalancer.EventLevelsImbalanced += new SymBuildParsingLib.Token.SymTokenBalancer.LevelsImbalancedEventHandler( TokenBalancer_EventLevelsImbalanced );

			// If we're handling an ifdef or ifndef expression, we can tell
			// the token balancer to discard all brackets, irrespective of their level.
			// For normal if statements, we want the brackets.
			bool emitLevelZeroBrackets = !( iConditionNode.Type == SymNodeCondition.TType.ETypeIfdef || iConditionNode.Type == SymNodeCondition.TType.ETypeIfndef );
			iTokenBalancer.RegisterBalancerTokens( emitLevelZeroBrackets );
		}
		#endregion

		#region From SymParserWorker
		public override SymParserWorker.TTokenConsumptionType OfferToken( SymToken aToken )
		{
			System.Diagnostics.Debug.Assert( WorkerContext.Document.CurrentNode is SymNodeCondition );
			//
			TTokenConsumptionType ret = TTokenConsumptionType.ETokenNotConsumed;
			if	( aToken.Class != SymToken.TClass.EClassNewLine )
			{
				if	( iTokenBalancer.OfferToken( aToken ) )
				{
					ret = TTokenConsumptionType.ETokenConsumed;
				}
			}

			// Try offering to base class....
			// Base class will dequeue us once we reach the new line
			if	( ret != TTokenConsumptionType.ETokenConsumed )
			{
				ret = base.OfferToken( aToken );
			}

			return ret;
		}
		#endregion

		#region From SymParserWorkerConsumer
		protected override void HandleTerminatingConditionMatch( SymToken aToken )
		{
			// Give the condition node ownership of the raw parsed tokens
			iConditionNode.AssignArgumentTokens( iTokenBalancer.DocumentTree );

			// We only need to evaluate the conditional arguments if we have not yet
			// found a positive branch.
			SymNodeConditionalExpression condExpNode = (SymNodeConditionalExpression) iConditionNode.Parent;
			if	( condExpNode.HasPositiveBranch == false )
			{
				iConditionNode.Evaluate( WorkerContext );
			}

			// We remain as the current node, even after we die. This allows
			// the body of the condition to be added as our child.
		}
		#endregion

		#region Properties
		public SymNodeCondition ConditionNode
		{
			get { return iConditionNode; }
		}
		#endregion

		#region Token balancer event handlers
		private void TokenBalancer_EventLevelFinished( int aLevelCount, SymNode aOldLevel, SymNode aNewLevel )
		{
		}

		private void TokenBalancer_EventLevelsImbalanced()
		{
			throw new Exception( "Levels not balanced during condition parsing" );
		}
		#endregion

		#region Data members
		private SymTokenBalancer iTokenBalancer = new SymTokenBalancer();
		private readonly SymNodeCondition iConditionNode;
		#endregion
	}
}
