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
using SymbianUtils.PluginManager;
using SymbianUtils.FileSystem;
using SymbianUtils.Settings;
using SymbianDebugLib.Engine;

namespace SymbianDebugLib.Entity.Descriptors
{
    public class DbgEntityDescriptorManager : IEnumerable<DbgEntityDescriptor>, IComparer<DbgEntityDescriptor>
    {
        #region Constructors
        internal DbgEntityDescriptorManager( DbgEngine aEngine )
        {
            iEngine = aEngine;
            //
            iDescriptors.Load( new object[] { this } );
            iDescriptors.Sort( this );
            //
            BuildExtensionList();
        }
        #endregion

        #region API
        internal DbgEntity Create( XmlSettingCategory aSettingsCategory )
        {
            DbgEntity ret = null;
            //
            foreach ( DbgEntityDescriptor descriptor in iDescriptors )
            {
                try
                {
                    ret = descriptor.Create( aSettingsCategory );
                    if ( ret != null )
                    {
                        break;
                    }
                }
                catch ( Exception )
                {
                }
            }
            //
            return ret;
        }

        internal DbgEntity Create( FSEntity aFSEntity )
        {
            DbgEntity ret = FindDescriptorAndCreateEntry( aFSEntity );
            return ret;
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iDescriptors.Count; }
        }

        public TDbgUiMode UiMode
        {
            get { return iUiMode; }
            set
            {
                if ( iUiMode != value )
                {
                    iUiMode = value;
                    foreach ( DbgEntityDescriptor descriptor in iDescriptors )
                    {
                        descriptor.OnUiModeChanged();
                    }
                }
            }
        }

        public FSExtensionList Extensions
        {
            get { return iAllExtensions; }
        }

        public DbgEngine Engine
        {
            get { return iEngine; }
        }

        public DbgEntityDescriptor this[ int aIndex ]
        {
            get { return iDescriptors[ aIndex ]; }
        }

        public DbgEntityDescriptor this[ string aCategoryName ]
        {
            get
            {
                DbgEntityDescriptor ret = null;
                //
                foreach ( DbgEntityDescriptor descriptor in iDescriptors )
                {
                    if ( descriptor.CategoryName.ToUpper() == aCategoryName.ToUpper() )
                    {
                        ret = descriptor;
                        break;
                    }
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        private void BuildExtensionList()
        {
            foreach ( DbgEntityDescriptor descriptor in iDescriptors )
            {
                FSExtensionList extensions = descriptor.Extensions;
                iAllExtensions.AddRange( extensions );
            }
        }

        private DbgEntity FindDescriptorAndCreateEntry( FSEntity aFSEntity )
        {
            DbgEntity ret = null;
            //
            foreach ( DbgEntityDescriptor descriptor in iDescriptors )
            {
                try
                {
                    ret = descriptor.Create( aFSEntity );
                    if ( ret != null )
                    {
                        break;
                    }
                }
                catch ( Exception )
                {
                }
            }
            //
            return ret;
        }
        #endregion

        #region From IEnumerable<DbgEntityDescriptor>
        public IEnumerator<DbgEntityDescriptor> GetEnumerator()
        {
            foreach ( DbgEntityDescriptor d in iDescriptors )
            {
                yield return d;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( DbgEntityDescriptor d in iDescriptors )
            {
                yield return d;
            }
        }
        #endregion

        #region From IComparer<DbgEntityDescriptor>
        public int Compare( DbgEntityDescriptor aLeft, DbgEntityDescriptor aRight )
        {
            int ret = -1;
            //
            if ( aLeft == null || aRight == null )
            {
                if ( aRight == null )
                {
                    ret = 1;
                }
            }
            else
            {
                ret = ( aLeft.DisplayOrder.CompareTo( aRight.DisplayOrder ) * -1 );
            }
            //
            return ret;
        }
        #endregion

        #region Data members
        private readonly DbgEngine iEngine;
        private TDbgUiMode iUiMode = TDbgUiMode.EUiEnabled;
        private FSExtensionList iAllExtensions = new FSExtensionList();
        private PluginManager<DbgEntityDescriptor> iDescriptors = new PluginManager<DbgEntityDescriptor>( 2 );
        #endregion
    }
}
