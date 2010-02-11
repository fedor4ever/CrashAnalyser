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
using System.Text;
using SymBuildParsingLib.Tree;
using SymBuildParsingLib.Token;
using SymBuildParsingLib.Common.Objects;
using SymBuildParsingLib.Parser.Framework.Parser;
using SymBuildParsingLib.Parser.Framework.Workers;
using SymBuildParsingLib.Parser.Framework.Document;
using SymBuildParsingLib.Parser.PreProcessor.Nodes;

namespace SymBuildParsingLib.Parser.PreProcessor.Workers
{
	public sealed class SymPreProcessorWorkerInclude : SymParserWorkerConsumer
	{
		#region Constructors & destructor
		public SymPreProcessorWorkerInclude( SymParserWorkerContext aContext )
			: base( aContext, SymToken.TClass.EClassNewLine, SymParserWorkerConsumer.TDyingAction.EWhenDyingMakeAbsoluteParentNodeCurrent )
		{
			// Make a new child node for the include
			iIncludeNode = new SymNodePreProcessorInclude();
			aContext.Document.CurrentNode.Add( iIncludeNode );
			aContext.Document.CurrentNode = iIncludeNode;
		}
		#endregion

		#region From SymParserWorkerConsumer
		protected override void HandleTerminatingConditionMatch( SymToken aToken )
		{
			// Update the include so that it contains a fully resolved path
			iIncludeNode.IncludeDefinition.AdjustRelativeInclude( WorkerContext.Parser.FileName );

			// We've now a fully resolved file name which we can parse, should
			// we desire...
			string includeFile = iIncludeNode.IncludeDefinition.Location;
			if	( Utils.SymFileSystemUtils.FileExists( includeFile ) )
			{
				// Make a new document context
				SymParserDocumentContext subDocumentContext = new SymParserDocumentContext( includeFile, WorkerContext );

				// Use the existing document, but with a new context
				WorkerContext.Document.PushContext( subDocumentContext );

				// Make a new carbon copy of this parser (whatever concrete type it may be)
				SymParserBase subParser = WorkerContext.Parser.CarbonCopy( new object[] { WorkerContext.Document } );

				// Make the waiting object
				SymParserWaiter waiter = new SymParserWaiter( subParser );

				// Now parse the file and wait for the result
				subParser.Parse();
				waiter.Wait();

				// Restore the original context
				WorkerContext.Document.PopContext();
			}
		}
		#endregion

		#region From SymParserWorker
		public override SymParserWorker.TTokenConsumptionType OfferToken( SymToken aToken )
		{
			TTokenConsumptionType ret = TTokenConsumptionType.ETokenNotConsumed;
			System.Diagnostics.Debug.Assert( WorkerContext.Document.CurrentNode is SymNodePreProcessorInclude );
			
			// Only consume tokens whilst we're unsure of the include type (system vs user).
			if	( iIncludeNode.IncludeDefinition.Type == SymIncludeDefinition.TType.ETypeUndefined )
			{
				if	( aToken.Class == SymToken.TClass.EClassQuotation )
				{
					// Must be a user include if its a quotation that we're handling
					iIncludeNode.IncludeDefinition.Type = SymIncludeDefinition.TType.ETypeUser;
					ret = TTokenConsumptionType.ETokenConsumed;
				}
				else if ( aToken.Class == SymToken.TClass.EClassSymbol && aToken.Value == "<" )
				{
					// Consume it, but don't absorb it
					iIncludeNode.IncludeDefinition.Type = SymIncludeDefinition.TType.ETypeSystem;
					ret = TTokenConsumptionType.ETokenConsumed;
				}
			}
			else if ( iIncludeNode.IncludeDefinition.Type != SymIncludeDefinition.TType.ETypeUndefined )
			{
				// Only consume the tokens whilst we've not yet identified the include
				// definition location.
				if	( iIncludeNode.IncludeDefinition.Location.Length == 0 )
				{
					if	( iIncludeNode.IncludeDefinition.Type == SymIncludeDefinition.TType.ETypeSystem && aToken.Class == SymToken.TClass.EClassSymbol && aToken.Value == ">" )
					{
						iIncludeNode.IncludeDefinition.Location = iWorkingIncludePath.ToString();
					}
					else if ( iIncludeNode.IncludeDefinition.Type == SymIncludeDefinition.TType.ETypeUser && aToken.Class == SymToken.TClass.EClassQuotation && aToken.Type != SymToken.TType.ETypeUnidentified )
					{
						iIncludeNode.IncludeDefinition.Location = iWorkingIncludePath.ToString();
					}
					else
					{
						iWorkingIncludePath.Append( aToken.Value );
					}
				}
			}

			// Base class will dequeue us once we reach the new line
			ret = base.OfferToken( aToken );
			return ret;
		}
		#endregion

		#region Data members
		private StringBuilder iWorkingIncludePath = new StringBuilder();
		private readonly SymNodePreProcessorInclude iIncludeNode;
		#endregion
	}
}
