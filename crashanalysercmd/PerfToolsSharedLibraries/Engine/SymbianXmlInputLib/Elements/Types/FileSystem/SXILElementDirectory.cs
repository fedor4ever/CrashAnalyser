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
using System.Xml;
using System.Text;
using System.Collections.Generic;

namespace SymbianXmlInputLib.Elements.Types.FileSystem
{
    public class SXILElementDirectory : SXILElementFileSystem
    {
        #region Constructors
        internal SXILElementDirectory( DirectoryInfo aDir )
        {
            iDirectory = aDir;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public override bool Exists
        {
            get { return iDirectory.Exists; }
        }

        public override string Name
        {
            get { return iDirectory.FullName; }
            set { iDirectory = new DirectoryInfo( value ); }
        }

        public DirectoryInfo Directory
        {
            get { return iDirectory; }
        }

        public FileInfo[] Files
        {
            get { return iDirectory.GetFiles(); }
        }
        #endregion

        #region Operators
        public static implicit operator DirectoryInfo( SXILElementDirectory aElement )
        {
            return aElement.iDirectory;
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private DirectoryInfo iDirectory;
        #endregion
    }
}
