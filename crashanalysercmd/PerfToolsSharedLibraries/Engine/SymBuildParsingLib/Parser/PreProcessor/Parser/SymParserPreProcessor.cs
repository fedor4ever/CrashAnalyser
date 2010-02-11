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
using SymBuildParsingLib.Tree;
using SymBuildParsingLib.Common.Objects;
using SymBuildParsingLib.Parser.PreProcessor.Workers;
using SymBuildParsingLib.Parser.PreProcessor.Document;
using SymBuildParsingLib.Parser.Framework.Nodes;
using SymBuildParsingLib.Parser.Framework.Document;
using SymBuildParsingLib.Parser.Framework.Workers;
using SymBuildParsingLib.Parser.Framework.Parser;


namespace SymBuildParsingLib.Parser.PreProcessor.Parser
{
	public abstract class SymPreProcessorParser : SymParserBase
	{
		#region Constructors & destructor
		public SymPreProcessorParser( SymParserDocument aDocument )
			: base( aDocument )
		{
		}
		#endregion

		#region Properties
		public SymPreProcessorWorker MainWorker
		{
			get { return iMainWorker; }
		}
		#endregion

		#region From SymParserBase
		protected override string ParserName
		{
			get { return this.ToString(); }
		}

		protected override void PrepareInitialWorkers()
		{
			SymParserWorkerContext context = new SymParserWorkerContext( Document.Context );
			iMainWorker = new SymPreProcessorWorker( context );
			//
			QueueWorker( iMainWorker );

			// Make a base call
			base.PrepareInitialWorkers();
		}
		#endregion

		#region Conditional expression related
		public bool ConditionalExpressionValue
		{
			get
			{
				bool ret = true;
				//
				SymNodeConditionalExpression top = ConditionalExpressionPeek();
				if	( top != null && top.HasCondition )
				{
					// Use the current inherited value as the first filter.
					// This means that nested #ifs are handled correctly, (e.g.
					// an outer #if that evaluates to false will prevent an inner
					// #if that evalutes to true from being emitted).
					ret = top.InheritedValue;

					if	( ret )
					{
						SymNodeCondition condition = top.CurrentCondition;
						ret = condition.EvaluationResult;
					}
				}
				//
				return ret;
			}
		}

		public void ConditionalExpressionPush( SymNodeConditionalExpression aConditionalExpression )
		{
			bool inheritedValue = true;
			//
			SymNodeConditionalExpression top = ConditionalExpressionPeek();
			if	( top != null )
			{
				inheritedValue = top.InheritedValue;
			}
			//
			aConditionalExpression.InheritedValue = inheritedValue;
			iCondExpResultStack.Push( aConditionalExpression );
		}

		public SymNodeConditionalExpression ConditionalExpressionPop()
		{
			System.Diagnostics.Debug.Assert( iCondExpResultStack.Count > 0 );
			//
			SymNodeConditionalExpression top = ConditionalExpressionPeek();
			iCondExpResultStack.Pop();
			return top;
		}

		public SymNodeConditionalExpression ConditionalExpressionPeek()
		{
			SymNodeConditionalExpression top = null;
			//
			if	( iCondExpResultStack.Count > 0 )
			{
				top = (SymNodeConditionalExpression) iCondExpResultStack.Peek();
			}
			//
			return top;
		}
		#endregion

		#region Data members
		private SymPreProcessorWorker iMainWorker;
		private Stack iCondExpResultStack = new Stack();
		#endregion
	}
}
