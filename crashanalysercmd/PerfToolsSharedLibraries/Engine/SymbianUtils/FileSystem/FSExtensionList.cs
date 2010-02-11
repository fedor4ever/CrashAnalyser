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
using System.Text;
using System.IO;

namespace SymbianUtils.FileSystem
{
    public class FSExtensionList : IEnumerable<FSExtensionDescriptor>
    {
        #region Constructors
        public FSExtensionList()
        {
        }
        #endregion

        #region API
        public void Clear()
        {
            iEntries.Clear();
        }

        public void Add( string aExtension, string aDescription )
        {
            Add( new FSExtensionDescriptor( aExtension, aDescription ) );
        }

        public void Add( FSExtensionDescriptor aEntry )
        {
            if ( !Contains( aEntry ) )
            {
                iEntries.Add( aEntry.GetHashCode(), aEntry );
            }
        }

        public void AddRange( IEnumerable<FSExtensionDescriptor> aEntries )
        {
            foreach ( FSExtensionDescriptor entry in aEntries )
            {
                Add( entry );
            }
        }

        public bool Contains( string aExtension )
        {
            bool ret = false;
            //
            foreach ( FSExtensionDescriptor entry in this )
            {
                if ( entry.ContainsExtension( aExtension ) )
                {
                    ret = true;
                    break;
                }
            }
            //
            return ret;
        }

        public bool Contains( FSExtensionDescriptor aEntry )
        {
            bool ret = iEntries.ContainsKey( aEntry.GetHashCode() );
            return ret;
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iEntries.Count; }
        }

        public FSExtensionDescriptor this[ int aIndex ]
        {
            get { return iEntries[ aIndex ]; }
        }

        public FSExtensionDescriptor this[ string aExtension ]
        {
            get
            {
                FSExtensionDescriptor ret = null;
                //
                if ( Contains( aExtension ) )
                {
                    FSExtensionDescriptor temp = new FSExtensionDescriptor( aExtension );
                    ret = iEntries[ temp.GetHashCode() ];
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From System.Object
        public override string ToString()
        {
            // Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*
            StringBuilder ret = new StringBuilder();

            foreach ( FSExtensionDescriptor ext in this )
            {
                string extString = ext.ToString();
                ret.AppendFormat( "{0};", extString );
            }

            // Add "all files"
            ret.Append( "All Files (*.*)|*.*" );

            return ret.ToString();
        }
        #endregion

        #region From IEnumerable<FSExtensionDescriptor>
        public IEnumerator<FSExtensionDescriptor> GetEnumerator()
        {
            foreach ( KeyValuePair<int, FSExtensionDescriptor> kvp in iEntries )
            {
                yield return kvp.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( KeyValuePair<int, FSExtensionDescriptor> kvp in iEntries )
            {
                yield return kvp.Value;
            }
        }
        #endregion

        #region Data members
        private SortedList<int, FSExtensionDescriptor> iEntries = new SortedList<int, FSExtensionDescriptor>();
        #endregion
    }
}
