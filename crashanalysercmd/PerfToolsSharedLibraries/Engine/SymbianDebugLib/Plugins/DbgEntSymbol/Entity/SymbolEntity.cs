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
using System.Drawing;
using SymbianUtils;
using SymbianUtils.Settings;
using SymbianUtils.FileSystem;
using SymbianDebugLib.Entity;
using SymbianDebugLib.Engine;
using SymbianDebugLib.Entity.Descriptors;
using SymbianDebugLib.PluginAPI;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.PluginAPI.Types.Symbol;
using DbgEntSymbol.Descriptor;

namespace DbgEntSymbol.Entity
{
    public class SymbolEntity : DbgEntity
    {
        #region Static constructors
        public static SymbolEntity New( DbgEntityDescriptor aDescriptor, FSEntity aFSEntity )
        {
            SymbolEntity ret = null;

            // Validate that it's a supported file
            if ( aFSEntity.Exists && aFSEntity.IsFile )
            {
                string type = string.Empty;
                DbgEngine engine = aDescriptor.Engine;
                DbgEngineSymbol symbolEngine = engine.Symbols;
                bool supported = symbolEngine.IsSupported( aFSEntity.FullName, out type );
                //
                if ( supported )
                {
                    ret = new SymbolEntity( aDescriptor, aFSEntity );
                }
            }
            //
            return ret;
        }

        public static SymbolEntity New( DbgEntityDescriptor aDescriptor, XmlSettingCategory aSettingsCategory )
        {
            SymbolEntity ret = null;
            //
            if ( aSettingsCategory.Contains( KSettingsKeyFileName ) )
            {
                string fileName = aSettingsCategory[ KSettingsKeyFileName ];
                ret = New( aDescriptor, FSEntity.New( fileName ) );
            }
            //
            return ret;
        }
        #endregion

        #region Constants
        public const string KSettingsKeyFileName = "SymbolFileName";
        #endregion

        #region Constructors
        private SymbolEntity( DbgEntityDescriptor aDescriptor, FSEntity aFSEntity )
            : base( aDescriptor, aFSEntity )
        {
        }
        #endregion

        #region From DbgEntity
        public override void Save( XmlSettingCategory aCategory )
        {
            aCategory[ KSettingsKeyFileName ] = base.FSEntity.FullName;
        }

        public override DbgPluginEngine PluginEngine
        {
            get { return base.Engine.Symbols; }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
