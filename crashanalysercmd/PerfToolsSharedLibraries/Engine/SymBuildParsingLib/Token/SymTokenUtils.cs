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
using SymbianTree;

namespace SymBuildParsingLib.Token
{
	public class SymTokenUtils
	{
		#region Trim functions
		public static SymNode TrimEntireTree( SymNode aNode )
		{
			return Trim( aNode, true );
		}

		public static SymNode Trim( SymNode aNode )
		{
			return Trim( aNode, false );
		}

		public static void RemoveWhiteSpace( SymNode aNode, bool aRecurse )
		{
			int count = aNode.ChildCount;
			for( int i=count-1; i>=0; i-- )
			{
				SymNode basicNode = aNode[ i ];

				// If the node is whitespace, then remove it
				if	( basicNode is SymNodeToken )
				{
					SymNodeToken tokenNode = (SymNodeToken) basicNode;
					bool isWhiteSpace = ( tokenNode.Token.Class == SymToken.TClass.EClassWhiteSpace );
					//
					if	( isWhiteSpace )
					{
						System.Diagnostics.Debug.Assert( basicNode.HasChildren == false );
						basicNode.Remove();
					}
				}

				// Remove whitespace from this node's children
				if	( basicNode.HasChildren && aRecurse )
				{
					RemoveWhiteSpace( basicNode, aRecurse );
				}
			}
		}
		#endregion

		#region Internal methods
		public static SymNode Trim( SymNode aNode, bool aRecurse )
		{
			// Forward pass
			while( aNode.HasChildren )
			{
				SymNode n = aNode.FirstChild;

				if	( n is SymNodeToken )
				{
					SymNodeToken nodeToken = (SymNodeToken) n;
					bool isWhiteSpace = ( nodeToken.Token.Class == SymToken.TClass.EClassWhiteSpace );
					//
					if	( isWhiteSpace )
					{
						System.Diagnostics.Debug.Assert( n.HasChildren == false );
						nodeToken.Remove();
					}
					else
					{
						break;
					}
				}
				else
				{
					break;
				}
			}
			
			// Backward pass
			while( aNode.HasChildren )
			{
				SymNode n = aNode.LastChild;

				if	( n is SymNodeToken )
				{
					SymNodeToken nodeToken = (SymNodeToken) n;
					bool isWhiteSpace = ( nodeToken.Token.Class == SymToken.TClass.EClassWhiteSpace );
					//
					if	( isWhiteSpace )
					{
						System.Diagnostics.Debug.Assert( n.HasChildren == false );
						nodeToken.Remove();
					}
					else
					{
						break;
					}
				}
				else
				{
					break;
				}
			}
		
			// Child pass
			if	( aRecurse )
			{
				foreach( SymNode n in aNode )
				{
					if	( n.HasChildren )
					{
						Trim( n, aRecurse );
					}
				}
			}

			return aNode;
		}
		#endregion
	}
}
