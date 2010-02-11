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
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace SymbianTree
{
	public abstract class SymNode : IEnumerable<SymNode>
	{
		#region Constructors
		public SymNode()
		{
		}

		public SymNode( SymNode aParent )
		{
			aParent.AppendChild( this );
		}

		public SymNode( object aData )
		{
			iData = aData;
		}

		public SymNode( object aData, SymNode aParent )
		{
			iData = aData;
			aParent.AppendChild( this );
		}
		#endregion

        #region API - siblings
        public void InsertBeforeMe( SymNode aNode )
		{
			if	( aNode.HasParent )
			{
				// Ensure that aNode is no longer registered with any existing parent.
				aNode.Parent.RemoveChild( aNode );
			}

			if	( HasPrevious )
			{
				// Need to link up my previous node with aNode and then
				// link aNode with me
				SymNode p = Previous;
				p.Next = aNode;
				aNode.Previous = p;
			}
			else
			{
				// I didn't have a previous node, so if I am inserting aNode
				// before me, it won't have a previous node either.
				aNode.Previous = null;
			}

			// Now prepend.
			aNode.Next = this;
			Previous = aNode;

			// Ensure that my new brother is a child of my parent
			if	( HasParent )
			{
				// Must insert the child at the correct position within 
				// my parents children container. aNode should be
				// inserted before me, i.e. after my *OLD* previous
				// node which is now aNode's previous node (since it
				// now sits in the middle).
				Parent.InsertNodeAfterSpecificChild( aNode, aNode.Previous );
			}
			aNode.Parent = Parent;
		}

		public void AppendAfterMe( SymNode aNode )
		{
			if	( aNode.HasParent )
			{
				// Ensure that aNode is no longer registered with any existing parent.
				aNode.Parent.RemoveChild( aNode );
			}

			if	( HasNext )
			{
				// Need to link up my next node with aNode and then
				// link aNode with me
				SymNode n = Next;
				n.Previous = aNode;
				aNode.Next = n;
			}
			else
			{
				// I didn't have a next node, so if I am inserting aNode
				// after me, it won't have a next node either.
				aNode.Next = null;
			}

			// Now prepend.
			aNode.Previous = this;
			Next = aNode;

			// Ensure that my new brother is a child of my parent
			if	( HasParent )
			{
				// Must insert the child at the correct position within 
				// my parents children container. aNode should be
				// inserted after me.
				Parent.InsertNodeAfterSpecificChild( aNode, this );
			}
			aNode.Parent = Parent;
		}

		public void AppendSibling( SymNode aNode )
		{
			#region Example
			// Find the last node in the current sibling branch, 
			// e.g. if we have nodes:
			// 
			//         [A] [B] [C] 
			//
			// and this = [B] and aNode = [X]
			// then we will create the resultant tree consisting of:
			//
			//         [A] [B] [C] [X]
			#endregion
			SymNode insertionPoint = this;
			while( insertionPoint.HasNext )
			{
				insertionPoint = insertionPoint.Next;
			}

			// Now append.
			insertionPoint.Next = aNode;
			aNode.Previous = insertionPoint;
			aNode.Next = null;

			// Ensure that my new brother is a child of my parent
			Parent.InsertChild( aNode, this );
		}

		public void PrependSibling( SymNode aNode )
		{
			#region Example
			// Find the first node in the current sibling branch, 
			// e.g. if we have nodes:
			// 
			//         [A] [B] [C] 
			//
			// and this = [B] and aNode = [X]
			// then we will create the resultant tree consisting of:
			//
			//         [X] [A] [B] [C]
			#endregion
			SymNode insertionPoint = this;
			while( insertionPoint.HasPrevious )
			{
				insertionPoint = insertionPoint.Previous;
			}

			// Now prepend.
			insertionPoint.Previous = aNode;
			aNode.Next = insertionPoint;
			aNode.Previous = null;

			// Ensure that my new brother is a child of my parent
			Parent.InsertChild( aNode, null );
		}

		public void InsertSibling( SymNode aNode, SymNode aAfterNode )
		{
			if	( aAfterNode == null )
			{
				PrependSibling( aNode );
			}
			else
			{
				System.Diagnostics.Debug.Assert( aAfterNode.Parent == Parent );
				System.Diagnostics.Debug.Assert( HasNext || HasPrevious );
				System.Diagnostics.Debug.Assert( DbgCheckNodeIsASibling( aAfterNode ) );

				#region Example
				// Preconditions:
				// ==============
				// Siblings		= [A] [B] [C]
				// aAfterNode	= [B]
				//    aNode		= [X]
				//
				// Output:
				// =======
				// Siblings		= [A] [B] [X] [C]
				#endregion

				// We need to ensure that the linkage for
				// B -> X -> C is persisted after the operation.
				//
				// First, obtain [C] by looking at [B]'s next node.
				SymNode nextNode = aAfterNode.Next;

				// Update [B]'s next node to point to [X]
				aAfterNode.Next = aNode;

				// Update [X]'s next and previous nodes
				aNode.Previous = aAfterNode; // [B]
				aNode.Next = nextNode; // [C]

				// Update [C]'s previous node
				nextNode.Previous = aNode;
	
				// Ensure that my new brother is a child of my parent
				Parent.InsertChild( aNode, aAfterNode );
			}
		}

		public bool SiblingExists( SymNode aNode )
		{
			bool found = false;
			SymNodeEnumeratorSiblings iterator = new SymNodeEnumeratorSiblings( FirstSibling );
			//
			foreach( SymNode node in iterator )
			{
				if	( node == aNode )
				{
					found = true;
					break;
				}
			}
			//
			return found;
		}

		public bool SiblingTypeExists( System.Type aType )
		{
			bool found = false;
			SymNodeEnumeratorSiblings iterator = new SymNodeEnumeratorSiblings( FirstSibling );
			//
			foreach( SymNode node in iterator )
			{
				if	( node.GetType() == aType )
				{
					found = true;
					break;
				}
			}
			//
			return found;
		}
		#endregion

		#region API - children
		public void AppendChild( SymNode aChild )
		{
			if	( aChild.HasParent )
			{
				// Ensure that aNode is no longer registered with any existing parent.
				aChild.Parent.RemoveChild( aChild );
			}

			if	( HasChildren )
			{
				SymNode originalLastChild = LastChild;
				originalLastChild.Next = aChild;
				aChild.Previous = originalLastChild;
			}
			//
			aChild.Parent = this;
			aChild.Next = null;
			Children.Add( aChild );
		}

		public void PrependChild( SymNode aChild )
		{
			if	( aChild.HasParent )
			{
				// Ensure that aNode is no longer registered with any existing parent.
				aChild.Parent.RemoveChild( aChild );
			}

			if	( HasChildren )
			{
				SymNode originalFirstChild = FirstChild;
				originalFirstChild.Previous = aChild;
				aChild.Next = originalFirstChild;
			}
			//
			aChild.Parent = this;
			aChild.Previous = null;
			Children.Insert( 0, aChild );
		}

		public void InsertChild( SymNode aNode, SymNode aAfterNode )
		{
			if	( aAfterNode == null )
			{
				PrependChild( aNode );
			}
			else
			{
				System.Diagnostics.Debug.Assert( aAfterNode.Parent == this );
				System.Diagnostics.Debug.Assert( iChildren != null && iChildren.Count > 0 );
				System.Diagnostics.Debug.Assert( DbgCheckNodeIsAChild( aAfterNode ) );

				if	( aNode.HasParent )
				{
					// Ensure that aNode is no longer registered with any existing parent.
					aNode.Parent.RemoveChild( aNode );
				}

				#region Example
				// Input:
				// ======
				// Children		= [A] [B] [C]
				// aAfterNode	= [B]
				//    aNode		= [X]
				//
				// Output:
				// =======
				// Children		= [A] [B] [X] [C]
				#endregion

				// We need to ensure that the linkage for
				// B -> X -> C is persisted after the operation.
				//
				// First, obtain [C] by looking at [B]'s next node.
				SymNode nextNode = aAfterNode.Next;

				// Update [B]'s next node to point to [X]
				aAfterNode.Next = aNode;

				// Update [X]'s next and previous nodes
				aNode.Previous = aAfterNode; // [B]
				aNode.Next = nextNode; // [C]

				// Update [C]'s previous node
				if	( nextNode != null )
				{
					nextNode.Previous = aNode; // [X]
				}

				// Ensure child is added to children array at
				// the correct position.
				aNode.Parent = this;
				InsertNodeAfterSpecificChild( aNode, aAfterNode );
			}
		}

        public void RemoveChild( SymNode aChild )
		{
			if	( IsChild( aChild ) )
			{
				int index = ChildIndex( aChild );
				Children.RemoveAt( index );
				aChild.Parent = null;
			}
		}

		public bool IsChild( SymNode aNode )
		{
            bool found = false;
            //
            if ( iChildren != null )
            {
                found = ( ChildIndex( aNode ) >= 0 );
            }
			//
			return found;
		}
		
		public int ChildIndex( SymNode aNode )
		{
            int index = iChildren.IndexOf( aNode );
			return index;
		}

		public bool ChildTypeExists( System.Type aType )
		{
			bool found = false;
			SymNodeEnumeratorChildren iterator = new SymNodeEnumeratorChildren( this );
			//
			foreach( SymNode node in iterator )
			{
				if	( node.GetType() == aType )
				{
					found = true;
					break;
				}
			}
			//
			return found;
		}

		public object ChildByType( System.Type aType )
		{
			object ret = null;
			SymNodeEnumeratorChildren iterator = new SymNodeEnumeratorChildren( this );
			//
			foreach( SymNode node in iterator )
			{
				if	( node.GetType() == aType )
				{
					ret = node;
					break;
				}
			}
			//
			return ret;
		}

		public object ChildByType( System.Type aType, ref int aStartIndex )
		{
			object ret = null;
			int count = iChildren.Count;
			for(; aStartIndex<count; aStartIndex++ )
			{
				SymNode node = (SymNode) iChildren[ aStartIndex ];
				//
				if	( node.GetType() == aType )
				{
					ret = node;
					break;
				}
			}
			//
			return ret;
		}

		public int ChildCountByType( System.Type aType )
		{
			int count = 0;
			SymNodeEnumeratorChildren iterator = new SymNodeEnumeratorChildren( this );
			//
			foreach( SymNode node in iterator )
			{
				System.Type type = node.GetType();
				//
				bool isInstanceOf = type.IsInstanceOfType( aType );
				bool isSubClassOf = type.IsSubclassOf( aType );
				bool isSameType = type.Equals( aType );
				//
				if	( isInstanceOf || isSubClassOf || isSameType )
				{
					++count;
				}
			}
			//
			return count;
		}
		#endregion

		#region API - copying / moving
		public void AppendChildrenFrom( SymNode aNode )
		{
			while( aNode.HasChildren )
			{
				SymNode childToMove = aNode[ 0 ];
				AppendChild( childToMove );
			}
		}

		public void InsertChildrenFrom( SymNode aNode, SymNode aChildToInsertAfter )
		{
			while( aNode.HasChildren )
			{
				SymNode childToMove = aNode.LastChild;
				InsertChild( childToMove, aChildToInsertAfter );
			}
		}
		#endregion

		#region API - removing from tree
		public void Remove()
		{
			// Need to tidy up siblings
			if	( HasPrevious )
			{
				SymNode p = Previous;
				p.Next = Next;
			}
			if	( HasNext )
			{
				SymNode n = Next;
				n.Previous = Previous;
			}

			// Need to unregister from parent
			if	( HasParent )
			{
				Parent.RemoveChild( this );
			}
		}

		public virtual void Replace( SymNode aReplacement )
		{
			InsertBeforeMe( aReplacement );
			Remove();
		}
		#endregion

        #region API - adding to tree
        public abstract void Add( SymNode aNode );
		#endregion

        #region API - XML
        protected virtual string XmlNodeName
        {
            get { return this.GetType().ToString(); }
        }

        protected virtual void Serialize( XmlWriter aSink )
        {
            string nodeName = XmlNodeName;
            //
            aSink.WriteStartElement( null, nodeName, null );
            SerializeChildren( aSink );
            aSink.WriteEndElement();
        }

        protected virtual void SerializeChildren( XmlWriter aSink )
        {
            SymNodeEnumeratorChildren iterator = new SymNodeEnumeratorChildren( this );
            foreach ( SymNode node in iterator )
            {
                node.Serialize( aSink );
            }
        }
        #endregion

		#region Properties - data
		public object Data
		{
			get { return iData; }
			set { iData = value; }
		}
		#endregion

		#region Properties - parents, root, depth
		public SymNode Parent
		{
			get { return iParent; }
			set { iParent = value; }
		}

		public SymNode Root
		{
			get
			{
				SymNode node = this;
				while( node.HasParent )
				{
					node = node.Parent;
				}
				//
				return node;
			}
		}

		public int Depth
		{
			get
			{
				int depth = 0;
				//
				SymNode node = this;
				while( node.HasParent )
				{
					node = node.Parent;
					++depth;
				}
				//
				return depth;
			}
		}
		#endregion

		#region Properties - children
		public List<SymNode> Children
		{
			get
			{
				if	( iChildren == null )
				{
                    CreateChildrenListNow( Granularity );
				}
                //
				return iChildren;
			}
		}

        public SymNode FirstChild
		{
			get
			{
				SymNode ret = null;
				//
				if	( iChildren != null && iChildren.Count > 0 )
				{
					ret = iChildren[ 0 ];
				}
				//
				return ret;
			}
		}

		public SymNode LastChild
		{
			get
			{
				SymNode ret = null;
				//
                if ( iChildren != null && iChildren.Count > 0 )
				{
                    ret = iChildren[ iChildren.Count - 1 ];
				}
				//
				return ret;
			}
		}

		public int ChildCount
		{
			get
            {
                int count = 0;
                //
                if ( iChildren != null )
                {
                    count = Children.Count;
                }
                //
                return count;
            }
		}

		public SymNode this[ int aIndex ]
		{
			get
			{
				return iChildren[ aIndex ];
			}
		}
		#endregion

		#region Properties - siblings
		public SymNode Next
		{
			get { return iNext; }
			set { iNext = value; }
		}

		public SymNode Previous
		{
			get { return iPrevious; }
			set { iPrevious = value; }
		}

		public SymNode FirstSibling
		{
			get
			{
				SymNode sibling = this;
				while( sibling.HasPrevious )
				{
					sibling = sibling.Previous;
				}
				//
				return sibling;
			}
		}

		public SymNode LastSibling
		{
			get
			{
				SymNode sibling = this;
				while( sibling.HasNext )
				{
					sibling = sibling.Next;
				}
				//
				return sibling;
			}
		}

        public int SiblingCount
		{
			get
			{
				int count = 1;
			
				// Work backwards first
				SymNode node = this;
				while( node.HasPrevious )
				{
					node = node.Previous;
					++count;
				}

				// Try all nodes after the current one
				node = this;
				while( node.HasNext )
				{
					node = node.Next;
					++count;
				}

				return count;
			}
		}
		#endregion

		#region Properties - relationships
		public bool IsRoot
		{
			get { return Parent == null; }
		}

		public bool HasParent
		{
			get { return iParent != null; }
		}

		public bool HasNext
		{
			get { return iNext != null; }
		}

		public bool HasPrevious
		{
			get { return iPrevious != null; }
		}

		public bool HasChildren
		{
			get { return ( iChildren != null && iChildren.Count > 0 ); }
		}
		#endregion

        #region Properties - misc
        public static int Granularity
        {
            get { return iGranularity; }
            set { iGranularity = value; }
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
		{
			return new SymNodeEnumeratorChildren( this );
		}

        IEnumerator<SymNode> IEnumerable<SymNode>.GetEnumerator()
        {
            return new SymNodeEnumeratorChildren( this );
        }
        #endregion

		#region Internal constants
		const int KSymNodeDefaultChildrenGranularity = 10;
		#endregion

		#region Internal methods
        protected void CreateChildrenListNow( int aGranularity )
        {
            iChildren = new List<SymNode>( aGranularity );
        }

		private void InsertNodeAfterSpecificChild( SymNode aNode, SymNode aAfterNode )
		{
			int index = 0;
			//
			if	( aAfterNode != null )
			{
				System.Diagnostics.Debug.Assert( DbgCheckNodeIsAChild( aAfterNode ) );
				index = ChildIndex( aAfterNode );
			}
			//
			iChildren.Insert( index + 1, aNode );
			aNode.Parent = this;
		}
		#endregion

		#region Debug checks
		private bool DbgCheckNodeIsAChild( SymNode aNode )
		{
			System.Diagnostics.Debug.Assert( aNode != this );
			System.Diagnostics.Debug.Assert( iChildren != null && iChildren.Count > 0 );

			int index = ChildIndex( aNode );
			if	( index < 0 || index >= ChildCount )
			{
				System.Diagnostics.Debug.Assert( false, "The specified node is not a child of the current node" );
			}
			return (index >= 0 && index < ChildCount );
		}

		private bool DbgCheckNodeIsASibling( SymNode aNode )
		{
			System.Diagnostics.Debug.Assert( aNode != this );
			System.Diagnostics.Debug.Assert( HasNext || HasPrevious );
			
			// Work backwards first
			SymNode node = this;
			while( node.HasPrevious )
			{
				node = node.Previous;
				if	( node == aNode )
				{
					return true;
				}
			}

			// Try all nodes after the current one
			node = this;
			while( node.HasNext )
			{
				node = node.Next;
				if	( node == aNode )
				{
					return true;
				}
			}

			System.Diagnostics.Debug.Assert( false, "The specified node is not a sibling of the current node" );
			return false;
		}
		#endregion

		#region Data members
        private object iData;
        private SymNode iParent;
		private SymNode iNext;
		private SymNode iPrevious;
        private List<SymNode> iChildren;
        private static int iGranularity = KSymNodeDefaultChildrenGranularity;
        #endregion
	}
}
