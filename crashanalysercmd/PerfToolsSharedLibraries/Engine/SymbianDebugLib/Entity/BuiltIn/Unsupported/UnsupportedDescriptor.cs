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
using SymbianDebugLib.Entity.Primer;
using SymbianDebugLib.Entity.Descriptors;
using SymbianDebugLib.PluginAPI.Types;

namespace SymbianDebugLib.Entity.BuiltIn.Unsupported
{
    public class UnsupportedDescriptor : DbgEntityDescriptor
    {
        #region Constructors
        public UnsupportedDescriptor( DbgEntityDescriptorManager aManager )
            : base( aManager )
        {
        }
        #endregion

        #region From DbgEntityDescriptor
        public override DbgEntity Create( FSEntity aEntity )
        {
            // Returns null if not supported
            DbgEntity ret = UnsupportedEntity.New( this, aEntity );
            return ret;
        }

        public override DbgEntity Create( XmlSettingCategory aSettingsCategory )
        {
            DbgEntity ret = UnsupportedEntity.New( this, aSettingsCategory );
            return ret;
        }

        public override Image Icon
        {
            get { return Properties.Resources.UnsupportedIcon; }
        }

        public override DbgEntityDescriptor.TFileSystemBrowserType FileSystemBrowserType
        {
            get { return TFileSystemBrowserType.EFiles; }
        }

        public override string CategoryName
        {
            get { return "Not Supported"; }
        }

        public override int DisplayOrder
        {
            get { return int.MinValue; }
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        #endregion
    }
}
