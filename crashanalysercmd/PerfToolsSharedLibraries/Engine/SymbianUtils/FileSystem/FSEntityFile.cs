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
    public class FSEntityFile : FSEntity
    {
        #region Constructors
        public FSEntityFile( FileInfo aFile )
        {
            iFile = aFile;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public override string FullName
        {
            get
            { 
                StringBuilder ret = new StringBuilder();
                //
                if ( IsValid )
                {
                    ret.Append( File.FullName );
                }
                //
                return ret.ToString(); 
            }
        }

        public override bool Exists
        {
            get
            {
                bool ret = false;
                //
                if ( IsValid )
                {
                    ret = iFile.Exists;
                }
                //
                return ret;
            }
        }

        public override bool IsValid
        {
            get { return iFile != null; }
        }

        public FileInfo File
        {
            get { return iFile; }
        }
        #endregion

        #region Operators
        public static implicit operator FileInfo( FSEntityFile aEntity )
        {
            return aEntity.File;
        }
        #endregion

        #region Internal properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private FileInfo iFile;
        #endregion
    }
}
