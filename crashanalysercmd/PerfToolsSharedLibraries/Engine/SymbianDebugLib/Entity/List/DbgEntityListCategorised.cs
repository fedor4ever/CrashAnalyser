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
    public class DbgEntityListCategorised : DbgEntityList
    {
        #region Constructors
        internal DbgEntityListCategorised( DbgEngine aEngine, DbgEntityDescriptor aDescriptor )
            : base( aEngine )
        {
            iDescriptor = aDescriptor;
        }
        #endregion

        #region API
        public override void Add( DbgEntity aEntity )
        {
            if ( aEntity.CategoryName != this.CategoryName )
            {
                throw new ArgumentException();
            }
            else{
                base.Add( aEntity );
            }
        }
        #endregion

        #region Properties
        public string CategoryName
        {
            get { return iDescriptor.CategoryName; }
        }

        public DbgEntityDescriptor Descriptor
        {
            get { return iDescriptor; }
        }
        #endregion

        #region Event handlers
        #endregion

        #region Internal properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly DbgEntityDescriptor iDescriptor;
        #endregion
    }
}
