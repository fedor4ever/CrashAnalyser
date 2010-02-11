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

using System.Collections.Generic;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI;
using SymbianDebugLib.PluginAPI.Types.Symbol;
using SymbianSymbolLib.SourceManagement.Provisioning;
using SymbianSymbolLib.SourceManagement.Source;

namespace SymbianSymbolLib.DbgEnginePlugin
{
    internal class SymbolPlugin : DbgEngineSymbol
    {
        #region Constructors
        public SymbolPlugin( DbgEngine aEngine )
            : base( aEngine )
        {
            iSourceManager = new SymSourceManager( this );
            iProvisioningManager = new SymSourceProviderManager( this, aEngine.IdAllocator );
		}
		#endregion

        #region From DbgSymbolEngine
        public override bool IsReady
        {
            get { return true; }
        }

        public override string Name
        {
            get { return "DbgSymbolPlugin"; }
        }

        public override bool IsSupported( string aFileName, out string aType )
        {
            SymSourceProvider provider = iProvisioningManager.GetProvider( aFileName );
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
            return new SymbolPrimer( this );
        }

        protected override DbgPluginView DoCreateView( string aName )
        {
            SymbolView ret = new SymbolView( aName, this );
            return ret;
        }
        
        protected override void DoClear()
        {
            if ( iSourceManager != null )
            {
                iSourceManager.Dispose();
            }
            iSourceManager = new SymSourceManager( this );
        }
        #endregion

		#region API
        internal void StoreSourcesThatWillBePrimed( IEnumerable<SymSource> aSourcesThatWillBePrimed )
        {
            SourceManager.AddRange( aSourcesThatWillBePrimed );
        }
        #endregion

		#region Properties
        internal SymSourceManager SourceManager
        {
            get { return iSourceManager; }
        }

        internal SymSourceProviderManager ProvisioningManager
        {
            get { return iProvisioningManager; }
        }
        #endregion

        #region Internal methods
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

        #region Data members
        private SymSourceManager iSourceManager;
        private readonly SymSourceProviderManager iProvisioningManager;
        #endregion
    }
}
