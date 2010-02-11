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
using SymbianTree;

namespace SymBuildParsingLib.Token
{
	public class SymTokenDocument : SymDocument
	{
		#region Constructors & destructor
		public SymTokenDocument()
		{
		}
		#endregion

		#region API
		public string ChildrenAsString( bool aIgnoreWhiteSpace, bool aRecurse )
		{
			SymNodeEnumeratorChildren enumerator = new SymNodeEnumeratorChildren( this );
			string ret = EnumerateNodesAsString( enumerator, aIgnoreWhiteSpace, aRecurse );
			return ret.ToString();
		}

		public SymTokenContainer ExtractTokensAsContainer( bool aIgnoreWhiteSpace, bool aRecurse )
		{
			SymTokenContainer container = new SymTokenContainer();
			//
			SymNodeEnumeratorChildren enumerator = new SymNodeEnumeratorChildren( this );
			ExtractTokens( enumerator, aIgnoreWhiteSpace, aRecurse, container );
			//
			return container;
		}

		public SymTokenDocument ExtractTokensAsDocument( bool aIgnoreWhiteSpace, bool aRecurse )
		{
			SymTokenDocument doc = new SymTokenDocument();
			//
			SymNodeEnumeratorChildren enumerator = new SymNodeEnumeratorChildren( this );
			ExtractTokens( enumerator, aIgnoreWhiteSpace, aRecurse, doc );
			//
			return doc;
		}
		#endregion

		#region Utility methods
		public string EnumerateNodesAsString( IEnumerable aEnumerable, bool aIgnoreWhiteSpace, bool aRecurse )
		{
			// Flatten the tokens into a container
			SymTokenContainer container = new SymTokenContainer();
			ExtractTokens( aEnumerable, aIgnoreWhiteSpace, aRecurse, container );

			// Convert the container to a string
			string ret = container.CoalescedTokenValue;
			return ret;
		}

		public void ExtractTokens( IEnumerable aEnumerable, bool aIgnoreWhiteSpace, bool aRecurse, SymTokenContainer aContainer )
		{
			foreach( SymNode node in aEnumerable )
			{
				if	( node.HasChildren )
				{
					if ( aRecurse )
					{
						ExtractTokens( node, aIgnoreWhiteSpace, aRecurse, aContainer );
					}
					else
					{
						// Ignore - its just a placeholder for child nodes
					}
				}
				else if ( node is SymNodeToken )
				{
					SymNodeToken tokenNode = (SymNodeToken) node;
					if	( !( aIgnoreWhiteSpace && tokenNode.Token.Class == SymToken.TClass.EClassWhiteSpace ) || aIgnoreWhiteSpace == false )
					{
						aContainer.Append( tokenNode.Token );
					}
				}
				else if ( NodeIsExtractable( node ) )
				{
					ExtractToContainer( node, aContainer );
				}
			}
		}

		public void ExtractTokens( IEnumerable aEnumerable, bool aIgnoreWhiteSpace, bool aRecurse, SymTokenDocument aDocument )
		{
			foreach( SymNode node in aEnumerable )
			{
				if	( node.HasChildren )
				{
					if ( aRecurse )
					{
						SymNode newLevelNode = new SymNodeAddAsChild();
						aDocument.CurrentNode.Add( newLevelNode );
						aDocument.CurrentNode = newLevelNode;
						ExtractTokens( node, aIgnoreWhiteSpace, aRecurse, aDocument );
						aDocument.MakeParentCurrent();
					}
					else
					{
						// Ignore - its just a placeholder for child nodes and we're not recursing
					}
				}
				else if ( node is SymNodeToken )
				{
					SymNodeToken tokenNode = (SymNodeToken) node;
					if	( !( aIgnoreWhiteSpace && tokenNode.Token.Class == SymToken.TClass.EClassWhiteSpace ) || aIgnoreWhiteSpace == false )
					{
						SymNodeToken copy = new SymNodeToken( tokenNode.Token );
						aDocument.CurrentNode.Add( copy );
					}
				}
				else if ( NodeIsExtractable( node ) )
				{
					ExtractToDocument( node, aDocument );
				}
			}
		}
		#endregion

		#region Internal framework API
		protected virtual void ExtractToContainer( SymNode aNode, SymTokenContainer aContainer )
		{
		}

		protected virtual void ExtractToDocument( SymNode aNode, SymTokenDocument aDocument )
		{
		}

		protected virtual bool NodeIsExtractable( SymNode aNode )
		{
			return false;
		}
		#endregion
	}
}
