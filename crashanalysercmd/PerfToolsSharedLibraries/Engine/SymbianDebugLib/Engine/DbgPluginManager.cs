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
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.PluginAPI.Types.Code;
using SymbianDebugLib.PluginAPI.Types.Trace;
using SymbianDebugLib.PluginAPI.Types.Symbol;
using SymbianDebugLib.PluginAPI.Types.KeyBindings;
using SymbianDebugLib.PluginAPI.Types.MetaDataConfig;

namespace SymbianDebugLib.Engine
{
    internal class DbgPluginManager : DisposableObject
    {
        #region Constructors
        public DbgPluginManager( DbgEngine aEngine )
        {
            iEngine = aEngine;
            //
            FindImplementations();
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public DbgEngineCode Code
        {
            get { return iCode; }
        }

        public DbgEngineSymbol Symbols
        {
            get { return iSymbols; } 
        }

        public DbgEngineKeyBindings KeyBindings
        {
            get { return iKeyBindings; }
        }

        public DbgEngineTrace TraceDictionaries
        {
            get { return iTraceDictionaries; }
        }

        public DbgEngineMetaDataConfig MetaDataConfig
        {
            get { return iMetaDataConfig; }
        }
        #endregion

        #region Internal methods
        private void FindImplementations()
        {
            FindImplementationCode();
            FindImplementationTraces();
            FindImplementationSymbols();
            FindImplementationKeyBindings();
            FindImplementationMetaDataConfig();
        }

        private void FindImplementationSymbols()
        {
            iSymbols = DbgEngineSymbol.New( iEngine );
            System.Diagnostics.Debug.Assert( iSymbols != null );
        }

        private void FindImplementationCode()
        {
            iCode = DbgEngineCode.New( iEngine );
            System.Diagnostics.Debug.Assert( iCode != null );
        }

        private void FindImplementationTraces()
        {
            iTraceDictionaries = DbgEngineTrace.New( iEngine );
            System.Diagnostics.Debug.Assert( iTraceDictionaries != null );
        }

        private void FindImplementationMetaDataConfig()
        {
            iMetaDataConfig = DbgEngineMetaDataConfig.New( iEngine );
            System.Diagnostics.Debug.Assert( iMetaDataConfig != null );
        }

        private void FindImplementationKeyBindings()
        {
            iKeyBindings = DbgEngineKeyBindings.New( iEngine );
            System.Diagnostics.Debug.Assert( iKeyBindings != null );
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
                if ( iSymbols != null )
                {
                    iSymbols.Dispose();
                    iSymbols = null;
                }
                if ( iCode != null )
                {
                    iCode.Dispose();
                    iCode = null;
                }
                if ( iTraceDictionaries != null )
                {
                    iTraceDictionaries.Dispose();
                    iTraceDictionaries = null;
                }
                if ( iKeyBindings != null )
                {
                    iKeyBindings.Dispose();
                    iKeyBindings = null;
                }
                if ( iMetaDataConfig != null )
                {
                    iMetaDataConfig.Dispose();
                    iMetaDataConfig = null;
                }
            }
        }
        #endregion

        #region Data members
        private readonly DbgEngine iEngine;
        private DbgEngineCode iCode;
        private DbgEngineSymbol iSymbols;
        private DbgEngineTrace iTraceDictionaries;
        private DbgEngineKeyBindings iKeyBindings;
        private DbgEngineMetaDataConfig iMetaDataConfig;
        #endregion
    }
}
