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

namespace SymbianDebugLib.Entity.BuiltIn.Missing
{
    public class MissingEntity : DbgEntity
    {
        #region Static constructors
        public static MissingEntity New( DbgEntityDescriptor aDescriptor, FSEntity aFSEntity )
        {
            MissingEntity ret = null;
            //
            if ( aFSEntity.IsFile && !aFSEntity.Exists )
            {
                ret = new MissingEntity( aDescriptor, aFSEntity );
            }
            //
            return ret;
        }

        public static MissingEntity New( DbgEntityDescriptor aDescriptor, XmlSettingCategory aSettingsCategory )
        {
            MissingEntity ret = null;
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
        public const string KSettingsKeyFileName = "MissingFileName";
        #endregion

        #region Constructors
        private MissingEntity( DbgEntityDescriptor aDescriptor, FSEntity aFSEntity )
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
