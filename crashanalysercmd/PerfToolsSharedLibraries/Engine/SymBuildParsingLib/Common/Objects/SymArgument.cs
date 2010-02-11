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

namespace SymBuildParsingLib.Common.Objects
{
	public class SymArgumentSubLevel : SymNodeAddAsChild
	{
		#region Constructors & destructor
		public SymArgumentSubLevel( SymNode aCopyFrom )
		{
			AppendChildrenFrom( aCopyFrom );
		}
		#endregion
	}

	public class SymArgument : SymDocument
	{
		#region Constructors & destructor
		public SymArgument()
		{
		}
		#endregion

		#region API
		#endregion

		#region Properties
		public string CoalescedTokenValue
		{
			get
			{
				StringBuilder ret = new StringBuilder();
				//
				BuildRecursiveTokenValueString( this, ref ret );
				//
				return ret.ToString();
			}
		}
		#endregion

		#region Internal methods
		private void BuildRecursiveTokenValueString( SymNode aNode, ref StringBuilder aString )
		{
			if	( aNode is SymNodeToken )
			{
				SymNodeToken tokenNode = (SymNodeToken) aNode;
				aString.Append( tokenNode.Token );
			}
			//
			foreach( SymNode child in aNode )
			{
				BuildRecursiveTokenValueString( child, ref aString );
			}
		}
		#endregion

		#region Data members
		#endregion
	}
}
