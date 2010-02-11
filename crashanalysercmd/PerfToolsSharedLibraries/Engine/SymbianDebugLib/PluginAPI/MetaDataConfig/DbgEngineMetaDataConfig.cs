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

namespace SymbianDebugLib.PluginAPI.Types.MetaDataConfig
{
    public abstract class DbgEngineMetaDataConfig : DbgPluginEngine
    {
        #region Factory function
        public static DbgEngineMetaDataConfig New( DbgEngine aEngine )
        {
            PluginManager<DbgEngineMetaDataConfig> loader = new PluginManager<DbgEngineMetaDataConfig>( 1 );
            loader.Load( new object[] { aEngine } );
            //
            DbgEngineMetaDataConfig ret = null;
            foreach ( DbgEngineMetaDataConfig engine in loader )
            {
                if ( engine is DbgEngineMetaDataConfigStub && loader.Count > 1 )
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
        protected DbgEngineMetaDataConfig( DbgEngine aEngine )
            : base( aEngine )
        {
        }
        #endregion

        #region From DbgPluginEngine
        protected override void DoClear()
        {
            base.Engine.ConfigManager.Clear();
        }
        #endregion

        #region Properties
        public bool IsConfigurationDataAvailable
        {
            get { return base.Engine.ConfigManager.Count > 0; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
