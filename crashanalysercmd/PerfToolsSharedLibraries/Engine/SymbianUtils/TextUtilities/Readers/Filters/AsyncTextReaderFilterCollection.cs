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
	public class AsyncTextReaderFilterCollection : IEnumerable
	{
		#region Enumerations
		public enum TCombinationType
		{
			ECombinationTypeUndefined = 0,
			ECombinationTypeOR,
			ECombinationTypeAND,
			ECombinationTypeNOT
		}
		#endregion

		#region Constructors
		public AsyncTextReaderFilterCollection()
		{
		}
		#endregion

		#region API - Filter processing
		public bool ProcessFilters( ref string aLine )
		{
			bool propagateLine = false;
			//
			if	( aLine.Length > 0 ) 
			{
				int matchCount = 0;
				int filterCount = Count;

				// First phase: check for filter matches
				for ( int i = 0; i < filterCount; i++ )
				{
					AsyncTextReaderFilter filter = this[ i ];
					//
					bool matchedFilter = filter.Matches( aLine );
					if ( matchedFilter )
					{
						++matchCount;
					}
				}

				// Second phase: check for overall match against combined
				// criteria.
				propagateLine = ( filterCount == 0 ); // Default to true when no filters
				switch ( CombinationType )
				{
					case TCombinationType.ECombinationTypeOR:
						propagateLine = ( matchCount > 0 );
						break;
					case TCombinationType.ECombinationTypeAND:
						propagateLine = ( matchCount == filterCount );
						break;
					case TCombinationType.ECombinationTypeNOT:
						propagateLine = ( matchCount == 0 );
						break;
					default:
					case TCombinationType.ECombinationTypeUndefined:
						break;
				}

				// Third phase: process filter
				if	( propagateLine )
				{
					for ( int i = 0; i < filterCount; i++ )
					{
						AsyncTextReaderFilter filter = this[ i ];
						filter.Process( ref aLine );
					}
				}
			}

			return propagateLine;
		}
		#endregion

		#region API - Filter management
		public void Add( AsyncTextReaderFilter aFilter )
		{
			iFilters.Add( aFilter );
		}

		public void RemoveAt( int aIndex )
		{
			iFilters.RemoveAt( aIndex );
		}

		public void Remove( AsyncTextReaderFilter aFilter )
		{
			iFilters.Remove( aFilter );
		}

		public void Clear()
		{
			iFilters.Clear();
		}

		#endregion

		#region Properties
		public int Count
		{
			get { return iFilters.Count; }
		}

		public AsyncTextReaderFilter this[ int aIndex ]
		{
			get { return (AsyncTextReaderFilter) iFilters[ aIndex ]; }
		}

		public TCombinationType CombinationType
		{
			get { return iCombinationType; }
			set { iCombinationType = value; }
		}
		#endregion

		#region IEnumerable Members
		public IEnumerator GetEnumerator()
		{
			return new AsyncTextReaderFilterCollectionEnumerator( this );
		}
		#endregion

		#region Data members
		private TCombinationType iCombinationType = TCombinationType.ECombinationTypeUndefined;
		private ArrayList iFilters = new ArrayList();
		#endregion
	}
}
