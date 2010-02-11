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
using SymbianDebugLib.PluginAPI.Types;

namespace SymbianDebugLib.Entity.BuiltIn.Unsupported
{
    public class UnsupportedEntity : DbgEntity
    {
        #region Static constructors
        public static UnsupportedEntity New( DbgEntityDescriptor aDescriptor, FSEntity aFSEntity )
        {
            UnsupportedEntity ret = null;
            //
            if ( aFSEntity.Exists )
            {
                ret = new UnsupportedEntity( aDescriptor, aFSEntity );
            }
            //
            return ret;
        }

        public static UnsupportedEntity New( DbgEntityDescriptor aDescriptor, XmlSettingCategory aSettingsCategory )
        {
            UnsupportedEntity ret = null;
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
        public const string KSettingsKeyFileName = "UnsupportedFileName";
        #endregion

        #region Constructors
        private UnsupportedEntity( DbgEntityDescriptor aDescriptor, FSEntity aFSEntity )
            : base( aDescriptor, aFSEntity )
        {
        }
        #endregion

        #region From DbgEntity
        public override void Save( XmlSettingCategory aCategory )
        {
            aCategory[ KSettingsKeyFileName ] = base.FSEntity.FullName;
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
