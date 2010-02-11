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
using System.IO;
using System.Collections.Generic;

namespace CrashAnalyserServerExe.Engine
{
    internal class CACmdLineFSEntityList<T> : IComparer<T>, IEnumerable<T> where T : CACmdLineFSEntity, new()
	{
        #region Constructors
        public CACmdLineFSEntityList()
		{
		}
        #endregion

		#region API
        public void Add( FileInfo aFile )
        {
            T entry = new T();
            entry.File = aFile;
            //
            AddInSortedOrder( entry );
        }

        public void Add( DirectoryInfo aDir )
        {
            T entry = new T();
            entry.Directory = aDir;
            //
            AddInSortedOrder( entry );
        }

        public void AddRange( FileInfo[] aFiles )
        {
            foreach ( FileInfo file in aFiles )
            {
                Add( file );
            }
        }

        public bool Contains( string aFileName )
        {
            CACmdLineFSEntity ret = this[ aFileName ]; 
            return ret != null;
        }

        public void AddToAll( CACmdLineMessage aMessage )
        {
            foreach ( CACmdLineFSEntity file in iFiles )
            {
                file.Add( aMessage );
            }
        }

        public T[] ToArray()
        {
            return iFiles.ToArray();
        }
        #endregion

		#region Properties
        public int Count
        {
            get { return iFiles.Count; }
        }

        public T this[ int aIndex ]
        {
            get { return iFiles[ aIndex ]; }
        }

        public T this[ string aFileName ]
        {
            get
            {
                T temp = new T();
                temp.File = new FileInfo( aFileName );
                //
                int pos = iFiles.BinarySearch( temp, this );
                //
                T ret = null;
                if ( pos >= 0 )
                {
                    ret = iFiles[ pos ];
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        private void AddInSortedOrder( T aEntry )
        {
            string fileName = aEntry.NameUC;
            //
            int pos = iFiles.BinarySearch( aEntry, this );
            if ( pos < 0 )
            {
                pos = ~pos;
                iFiles.Insert( pos, aEntry );
            }
            else
            {
                throw new ArgumentException( "Specified file already exists: " + aEntry );
            }
        }
        #endregion

        #region Operators
        public static implicit operator string[]( CACmdLineFSEntityList<T> aList )
        {
            List<string> ret = new List<string>();
            foreach ( T file in aList )
            {
                ret.Add( file.Name );
            }
            return ret.ToArray();
        }
        #endregion

        #region From IEnumerable<T>
        public IEnumerator<T> GetEnumerator()
        {
            foreach ( T file in iFiles )
            {
                yield return file;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( T file in iFiles )
            {
                yield return file;
            }
        }
        #endregion

        #region From IComparer<T>
        public int Compare( T aLeft, T aRight )
        {
            int ret = aLeft.NameUC.CompareTo( aRight.NameUC );
            return ret;
        }
        #endregion

        #region Data members
        private List<T> iFiles = new List<T>();
        #endregion
    }
}
