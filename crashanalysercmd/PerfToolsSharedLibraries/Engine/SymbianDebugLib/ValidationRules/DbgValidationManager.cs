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
using System.Text.RegularExpressions;
using SymbianUtils.PluginManager;
using SymbianDebugLib.Engine;

namespace SymbianDebugLib.ValidationRules
{
    public class DbgValidationManager
    {
        #region Constructors
        internal DbgValidationManager( DbgEngine aEngine )
        {
            iEngine = aEngine;
            iRules.Load( new object[] { this } );
        }
        #endregion

        #region API
        public bool IsValid( DbgValidationRule.TOperation aOperation, out string aErrorDescription )
        {
            StringBuilder error = new StringBuilder();
            //
            bool ret = true;
            //
            foreach ( DbgValidationRule rule in iRules )
            {
                string errorText = string.Empty;
                //
                bool valid = rule.IsValid( aOperation, ref errorText );
                if ( !valid )
                {
                    ret &= valid;
                    error.AppendLine( errorText );
                }
            }
            //
            aErrorDescription = error.ToString();
            return ret;
        }
        #endregion

        #region Properties
        public DbgEngine Engine
        {
            get { return iEngine; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly DbgEngine iEngine;
        private PluginManager<DbgValidationRule> iRules = new PluginManager<DbgValidationRule>();
        #endregion
    }
}
