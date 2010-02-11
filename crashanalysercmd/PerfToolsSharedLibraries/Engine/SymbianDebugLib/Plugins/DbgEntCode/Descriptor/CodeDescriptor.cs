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
using SymbianDebugLib.PluginAPI;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.PluginAPI.Types.Code;
using DbgEntCode.Entity;

namespace DbgEntCode.Descriptor
{
    public class CodeDescriptor : DbgEntityDescriptor
    {
        #region Constructors
        public CodeDescriptor( DbgEntityDescriptorManager aManager )
            : base( aManager )
        {
        }
        #endregion

        #region From DbgEntityDescriptor
        public override DbgEntity Create( FSEntity aEntity )
        {
            // Returns null if not supported
            CodeEntity ret = CodeEntity.New( this, aEntity );
            return ret;
        }

        public override DbgEntity Create( XmlSettingCategory aSettingsCategory )
        {
            CodeEntity ret = CodeEntity.New( this, aSettingsCategory );
            return ret;
        }

        public override Image Icon
        {
            get { return Properties.Resources.Icon; }
        }

        public override DbgEntityDescriptor.TFileSystemBrowserType FileSystemBrowserType
        {
            get { return TFileSystemBrowserType.EFiles; }
        }

        public override string CategoryName
        {
            get { return "Code"; }
        }

        public override int DisplayOrder
        {
            get { return int.MaxValue - 40; }
        }

        public override TUnderlyingType UnderlyingType
        {
            get { return TUnderlyingType.ETypeCode; }
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        #endregion
    }
}
