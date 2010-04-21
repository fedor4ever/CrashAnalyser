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

namespace SymbianDebugLib.Entity.Configurations
{
    public class DbgEntityConfigManager : DisposableObject
    {
        #region Constructors
        internal DbgEntityConfigManager( DbgEngine aEngine )
        {
            iEngine = aEngine;
        }
        #endregion

        #region API
        public void Clear()
        {
            lock ( iConfigurations )
            {
                iConfigurations.Clear();
            }
        }

        public void Add( DbgEntityConfig aConfiguration )
        {
            lock ( iConfigurations )
            {
                iConfigurations.Add( aConfiguration );
            }
        }

        public void SwitchConfigurationSynchronously( DbgEntityConfigIdentifier aId )
        {
            // Try to find a config that matches the specified value
            DbgEntityConfig config = ConfigById( aId );
            if ( config == null )
            {
                // Unload any old data and return
                iEngine.Clear();
                iEngine.Trace( "WARNING: DbgEntityConfigManager could not load config id: " + aId.ToString() );
            }
            else
            {
                if ( config == iEngine.CurrentConfiguration )
                {
                    // Nothing to do
                }
                else
                {
                    // Unload any old data
                    iEngine.Clear();

                    // Prepare list of files
                    List<string> files = new List<string>();
                    foreach ( DbgEntityConfig.CfgSet set in config )
                    {
                        foreach ( DbgEntityConfig.CfgFile file in set )
                        {
                            files.Add( file.FileNameAndPath );
                        }
                    }
                    iEngine.AddRange( files );
                    iEngine.Prime( TSynchronicity.ESynchronous );
                }
            }
        }

        public bool IsActiveRomId(uint aRomId)
        {
            return iEngine.IsActiveRomId(aRomId);
        }

        #endregion

        #region Event handlers
        #endregion

        #region Properties
        public int Count
        {
            get 
            {
                lock ( iConfigurations )
                {
                    return iConfigurations.Count;
                }
            }
        }

        internal DbgEngine Engine
        {
            get { return iEngine; }
        }
        #endregion

        #region Internal methods
        private DbgEntityConfig ConfigById( DbgEntityConfigIdentifier aId )
        {
            DbgEntityConfig ret = null;
            //
            lock ( iConfigurations )
            {
                foreach ( DbgEntityConfig cfg in iConfigurations )
                {
                    if ( cfg.Contains( aId ) )
                    {
                        ret = cfg;
                        break;
                    }
                }
            }
            //
            return ret;
        }
        #endregion

        #region Data members
        private readonly DbgEngine iEngine;
        private List<DbgEntityConfig> iConfigurations = new List<DbgEntityConfig>();
        #endregion
    }
}
