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
using System.Threading;
using SymbianUtils;
using SymbianUtils.PluginManager;
using SymbianUtils.FileSystem;
using SymbianUtils.Settings;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI;

namespace SymbianDebugLib.Entity.Primer
{
    internal class DbgEntityPrimerNull : IDbgEntityPrimer
    {
        #region Constructors
        public DbgEntityPrimerNull( DbgEntity aEntity )
        {
            iEntity = aEntity;
        }
        #endregion

        #region From IDbgEntityPrimer
        public void Prime( TSynchronicity aSynchronicity )
        {
            // Doesn't do anything
            iEntity.OnPrimeStart( this );
            iEntity.OnPrimeComplete( this );
        }

        public string PrimeErrorMessage
        {
            get { return string.Empty; }
        }

        public Exception PrimeException
        {
            get { return null; }
            internal set { }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly DbgEntity iEntity;
        #endregion
    }
}
