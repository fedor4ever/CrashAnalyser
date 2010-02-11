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
using System.Collections.Generic;
using SymbianUtils.Range;

namespace SymbianUtils.Range
{
	public class AddressRangeCollection : IComparer<AddressRange>
	{
		#region Constructors
        public AddressRangeCollection()
        {

        }

        public AddressRangeCollection( IEnumerable<AddressRange> aList )
		{
            foreach ( AddressRange entry in aList )
            {
                Add( entry );
            }
		}
        #endregion

        #region API
        public void Add( AddressRange aRange )
        {
            int pos = iRange.BinarySearch( aRange, this );

            // If not already added...
            if ( pos < 0 )
            {
                pos = ~pos;

                int prevItemPos = pos - 1;
                if ( prevItemPos >= 0 )
                {
                    AddressRange last = iRange[ pos - 1 ];
                    if ( last.Max + 1 == aRange.Min )
                    {
                        last.UpdateMax( aRange.Max );
                    }
                    else
                    {
                        iRange.Insert( pos, aRange );
                    }
                }
                else
                {
                    iRange.Insert( pos, aRange );
                }
            }
        }

        public bool Contains( uint aValue )
        {
            bool ret = false;
            //
            AddressRange temp = new AddressRange( aValue, aValue );
            int pos = NearestIndexOf( temp );
            if ( pos >= 0 )
            {
                ret = true;
            }
            //
            return ret;
        }

        public void Clear()
        {
            iRange.Clear();
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iRange.Count; }
        }

        public AddressRange this[ int aIndex ]
        {
            get { return iRange[ aIndex ]; }
        }

        public AddressRange RangeFirst
        {
            get
            {
                AddressRange ret = null;
                //
                if ( iRange.Count > 0 )
                {
                    ret = iRange[ 0 ];
                }
                //
                return ret;
            }
        }

        public AddressRange RangeLast
        {
            get
            {
                AddressRange ret = null;
                //
                int count = iRange.Count;
                if ( count > 0 )
                {
                    ret = iRange[ count - 1 ];
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        private int NearestIndexOf( AddressRange aRange )
        {
            int pos = iRange.BinarySearch( aRange, this );
            return pos;
        }
        #endregion

        #region From IComparer<AddressRange>
        public int Compare( AddressRange aLeft, AddressRange aRight )
        {
            int ret = -1;
            //
            if ( aLeft.Min > aRight.Max )
            {
                ret = 1;
            }
            else if ( aLeft.Contains( aRight ) || aRight.Contains( aLeft ) )
            {
                ret = 0;
            }
            //
            return ret;
        }
        #endregion

        #region Data members
        private List<AddressRange> iRange = new List<AddressRange>();
        #endregion
    }
}
