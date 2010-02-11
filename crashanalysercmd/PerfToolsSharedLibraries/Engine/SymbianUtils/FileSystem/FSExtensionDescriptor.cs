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
    public class FSExtensionDescriptor
    {
        #region Constructors
        public FSExtensionDescriptor( string aDescription )
            : this( aDescription, string.Empty )
        {
        }

        public FSExtensionDescriptor( string aDescription, string aExtension )
        {
            iDescription = aDescription.Trim();
            Add( aExtension );
        }

        public FSExtensionDescriptor( string aDescription, IEnumerable<string> aExtensions )
        {
            iDescription = aDescription.Trim();
            AddRange( aExtensions );
        }

        public FSExtensionDescriptor( string aDescription, params string[] aExtensions )
        {
            iDescription = aDescription.Trim();
            AddRange( aExtensions );
        }
        #endregion

        #region API
        public void Add( string aExtension )
        {
            if ( !string.IsNullOrEmpty( aExtension ) )
            {
                string ext = aExtension.Trim();
                if ( !string.IsNullOrEmpty( ext ) )
                {
                    iExtensions.Add( ext );
                }
            }
        }

        public void AddRange( IEnumerable<string> aExtensions )
        {
            foreach ( string e in aExtensions )
            {
                Add( e );
            }
        }

        public bool ContainsExtension( string aExtension )
        {
            string searchFor = aExtension.ToUpper();
            Predicate<string> searcher = delegate( string aEntry )
            {
                return ( aEntry.ToUpper() == searchFor );
            };
            //
            string ret = iExtensions.Find( searcher );
            bool found = !string.IsNullOrEmpty( ret );
            return found;
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iExtensions.Count; }
        }

        public string FirstExtension
        {
            get
            {
                string ret = string.Empty;
                //
                if ( Count > 0 )
                {
                    ret = this[ 0 ];
                }
                //
                return ret;
            }
        }

        public string[] Extensions
        {
            get { return iExtensions.ToArray(); }
        }

        public string Description
        {
            get { return iDescription; }
        }

        public string this[ int aIndex ]
        {
            get { return iExtensions[ aIndex ]; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From System.Object
        public override int GetHashCode()
        {
            return iDescription.GetHashCode();
        }

        public override string ToString()
        {
            // Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*
            StringBuilder ret = new StringBuilder( Description );
            //
            if ( ret.Length > 0 && iExtensions.Count > 0 )
            {
                StringBuilder extList = new StringBuilder();

                foreach ( string ext in iExtensions )
                {
                    extList.Append( ext );
                    extList.Append( ";" );
                }

                // Strip final ";".
                extList.Length = extList.Length - 1;

                // Save as formatted extension
                string list = extList.ToString();
                ret.AppendFormat( " ({0})|{1}", list, list );
            }
            //
            return ret.ToString();
        }
        #endregion

        #region Data members
        private readonly string iDescription;
        private List<string> iExtensions = new List<string>();
        #endregion
    }
}
