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
using SymbianInstructionLib.Arm.Library;
using SymbianInstructionLib.Arm.Instructions.Common;
using SymbianCodeLib.DbgEnginePlugin;
using SymbianCodeLib.SourceManagement.Source;

namespace SymbianCodeLib.SourceManagement.Provisioning
{
    public class CodeSourceProviderManager : DisposableObject, IEnumerable<CodeSourceProvider>, ITracer
    {
        #region Constructors
        internal CodeSourceProviderManager( CodePlugin aPlugin, IPlatformIdAllocator aIdAllocator )
        {
            iPlugin = aPlugin;
            iIdAllocator = aIdAllocator;
            iInstructionLibrary = new ArmLibrary();
            //
            iProviders.Load( new object[] { this } );
        }
        #endregion

        #region API
        public SymFileTypeList SupportedFileTypes()
        {
            SymFileTypeList ret = new SymFileTypeList();
            //
            foreach ( CodeSourceProvider provider in iProviders )
            {
                SymFileTypeList list = provider.FileTypes;
                ret.AddRange( list );
            }
            //
            return ret;
        }

        public CodeSourceProvider GetProvider( string aFileName )
        {
            CodeSourceProvider ret = null;
            //
            foreach ( CodeSourceProvider provider in iProviders )
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

        public IEnumerator<CodeSource> GetSourceEnumerator()
        {
            return iPlugin.SourceManager.GetEnumerator();
        }

        public void PrepareToCreateSources( IEnumerable<string> aFileNames )
        {
            foreach ( CodeSourceProvider provider in iProviders )
            {
                provider.PrepareToCreateSources( aFileNames );
            }
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iProviders.Count; }
        }

        public IPlatformIdAllocator IdAllocator
        {
            get { return iIdAllocator; }
        }

        public ArmLibrary InstructionLibrary
        {
            get
            {
                return iInstructionLibrary;
            }
        }

        public CodeSourceProvider this[ string aName ]
        {
            get
            {
                CodeSourceProvider ret = null;
                //
                foreach ( CodeSourceProvider prov in iProviders )
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

        public CodeSourceProvider this[ int aIndex ]
        {
            get { return iProviders[ aIndex ]; }
        }

        internal CodePlugin Plugin
        {
            get { return iPlugin; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From IEnumerable<CodeSourceProvider>
        public IEnumerator<CodeSourceProvider> GetEnumerator()
        {
            foreach ( CodeSourceProvider prov in iProviders )
            {
                yield return prov;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( CodeSourceProvider prov in iProviders )
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
        private readonly CodePlugin iPlugin;
        private readonly IPlatformIdAllocator iIdAllocator;
        private readonly ArmLibrary iInstructionLibrary;
        private PluginManager<CodeSourceProvider> iProviders = new PluginManager<CodeSourceProvider>();
        #endregion
    }
}
