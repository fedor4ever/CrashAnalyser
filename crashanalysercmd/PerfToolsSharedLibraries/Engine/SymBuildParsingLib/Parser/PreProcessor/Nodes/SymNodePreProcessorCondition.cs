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
using SymBuildParsingLib.Utils;
using SymBuildParsingLib.Parser.Framework.Document;
using SymBuildParsingLib.Parser.Framework.Nodes;
using SymbianTree;

namespace SymBuildParsingLib.Parser.PreProcessor.Nodes
{
	public class SymNodePreProcessorCondition : SymNodeCondition
	{
		#region Constructors & destructor
		public SymNodePreProcessorCondition( SymNodeCondition.TType aType )
		: base( aType )
		{
		}

		public SymNodePreProcessorCondition( string aName )
		: base( aName )
		{
		}
		#endregion

		#region From SymNodeCondition
		public override void Evaluate( SymParserDocumentContext aContext )
		{
			IsEvaluated = false;

			if	( Type == TType.ETypeIf || Type == TType.ETypeElseIf )
			{
				// Evaluate the expression, taking into account the current #define'd 
				// values. Prepares a new document with these expressions evaluated.
				SymTokenDocument evalDoc = new SymTokenDocument();
				EvaluateDefineNodes( BalancedArguments, evalDoc, aContext );
				string expression = evalDoc.ChildrenAsString( false, true );
				EvaluationResult = SymExpressionEvaluator.EvaluateAsBoolean( expression );
			}
			else if ( Type == TType.ETypeIfdef || Type == TType.ETypeIfndef )
			{
				// Convert the tree to a flat expression
				string symbol = BalancedArguments.ChildrenAsString( true, true );

				// Check if the symbol is defined
				if ( Type == TType.ETypeIfdef )
				{
					EvaluationResult = aContext.DefineDirectory.IsDefined( symbol );
				}
				else if ( Type == TType.ETypeIfndef )
				{
					EvaluationResult = !aContext.DefineDirectory.IsDefined( symbol );
				}
			}
			else if ( Type == TType.ETypeElse )
			{
				// Else statements always evaluate to true. We let the parent
				// conditional expression node decide whether or not this item should
				// 'fire'
				EvaluationResult = true;
			}

			IsEvaluated = true;
		}
		#endregion

		#region Properties
		#endregion

		#region Internal methods
		private void EvaluateDefineNodes( SymNode aNode, SymTokenDocument aDocument, SymParserDocumentContext aContext )
		{
			foreach( SymNode n in aNode )
			{
				if	( n is SymTokenBalancerMarkerLevelNode )
				{
					bool added = false;
					//
					SymTokenBalancerMarkerLevelNode levelNode = (SymTokenBalancerMarkerLevelNode) n;
					if	( levelNode.IsFunction )
					{
						SymNodeToken functionNameNode = levelNode.FunctionName;
						//
						if	( functionNameNode.Token.Equals( iDefinedNodeToken ) )
						{
							SymTokenContainer defineName = levelNode.ChildTokens;
							string flattened = defineName.CoalescedTokenValue;
							
							// Get definition result
							bool isDefined = aContext.DefineDirectory.IsDefined( flattened );
							SymToken isDefinedToken = new SymToken( isDefined.ToString().ToLower(), SymToken.TClass.EClassAlphaNumeric, SymToken.TType.ETypeAlphaNumericNormal );

							// Remove already added "defined" text node from output document
							if	( aDocument.CurrentNode.LastChild is SymNodeToken )
							{
								SymNodeToken last = (SymNodeToken) aDocument.CurrentNode.LastChild;
								if	( last.Token.Equals( iDefinedNodeToken ) )
								{
									last.Remove();
								}
							}

							// Add result
							aDocument.CurrentNode.Add( new SymNodeToken( isDefinedToken ) );
							added = true;
						}
					}

					if	( added == false )
					{
						if	( levelNode.HasPrevious && levelNode.Previous is SymTokenBalancerNodeEmittedElement )
						{
							levelNode.EmittedElementPrevious.AddToDocumentIfEmittable( aDocument );
						}

						SymNode newLevelNode = new SymNodeAddAsChild();
						aDocument.CurrentNode.Add( newLevelNode );
						aDocument.CurrentNode = newLevelNode;
						EvaluateDefineNodes( n, aDocument, aContext );
						aDocument.MakeParentCurrent();

						if	( levelNode.HasNext && levelNode.Next is SymTokenBalancerNodeEmittedElement )
						{
							levelNode.EmittedElementNext.AddToDocumentIfEmittable( aDocument );
						}
					}
				}
				else if ( n is SymNodeToken )
				{
					SymNodeToken node = (SymNodeToken) n;
					SymNodeToken copy = new SymNodeToken( node.Token );
					aDocument.CurrentNode.Add( copy );
				}
				else if	( n is SymTokenBalancerNodeEmittedElement )
				{
					// Handled when the level marker is reached
				}
			}
		}
		#endregion

		#region Data members
		private static SymToken iDefinedNodeToken = new SymToken( "defined", SymToken.TClass.EClassAlphaNumeric, SymToken.TType.ETypeAlphaNumericNormal );
		#endregion
	}
}
