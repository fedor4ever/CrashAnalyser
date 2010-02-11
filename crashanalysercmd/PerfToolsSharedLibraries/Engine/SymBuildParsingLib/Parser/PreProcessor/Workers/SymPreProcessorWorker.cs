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
using SymBuildParsingLib.Tree;
using SymBuildParsingLib.Token;
using SymBuildParsingLib.Parser.Framework;
using SymBuildParsingLib.Parser.Framework.Workers;
using SymBuildParsingLib.Parser.Framework.Nodes;
using SymBuildParsingLib.Parser.PreProcessor.Parser;

namespace SymBuildParsingLib.Parser.PreProcessor.Workers
{
	public class SymPreProcessorWorker : SymParserWorker
	{
		#region Constructors & destructor
		public SymPreProcessorWorker( SymParserWorkerContext aContext )
			: base( aContext, 1000 )
		{
		}
		#endregion

		#region From SymParserWorker
		public override SymParserWorker.TTokenConsumptionType OfferToken( SymToken aToken )
		{
			TTokenConsumptionType ret = TTokenConsumptionType.ETokenNotConsumed;
			
			#region Identify preprocessor tokens
			if ( aToken.Class == SymToken.TClass.EClassPreProcessor ) 
			{
				if	( ChildCount == 0 )
				{
					if	( aToken.Value == "#" )
					{
						// Always consume initial preprocessor tokens...
						ret = TTokenConsumptionType.ETokenConsumed;
					}
					else
					{
						// Try to find a new child worker to handle this kind
						// of data.
						SymParserWorker worker = CreateWorkerByTokenType( aToken );
						//
						if	( worker != null )
						{
							System.Diagnostics.Debug.WriteLine( "SymPreProcessorWorker.OfferToken() - FOUND HANDLER FOR: " + aToken.Value );

							AddChild( worker );
							ret = TTokenConsumptionType.ETokenConsumed;
						}
					}
				}
			}
			#endregion

			// Give it to the children
			if	( ret == TTokenConsumptionType.ETokenNotConsumed )
			{
				// Check whether we're inside a conditional expression skip state.
				bool allowedToOffer = Parser.ConditionalExpressionValue;
				if	( allowedToOffer )
				{
					ret = base.OfferToken( aToken );
				}
			}
			//
			return ret;
		}
		#endregion

		#region Internal methods
		private SymParserWorker CreateWorkerByTokenType( SymToken aToken )
		{
			// Find a worker to handle the token type
			SymParserWorker worker = null;
			SymParserWorkerContext context = new SymParserWorkerContext( WorkerContext, this, aToken );
			//
			switch( aToken.Value )
			{
				// Simple preprocessor operations
				case "define":
					worker = new SymPreProcessorWorkerDefine( context );
					break;
				case "undef":
					break;
				case "include":
					worker = new SymPreProcessorWorkerInclude( context );
					break;

				// Conditionality
				case "if":
					worker = new SymPreProcessorWorkerIf( context );
					break;
				case "ifdef":
					worker = new SymPreProcessorWorkerIfdef( context );
					break;
				case "ifndef":
					worker = new SymPreProcessorWorkerIfndef( context );
					break;
				case "else":
					worker = new SymPreProcessorWorkerElse( context );
					break;
				case "elif":
					worker = new SymPreProcessorWorkerElseIf( context );
					break;
				case "endif":
					worker = new SymPreProcessorWorkerEndif( context );
					break;

				// Skip unhandled preprocessor directives
				default:
					worker = new SymParserWorkerConsumer( context, SymToken.TClass.EClassNewLine );
					break;
			}
			//
			return worker;
		}
		#endregion

		#region Internal properties
		private SymPreProcessorParser Parser
		{
			get
			{
				return (SymPreProcessorParser) WorkerContext.Parser;
			}
		}
		#endregion

		#region Data members
		#endregion
	}
}
