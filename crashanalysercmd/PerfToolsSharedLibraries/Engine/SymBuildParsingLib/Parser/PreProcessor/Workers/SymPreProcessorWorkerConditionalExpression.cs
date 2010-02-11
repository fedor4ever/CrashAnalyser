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
using SymBuildParsingLib.Parser.PreProcessor.Parser;

namespace SymBuildParsingLib.Parser.PreProcessor.Workers
{
	public class SymPreProcessorWorkerConditionalExpression : SymParserWorkerConditionalExpression
	{
		#region Constructors & destructor
		public SymPreProcessorWorkerConditionalExpression( SymParserWorkerContext aContext )
			: base( aContext )
		{
			System.Diagnostics.Debug.Assert( aContext.Document.CurrentNode is SymNodeConditionalExpression );

			// Inform the parser about the conditional expression node. It will use this
			// as a means of identifying whether to skip tokens until a positive branch is identified.
			SymPreProcessorParser parser = (SymPreProcessorParser) aContext.Parser;
			parser.ConditionalExpressionPush( aContext.Document.CurrentNode as SymNodeConditionalExpression );
		}
		#endregion
	}
}
