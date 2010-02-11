/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using SymbianUtils.Tracer;
using SymbianUtils.FileTypes;
using SymbianStructuresLib.Debug.Common.Id;
using SymbianStructuresLib.Debug.Common.Interfaces;
using SymbianSymbolLib.DbgEnginePlugin;
using SymbianSymbolLib.SourceManagement.Source;

namespace SymbianSymbolLib.SourceManagement.Provisioning
{
    // <summary>
    // A source provider is an entity that understands how to read a certain
    // format of file that contains symbolic information.
    // 
    // The provider's purpose is to read a specified source and transform it
    // into generic symbol data.
    // </summary>
    public abstract class SymSourceProvider
    {
        #region Constructors
        protected SymSourceProvider( SymSourceProviderManager aManager )
        {
            iManager = aManager;
        }
        #endregion

        #region Framework API
        public virtual bool IsSupported( string aFileName )
        {
            SymFileTypeList fileTypes = FileTypes;
            string extension = Path.GetExtension( aFileName );
            //
            bool ret = fileTypes.IsSupported( extension );
            return ret;
        }

        public virtual SymSourceCollection CreateSources( string aName )
        {
            throw new NotSupportedException( "Support not implemented by " + this.GetType().ToString() );
        }

        public virtual void ReadSource( SymSource aSource, TSynchronicity aSynchronicity )
        {
            throw new NotSupportedException();
        }

        public abstract SymFileTypeList FileTypes
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        public virtual string GetFileName( SymSource aSource )
        {
            string ret = aSource.URI;
            return ret;
        }

        public virtual void PrepareToCreateSources()
        {
        }
        #endregion

        #region Properties
        public ITracer Tracer
        {
            get { return iManager; }
        }

        public IPlatformIdAllocator IdAllocator
        {
            get { return iManager.IdAllocator; }
        }

        protected SymSourceProviderManager ProvisioningManager
        {
            get { return iManager; }
        }
        #endregion

        #region Data members
        private readonly SymSourceProviderManager iManager;
        #endregion
    }
}
