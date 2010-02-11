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
using System.Text;
using SymbianUtils;

namespace SymbianUtils.FileTypes
{
    public class SymFileTypeList : IEnumerable<SymFileType>
    {
        #region Constructors
        public SymFileTypeList()
        {
        }
        #endregion

        #region API
        public void Clear()
        {
            iTypes.Clear();
        }

        public void Add( SymFileType aType )
        {
            if ( !iTypes.ContainsKey( aType.Extension ) )
            {
                iTypes.Add( aType.Extension.ToUpper(), aType );
            }
        }

        public void AddRange( IEnumerable<SymFileType> aTypes )
        {
            foreach ( SymFileType type in aTypes )
            {
                Add( type );
            }
        }

        public bool IsSupported( string aFileName )
        {
            string ext = Path.GetExtension( aFileName );
            bool ret = iTypes.ContainsKey( ext.ToUpper() );
            return ret;
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iTypes.Count; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            // Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*
            StringBuilder ret = new StringBuilder();
            //
            foreach ( KeyValuePair<string, SymFileType> kvp in iTypes )
            {
                string extension = kvp.Value.ToString();
                ret.Append( "|" );
            }

            // Remove last | if present
            int length = ret.Length;
            if ( length > 0 )
            {
                if ( ret[ length - 1 ] == '|' )
                {
                    ret.Length = length - 1; 
                }
            }
            //
            return ret.ToString();
        }
        #endregion

        #region From IEnumerable<SymFileType>
        public IEnumerator<SymFileType> GetEnumerator()
        {
            foreach ( KeyValuePair<string,SymFileType> type in iTypes )
            {
                yield return type.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( KeyValuePair<string, SymFileType> type in iTypes )
            {
                yield return type.Value;
            }
        }
        #endregion

        #region Data members
        private Dictionary<string, SymFileType> iTypes = new Dictionary<string, SymFileType>();
        #endregion
    }
}
