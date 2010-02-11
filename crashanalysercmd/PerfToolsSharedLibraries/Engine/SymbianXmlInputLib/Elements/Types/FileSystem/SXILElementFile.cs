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
    public class SXILElementFile : SXILElementFileSystem
    {
        #region Constructors
        internal SXILElementFile( FileInfo aFile )
        {
            iFile = aFile;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public override bool Exists
        {
            get { return iFile.Exists; }
        }

        public override string Name
        {
            get { return iFile.FullName; }
            set { iFile = new FileInfo( value ); }
        }
        #endregion

        #region Operators
        public static implicit operator FileInfo( SXILElementFile aElement )
        {
            return aElement.iFile;
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private FileInfo iFile;
        #endregion
    }
}
