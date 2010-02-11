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
using SymbianTree;

#region Example layout
//
// SymNodeConditionalExpression
//     |
//     +----- [if] SymNodeCondition [neg]
//     +---------- { 
//     +---------- do something
//     +---------- [if] SymNodeCondition [pos, but inherits neg]
//     +--------------- { 
//     +--------------- do something else
//     +--------------- } 
//     +---------- } 
//     |
//     +----- [else if] SymNodeCondition [neg]
//     +---------- { 
//     +---------- do something else
//     +---------- } 
//     |
//     +----- [else if] SymNodeCondition [pos]
//     +---------- { 
//     +---------- do something else
//     +---------- } 
//     |
//     +----- [else] SymNodeCondition [neg]
//     +---------- { 
//     +---------- do something else
//     +---------- } 
//
#endregion

namespace SymBuildParsingLib.Parser.Framework.Nodes
{
	public class SymNodeConditionalExpression : SymNodeToken
	{
		#region Constructors & destructor
		public SymNodeConditionalExpression( SymToken aType )
			: base( aType )
		{
		}
		#endregion

		#region Properties
		public bool InheritedValue
		{
			get { return iInheritedValue; }
			set { iInheritedValue = value; }
		}

		public bool HasPositiveBranch
		{
			get
			{
				bool found = ( PositiveBranch != null );
				return found;
			}
		}

		public SymNodeCondition PositiveBranch
		{
			get
			{
				SymNodeCondition found = null;
				//
				foreach( SymNode c in this )
				{
					if	( c is SymNodeCondition )
					{
						SymNodeCondition cond = (SymNodeCondition) c;
						if	( cond.IsEvaluated && cond.EvaluationResult == true )
						{
							found = cond;
							break;
						}
					}
				}
				//
				return found;
			}
		}

		public bool HasCondition
		{
			get
			{
				int count = ChildCountByType( typeof(SymNodeCondition) );
				return count > 0;
			}
		}

		public SymNodeCondition CurrentCondition
		{
			get
			{
				SymNodeCondition ret = (SymNodeCondition) LastChild;
				return ret;
			}
		}
		#endregion

		#region Data members
		private bool iInheritedValue;
		#endregion
	}
}
