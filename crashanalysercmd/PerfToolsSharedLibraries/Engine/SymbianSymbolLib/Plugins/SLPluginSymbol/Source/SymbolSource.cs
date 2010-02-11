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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SymbianStructuresLib.Debug.Symbols;
using SymbianStructuresLib.Debug.Symbols.Interfaces;
using SymbianSymbolLib.SourceManagement.Source;
using SymbianSymbolLib.SourceManagement.Provisioning;
using SymbianUtils.FileTypes;
using SymbianUtils;
using SLPluginSymbol.Data;
using SLPluginSymbol.Reader;
using SLPluginSymbol.Utilities;

namespace SLPluginSymbol.Source
{
    internal class SymbolSource : SymSource
    {
        #region Constructors
        public SymbolSource( string aURI, SymSourceProvider aProvider )
            : base( aURI, aProvider )
        {
            iData = new SymbolFileData( this );
            //
            FindCollections();
        }
        #endregion

        #region API
        public SymbolFileData ExcavateData()
        {
            SymbolFileData ret = iData;
            iData = null;
            return ret;
        }
        #endregion

        #region From SymSource
        public override void Read( TSynchronicity aSynchronicity )
        {
            EnsureCollectionsAreFound();
            //
            base.Read( aSynchronicity );
        }
        #endregion

        #region Properties
        public new SymSourceProvider Provider
        {
            get
            {
                SymSourceProvider provider = (SymSourceProvider) base.Provider;
                return provider;
            }
        }
        #endregion

        #region Event handlers
        #endregion

        #region Internal constants
        #endregion

        #region Internal methods
        private void EnsureCollectionsAreFound()
        {
            System.Diagnostics.Debug.Assert( iFindCollectionsARE != null );
            using ( iFindCollectionsARE )
            {
                iFindCollectionsARE.WaitOne();
            }
            //
            iFindCollectionsARE = null;
        }

        private void FindCollections()
        {
            iFindCollectionsARE = new AutoResetEvent( false );
            //
            iData.Split( System.Environment.ProcessorCount );
            iData.DataPrepared += new SymbolFileData.DataPreparedHandler( SymbolFileData_DataPrepared );
            iData.FindCollections();
        }

        private void SymbolFileData_DataPrepared( SymbolFileData aData )
        {
            System.Diagnostics.Debug.Assert( iFindCollectionsARE != null );
            System.Diagnostics.Debug.Assert( base.HaveBeenDisposedOf == false );
            //
            lock ( iAutoResetEventLock )
            {
                iFindCollectionsARE.Set();
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
                if ( iFindCollectionsARE != null )
                {
                    iFindCollectionsARE.Close();
                    iFindCollectionsARE = null;
                }
            }
        }
        #endregion

        #region Data members
        private SymbolFileData iData;
        private object iAutoResetEventLock = new object();
        private AutoResetEvent iFindCollectionsARE = null;
        #endregion
    }
}
