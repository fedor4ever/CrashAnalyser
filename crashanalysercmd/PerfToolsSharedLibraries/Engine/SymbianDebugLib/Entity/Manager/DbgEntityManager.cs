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
using SymbianUtils;
using SymbianUtils.Settings;
using SymbianUtils.FileSystem;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI;
using SymbianDebugLib.Entity.Descriptors;

namespace SymbianDebugLib.Entity.Manager
{
    public class DbgEntityManager : IEnumerable<DbgEntity>, IXmlSettingsExtended
    {
        #region Constructors
        internal DbgEntityManager( DbgEngine aEngine )
        {
            iEngine = aEngine;
        }
        #endregion

        #region API
        public void Clear()
        {
            foreach ( KeyValuePair<string, DbgEntityListCategorised> kvp in iLists )
            {
                DbgEntityListCategorised list = kvp.Value;

                // Tell the descriptor it is being cleared
                list.Descriptor.OnCleared();

                // Get rid of the list
                list.Dispose();
            }
            //
            iLists.Clear();
            iDisplayOrderList.Clear();
            Engine.OnCleared();
        }

        public DbgEntity Add( FSEntity aFSEntity )
        {
            DbgEntityDescriptorManager descManager = Engine.DescriptorManager;
            //
            DbgEntity entity = descManager.Create( aFSEntity );
            if ( entity != null )
            {
                SaveEntity( entity );
            }
            //
            return entity;
        }

        public DbgEntity AddFile( FileInfo aFile )
        {
            return Add( new FSEntityFile( aFile ) );
        }

        public DbgEntity AddDirectory( DirectoryInfo aDirectory )
        {
            return Add( new FSEntityDirectory( aDirectory ) );
        }

        public void Remove( DbgEntity aEntity )
        {
            DbgEntityDescriptor descriptor = aEntity.Descriptor;
            if ( descriptor != null )
            {
                DbgEntityListCategorised list;
                if ( iLists.TryGetValue( descriptor.CategoryName, out list ) )
                {
                    // Try to find the entity from the list and, if found,
                    // destroy it.
                    using ( DbgEntity removed = list.Remove( aEntity ) )
                    {
                        if ( removed != null )
                        {
                            // Tell the entity
                            removed.OnRemoved();

                            // Tell everybody else
                            Engine.OnRemoved( removed );
                        }
                    }
                }
                if ( iDisplayOrderList.TryGetValue( descriptor, out list ) )
                {
                    iDisplayOrderList.Remove( descriptor );
                }
            }
        }

        public bool IsReadyToPrime( out string aErrorList )
        {
            bool ready = true;
            aErrorList = string.Empty;
            //
            foreach ( DbgEntity e in this )
            {
                if ( !e.IsReadyToPrime( out aErrorList ) )
                {
                    ready = false;
                    break;
                }
            }
            //
            return ready;
        }

        public bool Contains( FSEntity aFSEntity )
        {
            bool ret = false;
            //
            foreach ( KeyValuePair<DbgEntityDescriptor, DbgEntityListCategorised> kvp in iDisplayOrderList )
            {
                DbgEntityListCategorised list = kvp.Value;
                if ( list.Contains( aFSEntity ) )
                {
                    ret = true;
                    break;
                }
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        public int Count
        {
            get
            {
                int ret = 0;
                //
                foreach ( KeyValuePair<DbgEntityDescriptor, DbgEntityListCategorised> kvp in iDisplayOrderList )
                {
                    ret += kvp.Value.Count;
                }
                //
                return ret;
            }
        }

        public DbgEntity this[ int aIndex ]
        {
            get
            {
                int i = 0;
                DbgEntity ret = null;
                //
                foreach ( KeyValuePair<DbgEntityDescriptor, DbgEntityListCategorised> kvp in iDisplayOrderList )
                {
                    DbgEntityListCategorised list = kvp.Value;
                    int listCount = list.Count;
                    if ( aIndex < ( i + listCount ) )
                    {
                        ret = list[ aIndex - i ];
                        break;
                    }
                    else
                    {
                        i += listCount;
                    }
                }
                //
                if ( ret == null )
                {
                    throw new ArgumentException( "aIndex is out of range" );
                }
                //
                return ret;
           }
        }

        public DbgEngine Engine
        {
            get { return iEngine; }
        }

        public FSEntity[] FileSystemEntities
        {
            get
            {
                List<FSEntity> entities = new List<FSEntity>();
                //
                foreach ( DbgEntity e in this )
                {
                    FSEntity clone = FSEntity.New( e );
                    entities.Add( clone );
                }
                //
                return entities.ToArray();
            }
        }
        #endregion

        #region Event handlers
        #endregion

        #region Settings related
        public void XmlSettingsSave( XmlSettings aSettings, string aCategory )
        {
            aSettings.Clear();
            aSettings[ aCategory, "__Count" ] = Count;
            int index = 0;
            foreach ( DbgEntity entity in this )
            {
                string entityCategory = string.Format( "DbgEntity_{0:d5}", index++ );

                // Get the category where we'll save the settings for this entity to...
                XmlSettingCategory category = aSettings[ entityCategory ];

                // Save entity specific settings
                entity.Save( category );
            }
        }

        public void XmlSettingsLoad( XmlSettings aSettings, string aCategory )
        {
            Clear();
            //
            int count = aSettings.Load( aCategory, "__Count", 0 );
            for ( int i = 0; i < count; i++ )
            {
                try
                {
                    // Create category name and try to obtain it...
                    string entityCategory = string.Format( "DbgEntity_{0:d5}", i );
                    XmlSettingCategory category = aSettings[ entityCategory ];
                    if ( category != null )
                    {
                        // Make a new entity object based upon the type
                        DbgEntityDescriptorManager descManager = Engine.DescriptorManager;
                        DbgEntity entity = descManager.Create( category );
                        if ( entity != null )
                        {
                            SaveEntity( entity );
                        }
                    }
                }
                catch ( Exception )
                {
                }
            }
        }
        #endregion

        #region Internal properties
        #endregion

        #region Internal methods
        private void SaveEntity( DbgEntity aEntity )
        {
            // Check if the dictionary already contains an entity descriptor list
            DbgEntityDescriptor descriptor = aEntity.Descriptor;
            DbgEntityListCategorised list;
            if ( !iLists.TryGetValue( descriptor.CategoryName, out list ) )
            {
                list = new DbgEntityListCategorised( Engine, descriptor );

                // We create two lists. One sorts and indexes by display order, the other indexes by category.
                iLists.Add( descriptor.CategoryName, list );
                if ( !iDisplayOrderList.ContainsKey( descriptor ) )
                {
                    iDisplayOrderList.Add( descriptor, list );
                }
            }
           
            // Add to list
            list.Add( aEntity );

            // Notify engine
            Engine.OnAdded( aEntity );
        }
        #endregion

        #region From IEnumerable<DbgEntity>
        public IEnumerator<DbgEntity> GetEnumerator()
        {
            foreach ( KeyValuePair<DbgEntityDescriptor, DbgEntityListCategorised> kvp in iDisplayOrderList )
            {
                foreach ( DbgEntity e in kvp.Value )
                {
                    yield return e;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( KeyValuePair<DbgEntityDescriptor, DbgEntityListCategorised> kvp in iDisplayOrderList )
            {
                foreach ( DbgEntity e in kvp.Value )
                {
                    yield return e;
                }
            }
        }
        #endregion

        #region Data members
        private readonly DbgEngine iEngine;
        private SortedList<DbgEntityDescriptor, DbgEntityListCategorised> iDisplayOrderList = new SortedList<DbgEntityDescriptor, DbgEntityListCategorised>();
        private Dictionary<string, DbgEntityListCategorised> iLists = new Dictionary<string, DbgEntityListCategorised>();
        #endregion
    }
}
