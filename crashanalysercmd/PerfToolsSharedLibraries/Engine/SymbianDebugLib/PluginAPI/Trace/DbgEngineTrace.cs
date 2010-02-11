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
using SymbianStructuresLib.Debug.Trace;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI.Types;

namespace SymbianDebugLib.PluginAPI.Types.Trace
{
    public abstract class DbgEngineTrace : DbgPluginEngine
    {
        #region Factory function
        public static DbgEngineTrace New( DbgEngine aEngine )
        {
            PluginManager<DbgEngineTrace> loader = new PluginManager<DbgEngineTrace>( 1 );
            loader.Load( new object[] { aEngine } );
            //
            DbgEngineTrace ret = null;
            foreach ( DbgEngineTrace engine in loader )
            {
                if ( engine is DbgEngineTraceStub && loader.Count > 1 )
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
        protected DbgEngineTrace( DbgEngine aEngine )
            : base( aEngine )
        {
        }
        #endregion

        #region From DbgPluginEngine
        protected override DbgPluginView DoCreateView( string aName )
        {
            // This engine doesn't support views.
            throw new NotSupportedException();
        }
        #endregion

        #region API
        public abstract TraceLine[] Decode( byte[] aData );
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
