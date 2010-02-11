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
using SymbianUtils.PluginManager;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI.Types;

namespace SymbianDebugLib.PluginAPI.Types.Code
{
    public abstract class DbgEngineCode : DbgPluginEngine
    {
        #region Factory function
        public static DbgEngineCode New( DbgEngine aEngine )
        {
            PluginManager<DbgEngineCode> loader = new PluginManager<DbgEngineCode>( 1 );
            loader.Load( new object[] { aEngine } );
            //
            DbgEngineCode ret = null;
            foreach ( DbgEngineCode engine in loader )
            {
                if ( engine is DbgEngineCodeStub && loader.Count > 1 )
                {
                    continue;
                }
                else
                {
                    ret = engine;
                    break;
                }
            }
            //
            return ret;
        }
        #endregion

        #region Constructors
        protected DbgEngineCode( DbgEngine aEngine )
            : base( aEngine )
        {
        }
        #endregion

        #region Framework API
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
