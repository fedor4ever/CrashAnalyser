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
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace SymbianUtils.RawItems
{
	public class RawItemCollection : IEnumerable<RawItem>
	{
		#region Constructors
		public RawItemCollection()
		{
		}
		#endregion

		#region API
        public void Clear()
        {
            iRawItems.Clear();
        }

        public void Add( RawItem aItem )
        {
            iRawItems.Add( aItem );
        }

        public int CountPadding( byte aPaddingByte )
        {
            int ret = 0;
            bool finished = false;
            //
            for ( int i = Count - 1; !finished && i >= 0; i-- )
            {
                RawItem item = this[ i ];
                long dataValue = item.OriginalData;
                //
                for ( int j = 0; j < 4; j++ )
                {
                    long mask = 0x000000FFL << ( j * 8 );
                    long value = ( mask & dataValue );
                    int shiftBy = ( j * 8 );
                    long valueClean = value >> shiftBy;
                    if ( valueClean == aPaddingByte )
                    {
                        ++ret;
                    }
                    else
                    {
                        finished = true;
                        break;
                    }
                }
            }
            //
            return ret;
        }

        public int PrintableCharacterCount( bool aUnicode, bool aIncludeFirstItem, out int aDotCount )
        {
            aDotCount = 0;
            int i = ( aIncludeFirstItem ) ? 0 : 1;
            //
            int ret = 0;
            for ( ; i < iRawItems.Count; i++ )
            {
                RawItem item = iRawItems[ i ];
                ret += item.PrintableCharacterCount( aUnicode, ref aDotCount );
            }
            //
            return ret;
        }

        public int PrintableCharacterCount( bool aUnicode, out int aDotCount )
        {
            int ret = 0;
            aDotCount = 0;
            //
            foreach ( RawItem item in iRawItems )
            {
                ret += item.PrintableCharacterCount( aUnicode, ref aDotCount );
            }
            //
            return ret;
        }
        #endregion

		#region Properties
        public string FirstLine
        {
            get
            {
                StringBuilder ret = new StringBuilder();
                //
                int count = Count;
                for ( int i = 0; i < 8; i += 8 )
                {
                    for ( int j = 0; j < 8 && i + j < count; j++ )
                    {
                        RawItem item = this[ i + j ];
                        ret.Append( item.OriginalCharacterisedData );
                    }
                }
                //
                return ret.ToString();
            }
        }
        
        public int Count
		{
			get { return iRawItems.Count; }
		}

        public RawItem this[ int aIndex ]
		{
            get
            {
                RawItem ret = iRawItems[ aIndex ];
                return ret;
            }
		}
		#endregion

		#region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
		{
			return new RawItemCollectionEnumerator( this );
		}

        IEnumerator<RawItem> IEnumerable<RawItem>.GetEnumerator()
        {
            return new RawItemCollectionEnumerator( this );
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            int count = Count;
            for ( int i = 0; i < count; i += 8 )
            {
                for ( int j = 0; j < 8 && i + j < count; j++ )
                {
                    RawItem item = this[ i + j ];
                    ret.Append( item.OriginalCharacterisedData );
                }
                ret.Append( System.Environment.NewLine );
            }
            //
            return ret.ToString();
        }
        #endregion

		#region Data members
        private List<RawItem> iRawItems = new List<RawItem>();
		#endregion
	}
}
