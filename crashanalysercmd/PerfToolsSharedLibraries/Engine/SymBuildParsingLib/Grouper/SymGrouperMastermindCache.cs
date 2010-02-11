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
using System.Threading;
using System.Collections;
using SymBuildParsingLib.Token;

namespace SymBuildParsingLib.Grouper
{
	public class SymGrouperMastermindCache : SymTokenContainer
	{
		#region Constructors & destructor
		public SymGrouperMastermindCache()
		{
		}
		#endregion

		#region API
		public int CountByType( SymToken aToken )
		{
			int count = 0;
			//
			foreach( SymToken token in this )
			{
				if	( aToken.Class == token.Class &&
					  aToken.Type == token.Type &&
					  aToken.Value == token.Value )
				{
					++count;
				}
			}
			//
			return count;
		}

		public bool CheckTokensAreOfClass( SymToken.TClass aClass )
		{
			bool tokensAreAllTheSameClass = CheckTokensAreOfClass( aClass, 0 );
			return tokensAreAllTheSameClass;
		}

		public bool CheckTokensAreOfClass( SymToken.TClass aClass, int aStartIndex )
		{
			bool tokensAreAllTheSameClass = true;
			//
			int count = Count;
			for( int i=aStartIndex; i<count; i++ )
			{
				SymToken token = this[ i ];
				//
				if	( token.Class != aClass )
				{
					tokensAreAllTheSameClass = false;
					break;
				}
			}
			//
			return tokensAreAllTheSameClass;
		}

		public bool CheckTokensAreOfEitherClass( SymToken.TClass aClass1, SymToken.TClass aClass2 )
		{
			bool tokensAreAllTheSameClass = CheckTokensAreOfEitherClass( aClass1, aClass2, 0 );
			return tokensAreAllTheSameClass;
		}

		public bool CheckTokensAreOfEitherClass( SymToken.TClass aClass1, SymToken.TClass aClass2, int aStartIndex )
		{
			bool tokensAreAllTheSameClass = true;
			//
			int count = Count;
			for( int i=aStartIndex; i<count; i++ )
			{
				SymToken token = this[ i ];
				//
				if	( !(token.Class == aClass1 || token.Class == aClass2 ) )
				{
					tokensAreAllTheSameClass = false;
					break;
				}
			}
			//
			return tokensAreAllTheSameClass;
		}

		public void MergeAllTokensWithinRange( int aStartIndex, int aEndIndex, bool aMergeInContinuations, bool aForceMerge )
		{
			int count = Count;
			//
			System.Diagnostics.Debug.Assert( count > aStartIndex );
			System.Diagnostics.Debug.Assert( aEndIndex < count );
			
			// Have to do this in two passes to ensure token
			// text remains from left to right.
			SymToken startingToken = this[ aStartIndex++ ];
			if	( aForceMerge == false )
			{
				// Not force-merging, so need to find a valid combinable starting element
				while( startingToken.CombiningAllowed == false && aStartIndex < aEndIndex )
				{
					startingToken = this[ ++aStartIndex ];
				}
			}

			// First pass - join tokens
			for( int i=aStartIndex; i<=aEndIndex; i++ )
			{
				SymToken thisToken = this[ i ];
				
				// Ignore continuations during merging
				if	( thisToken.Class != SymToken.TClass.EClassContinuation || aMergeInContinuations )
				{
					if	( aForceMerge == false )
					{
						startingToken.Combine( thisToken );
					}
					else
					{
						startingToken.ForceCombine( thisToken );
					}
				}
			}

			// Second pass - discard merged tokens.
			for( int i=aEndIndex-1; i>=aStartIndex; i-- )
			{
				Remove( i );
			}

			//System.Diagnostics.Debug.WriteLine( "Merged: " + startingToken.Value );
		}
		#endregion

		#region Data members
		#endregion
	}
}
