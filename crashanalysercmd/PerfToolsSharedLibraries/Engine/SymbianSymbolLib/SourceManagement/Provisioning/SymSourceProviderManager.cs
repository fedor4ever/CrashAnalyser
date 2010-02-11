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
using SymbianUtils.PluginManager;
using SymbianStructuresLib.Debug.Common.Interfaces;
using SymbianSymbolLib.DbgEnginePlugin;
using SymbianSymbolLib.SourceManagement.Source;

namespace SymbianSymbolLib.SourceManagement.Provisioning
{
    public class SymSourceProviderManager : DisposableObject, IEnumerable<SymSourceProvider>, ITracer
    {
        #region Constructors
        internal SymSourceProviderManager( SymbolPlugin aPlugin, IPlatformIdAllocator aIdAllocator )
        {
            iPlugin = aPlugin;
            iIdAllocator = aIdAllocator;
            //
            iProviders.Load( new object[] { this } );
        }
        #endregion

        #region API
        public SymFileTypeList SupportedFileTypes()
        {
            SymFileTypeList ret = new SymFileTypeList();
            //
            foreach ( SymSourceProvider provider in iProviders )
            {
                SymFileTypeList list = provider.FileTypes;
                ret.AddRange( list );
            }
            //
            return ret;
        }

        public SymSourceProvider GetProvider( string aFileName )
        {
            SymSourceProvider ret = null;
            //
            foreach ( SymSourceProvider provider in iProviders )
            {
                bool canRead = provider.IsSupported( aFileName );
                if ( canRead )
                {
                    ret = provider;
                    break;
                }
            }
            //
            return ret;
        }

        public IEnumerator<SymSource> GetSourceEnumerator()
        {
            return iPlugin.SourceManager.GetEnumerator();
        }

        public void PrepareToCreateSources()
        {
            foreach ( SymSourceProvider provider in iProviders )
            {
                provider.PrepareToCreateSources();
            }
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iProviders.Count; }
        }

        public SymSourceProvider this[ string aName ]
        {
            get
            {
                SymSourceProvider ret = null;
                //
                foreach ( SymSourceProvider prov in iProviders )
                {
                    if ( prov.Name == aName )
                    {
                        ret = prov;
                        break;
                    }
                }
                //
                return ret;
            }
        }

        public IPlatformIdAllocator IdAllocator
        {
            get { return iIdAllocator; }
        }

        public SymSourceProvider this[ int aIndex ]
        {
            get { return iProviders[ aIndex ]; }
        }
        #endregion

        #region Internal methods
        internal SymbolPlugin Plugin
        {
            get { return iPlugin; }
        }
        #endregion

        #region From IEnumerable<SymSourceProvider>
        public IEnumerator<SymSourceProvider> GetEnumerator()
        {
            foreach ( SymSourceProvider prov in iProviders )
            {
                yield return prov;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( SymSourceProvider prov in iProviders )
            {
                yield return prov;
            }
        }
        #endregion

        #region From ITracer
        public void Trace( string aMessage )
        {
            iPlugin.Trace( aMessage );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            iPlugin.Trace( aFormat, aParams );
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
                iProviders.Unload();
                iProviders.Dispose();
            }
        }
        #endregion

        #region Data members
        private readonly SymbolPlugin iPlugin;
        private readonly IPlatformIdAllocator iIdAllocator;
        private PluginManager<SymSourceProvider> iProviders = new PluginManager<SymSourceProvider>();
        #endregion
    }
}
