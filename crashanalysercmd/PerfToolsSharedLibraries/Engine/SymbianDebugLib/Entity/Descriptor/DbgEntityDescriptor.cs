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
using SymbianUtils.FileSystem;
using SymbianUtils.Settings;
using SymbianDebugLib.Engine;
using SymbianDebugLib.Entity.Primer;
using SymbianDebugLib.PluginAPI;

namespace SymbianDebugLib.Entity.Descriptors
{
    public abstract class DbgEntityDescriptor : IComparable<DbgEntityDescriptor>
    {
        #region Enumerations
        public enum TFileSystemBrowserType
        {
            EFiles = 0,
            EDirectories
        }

        public enum TUnderlyingType
        {
            ETypeUnknown = -1,
            ETypeSymbols = 0,
            ETypeCode,
            ETypeConfigMetaData,
            ETypeKeyBindings,
            ETypeTraceDictionaries
        }
        #endregion

        #region Constructors
        protected DbgEntityDescriptor( DbgEntityDescriptorManager aManager )
        {
            iManager = aManager;
        }
        #endregion

        #region Framework API
        public abstract DbgEntity Create( FSEntity aEntity );

        public abstract DbgEntity Create( XmlSettingCategory aSettingsCategory );

        public virtual Image Icon
        {
            get { return null; }
        }

        public abstract TFileSystemBrowserType FileSystemBrowserType
        {
            get;
        }

        public abstract string CategoryName
        {
            get;
        }

        public abstract int DisplayOrder
        {
            get;
        }

        public virtual FSExtensionList Extensions
        {
            get { return new FSExtensionList(); }
        }

        public virtual void OnUiModeChanged()
        {
        }

        public virtual TUnderlyingType UnderlyingType
        {
            get { return TUnderlyingType.ETypeUnknown; }
        }

        public virtual void OnCleared()
        {
        }
        #endregion

        #region API
        internal void Prime( DbgEntity aEntity, TSynchronicity aSynchronicity )
        {
            // Make a new result
            aEntity.PrimerResult = new DbgEntityPrimerResult( aEntity );

            // The primer to use
            IDbgEntityPrimer primer = null;

            // We can't sensibly prime if we don't have a plugin engine associated with the
            // entity.
            DbgPluginEngine plugin = aEntity.PluginEngine;
            if ( plugin != null )
            {
                // Get primer object
                switch ( UiMode )
                {
                case TDbgUiMode.EUiDisabled:
                    primer = new DbgEntityPrimerSilent( aEntity, plugin );
                    break;
                default:
                case TDbgUiMode.EUiEnabled:
                    primer = new DbgEntityPrimerUi( aEntity, plugin );
                    break;
                }
            }
            else
            {
                primer = new DbgEntityPrimerNull( aEntity );
                Engine.Trace( "WARNING: Entity {0} does not supply plugin engine", aEntity.FullName );
            }

            // Make sure we indicate that we actually atttempted to prime
            // the entity.
            aEntity.PrimerResult.PrimeAttempted = true;

            // And prime away
            primer.Prime( aSynchronicity );
        }
        #endregion

        #region Properties
        public DbgEngine Engine
        {
            get { return Manager.Engine; }
        }

        public TDbgUiMode UiMode
        {
            get { return iManager.UiMode; }
        }

        protected DbgEntityDescriptorManager Manager
        {
            get { return iManager; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From System.Object
        public override bool Equals( object aObject )
        {
            if ( aObject != null )
            {
                if ( aObject is DbgEntityDescriptor )
                {
                    DbgEntityDescriptor other = (DbgEntityDescriptor) aObject;
                    return other.CategoryName == this.CategoryName;
                }
            }
            //
            return base.Equals( aObject );
        }

        public override string ToString()
        {
            return CategoryName;
        }

        public override int GetHashCode()
        {
            return CategoryName.GetHashCode();
        }
        #endregion

        #region From IComparable<DbgEntityDescriptor>
        public int CompareTo( DbgEntityDescriptor aOther )
        {
            int ret = 1;
            //
            if ( aOther != null )
            {
                ret = this.DisplayOrder.CompareTo( aOther.DisplayOrder );
            }
            //
            return ret;
        }
        #endregion

        #region Data members
        private readonly DbgEntityDescriptorManager iManager;
        #endregion
    }
}
