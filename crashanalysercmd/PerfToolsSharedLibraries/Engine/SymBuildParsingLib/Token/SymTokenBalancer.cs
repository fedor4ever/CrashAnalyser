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
using SymbianTree;

namespace SymBuildParsingLib.Token
{
	public class SymTokenBalancer
	{
		#region Observer interface
		public delegate void LevelsBalancedEventHandler();
		public delegate void LevelsImbalancedEventHandler();
		public delegate void LevelChangeEventHandler( int aLevelCount, SymNode aOldLevel, SymNode aNewLevel );
		#endregion

		#region Events
		public event LevelChangeEventHandler EventLevelStarted;
		public event LevelChangeEventHandler EventLevelFinished;
		public event LevelsBalancedEventHandler EventLevelsBalanced;
		public event LevelsImbalancedEventHandler EventLevelsImbalanced;
		#endregion
		
		#region Constructors & destructor
		public SymTokenBalancer()
		{
			Reset();
			System.Diagnostics.Debug.IndentSize = 1;
		}
		#endregion

		#region API
		public void RegisterBalancerTokens()
		{
			RegisterBalancerTokens( false );
		}

		public virtual void RegisterBalancerTokens( bool aEmitLevelZeroBrackets )
		{
			// These are the important tokens relating to function arguments
			SymToken bracketTokenOpening = new SymToken( "(", SymToken.TClass.EClassSymbol, SymToken.TType.ETypeUnidentified );
			SymToken bracketTokenClosing = new SymToken( ")", SymToken.TClass.EClassSymbol, SymToken.TType.ETypeUnidentified );

			// When parsing a function name and arguments, we want to start a new level when we see the first opening
			// brace, but we don't want to emit the token.
			RegisterOpeningToken( bracketTokenOpening, bracketTokenClosing, aEmitLevelZeroBrackets, true, TLevelExpectations.ELevelExpectationsAtLevel, 0, TAssociatedBehaviour.EBehaviourRemoveReduntantBracketing );
			RegisterClosingToken( bracketTokenClosing, bracketTokenOpening, aEmitLevelZeroBrackets, true, TLevelExpectations.ELevelExpectationsAtLevel, 0, TAssociatedBehaviour.EBehaviourRemoveReduntantBracketing );
			
			// For other brackets, we want to reduce the complexity by removing any redundant entries
			RegisterOpeningToken( bracketTokenOpening, bracketTokenClosing, true, true, TLevelExpectations.ELevelExpectationsAboveLevelNumber, 0, TAssociatedBehaviour.EBehaviourRemoveReduntantBracketing );
			RegisterClosingToken( bracketTokenClosing, bracketTokenOpening, true, true, TLevelExpectations.ELevelExpectationsAboveLevelNumber, 0, TAssociatedBehaviour.EBehaviourRemoveReduntantBracketing );
		}

		public virtual bool OfferToken( SymToken aToken )
		{
			bool consumed = false;
			int currentLevelNumber = CurrentLevelNumber;
			SymTokenBalancerMatchCriteria tokenExtendedInfo;

			// Check for bracket matches
			if	( IsOpeningTokenMatch( aToken, out tokenExtendedInfo, currentLevelNumber ) )
			{
				LevelStarted( aToken, tokenExtendedInfo );
				consumed = true;
			}
			else if ( IsClosingTokenMatch( aToken, out tokenExtendedInfo, currentLevelNumber ) )
			{
				LevelFinished( aToken, tokenExtendedInfo );
				consumed = true;
			}

			// Always consume the token if it didn't match
			if	( ! consumed )
			{
				AddToCurrentLevel( aToken );
				consumed = true;
			}
			//
			return consumed;
		}

		public virtual void Reset()
		{
			iTree = new SymTokenBalancerDocument();
		}
		#endregion

		#region API - registration
		public void RegisterOpeningToken( SymToken aToken, SymToken aDiametricToken, bool aEmit, bool aStartsNewLevel, TLevelExpectations aLevelExpectations, int aAssociatedLevel, TAssociatedBehaviour aBehaviour  )
		{
			SymTokenBalancerMatchCriteria criteria = new SymTokenBalancerMatchCriteria( aDiametricToken, aEmit, aStartsNewLevel, aLevelExpectations, aAssociatedLevel, aBehaviour );
			RegisterOpeningToken( aToken, criteria );
		}

		public void RegisterOpeningToken( SymToken aToken, SymTokenBalancerMatchCriteria aCriteria )
		{
			SymToken copy = new SymToken( aToken );
			copy.Tag = aCriteria;
			//
			if	( IsTokenExactMatch( copy, iOpeningTokens ) == false )
			{
				iOpeningTokens.Append( copy );
			}
		}

		public void RegisterClosingToken( SymToken aToken, SymToken aDiametricToken, bool aEmit, bool aStartsNewLevel, TLevelExpectations aLevelExpectations, int aAssociatedLevel, TAssociatedBehaviour aBehaviour )
		{
			SymTokenBalancerMatchCriteria criteria = new SymTokenBalancerMatchCriteria( aDiametricToken, aEmit, aStartsNewLevel, aLevelExpectations, aAssociatedLevel, aBehaviour );
			RegisterClosingToken( aToken, criteria );
		}

        public void RegisterClosingToken( SymToken aToken, SymTokenBalancerMatchCriteria aCriteria )
		{
			SymToken copy = new SymToken( aToken );
			copy.Tag = aCriteria;
			//
			if	( IsTokenExactMatch( copy, iClosingTokens ) == false )
			{
				iClosingTokens.Append( copy );
			}
		}
		#endregion

		#region Properties - level related
		public int CurrentLevelNumber
		{
			get
			{
				int depth = iTree.CurrentNode.Depth;
				return depth;
			}
		}

		public SymNode CurrentNode
		{
			get { return iTree.CurrentNode; }
		}

		public SymTokenBalancerDocument DocumentTree
		{
			get { return iTree; }
		}
		#endregion

		#region Notification framework
		protected virtual void NotifyEventLevelStarted( int aLevelCount, SymNode aOldLevel, SymNode aNewLevel )
		{
			if	( EventLevelStarted != null )
			{
				EventLevelStarted( aLevelCount, aOldLevel, aNewLevel );
			}
		}

		protected virtual void NotifyEventLevelFinished( int aLevelCount, SymNode aOldLevel, SymNode aNewLevel )
		{
			if	( EventLevelFinished != null )
			{
				EventLevelFinished( aLevelCount, aOldLevel, aNewLevel );
			}
		}

		protected virtual void NotifyEventLevelsImbalanced()
		{
			if	( EventLevelsImbalanced != null )
			{
				EventLevelsImbalanced();
			}
		}
		#endregion

		#region Utility methods
		protected bool IsTokenExactMatch( SymToken aToken, SymTokenContainer aContainerToSearch )
		{
			bool ret = false;
			//
			foreach( SymToken t in aContainerToSearch )
			{
				if	( t.Equals( aToken ) )
				{
					if	( aToken.Tag != null && t.Tag != null )
					{
						if	( ( aToken.Tag is SymTokenBalancerMatchCriteria ) && ( t.Tag is SymTokenBalancerMatchCriteria ) )
						{
							SymTokenBalancerMatchCriteria extendedInfo1 = (SymTokenBalancerMatchCriteria) aToken.Tag;
							SymTokenBalancerMatchCriteria extendedInfo2 = (SymTokenBalancerMatchCriteria) t.Tag;
							//
							if	( extendedInfo1.Equals( extendedInfo2 ) )
							{
								ret = true;
								break;
							}
						}
					}
				}
			}
			//
			return ret;
		}

		protected bool IsOpeningTokenMatch( SymToken aToken, out SymTokenBalancerMatchCriteria aCriteria, int aLevelNumber )
		{
			aCriteria = null;
			bool matchFound = false;
			//
			int index = iOpeningTokens.IndexOf( aToken );
			while( index >= 0 && matchFound == false )
			{
				SymToken token = iOpeningTokens[ index ];
				System.Diagnostics.Debug.Assert ( token.Tag != null && token.Tag is SymTokenBalancerMatchCriteria );
				SymTokenBalancerMatchCriteria criteria = (SymTokenBalancerMatchCriteria) token.Tag;

				if	( criteria.Matches( aLevelNumber ) )
				{
					aCriteria = criteria;
					matchFound = true;
				}
				else
				{
					index = iOpeningTokens.IndexOf( aToken, index+1 );
				}
			}

			return matchFound;
		}

		protected bool IsClosingTokenMatch( SymToken aToken, out SymTokenBalancerMatchCriteria aCriteria, int aLevelNumber )
		{
			aCriteria = null;
			bool matchFound = false;
			//
			int index = iClosingTokens.IndexOf( aToken );
			while( index >= 0 && matchFound == false )
			{
				SymToken token = iClosingTokens[ index ];
				System.Diagnostics.Debug.Assert ( token.Tag != null && token.Tag is SymTokenBalancerMatchCriteria );
				SymTokenBalancerMatchCriteria criteria = (SymTokenBalancerMatchCriteria) token.Tag;

				if	( criteria.Matches( aLevelNumber ) )
				{
					aCriteria = criteria;
					matchFound = true;
				}
				else
				{
					index = iClosingTokens.IndexOf( aToken, index+1 );
				}
			}

			return matchFound;
		}

		protected static int CountTokenByType( SymNode aNodeWithChildren, SymToken.TClass aClass )
		{
			int count = 0;
			//
			foreach( SymNode n in aNodeWithChildren )
			{
				bool isNodeToken = ( n is SymNodeToken );
				//
				if	( isNodeToken )
				{
					bool isSpecial = ( ( n is SymTokenBalancerNode ) || ( n is SymTokenBalancerNodeEmittedElement ) );
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

		protected bool LevelCanBeMergedWithParent( SymTokenBalancerMarkerLevelNode aLevelNode )
		{
			#region Example
			// We can replace the opening bracket, the level marker and the closing bracket
			// with the level marker's child.
			//
			// E.g. 
			// ( = opening bracket
			// * = level marker
			// ) = closing bracket
			// V = 'real' node value
			//
			// This:
			//
			//             ( -- * -- )
			//                  |
			//                  V
			// 
			// Can become:
			//
			//                  V
			//
			//
			// Or this:
			//
			//             ( -- * -- )
			//                  |
			//             [ -- * -- ]
			//					|
			//                  V
			// 
			// Can become:
			//
			//             [ -- * -- ]
			//					|
			//                  V
			//
			#endregion
			System.Diagnostics.Debug.Assert( aLevelNode.HasPrevious && aLevelNode.HasNext );
			System.Diagnostics.Debug.Assert( aLevelNode.Previous is SymTokenBalancerNodeEmittedElement && aLevelNode.Next is SymTokenBalancerNodeEmittedElement );
			//
			bool ret = false;
			//
			SymTokenBalancerNodeEmittedElement previous = (SymTokenBalancerNodeEmittedElement) aLevelNode.Previous;
			SymTokenBalancerNodeEmittedElement next = (SymTokenBalancerNodeEmittedElement) aLevelNode.Next;

			// We should be able to remove these brackets, providing they are diametrically oposites.
			SymTokenBalancerMatchCriteria matchInfo;
			if	( IsOpeningTokenMatch( previous.Token, out matchInfo, CurrentLevelNumber ) )
			{
				// We've now got the match info for this opening token. We can compare the
				// match info's diametric token against the closing token to check that they 
				// really are equal and oposite.
				if	( matchInfo.DiametricToken.Equals( next.Token ) )
				{
					// Check whether the children are suitable for coalescing
					int grandChildCount = aLevelNode.ChildCount;
					int numberOfWhiteSpaceGrandChildren = CountTokenByType( aLevelNode, SymToken.TClass.EClassWhiteSpace );
					int nonWhiteSpaceGrandChildrenCount = grandChildCount - numberOfWhiteSpaceGrandChildren;

					if	( nonWhiteSpaceGrandChildrenCount == 1 )
					{
						// If there is just a single non-whitespace token then we are able to
						// coalesce this branch
						ret = true;
					}
					else if ( nonWhiteSpaceGrandChildrenCount == 3 )
					{
						// Branch contains several non-whitespace children. Must check further
						// to see if they are simply bracketed regions themselves
						object levelObject = aLevelNode.ChildByType( typeof( SymTokenBalancerMarkerLevelNode ) );
						if	( levelObject != null )
						{
							SymTokenBalancerMarkerLevelNode childLevel = (SymTokenBalancerMarkerLevelNode) levelObject;
							//
							if	( childLevel.HasPrevious && childLevel.HasNext )
							{
								ret = ( childLevel.Previous is SymTokenBalancerNodeEmittedElement && childLevel.Next is SymTokenBalancerNodeEmittedElement );
							}
						}
						else
						{
							ret = true;
						}
					}
				}
			}
			//
			return ret;
		}

		protected void SimplifyLevel( SymTokenBalancerMarkerLevelNode aLevel )
		{
			System.Diagnostics.Debug.Assert( aLevel.IsRoot == false && aLevel.HasParent );
			SymNode parent = aLevel.Parent;
			int levelNumber = parent.Depth;
			//
			int childCount = parent.ChildCount;
			while( --childCount >= 0 )
			{
				SymNode possibleLevelNode = parent[ childCount ];

				// We're looking to remove redundant bracketing from either side of the level
				// node. First check if we have a level node...
				if	( possibleLevelNode is SymTokenBalancerMarkerLevelNode )
				{
					// Then check whether it has a previous and a next node. These should
					// be the SymTokenBalancerNodeEmittedElement nodes
					if	( possibleLevelNode.HasPrevious && possibleLevelNode.HasNext )
					{
						if	( possibleLevelNode.Previous is SymTokenBalancerNodeEmittedElement && possibleLevelNode.Next is SymTokenBalancerNodeEmittedElement )
						{
							if	( LevelCanBeMergedWithParent( possibleLevelNode as SymTokenBalancerMarkerLevelNode ) )
							{
								SymTokenBalancerNodeEmittedElement previous = (SymTokenBalancerNodeEmittedElement) possibleLevelNode.Previous;
								SymTokenBalancerNodeEmittedElement next = (SymTokenBalancerNodeEmittedElement) possibleLevelNode.Next;

								// Insert value node prior to previous node (which is the opening bracket token).
								parent.InsertChildrenFrom( possibleLevelNode, previous.Previous );

								// Remove the opening bracket token
								previous.Remove();

								// Remove the level marker token
								possibleLevelNode.Remove();

								// Remove the closing bracket token.
								next.Remove();
							}
						}
					}
				}
			}
		}
		#endregion

		#region Internal level manipulation methods
		protected virtual void LevelStarted( SymToken aToken, SymTokenBalancerMatchCriteria aMatchCriteria )
		{
			System.Diagnostics.Debug.Write( System.Environment.NewLine + aToken.Value );

			// Always store the node element so that we can balance brackets
			SymTokenBalancerNodeEmittedElement node = new SymTokenBalancerNodeEmittedElement( aToken, aMatchCriteria );
			iTree.CurrentNode.Add( node );

			// Store the token (with the level) so we can check for open/close mis-matches
			SymTokenBalancerMarkerLevelNode levelNode = new SymTokenBalancerMarkerLevelNode( aMatchCriteria );
			iTree.CurrentNode.Add( levelNode );

			SymNode oldLevel = iTree.CurrentNode;
			SymNode newLevel = levelNode;
			NotifyEventLevelStarted( CurrentLevelNumber, oldLevel, newLevel  );

			iTree.CurrentNode = levelNode;
		}

		protected virtual void LevelFinished( SymToken aToken, SymTokenBalancerMatchCriteria aMatchCriteria )
		{
			#region Example

			// #define TEST1((( B )+( C ))+(E)) 

			#region Key
			// |y| = level y
			// [x] = token of value 'x'
			// [*] = level marker node
			// [@] = (current) level marker node
			// [,] = argument node
			// [(] = opening level emitted token node
			// [)] = closing level emitted token node
			#endregion

			#region 1) First wave of opening level tokens processed...
			//
			//  |0|                    [ROOT]
			//                           |
			//  |1|          [TEST] [(] [*]
			//                           |
			//  |2|                 [(] [*]
			//                           |
			//  |3|                 [(] [@]
			//                           |
			//  |4|                      B
			//
			#endregion

			#region 2) Add closing level token to level |3|, which means that we can now
			//    attempt to simplify level |4|. Level |4| does not contain any child
			//    level nodes, so there is nothing to do here.
			//
			//  |0|                    [ROOT]
			//                           |
			//  |1|          [TEST] [(] [*]
			//                           |
			//  |2|                 [(] [@]
			//                           |
			//  |3|                 [(] [*] [)]
			//                           |
			//  |4|                 [ ] [B] [ ]
			//
			#endregion

			#region 3) Add plus symbol. This obviously becomes a child of the current node,
			//    i.e. a child of level |2|.
			//
			//  |0|                            [ROOT]
			//                                   |
			//  |1|                  [TEST] [(] [*]
			//                                   |
			//  |2|                         [(] [@]
			//                                   |
			//  |3|                 [(] [*] [)] [+]
			//                           |
			//  |4|                 [ ] [B] [ ]
			//
			#endregion

			#region 4) Start new opening level node on level |3|. The newly added level node
			//    on level |3| becomes the current node.
			//
			//  |0|                            [ROOT]
			//                                   |
			//  |1|                  [TEST] [(] [*]
			//                                   |
			//  |2|                         [(] [*]
			//                                   |
			//  |3|                 [(] [*] [)] [+] [(] [@] 
			//                           |               |
			//  |4|                 [ ] [B] [ ]
			//
			#endregion
		
			#region 5) Add the tokens near the 'C' to level |4|
			//
			//  |0|                            [ROOT]
			//                                   |
			//  |1|                  [TEST] [(] [*]
			//                                   |
			//  |2|                         [(] [*]
			//                                   |
			//  |3|                 [(] [*] [)] [+] [(] [@] 
			//                           |               |
			//  |4|                 [ ] [B] [ ]     [ ] [C] [ ]
			//
			#endregion

			#region 6) Add closing level token to level |3|, which means that we can now
			//    attempt to simplify level |4|, this time the [C] branch.
			//    Since this branch does not contain any sub-level nodes, there is nothing 
			//    to be done and therefore levels |3| and |4| remain unchanged. 
			//    The level node at level |2| becomes the current node.
			//
			//  |0|                            [ROOT]
			//                                   |
			//  |1|                  [TEST] [(] [*]
			//                                   |
			//  |2|                         [(] [@]
			//                                   |
			//  |3|                 [(] [*] [)] [+] [(] [*] [)]
			//                           |               |
			//  |4|                 [ ] [B] [ ]     [ ] [C] [ ]
			//
			#endregion

			#region 7a) Add closing level token to level |2|, which means that we can now
			//     attempt to simplify level |3|.
			//
			//  |0|                            [ROOT]
			//                                   |
			//  |1|                  [TEST] [(] [*]
			//                                   |
			//  |2|                         [(] [@] [)]
			//                                   |
			//  |3|                 [(] [*] [)] [+] [(] [*] [)]
			//                           |               |
			//  |4|                 [ ] [B] [ ]     [ ] [C] [ ]
			//
			#endregion

			#region 7b) We iterate through all the child level nodes of level |3| looking
			//     to see if any of their children are simple enough to merge up into level
			//     |3| itself. There are two level nodes in level |3| and both contain
			//     children that are considered simple enough to merge up. This is because
			//     they contain only a single non-whitespace node so we can simplify the level by
			//     merging [B] from level |4| into level |3|. Likewise for [C].
			//
			//     This means that the bracketry on level |3| is redundant since level 
			//     |4| contains two sets of only a single non-whitespace token. Level |4|
			//     is therefore entirely removed.
			//
			//     The node at level |1| becomes the current node now.
			//
			//  |0|                            [ROOT]
			//                                   |
			//  |1|                  [TEST] [(] [@]
			//                                   |
			//  |2|                         [(] [*] [)]
			//                                   |
			//  |3|                 [ ] [B] [ ] [+] [ ] [C] [ ]
			//
			#endregion

			#region 8) Add the plus symbol to level |2|. Then we start a new level
			//    as a result of adding the opening bracket prior to 'E.'
			//    This new level node at level |2| now becomes the current node.
			//
			//  |0|                                 [ROOT]
			//                                         |
			//  |1|                  [TEST]   [(]     [*]
			//                                         |
			//  |2|             [(] [*] [)]           [+] [(] [@]
			//                       |                         |
			//  |3|     [ ] [B] [ ] [+] [ ] [C] [ ]
			//
			#endregion

			#region 9) Now add the 'E' token to level |3|.
			//
			//  |0|                                 [ROOT]
			//                                         |
			//  |1|                  [TEST]   [(]     [*]
			//                                         |
			//  |2|             [(] [*] [)]           [+] [(] [@]
			//                       |                         |
			//  |3|     [ ] [B] [ ] [+] [ ] [C] [ ]           [E]
			#endregion

			#region 10) We then add the closing bracket to level |2| which means
			//  that we can attempt to simplify level |3|'s 'E' branch. There's nothing
			//  to do though, since it doesn't contain any child level nodes.
			//  The level node at level |1| again becomes current.
			//
			//  |0|                                 [ROOT]
			//                                         |
			//  |1|                  [TEST]   [(]     [@]
			//                                         |
			//  |2|             [(] [*] [)]           [+] [(] [*] [)]
			//                       |                         |
			//  |3|     [ ] [B] [ ] [+] [ ] [C] [ ]           [E]
			#endregion

			#region 11a) We then add the closing bracket to level |1| which means
			//  that we can attempt to simplify level |2| entirely. This is the
			//  situation prior to simplification.
			//
			//  |0|                                 [ROOT]
			//                                         |
			//  |1|                  [TEST]   [(]     [@] [)]
			//                                         |
			//  |2|             [(] [*] [)]           [+] [(] [*] [)]
			//                       |                         |
			//  |3|     [ ] [B] [ ] [+] [ ] [C] [ ]           [E]
			#endregion

			#region 11b) The two level nodes have been taged as *1 and *2 to make it
			//  easier to explain the process. First we attempt to simplify level *1.
			//  However, since its children consist of more than a single non-whitespace 
			//  token, we cannot make level |3|'s " B + C " branch as a replacement
			//  for the bracket *1 tokens. Therefore this remains unchanged
			//  
			//
			//  |0|                                [ROOT]
			//                                        |
			//  |1|                  [TEST]   [(]    [@]     [)]
			//                                        |
			//  |2|            [(] [*1] [)]          [+] [(] [*2] [)]
			//                       |                         |
			//  |3|     [ ] [B] [ ] [+] [ ] [C] [ ]           [E]
			#endregion
			
			#region 11c) The level *2 node, however, contains only a single non-whitespace
			//  child token, so it can be simplified. The level *2 node is replaced by
			//  its child (E).
			//
			// The final tree looks like this, with the root as the current node once more:
			//  
			//
			//  |0|                                 [@ROOT@]
			//                                         |
			//  |1|        [TEST] [(]                 [*]                         [)]
			//                                         |
			//  |2|                   [(]             [*]             [)] [+] [E]
			//                                         |               
			//  |3|                       [ ] [B] [ ] [+] [ ] [C] [ ]   
			#endregion
			
			#endregion

			System.Diagnostics.Debug.Write( aToken.Value );
			System.Diagnostics.Debug.WriteLine( " " );

			// First of all, check if the current node has a parent. If we're at the root
			// node and we see a closing token, then we've got an imbalanced stack.
			if	( iTree.CurrentNode.IsRoot )
			{
				NotifyEventLevelsImbalanced();
			}
			else
			{
				// We expect the parent to be a level node
				System.Diagnostics.Debug.Assert( iTree.CurrentNode is SymTokenBalancerMarkerLevelNode );

				// Notify that we're about to change levels
				SymTokenBalancerMarkerLevelNode currentLevel = (SymTokenBalancerMarkerLevelNode) iTree.CurrentNode;
				SymNode newLevel = currentLevel.Parent;
				
				// The new level should not be null in this case
				System.Diagnostics.Debug.Assert( newLevel != null );

				// Check whether the closing token type is the same type as was used to start
				// the level. E.g. for this case is "([ANDMORE)]" which has a mis-match
				// between the opening and closing braces on each level. We can't simplify this
				// during the 'end level behaviour' stage.
				bool levelsBalanced = currentLevel.MatchCriteria.DiametricToken.Equals( aToken );

				// Switch levels
				iTree.CurrentNode = newLevel;

				// We have to refetch up-to-date match info, since we've changed levels, and the match
				// info that was used to enter this method is associated with the previous level
				// (not the new level number, which is now one less).
				SymTokenBalancerMatchCriteria matchCriteria;
				if	( IsClosingTokenMatch( aToken, out matchCriteria, CurrentLevelNumber ) == false )
				{
					matchCriteria = aMatchCriteria;
				}

				// Always store the node element so that we can balance brackets
				SymTokenBalancerNodeEmittedElement node = new SymTokenBalancerNodeEmittedElement( aToken, matchCriteria );
				iTree.CurrentNode.Add( node );

				// We have finished the current level. E.g. see step 6. Need to simplify level |2|, where possible.
				PerformEndLevelBehaviour( currentLevel );

				if	( levelsBalanced )
				{
					// Notify that we've finished some level
					NotifyEventLevelFinished( CurrentLevelNumber, currentLevel, newLevel  );
				}
				else
				{
					// Imbalance
					NotifyEventLevelsImbalanced();
				}
			}
		}

		protected virtual void AddToCurrentLevel( SymToken aToken )
		{
			System.Diagnostics.Debug.Write( aToken.Value );

			SymNodeToken node = new SymNodeToken( aToken );
			iTree.CurrentNode.Add( node );
		}

		#endregion

		#region End level behaviour related
		protected void PerformEndLevelBehaviour( SymTokenBalancerMarkerLevelNode aLevel )
		{
			PerformEndLevelBehaviour( aLevel, aLevel.MatchCriteria );
		}
		
		protected virtual void PerformEndLevelBehaviour( SymNode aLevel, SymTokenBalancerMatchCriteria aCriteria )
		{
			#region Example step (11a) from LevelFinished method 
			//
			// We then add the closing bracket to level |1| which means
			// that we can attempt to simplify level |2| entirely. This is the
			// situation prior to simplification.
			//
			//  |0|                                 [ROOT]
			//                                         |
			//  |1|                  [TEST]   [(]     [@] [)]
			//                                         |
			//  |2|             [(] [*] [)]           [+] [(] [*] [)]
			//                       |                         |
			//  |3|     [ ] [B] [ ] [+] [ ] [C] [ ]           [E]
			//
			// aLevel would be the @ node at level |1|.
			//
			// We remove redundant bracketing from our children, i.e. those on level |2|, not from our own level. 
			// Our parent takes care of removing this level's redundant bracketing (when it's level is completed)
			// or then when an argument separator token is handled.
			//
			// We must iterate through level |1|'s children to find other level nodes. We check whether each
			// child level node can be simplified by checking its children (i.e. our grandchildren).
			//
			#endregion

			if	( aCriteria.IsAssociatedBehaviourRemoveRedundantBracketing )
			{
				int index = 0;
				object childLevelNodeObject = aLevel.ChildByType( typeof(SymTokenBalancerMarkerLevelNode), ref index );
				while( childLevelNodeObject != null )
				{
					SymTokenBalancerMarkerLevelNode childLevelNode = (SymTokenBalancerMarkerLevelNode) childLevelNodeObject;
					if	( childLevelNode.CanBeSubsumed )
					{
						childLevelNode.Subsume();
					}

					// Try to find next level node
					++index;
					childLevelNodeObject = aLevel.ChildByType( typeof(SymTokenBalancerMarkerLevelNode), ref index );
				}
			}
		}
		#endregion

		#region Data members
		private SymTokenBalancerDocument iTree;
		private SymTokenContainer iOpeningTokens = new SymTokenContainer();
		private SymTokenContainer iClosingTokens = new SymTokenContainer();
		#endregion
	}
}
