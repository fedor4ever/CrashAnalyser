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
using System.Collections.Generic;

namespace SymbianUtils.FileSystem.FilePair
{
	public class FileNamePairCollection : IEnumerable<FileNamePair>
	{
		#region Constructor
        public FileNamePairCollection()
		{
		}
		#endregion

		#region API
        public void Clear()
        {
            iList.Clear();
        }

        public void Add( IEnumerable<FileNamePair> aList )
        {
            foreach ( FileNamePair entry in aList )
            {
                Add( entry );
            }
        }

        public void Add( FileNamePair aItem )
        {
            iList.Add( aItem );
        }

        public void RemoveAt( int aIndex )
        {
            iList.RemoveAt( aIndex );
        }

        public bool Contains( FileNamePair aItem )
        {
            bool exists = false;
            //
            foreach ( FileNamePair entry in iList )
            {
                if ( entry.Source.ToLower() == aItem.Source.ToLower() )
                {
                    exists = true;
                    break;
                }
                else if ( entry.Destination.ToLower() == aItem.Destination.ToLower() )
                {
                    exists = true;
                    break;
                }
            }
            //
            return exists;
        }
		#endregion

        #region Properties
        public int Count
		{
			get { return iList.Count; }
		}

        public FileNamePair this[ int aIndex ]
		{
			get
			{
                FileNamePair item = (FileNamePair) iList[ aIndex ];
				return item;
			}
		}
		#endregion

        #region From IEnumerable
        IEnumerator<FileNamePair> IEnumerable<FileNamePair>.GetEnumerator()
        {
            foreach ( FileNamePair entry in iList )
            {
                yield return entry;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( FileNamePair entry in iList )
            {
                yield return entry;
            }
        }
        #endregion

		#region Data members
        private List<FileNamePair> iList = new List<FileNamePair>();
		#endregion
    }
}
