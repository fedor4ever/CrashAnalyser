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
using SymBuildParsingLib.Parser.Framework.Workers;
using SymbianTree;

namespace SymBuildParsingLib.Parser.Framework.Nodes
{
	public class SymNodeCondition : SymNodeAddAsChild
	{
		#region Enumerations
		public enum TType
		{
			ETypeUnknown = -1,
			ETypeIf = 0,
			ETypeIfdef,
			ETypeIfndef,
			ETypeElseIf,
			ETypeElse
		}
		#endregion

		#region Constructors & destructor
		public SymNodeCondition( TType aType )
		{
			iType = aType;
		}

		public SymNodeCondition( string aName )
			: this( SymNodeCondition.TypeByName( aName ) )
		{
			if	( Type == TType.ETypeUnknown )
			{
				throw new ArgumentException( "Invalid condition name: " + aName );
			}
		}
		#endregion

		#region API
		public void MakeConditionalArgumentCurrent( SymParserDocument aDocument )
		{
			SymNodeConditionalExpression condExpNode = SymParserWorkerConditionalExpression.FindMostRecentConditionalExpression( this );
			if	( condExpNode == null )
			{
				throw new Exception( "Unable to locate conditional expression. Document is corrupt" );
			}

			aDocument.CurrentNode = condExpNode;
		}

		static public TType TypeByName( string aName )
		{
			TType ret = TType.ETypeUnknown;
			//
			switch( aName )
			{
				case "if":
					ret = TType.ETypeIf;
					break;
				case "ifdef":
					ret = TType.ETypeIfdef;
					break;
				case "ifndef":
					ret = TType.ETypeIfndef;
					break;
				case "elif":
				case "else if":
					ret = TType.ETypeElseIf;
					break;
				case "else":
					ret = TType.ETypeElse;
					break;
				default:
					break;
			}
			//
			return ret;
		}

		public void AssignArgumentTokens( SymTokenBalancerDocument aDocument )
		{
			Data = aDocument;
		}

		public virtual void Evaluate( SymParserDocumentContext aContext )
		{
			IsEvaluated = false;

			// Evaluate the expression, taking into account the current #define'd 
			// values. Prepares a new document with these expressions evaluated.
			SymTokenDocument evalDoc = BalancedArguments.ExtractTokensAsDocument( false, true );
			string expression = evalDoc.ChildrenAsString( false, true );

			// Get evaluated result from evaluator
			iEvaluationResult = SymExpressionEvaluator.EvaluateAsBoolean( expression );

			IsEvaluated = true;
		}
		#endregion

		#region Properties
		public TType Type
		{
			get { return iType; }
		}

		public bool EvaluationResult
		{
			get { return iEvaluationResult; }
			set { iEvaluationResult = value; }
		}

		public bool IsEvaluated
		{
			get { return iIsEvaluated; }
			set { iIsEvaluated = value; }
		}

		public SymTokenBalancerDocument BalancedArguments
		{
			get { return (SymTokenBalancerDocument) Data; }
		}
		#endregion

		#region Data members
		private bool iIsEvaluated = false;
		private bool iEvaluationResult = true;
		private readonly TType iType;
		#endregion
	}
}
