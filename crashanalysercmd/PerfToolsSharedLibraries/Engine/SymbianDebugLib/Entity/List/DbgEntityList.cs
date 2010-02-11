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
using SymbianDebugLib.Entity.Descriptors;

namespace SymbianDebugLib.Entity
{
    public class DbgEntityList : DisposableObject, IEnumerable<DbgEntity>
    {
        #region Constructors
        internal DbgEntityList( DbgEngine aEngine )
        {
            iEngine = aEngine;
        }
        #endregion

        #region API
        public virtual void Add( DbgEntity aEntity )
        {
            iEntities.Add( aEntity );
        }

        public virtual DbgEntity Remove( DbgEntity aEntity )
        {
            Predicate<DbgEntity> predicate = delegate( DbgEntity entity ) { return entity == aEntity; };
            DbgEntity found = iEntities.Find( predicate );
            if ( found != null )
            {
                iEntities.Remove( found );
            }
            //
            return found;
        }

        public bool Contains( FSEntity aFSEntity )
        {
            bool ret = false;
            //
            foreach ( DbgEntity e in iEntities )
            {
                if ( e.FSEntity == aFSEntity )
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
            get { return iEntities.Count; }
        }

        public DbgEntity this[ int aIndex ]
        {
            get { return iEntities[ aIndex ]; }
        }

        public DbgEngine Engine
        {
            get { return iEngine; }
        }

        public string[] FileNames
        {
            get
            {
                List<string> ret = new List<string>();
                foreach ( DbgEntity e in iEntities )
                {
                    ret.Add( e.FullName );
                }
                return ret.ToArray();
            }
        }
        #endregion

        #region From DisposableObject
        protected override void CleanupManagedResources()
        {
            try
            {
                base.CleanupManagedResources();
            }
            finally
            {
                foreach ( DbgEntity entity in iEntities )
                {
                    entity.Dispose();
                }
            }
        }
        #endregion

        #region Internal properties
        #endregion

        #region Internal methods
        #endregion

        #region From IEnumerable<DbgEntity>
        public IEnumerator<DbgEntity> GetEnumerator()
        {
            foreach ( DbgEntity e in iEntities )
            {
                yield return e;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( DbgEntity e in iEntities )
            {
                yield return e;
            }
        }
        #endregion

        #region Data members
        private readonly DbgEngine iEngine;
        private List<DbgEntity> iEntities = new List<DbgEntity>();
        #endregion
    }
}
