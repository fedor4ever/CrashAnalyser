/*
* Copyright (c) 2009 Nokia Corporation and/or its subsidiary(-ies). 
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
using System.Collections.Generic;

namespace SymbianTree
{
	#region SymNodeEnumeratorSiblings 
	public class SymNodeEnumeratorSiblings : IEnumerator<SymNode>, IEnumerable<SymNode>
	{
		#region Constructors
		public SymNodeEnumeratorSiblings( SymNode aNodeToEnumerate )
		{
			iNode = aNodeToEnumerate;
		}
		#endregion

        #region From IEnumerable<SymNode>
        IEnumerator<SymNode> IEnumerable<SymNode>.GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
        #endregion

		#region IEnumerator Members
		void IEnumerator.Reset()
		{
			iCurrentNode = null;
		}

		object IEnumerator.Current
		{
			get
			{
				return iCurrentNode;
			}
		}

		bool IEnumerator.MoveNext()
		{
			if	( iCurrentNode == null )
			{
				iCurrentNode = iNode;
			}
			else
			{
				iCurrentNode = iCurrentNode.Next;
			}

			bool haveMoreNodes = ( iCurrentNode.Next != null );
			return haveMoreNodes;
		}
		#endregion

        #region From IEnumerator<SymNode>
        SymNode IEnumerator<SymNode>.Current
        {
            get { return iCurrentNode; }
        }
        #endregion

        #region From IDisposable
        public void Dispose()
        {
        }
        #endregion

		#region Data members
		private readonly SymNode iNode;
		private SymNode iCurrentNode = null;
		#endregion
    }
	#endregion

	#region SymNodeEnumeratorChildren 
    public class SymNodeEnumeratorChildren : IEnumerator<SymNode>, IEnumerable<SymNode>
	{
		#region Constructors
		public SymNodeEnumeratorChildren( SymNode aNodeToEnumerate )
		{
			iNode = aNodeToEnumerate;
		}
		#endregion

        #region From IEnumerable<SymNode>
        IEnumerator<SymNode> IEnumerable<SymNode>.GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
        #endregion

		#region IEnumerator Members
		void IEnumerator.Reset()
		{
			iEnumeratorIndex = -1;
		}

		object IEnumerator.Current
		{
			get
			{
				return iNode.Children[ iEnumeratorIndex ];
			}
		}

		bool IEnumerator.MoveNext()
		{
			return ( ++iEnumeratorIndex < iNode.ChildCount );
		}
		#endregion

        #region From IEnumerator<SymNode>
        SymNode IEnumerator<SymNode>.Current
        {
            get { return iNode.Children[ iEnumeratorIndex ]; }
        }
        #endregion

        #region From IDisposable
        public void Dispose()
        {
        }
        #endregion

		#region Data members
		private readonly SymNode iNode;
		private int iEnumeratorIndex = -1;
		#endregion
	}
	#endregion

	#region SymNodeEnumeratorTreeChildrenFirst 
    public class SymNodeEnumeratorTreeChildrenFirst : IEnumerator<SymNode>, IEnumerable<SymNode>
	{
		#region Constructors
		public SymNodeEnumeratorTreeChildrenFirst( SymNode aStartingNode )
		{
			iStartingNode = aStartingNode;
		}
		#endregion

        #region From IEnumerable<SymNode>
        IEnumerator<SymNode> IEnumerable<SymNode>.GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
        #endregion

		#region IEnumerator Members
		void IEnumerator.Reset()
		{
			iCurrentNode = null;
		}

		object IEnumerator.Current
		{
			get
			{
				return iCurrentNode;
			}
		}

		bool IEnumerator.MoveNext()
		{
			iCurrentNode = NextNode( iCurrentNode );

			bool haveMoreNodes = ( NextNode( iCurrentNode ) != null );
			return haveMoreNodes;
		}
		#endregion

        #region From IEnumerator<SymNode>
        SymNode IEnumerator<SymNode>.Current
        {
            get { return iCurrentNode; }
        }
        #endregion

        #region From IDisposable
        public void Dispose()
        {
        }
        #endregion

		#region Internal methods
		private SymNode NextNode( SymNode aCurrentNode )
		{
			SymNode nextNode = null;
			//
			if	( aCurrentNode == null )
			{
				nextNode = iStartingNode;
			}
			else
			{
				#region Example
				//
				//                [A]
				//               /
				//              /
				//            [B]
				//           / | \
				//          /  |  \
				//        [C] [E] [I]
				//       /    / \   \
				//      /    /   \   \
				//    [D]  [F]   [H]  [J]
				//        / 
				//       /
				//     [G]
				//
				// Navigation plan:
				//
				//    [A] -> [B] -> [C] -> [D] -> [E] -> [F] -> [G] -> [H] -> [I] -> [J]
				// 
				#endregion

				if	( aCurrentNode.HasChildren )
				{
					// Try to visit the node's children first.
					nextNode = aCurrentNode.FirstChild;
				}
				else
				{
					// No children...
					if	( aCurrentNode.HasNext )
					{
						// Go to next sibling
						nextNode = aCurrentNode.Next;
					}
					else
					{
						// No (more) siblings - go to parent's next sibling.
						// For example, in the case of the current node being [G]
						// we need to traverse seemlessly to [H]. Therefore
						// we go from [G] -> [F] -> [H]
						nextNode = aCurrentNode.Parent; // [F]
						while( nextNode != null && nextNode.HasNext == false )
						{
							nextNode = nextNode.Parent;
						}

						// We'll now be at [F]
						if	( nextNode != null )
						{
							// Now we'll be at [H]
							nextNode = nextNode.Next;
						}
					}
				}
			}
			//
			return nextNode;
		}
		#endregion

		#region Data members
		private readonly SymNode iStartingNode;
		private SymNode iCurrentNode = null;
		#endregion
	}
	#endregion

	#region SymNodeEnumeratorUpTreeSiblingsFirst
    public class SymNodeEnumeratorUpTreeSiblingsFirst : IEnumerator<SymNode>, IEnumerable<SymNode>
	{
		#region Constructors
		public SymNodeEnumeratorUpTreeSiblingsFirst( SymNode aStartingNode )
		{
			iStartingNode = aStartingNode.FirstSibling;
		}
		#endregion

        #region From IEnumerable<SymNode>
        IEnumerator<SymNode> IEnumerable<SymNode>.GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
        #endregion

		#region IEnumerator Members
		void IEnumerator.Reset()
		{
			iCurrentNode = null;
		}

		object IEnumerator.Current
		{
			get
			{
				return iCurrentNode;
			}
		}

		bool IEnumerator.MoveNext()
		{
			if	( iCurrentNode == null )
			{
				iCurrentNode = iStartingNode;
			}
			else
			{
				if	( iCurrentNode.HasNext )
				{
					iCurrentNode = iCurrentNode.Next;
				}
				else
				{
					iCurrentNode = iCurrentNode.Parent.FirstSibling;
				}

				System.Diagnostics.Debug.Assert( iCurrentNode != null );
			}

			bool haveMoreNodes = iCurrentNode.HasNext;
			if	( haveMoreNodes == false )
			{
				haveMoreNodes = iCurrentNode.HasParent;
			}
			return haveMoreNodes;
		}
		#endregion

        #region From IEnumerator<SymNode>
        SymNode IEnumerator<SymNode>.Current
        {
            get { return iCurrentNode; }
        }
        #endregion

        #region From IDisposable
        public void Dispose()
        {
        }
        #endregion

		#region Data members
		private readonly SymNode iStartingNode;
		private SymNode iCurrentNode = null;
		#endregion
	}
	#endregion
}
