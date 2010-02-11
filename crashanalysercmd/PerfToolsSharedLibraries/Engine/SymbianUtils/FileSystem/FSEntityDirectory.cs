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
    public class FSEntityDirectory : FSEntity
    {
        #region Constructors
        public FSEntityDirectory( DirectoryInfo aDirectory )
        {
            iDirectory = aDirectory;
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
                    ret.Append( Directory.FullName );
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
                    ret = Directory.Exists;
                }
                //
                return ret;
            }
        }

        public override bool IsValid
        {
            get { return iDirectory != null; }
        }

        public DirectoryInfo Directory
        {
            get { return iDirectory; }
        }
        #endregion

        #region Operators
        public static implicit operator DirectoryInfo( FSEntityDirectory aEntity )
        {
            return aEntity.Directory;
        }
        #endregion

        #region Internal properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private DirectoryInfo iDirectory;
        #endregion
    }
}
