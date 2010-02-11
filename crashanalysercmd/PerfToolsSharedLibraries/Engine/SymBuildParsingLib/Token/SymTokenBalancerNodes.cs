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
using SymBuildParsingLib.Tree;
using SymBuildParsingLib.Token;
using SymBuildParsingLib.Common.Objects;
using SymbianTree;

namespace SymBuildParsingLib.Token
{
	#region SymTokenBalancerDocument
	public class SymTokenBalancerDocument : SymTokenDocument
	{
		#region Constructors & destructor
		public SymTokenBalancerDocument()
		{
		}
		#endregion

		#region Internal framework API
		protected override void ExtractToContainer( SymNode aNode, SymTokenContainer aContainer )
		{
			if	( aNode is SymTokenBalancerNodeEmittedElement )
			{
				SymTokenBalancerNodeEmittedElement node = (SymTokenBalancerNodeEmittedElement) aNode;
				node.AddToContainerIfEmittable( aContainer );
			}
		}

		protected override void ExtractToDocument( SymNode aNode, SymTokenDocument aDocument )
		{
			if	( aNode is SymTokenBalancerNodeEmittedElement )
			{
				SymTokenBalancerNodeEmittedElement node = (SymTokenBalancerNodeEmittedElement) aNode;
				node.AddToDocumentIfEmittable( aDocument );
			}
		}

		protected override bool NodeIsExtractable( SymNode aNode )
		{
			bool ret = false;
			//
			if	( aNode is SymTokenBalancerNodeEmittedElement )
			{
				SymTokenBalancerNodeEmittedElement node = (SymTokenBalancerNodeEmittedElement) aNode;
				ret = node.Emit;
			}
			//
			return ret;
		}
		#endregion
	}
	#endregion

	#region SymTokenBalancerNode
	public class SymTokenBalancerNode : SymNodeToken
	{
		#region Constructors & destructor
		public SymTokenBalancerNode( SymToken aToken )
			: base( aToken )
		{
		}
		#endregion
	}
	#endregion

	#region SymTokenBalancerMarkerNode
	public class SymTokenBalancerMarkerNode : SymNodeAddAsChild
	{
		#region Constructors & destructor
		public SymTokenBalancerMarkerNode( SymTokenBalancerMatchCriteria aMatchCriteria )
		{
			iMatchCriteria = aMatchCriteria;
		}
		#endregion

		#region Properties
		public SymTokenBalancerMatchCriteria MatchCriteria
		{
			get { return iMatchCriteria; }
		}
		#endregion

		#region Data members
		private readonly SymTokenBalancerMatchCriteria iMatchCriteria;
		#endregion
	}
	#endregion

	#region SymTokenBalancerMarkerLevelNode
	public class SymTokenBalancerMarkerLevelNode : SymTokenBalancerMarkerNode
	{
		#region Constructors & destructor
		public SymTokenBalancerMarkerLevelNode( SymTokenBalancerMatchCriteria aMatchCriteria )
			: base( aMatchCriteria )
		{
		}
		#endregion

		#region From SymNode		
		public override void Replace( SymNode aReplacement )
		{
			if	( HasPrevious && Previous is SymTokenBalancerNodeEmittedElement )
			{
				Previous.Remove();
			}
			if	( HasNext && Next is SymTokenBalancerNodeEmittedElement )
			{
				Next.Remove();
			}

			base.Replace( aReplacement );
		}
		#endregion

		#region API
		public SymArgumentSubLevel AsArgumentSubLevel( bool aRecurse )
		{
			SymArgumentSubLevel ret = new SymArgumentSubLevel( this );
			//
			if	( aRecurse )
			{
				foreach( SymNode child in ret )
				{
					if	( child is SymTokenBalancerMarkerLevelNode )
					{
						SymTokenBalancerMarkerLevelNode markerNode = (SymTokenBalancerMarkerLevelNode) child;
						SymArgumentSubLevel subLevel = markerNode.AsArgumentSubLevel( aRecurse );
						ret.InsertChild( subLevel, markerNode );
						markerNode.Remove();
					}
				}
			}
			//
			return ret;
		}

		public void ConvertEmittedElementsToRealTokenNodes( bool aRecurse )
		{
			int i = ChildCount;
			//
			while( i > 0 )
			{
				SymNode child = this[ --i ];
				//
				if	( child is SymTokenBalancerNodeEmittedElement )
				{
					SymTokenBalancerNodeEmittedElement emittedElement = (SymTokenBalancerNodeEmittedElement) child;
					if	( emittedElement.Emit )
					{
						SymNodeToken replacement = new SymNodeToken( emittedElement.Token ); 
						InsertChild( replacement, child );
					}
					child.Remove();
				}
				else if ( child is SymTokenBalancerMarkerLevelNode && aRecurse )
				{
					SymTokenBalancerMarkerLevelNode childLevel = (SymTokenBalancerMarkerLevelNode) child;
					childLevel.ConvertEmittedElementsToRealTokenNodes( aRecurse );
				}
			}
		}

		public int CountTokenByType( SymToken.TClass aClass )
		{
			int count = 0;
			//
			foreach( SymNode n in this )
			{
				if	( n is SymNodeToken )
				{
					bool isSpecial = ( n is SymTokenBalancerNode );
					//
					if	( isSpecial == false )
					{
						SymToken t = ((SymNodeToken) n).Token;
						//
						if	( t.Class == aClass )
						{
							++count;
						}
					}
				}
			}
			//
			return count;
		}

		public void Subsume()
		{
			System.Diagnostics.Debug.Assert( IsComplete );
			//
			SymTokenBalancerNodeEmittedElement previous = EmittedElementPrevious;
			SymTokenBalancerNodeEmittedElement next = EmittedElementNext;

			// Insert all my children as siblings of myself (i.e. make my parent's
			// grandchildren its direct children - i.e. promote them up the tree
			// by one level).
			SymNode parent = Parent;
			ArrayList parentsChildren = Parent.Children;
			parent.InsertChildrenFrom( this /* my children */, previous.Previous /* move them before the opening bracket */);

			// Remove the opening bracket token
			previous.Remove();

			// Remove the closing bracket token.
			next.Remove();

			// Remove the level marker token, i.e myself
			Remove();
		}
		#endregion

		#region Properties
		public bool CanBeSubsumed
		{
			get
			{
				bool canBeSubsumed = false;
				//
				if	( IsComplete )
				{
					if	( AreLocalEmittedNodesDiametricallyOposite )
					{
						bool isSimple = IsSimple;
						//
						if	( isSimple )
						{
							// Deal with the case whereby we have something like this: ([SOMETHINGELSE])
							if	( ( Parent is SymTokenBalancerMarkerLevelNode) && ChildCountLevelNodes == 0 )
							{
								// We don't have any child level nodes and our parent is a level (it always sound be in any case).
								SymTokenBalancerMarkerLevelNode parent = (SymTokenBalancerMarkerLevelNode) Parent;
								if	( parent.IsComplete )
								{
									// Check whether the parent is a function. If it is, then we don't want to subsume the children.
									bool isFunc = parent.IsFunction;
									if	( isFunc == false )
									{
										// The parent is complete, which means we can compare this level's open and closing tokens
										// with the parents.
										canBeSubsumed = ( parent.EmittedElementPrevious.Token.Equals( EmittedElementPrevious.Token ) && 
											parent.EmittedElementNext.Token.Equals( EmittedElementNext.Token ) );
									}
								}
								else
								{
									canBeSubsumed = true;
								}
								
							}
							else
							{
								canBeSubsumed = true;
							}
						}
						else
						{
							// Check whether our children contains at least a single argument separator node.
							// If it does, we can't allow it to be subsumed.
							if	( ChildCountArgumentNodes == 0 )
							{
								if	( Parent is SymTokenBalancerMarkerLevelNode )
								{
									SymTokenBalancerMarkerLevelNode parent = (SymTokenBalancerMarkerLevelNode) Parent;
									bool parentIsSimple = parent.IsSimple;
									canBeSubsumed = parentIsSimple;
								}
							}
						}
					}
				}
				//
				return canBeSubsumed;
			}
		}

		public bool IsFunction
		{
			get
			{
				#region Example
				//
				// [FUNC_NAME] [(] [*] [)] 
				//                  |
				//        [alpha_numeric_child]
				//
				#endregion
				
				bool bracketsAreCorrect = false;
				bool hasFunctionName = false;
				bool childIsValid = false;
				//
				if	( AreLocalEmittedNodesDiametricallyOposite )
				{
					// Check that the previous node is a function name
					if	( Previous.HasPrevious )
					{
						SymNode previousPrevious = Previous.Previous;
						if	( previousPrevious is SymNodeToken )
						{
							SymNodeToken nodeToken = (SymNodeToken) previousPrevious;
							hasFunctionName = (nodeToken.Token.Class == SymToken.TClass.EClassAlphaNumeric );
						}
					}

					// Check that the brackets are ( and )
					bracketsAreCorrect = ( EmittedElementPrevious.Token.Value == "(" && EmittedElementNext.Token.Value == ")" );

					// Check that the level node has but one child and that it
					// is alphanumeric in nature.
					if	( ChildCountByType( typeof(SymNodeToken) ) > 0 )
					{
						foreach( SymNode child in this )
						{
							if	( child is SymNodeToken )
							{
								SymNodeToken tokenNode = (SymNodeToken) child;
								//
								if	( tokenNode.Token.Class == SymToken.TClass.EClassAlphaNumeric )
								{
									childIsValid = true;
									break;
								}
							}
						}
					}
				}

				bool isFunction = (bracketsAreCorrect && hasFunctionName && childIsValid );
				return isFunction;
			}
		}

		public bool IsSimple
		{
			get
			{
				bool isSimple = false;
				//
				if	( IsFunction )
				{
					isSimple = false;
				}
				else if	( ChildCountNonWhiteSpace == 1 )
				{
					// My child is but a single node...
					isSimple = true;
				}
				else if ( ChildCountWhiteSpace == ChildCount )
				{
					// My children are just whitespace
					isSimple = true;
				}
				else if ( ChildCountLevelNodes == 1 )
				{
					int childCountLevelAndAssociatedEmittedNodes = ChildCountLevelAndAssociatedEmittedNodes;
					int childCount = ChildCount;
					int whiteSpaceCount = ChildCountWhiteSpace;
					//
					if	( (childCount - whiteSpaceCount) == childCountLevelAndAssociatedEmittedNodes )
					{
						// Ignoring the whitespace nodes, all we had was a single level node plus its
						// two associated emitted elements...
						isSimple = true;
					}
				}
				//
				return isSimple;
			}
		}

		public bool IsComplete
		{
			get
			{
				bool complete = false;

				// Must have a next and a previous node. Next and previous must be
				// emitted element nodes
				if	( HasNext && HasPrevious )
				{
					complete = ( Previous is SymTokenBalancerNodeEmittedElement && Next is SymTokenBalancerNodeEmittedElement );
				}

				return complete;
			}
		}

		public bool AreLocalEmittedNodesDiametricallyOposite
		{
			get
			{
				bool ret = false;
				//
				if	( IsComplete )
				{
					SymTokenBalancerNodeEmittedElement previous = (SymTokenBalancerNodeEmittedElement) Previous;
					SymTokenBalancerNodeEmittedElement next = (SymTokenBalancerNodeEmittedElement) Next;
					//
					if	( previous.MatchCriteria.DiametricToken.Equals( next.Token ) )
					{
						// Paranoid
						ret = ( next.MatchCriteria.DiametricToken.Equals( previous.Token ) );
					}
				}
				//
				return ret;
			}
		}

		public SymNodeToken FunctionName
		{
			get
			{
				if	( IsFunction == false )
				{
					throw new ArgumentException( "Level node is not a function node" );
				}

				System.Diagnostics.Debug.Assert( Previous.HasPrevious );
				System.Diagnostics.Debug.Assert( Previous.Previous is SymNodeToken );
				SymNodeToken functionNameToken = (SymNodeToken) Previous.Previous;
				return functionNameToken;
			}
		}

		public SymTokenContainer ChildTokens
		{
			get
			{
				SymTokenContainer ret = new SymTokenContainer();
				//
				foreach( SymNode c in this )
				{
					if	( c is SymNodeToken )
					{
						SymNodeToken t = (SymNodeToken) c;
						ret.Append( t.Token );
					}
				}
				//
				return ret;
			}
		}

		public int ChildCountWhiteSpace
		{
			get
			{
				return CountTokenByType( SymToken.TClass.EClassWhiteSpace );
			}
		}

		public int ChildCountNonWhiteSpace
		{
			get
			{
				return ChildCount - ChildCountWhiteSpace;
			}
		}

		public int ChildCountLevelNodes
		{
			get
			{
				int count = ChildCountByType( typeof(SymTokenBalancerMarkerLevelNode) );
				return count;
			}
		}

		public int ChildCountLevelAndAssociatedEmittedNodes
		{
			get
			{
				int count = 0;
				//
				int index = 0;
				object childLevelNodeObject = ChildByType( typeof(SymTokenBalancerMarkerLevelNode), ref index );
				while( childLevelNodeObject != null )
				{
					// Count is incremented by one each time we find a level node
					SymTokenBalancerMarkerLevelNode childLevelNode = (SymTokenBalancerMarkerLevelNode) childLevelNodeObject;
					++count;

					// It should always have a previous token.
					System.Diagnostics.Debug.Assert( childLevelNode.HasPrevious );
					if	( childLevelNode.Previous is SymTokenBalancerNodeEmittedElement )
					{
						++count;
					}

					// It may have a next node, assuming its complete...
					if	( childLevelNode.HasNext )
					{
						if	( childLevelNode.Next is SymTokenBalancerNodeEmittedElement )
						{
							++count;
						}
					}

					// Move to next child level node
					++index;
					childLevelNodeObject = ChildByType( typeof(SymTokenBalancerMarkerLevelNode), ref index );
				}
				//
				return count;
			}
		}

		public int ChildCountArgumentNodes
		{
			get
			{
				int count = ChildCountByType( typeof(SymTokenBalancerMarkerArgumentNode) );
				return count;
			}
		}

		public SymTokenBalancerNodeEmittedElement EmittedElementPrevious
		{
			get
			{
				return (SymTokenBalancerNodeEmittedElement) Previous;
			}
		}

		public SymTokenBalancerNodeEmittedElement EmittedElementNext
		{
			get
			{
				return (SymTokenBalancerNodeEmittedElement) Next;
			}
		}

		#endregion

		#region Internal methods
		#endregion
	}
	#endregion

	#region SymTokenBalancerMarkerArgumentNode
	public class SymTokenBalancerMarkerArgumentNode : SymTokenBalancerMarkerNode
	{
		#region Constructors & destructor
		public SymTokenBalancerMarkerArgumentNode( SymTokenBalancerMatchCriteria aMatchCriteria )
			: base( aMatchCriteria )
		{
		}
		#endregion
	}
	#endregion

	#region SymTokenBalancerNodeEmittedElement
	public class SymTokenBalancerNodeEmittedElement : SymTokenBalancerMarkerNode
	{
		#region Constructors & destructor
		public SymTokenBalancerNodeEmittedElement( SymToken aToken, SymTokenBalancerMatchCriteria aMatchCriteria )
			: base( aMatchCriteria )
		{
			iToken = aToken;
		}
		#endregion

		#region API
		public void AddToDocumentIfEmittable( SymDocument aDocument )
		{
			if	( Emit )
			{
				SymNodeToken node = new SymNodeToken( Token );
				aDocument.CurrentNode.Add( node );
			}
		}

		public void AddToContainerIfEmittable( SymTokenContainer aContainer )
		{
			if	( Emit )
			{
				aContainer.Append( Token );
			}
		}
		#endregion

		#region Properties
		public SymToken Token
		{
			get { return iToken; }
		}

		public bool Emit
		{
			get { return MatchCriteria.Emit; }
		}
		#endregion

		#region Data members
		private readonly SymToken iToken;
		#endregion
	}
	#endregion
}
