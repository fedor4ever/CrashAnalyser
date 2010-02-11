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
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;

namespace SymbianUtils
{
	public class AsyncTextReaderFilter
	{
		#region Enumerations
		public enum TSpecificMatchType
		{
			ESpecificMustMatch = 0,
			ESpecificMustNotMatch
		}

		public enum TRemoveType
		{
			ERemoveNothing = 0,
			ERemoveAllInstances,
			ERemoveLastInstanceOnwards,
			ERemoveLastInstanceStartingAtPreviousSpaceOnwards
		}
		#endregion

		#region Construct & destruct
		public AsyncTextReaderFilter( string aMatchString )
			:   this( aMatchString, TRemoveType.ERemoveAllInstances )
		{
		}

		public AsyncTextReaderFilter( string aMatchString, TRemoveType aRemoveType )
			:	this( aMatchString, aRemoveType, TSpecificMatchType.ESpecificMustMatch )
		{
		}

		public AsyncTextReaderFilter( string aMatchString, TRemoveType aRemoveType, TSpecificMatchType aMatchType )
		{
			iMatchString = aMatchString;
			iRemoveType = aRemoveType;
			iMatchType = aMatchType;
		}
		#endregion

		#region Properties
		public string MatchString
		{
			get { return iMatchString; }
			set { iMatchString = value; }
		}

		public TRemoveType RemoveType
		{
			get { return iRemoveType; }
			set { iRemoveType = value; }
		}

		public TSpecificMatchType MatchType
		{
			get { return iMatchType; }
			set { iMatchType = value; }
		}
		#endregion

		#region Internal API
		internal bool Matches( string aLine )
		{
			int index = aLine.IndexOf( MatchString );
			//
			bool matchedFilter = false;
			if	( index >= 0 )
			{
				matchedFilter = ( MatchType == TSpecificMatchType.ESpecificMustMatch );
			}
			else if ( index < 0 )
			{
				matchedFilter = ( MatchType == TSpecificMatchType.ESpecificMustNotMatch );
			}
			//
			return matchedFilter;
		}

		internal void Process( ref string aLine )
		{
			int index = aLine.IndexOf( MatchString );
			//
			if	( index >= 0 )
			{
				switch( RemoveType )
				{
					default:
					case TRemoveType.ERemoveNothing:
					{
						break;
					}
					case TRemoveType.ERemoveAllInstances:
					{
						aLine = aLine.Replace( MatchString, "" );
						break;
					}
					case TRemoveType.ERemoveLastInstanceOnwards:
					{
						index = aLine.LastIndexOf( MatchString );
						aLine = aLine.Substring( 0, index );
						break;
					}
					case TRemoveType.ERemoveLastInstanceStartingAtPreviousSpaceOnwards:
					{
						int lastSpacePos = aLine.LastIndexOf( ' ', index );
						if	( lastSpacePos < index )
						{
							aLine = aLine.Substring( 0, lastSpacePos );
						}
						else
						{
							aLine = aLine.Substring( 0, index );
						}
						break;
					}
				}
			}
		}
		#endregion

		#region Data members
		private string iMatchString = string.Empty;
		private TRemoveType iRemoveType = TRemoveType.ERemoveLastInstanceOnwards;
		private TSpecificMatchType iMatchType = TSpecificMatchType.ESpecificMustMatch;
		#endregion
	}
}
