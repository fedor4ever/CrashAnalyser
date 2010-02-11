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
    public class FSDeletableResource : DisposableObject
    {
        #region Constructors
        public FSDeletableResource( string aFileName )
            : this( new FileInfo( aFileName ) )
        {
        }

        public FSDeletableResource( FileInfo aFile )
        {
            iFile = aFile;
        }
        #endregion

        #region Properties
        public FileInfo File
        {
            get { return iFile; }
        }

        public string FileName
        {
            get { return iFile.FullName; }
        }
        #endregion

        #region DisposableObject
        protected override void CleanupManagedResources()
        {
            try
            {
                base.CleanupManagedResources();
            }
            finally
            {
                if ( iFile != null )
                {
                    Utilities.FSUtilities.DeleteFile( iFile.FullName );
                }
                iFile = null;
            }
        }
        #endregion

        #region Data members
        private FileInfo iFile = null;
        #endregion
    }
}
