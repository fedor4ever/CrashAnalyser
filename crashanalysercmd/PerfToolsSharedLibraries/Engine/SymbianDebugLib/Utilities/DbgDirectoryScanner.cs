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
using System.ComponentModel;
using SymbianUtils.FileSystem;
using SymbianDebugLib.Engine;
using SymbianDebugLib.Entity;

namespace SymbianDebugLib.Utilities
{
    public class DbgDirectoryScanner : FSDirectoryScanner
    {
        #region Constructors
        public DbgDirectoryScanner( DbgEngine aEngine )
        {
            iEngine = aEngine;
        }
        #endregion

        #region From FSDirectoryScanner
        protected override void OnFileLocated( FileInfo aFile )
        {
            base.OnFileLocated( aFile );
            //
            DbgEntity entity = iEngine.Add( aFile );
            if ( entity != null )
            {
                // Files found this way are not explict
                entity.WasAddedExplicitly = false;
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly DbgEngine iEngine;
        #endregion
    }
}
