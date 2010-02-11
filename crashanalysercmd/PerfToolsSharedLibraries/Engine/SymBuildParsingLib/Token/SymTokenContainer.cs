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

namespace SymBuildParsingLib.Token
{
	public class SymTokenContainer : IEnumerable
	{
		#region Observer interface
		public delegate void TokenHandler( SymToken aNewToken );
		#endregion

		#region Events
		public event TokenHandler iTokenHandlers;
		#endregion

		#region Constructors & destructor
		public SymTokenContainer()
		{
		}

		public SymTokenContainer( SymTokenContainer aCopy )
		{
			foreach( SymToken t in aCopy )
			{
				Append( t );
			}
		}
		#endregion

		#region API
		public void Append( SymToken aToken )
		{
			iTokens.Add( aToken );
			//
			if	( iTokenHandlers != null )
			{
				iTokenHandlers( aToken );
			}
		}

		public SymToken PopTail()
		{
			System.Diagnostics.Debug.Assert( Count > 0 );
			SymToken ret = PeekTail;
			iTokens.RemoveAt( iTokens.Count - 1 );
			return ret;
		}

		public SymToken PopHead()
		{
			System.Diagnostics.Debug.Assert( Count > 0 );
			SymToken ret = PeekHead;
			iTokens.RemoveAt( 0 );
			return ret;
		}

		public void Reset()
		{
			iTokens.Clear();
		}

		public void Remove( int aIndex )
		{
			iTokens.RemoveAt( aIndex );
		}
		#endregion

		#region Query API
		public bool IsPresent( SymToken aToken )
		{
			int index = IndexOf( aToken );
			return ( index >= 0 && index < Count );
		}

		public int IndexOf( SymToken aToken )
		{
			int index = iTokens.IndexOf( aToken );
			return index;
		}

		public int IndexOf( SymToken aToken, int aStartIndex )
		{
			int index = iTokens.IndexOf( aToken, aStartIndex );
			return index;
		}

		public int LastIndexOf( SymToken aToken )
		{
			int index = iTokens.LastIndexOf( aToken );
			return index;
		}

		public int LastIndexOf( SymToken aToken, int aStartIndex )
		{
			int index = iTokens.LastIndexOf( aToken, aStartIndex );
			return index;
		}

		public int TokenCount( SymToken aTokenToCount )
		{
			int count = 0;
			//
			foreach( SymToken t in this )
			{
				if	( t.Equals( aTokenToCount ) )
				{
					++count;
				}
			}
			//
			return count;
		}
		#endregion

		#region Properties
		public SymToken PeekHead
		{
			get
			{
				// System.Diagnostics.Debug.Assert( Count > 0 ); - bloody visual debugger stuffs this assert up.
				SymToken ret = null;
				//
				if	( Count > 0 )
				{
					ret = (SymToken) iTokens[ 0 ];
				}
				//
				return ret;
			}
		}

		public SymToken PeekTail
		{
			get
			{
				// System.Diagnostics.Debug.Assert( Count > 0 ); - bloody visual debugger stuffs this assert up.
				SymToken ret = null;
				//
				if	( Count > 0 )
				{
					ret = (SymToken) iTokens[ iTokens.Count - 1 ];
				}
				//
				return ret;
			}
		}

		public int Count
		{
			get { return iTokens.Count; }
		}

		public string CoalescedTokenValue
		{
			get
			{
				StringBuilder ret = new StringBuilder();
				//
				foreach( SymToken token in this )
				{
					ret.Append( token );
				}
				//
				return ret.ToString();
			}
		}
		#endregion

		#region IEnumerable Members
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new SymTokenContainerEnumerator( this );
		}
		#endregion

		#region Indexers
		public SymToken this[ int aIndex ]
		{
			get
			{
				return (SymToken) iTokens[ aIndex ];
			}
		}
		#endregion

		#region Data members
		private ArrayList iTokens = new ArrayList( 250 );
		#endregion
	}
}
