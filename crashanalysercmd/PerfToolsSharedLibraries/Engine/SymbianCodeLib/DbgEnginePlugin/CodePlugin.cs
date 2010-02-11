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
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianDebugLib.Engine;
using SymbianDebugLib.Entity;
using SymbianDebugLib.PluginAPI;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.PluginAPI.Types.Code;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianCodeLib.SourceManagement;
using SymbianCodeLib.SourceManagement.Source;
using SymbianCodeLib.SourceManagement.Provisioning;

namespace SymbianCodeLib.DbgEnginePlugin
{
    internal class CodePlugin : DbgEngineCode, ITracer
    {
        #region Constructors
        public CodePlugin( DbgEngine aEngine )
            : base( aEngine )
        {
            iSourceManager = new CodeSourceManager( this );
            iProvisioningManager = new CodeSourceProviderManager( this, aEngine.IdAllocator );
        }
        #endregion

        #region From DbgSymbolEngine
        public override bool IsReady
        {
            get { return true; }
        }

        public override string Name
        {
            get { return "DbgPluginCode"; }
        }

        public override bool IsSupported( string aFileName, out string aType )
        {
            CodeSourceProvider provider = iProvisioningManager.GetProvider( aFileName );
            //
            if ( provider != null )
            {
                aType = provider.Name;
            }
            else
            {
                aType = string.Empty;
            }
            //
            return provider != null;
        }

        public override DbgPluginPrimer CreatePrimer()
        {
            return new CodePrimer( this );
        }

        public override void PrepareToPrime( DbgEntityList aEntities )
        {
            List<string> fileNames = new List<string>();
            foreach ( DbgEntity entity in aEntities )
            {
                fileNames.Add( entity.FullName );
            }

            ProvisioningManager.PrepareToCreateSources( fileNames );
        }

        protected override DbgPluginView DoCreateView( string aName )
        {
            CodeView ret = new CodeView( aName, this );
            return ret;
        }

        protected override void DoClear()
        {
            if ( iSourceManager != null )
            {
                iSourceManager.Dispose();
            }
            iSourceManager = new CodeSourceManager( this );
        }
        #endregion

        #region API
        #endregion

        #region Properties
        internal CodeSourceManager SourceManager
        {
            get { return iSourceManager; }
        }

        internal CodeSourceProviderManager ProvisioningManager
        {
            get { return iProvisioningManager; }
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
                iProvisioningManager.Dispose();
                iSourceManager.Dispose();
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private CodeSourceManager iSourceManager;
        private readonly CodeSourceProviderManager iProvisioningManager;
        #endregion
   }
}
