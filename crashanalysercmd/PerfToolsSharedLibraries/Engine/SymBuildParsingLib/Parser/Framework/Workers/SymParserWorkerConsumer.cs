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
using SymbianTree;

namespace SymBuildParsingLib.Parser.Framework.Workers
{
	public class SymParserWorkerConsumer : SymParserWorker
	{
		#region Enumerations
		public enum TDyingAction
		{
			EWhenDyingTakeNoAction = 0,
			EWhenDyingMakeRelativeParentNodeCurrent,
			EWhenDyingMakeAbsoluteParentNodeCurrent
		}
		#endregion

		#region Constructors & destructor
		public SymParserWorkerConsumer( SymParserWorkerContext aContext, SymToken.TClass aTerminatingClassType )
			: this( aContext, aTerminatingClassType, TDyingAction.EWhenDyingTakeNoAction )
		{
		}

		public SymParserWorkerConsumer( SymParserWorkerContext aContext, SymToken.TClass aTerminatingClassType, TDyingAction aDyingAction )
			: base( aContext )
		{
			iTerminatingClassType = aTerminatingClassType;
			iDyingAction = aDyingAction;
		}
		#endregion

		#region Properties
		public SymTokenContainer ConsumedTokens
		{
			get { return iConsumedTokens; }
		}
		#endregion

		#region Abstract API
		protected virtual void HandleTerminatingConditionMatch( SymToken aToken )
		{
		}
		#endregion

		#region From SymParserWorker
		public override SymParserWorker.TTokenConsumptionType OfferToken( SymToken aToken )
		{
			iConsumedTokens.Append( aToken );

			if	( aToken.Class == iTerminatingClassType )
			{
				// Work out which node will become the parent (if we are configured in that way)
				SymNode newParent = CalculateNewParentNode();

				// Call back to parent class
				HandleTerminatingConditionMatch( aToken );

				// Reached the new line token. Stop receiving the tokens. We're done.
				WorkerContext.Parent.RemoveChild( this );

				// If the dying action was to make the relative parent node
				// the current one, then we must call CalculateNewParentNode again
				// after the HandleTerminatingConditionMatch callback - since 
				// it may have changed the tree.
				if	( iDyingAction == TDyingAction.EWhenDyingMakeRelativeParentNodeCurrent )
				{
					newParent = CalculateNewParentNode();
				}

				// Update the document with the new parent node
				WorkerContext.Document.CurrentNode = newParent;
			}
			//
			return TTokenConsumptionType.ETokenConsumed;
		}
		#endregion

		#region Internal methods
		private SymNode CalculateNewParentNode()
		{
			SymNode ret = WorkerContext.Document.CurrentNode;
			//
			if	( iDyingAction != TDyingAction.EWhenDyingTakeNoAction && WorkerContext.Document.CurrentNode.HasParent )
			{
				ret = WorkerContext.Document.CurrentNode.Parent;
			}
			//
			return ret;
		}
		#endregion

		#region Data members
		private readonly SymToken.TClass iTerminatingClassType;
		private readonly TDyingAction iDyingAction = TDyingAction.EWhenDyingTakeNoAction;
		private SymTokenContainer iConsumedTokens = new SymTokenContainer();
		#endregion
	}
}
