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
using System.Collections;
using SymBuildParsingLib.Token;
using SymBuildParsingLib.Tree;
using SymBuildParsingLib.Common.Objects;
using SymbianTree;

namespace SymBuildParsingLib.Parser.Framework.Utils
{
	public class SymFunctionParser : SymTokenBalancer
	{
		#region Observer interface
		public delegate void ArgumentAvailable( SymArgument aArgument, SymToken aDelimitingToken );
		#endregion

		#region Events
		public event ArgumentAvailable EventArgumentAvailableHandler;
		#endregion

		#region Constructors & destructor
		public SymFunctionParser()
		{
		}
		#endregion

		#region API
		public virtual void RegisterFunctionParserTokens()
		{
			// Base class registration
			RegisterBalancerTokens();

			// These are the important tokens relating to function arguments
			SymToken bracketTokenOpening = new SymToken( "(", SymToken.TClass.EClassSymbol, SymToken.TType.ETypeUnidentified );
			SymToken bracketTokenClosing = new SymToken( ")", SymToken.TClass.EClassSymbol, SymToken.TType.ETypeUnidentified );
			SymToken squareBracketTokenOpening = new SymToken( "[", SymToken.TClass.EClassSymbol, SymToken.TType.ETypeUnidentified );
			SymToken squareBracketTokenClosing = new SymToken( "]", SymToken.TClass.EClassSymbol, SymToken.TType.ETypeUnidentified );
			SymToken templateBracketTokenOpening = new SymToken( "<", SymToken.TClass.EClassSymbol, SymToken.TType.ETypeUnidentified );
			SymToken templateBracketTokenClosing = new SymToken( ">", SymToken.TClass.EClassSymbol, SymToken.TType.ETypeUnidentified );

			// We want to track levels for square brackets and template arguments in order to ensure we balance correctly.
			// We don't want to remove any redundancy here as these may have special meaning.
			RegisterOpeningToken( squareBracketTokenOpening, squareBracketTokenClosing, true, true, TLevelExpectations.ELevelExpectationsAboveLevelNumber, 0, TAssociatedBehaviour.EBehaviourNone );
			RegisterClosingToken( squareBracketTokenClosing, squareBracketTokenOpening, true, true, TLevelExpectations.ELevelExpectationsAboveLevelNumber, 0, TAssociatedBehaviour.EBehaviourNone );
			RegisterOpeningToken( templateBracketTokenOpening, templateBracketTokenClosing, true, true, TLevelExpectations.ELevelExpectationsAboveLevelNumber, 0, TAssociatedBehaviour.EBehaviourNone );
			RegisterClosingToken( templateBracketTokenClosing, templateBracketTokenOpening, true, true, TLevelExpectations.ELevelExpectationsAboveLevelNumber, 0, TAssociatedBehaviour.EBehaviourNone );

			// Define our argument separation token(s).
			TAssociatedBehaviour flags = TAssociatedBehaviour.EBehaviourNone;
			flags |= TAssociatedBehaviour.EBehaviourCreateSubTree;
			flags |= TAssociatedBehaviour.EBehaviourRemoveReduntantBracketing;
			//
			SymToken commaDelimiterToken = new SymToken( ",", SymToken.TClass.EClassSymbol, SymToken.TType.ETypeUnidentified );
			RegisterArgumentSeparatorToken( commaDelimiterToken, new SymTokenBalancerMatchCriteria( SymToken.NullToken(), false, true, TLevelExpectations.ELevelExpectationsAtLevel, 1, flags ) );
			RegisterArgumentSeparatorToken( commaDelimiterToken, new SymTokenBalancerMatchCriteria( SymToken.NullToken(), true, true, TLevelExpectations.ELevelExpectationsAboveLevelNumber, 1, TAssociatedBehaviour.EBehaviourNone ) );
		}

		public virtual SymArgument MakeArgument()
		{
			SymNode levelNodeToObtainArgumentsFrom = CurrentNode;

			// Try to work out whether we have a leve node at the current
			// scope which should be preferred t
			object levelNodeObject = levelNodeToObtainArgumentsFrom.ChildByType( typeof(SymTokenBalancerMarkerLevelNode) );
			if	( levelNodeObject != null )
			{
				levelNodeToObtainArgumentsFrom = (SymNode) levelNodeObject;
			}

			return MakeArgument( levelNodeToObtainArgumentsFrom );
		}

		public virtual SymArgument MakeArgument( SymNode aLevelToMakeArgumentsFrom )
		{
			SymArgument argument = new SymArgument();

			// Convert (recursively) any emitted elements to real tokens
			if	( aLevelToMakeArgumentsFrom is SymTokenBalancerMarkerLevelNode )
			{
				SymTokenBalancerMarkerLevelNode levelNode = (SymTokenBalancerMarkerLevelNode) aLevelToMakeArgumentsFrom;
				levelNode.ConvertEmittedElementsToRealTokenNodes( true /*recurse*/ );
			}

			// Now actually obtain the argument tokens
			int count = aLevelToMakeArgumentsFrom.ChildCount;
			int i = 0;
			//
			while( i < count )
			{
				SymNode n = aLevelToMakeArgumentsFrom[ 0 ];

				// We always remove any other nodes, irrespective of their type.
				// This is to ensure that the document tree does not get cluttered
				// with redundant argument token info.
				n.Remove();

				// Now we decide what to do...
				if	( n is SymTokenBalancerMarkerArgumentNode )
				{
					// We've reached the argument itself. This is the
					// signal to stop processing. We remove the argument node
					// since its not relevant to the production of the tree.
					break;
				}
				else if ( n is SymTokenBalancerMarkerLevelNode )
				{
					// Create a new sub-argument node and copy over the 
					// children.
					SymTokenBalancerMarkerLevelNode levelNode = (SymTokenBalancerMarkerLevelNode) n;
					SymArgumentSubLevel subLevel = levelNode.AsArgumentSubLevel( true );
					argument.CurrentNode.Add( subLevel );
				}
				else if ( n is SymTokenBalancerNodeEmittedElement )
				{
					System.Diagnostics.Debug.Assert( false ); // shouldn't get here anymore!
				}
				else if	( n is SymNodeToken )
				{
					// Node is implicitly removed since it transfers
					// from one tree to another.
					argument.CurrentNode.Add( n );
				}

				count = aLevelToMakeArgumentsFrom.ChildCount;
			}
			//
			SymTokenUtils.RemoveWhiteSpace( argument, true );
			return argument;
		}

		public string MakeFunctionName()
		{
			StringBuilder name = new StringBuilder();

			// Pull out all the level one tokens from the balancer. These form
			// the function/define name.
			foreach( SymNode n in DocumentTree )
			{
				if	( n is SymTokenBalancerMarkerNode )
				{
					// Not part of the name
					break;
				}
				else if ( n is SymTokenBalancerNodeEmittedElement )
				{
					// Not part of the name - also, this is indicator to stop iterating!
					break;
				}
				else if ( n is SymNodeToken )
				{
					SymNodeToken tokenNode = (SymNodeToken) n;
					name.Append( tokenNode.Token.Value );
				}
			}

			return name.ToString();
		}
		#endregion

		#region API - registration
		public void RegisterArgumentSeparatorToken( SymToken aToken, SymTokenBalancerMatchCriteria aCriteria )
		{
			SymToken copy = new SymToken( aToken );
			copy.Tag = aCriteria;
			//
			if	( IsTokenExactMatch( copy, iArgumentSeparators ) == false )
			{
				iArgumentSeparators.Append( copy );
			}
		}
		#endregion

		#region From SymTokenBalancer
		public override bool OfferToken( SymToken aToken )
		{
			bool consumed = false;
			int currentLevelNumber = CurrentLevelNumber;

			// Check for bracket matches
			SymTokenBalancerMatchCriteria tokenExtendedInfo;
			if	( IsArgumentSeparatorMatch( aToken, out tokenExtendedInfo, currentLevelNumber ) )
			{
				ArgumentStarted( aToken, tokenExtendedInfo );
				consumed = true;
			}
			
			// If it wasn't an arg token or it was, but it wasn't at the 
			// correct (expected) level, then give it to the base class
			// to handle.
			if	( consumed == false )
			{
				consumed = base.OfferToken( aToken );
			}
			//
			return consumed;
		}
		#endregion

		#region Notification framework
		protected virtual void NotifyArgumentAvailable( SymArgument aArgument, SymToken aDelimitingToken )
		{
			if	( EventArgumentAvailableHandler != null )
			{
				EventArgumentAvailableHandler( aArgument, aDelimitingToken );
			}
		}
		#endregion

		#region New internal framework
		protected virtual void ArgumentStarted( SymToken aToken, SymTokenBalancerMatchCriteria aCriteria )
		{
			System.Diagnostics.Debug.Write( aToken.Value );

			int currentLevelNumber = CurrentLevelNumber;

			// Perform any base class end level behaviour
			PerformEndLevelBehaviour( CurrentNode, aCriteria );

			// Add the emit node (the rest of the code will work out whether it needs to quote it when the final
			// tree is formed).
			SymTokenBalancerNodeEmittedElement argEmitElement = new SymTokenBalancerNodeEmittedElement( aToken, aCriteria );
			DocumentTree.CurrentNode.Add( argEmitElement );

			// Always add the argument node
			SymTokenBalancerMarkerArgumentNode argNode = new SymTokenBalancerMarkerArgumentNode( aCriteria );
			DocumentTree.CurrentNode.Add( argNode );

			if	( aCriteria.IsAssociatedBehaviourCreateSubTree )
			{
				// Make a new argument definition based upon the tokens we have in
				// the main document tree.
				SymArgument argument = MakeArgument( DocumentTree.CurrentNode );

				// Then notify the observer
				NotifyArgumentAvailable( argument, aToken );
			}
		}
		#endregion

		#region Internal utility methods
		protected bool IsArgumentSeparatorMatch( SymToken aToken, out SymTokenBalancerMatchCriteria aCriteria, int aLevelNumber )
		{
			aCriteria = null;
			bool matchFound = false;
			//
			int index = iArgumentSeparators.IndexOf( aToken );
			while( index >= 0 && matchFound == false )
			{
				SymToken token = iArgumentSeparators[ index ];
				System.Diagnostics.Debug.Assert ( token.Tag != null && token.Tag is SymTokenBalancerMatchCriteria );
				SymTokenBalancerMatchCriteria criteria = (SymTokenBalancerMatchCriteria) token.Tag;

				if	( criteria.Matches( aLevelNumber ) )
				{
					aCriteria = criteria;
					matchFound = true;
				}
				else
				{
					index = iArgumentSeparators.IndexOf( aToken, index+1 );
				}
			}

			return matchFound;
		}

		#endregion

		#region Data members
		private SymTokenContainer iArgumentSeparators = new SymTokenContainer();
		#endregion
	}
}
