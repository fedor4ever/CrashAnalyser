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
using SymBuildParsingLib.Parser.Framework.Workers;
using SymBuildParsingLib.Parser.PreProcessor.Nodes;

namespace SymBuildParsingLib.Parser.PreProcessor.Workers
{
	public class SymPreProcessorWorkerIf : SymPreProcessorWorkerConditionalExpression
	{
		#region Constructors & destructor
		public SymPreProcessorWorkerIf( SymParserWorkerContext aContext )
			: base( aContext )
		{
			// The base class will make a new conditional expression node at the current document position.
			// It will change the parent node to be this new node. We must make a new condition worker
			// that will handle reading the condition arguments. 
			SymParserWorkerContext context = new SymParserWorkerContext( WorkerContext.Document.Context, this );
			SymParserWorkerCondition conditionWorker = new SymParserWorkerCondition( context, new SymNodePreProcessorCondition( aContext.CurrentToken.Value ) );
			AddChild( conditionWorker );
		}
		#endregion
	}

	public class SymPreProcessorWorkerIfdef : SymPreProcessorWorkerConditionalExpression
	{
		#region Constructors & destructor
		public SymPreProcessorWorkerIfdef( SymParserWorkerContext aContext )
			: base( aContext )
		{
			// The base class will make a new conditional expression node at the current document position.
			// It will change the parent node to be this new node. We must make a new condition worker
			// that will handle reading the condition arguments. 
			SymParserWorkerContext context = new SymParserWorkerContext( WorkerContext.Document.Context, this );
			SymParserWorkerCondition conditionWorker = new SymParserWorkerCondition( context, new SymNodePreProcessorCondition( aContext.CurrentToken.Value ) );
			AddChild( conditionWorker );
		}
		#endregion
	}


	public class SymPreProcessorWorkerIfndef : SymPreProcessorWorkerConditionalExpression
	{
		#region Constructors & destructor
		public SymPreProcessorWorkerIfndef( SymParserWorkerContext aContext )
			: base( aContext )
		{
			// The base class will make a new conditional expression node at the current document position.
			// It will change the parent node to be this new node. We must make a new condition worker
			// that will handle reading the condition arguments. 
			SymParserWorkerContext context = new SymParserWorkerContext( WorkerContext.Document.Context, this );
			SymParserWorkerCondition conditionWorker = new SymParserWorkerCondition( context, new SymNodePreProcessorCondition( aContext.CurrentToken.Value ) );
			AddChild( conditionWorker );
		}
		#endregion
	}
}
