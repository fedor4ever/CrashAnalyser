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
using SymbianUtils.PluginManager;
using SymbianUtils.FileSystem;
using SymbianUtils.Settings;
using SymbianDebugLib.Engine;
using SymbianUtils;

namespace SymbianDebugLib.Entity.Primer
{
    public class DbgEntityPrimerResult
    {
        #region Constructors
        internal DbgEntityPrimerResult( DbgEntity aEntity )
        {
            iEntity = aEntity;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public bool PrimeAttempted
        {
            get { return iPrimeAttempted; }
            internal set { iPrimeAttempted = value; }
        }

        public bool PrimedOkay
        {
            get
            { 
                bool ret = false;
                //
                if ( iPrimeAttempted )
                {
                    ret = ( iPrimeException == null );
                }
                //
                return ret;
            }
        }

        public string PrimeErrorMessage
        {
            get
            {
                string ret = string.Empty;
                //
                if ( PrimedOkay == false )
                {
                    ret = string.Format( "Could not process debug entity \'{0}\'", iEntity );
                }
                //
                return ret;
            }
        }

        public Exception PrimeException
        {
            get { return iPrimeException; }
            internal set { iPrimeException = value; }
        }

        public DbgEntity Entity
        {
            get { return iEntity; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly DbgEntity iEntity;
        private bool iPrimeAttempted = false;
        private Exception iPrimeException = null;
        #endregion
    }
}
