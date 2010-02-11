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
using SymbianStructuresLib.Debug.Symbols;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI.Types;

namespace SymbianDebugLib.PluginAPI.Types.Symbol
{
    public abstract class DbgEngineSymbol : DbgPluginEngine
    {
        #region Factory function
        public static DbgEngineSymbol New( DbgEngine aEngine )
        {
            PluginManager<DbgEngineSymbol> loader = new PluginManager<DbgEngineSymbol>( 1 );
            loader.Load( new object[] { aEngine } );
            //
            DbgEngineSymbol ret = null;
            foreach ( DbgEngineSymbol engine in loader )
            {
                if ( engine is DbgEngineSymbolStub && loader.Count > 1 )
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
        protected DbgEngineSymbol( DbgEngine aEngine )
            : base( aEngine )
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
