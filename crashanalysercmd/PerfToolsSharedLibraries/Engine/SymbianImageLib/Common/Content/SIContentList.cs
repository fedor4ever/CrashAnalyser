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
using System.Collections.ObjectModel;
using System.Text;

namespace SymbianImageLib.Common.Content
{
    public class SIContentList : IEnumerable<SIContent>
    {
        #region Constructors
        public SIContentList()
        {
        }
        #endregion

        #region API
        public bool TryToGetFile( string aFileName, out SIContent aFile )
        {
            aFile = null;
            bool ret = iList.Contains( aFileName );
            if ( ret )
            {
                aFile = iList[ aFileName ];
            }
            return ret;
        }

        public bool Contains( SIContent aFile )
        {
            return iList.Contains( aFile );
        }

        public void Add( SIContent aFile )
        {
            if ( iList.Contains( aFile ) )
            {
                throw new ArgumentException( "Specified file is already part of the list" );
            }
            iList.Add( aFile );
        }

        public void Remove( SIContent aFile )
        {
            if ( iList.Contains( aFile ) )
            {
                iList.Remove( aFile );
            }
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iList.Count; }
        }

        public SIContent this[ int aIndex ]
        {
            get { return iList[ aIndex ]; }
        }

        public SIContent this[ string aFileName ]
        {
            get
            {
                string key = aFileName.ToUpper();
                return iList[ key ];
            }
        }
        #endregion

        #region Internal class
        private class FileList : KeyedCollection<string, SIContent>
        {
            #region Constructors
            public FileList()
            {
            }
            #endregion

            #region From KeyedCollection
            protected override string GetKeyForItem( SIContent aItem )
            {
                return aItem.FileName.ToUpper();
            }
            #endregion
        }
        #endregion

        #region From IEnumerable<SymbianImageContentFile>
        public IEnumerator<SIContent> GetEnumerator()
        {
            foreach ( SIContent file in iList )
            {
                yield return file;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( SIContent file in iList )
            {
                yield return file;
            }
        }
        #endregion

        #region Data members
        private FileList iList = new FileList();
        #endregion
    }
}
