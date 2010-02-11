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

namespace SymbianDebugLib.Entity.BuiltIn.Missing
{
    public class MissingDescriptor : DbgEntityDescriptor
    {
        #region Constructors
        public MissingDescriptor( DbgEntityDescriptorManager aManager )
            : base( aManager )
        {
        }
        #endregion

        #region From DbgEntityDescriptor
        public override DbgEntity Create( FSEntity aEntity )
        {
            // Returns null if not supported
            MissingEntity ret = MissingEntity.New( this, aEntity );
            return ret;
        }

        public override DbgEntity Create( XmlSettingCategory aSettingsCategory )
        {
            MissingEntity ret = MissingEntity.New( this, aSettingsCategory );
            return ret;
        }

        public override Image Icon
        {
            get { return Properties.Resources.MissingIcon; }
        }

        public override DbgEntityDescriptor.TFileSystemBrowserType FileSystemBrowserType
        {
            get { return TFileSystemBrowserType.EFiles; }
        }

        public override string CategoryName
        {
            get { return "Missing"; }
        }

        public override int DisplayOrder
        {
            get { return int.MinValue + 10; }
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        #endregion
    }
}
