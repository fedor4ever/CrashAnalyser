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
using SymbianStructuresLib.Debug.Symbols;
using SymbianStructuresLib.Debug.Symbols.Utilities;
using SymbianSymbolLib.SourceManagement.Source;
using SymbianSymbolLib.SourceManagement.Provisioning;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianUtils.FileTypes;

namespace SLPluginMap.Reader
{
    internal abstract class MapReader : AsyncTextFileReader
	{
        #region Constructors
        protected MapReader( SymSource aSource, ITracer aTracer )
        : base( aSource.FileName, aTracer )
		{
            iSource = aSource;
            iHarmoniser = new SymbolCollectionHarmoniser( Collection );
		}
		#endregion

        #region API
        public void Read( TSynchronicity aSynchronicity )
        {
            base.Trace( "[SLPluginMap] Starting to read: {0}", base.FileName );
            base.StartRead( aSynchronicity );
        }
        #endregion

        #region Properties
        public uint GlobalBaseAddress
        {
            get { return iGlobalBaseAddress; }
            set { iGlobalBaseAddress = value; } 
        }

        protected SymbolCollection Collection
        {
            get
            {
                SymbianUtils.SymDebug.SymDebugger.Assert( iSource.Count == 1 );
                return iSource[ 0 ];
            }
        }
		#endregion

        #region Internal framework API
        protected void ReportSymbol( Symbol aSymbol )
        {
            // Make sure it's tagged as a map file symbol
            aSymbol.Source = TSymbolSource.ESourceWasMapFile;

            // Hand off to harmoniser for filing
            if ( iHarmoniser != null )
            {
                bool saved = iHarmoniser.Add( aSymbol );
            }
        }
        #endregion

		#region From AsyncTextReaderBase
        protected override void HandleReadStarted()
        {
            iSource.ReportEvent( SymSource.TEvent.EReadingStarted );
            Collection.TransactionBegin();
            base.HandleReadStarted();
        }

        protected override void HandleReadCompleted()
        {
            try
            {
                iHarmoniser.Dispose();
                iHarmoniser = null;
                Collection.TransactionEnd();
                base.Trace( "[SLPluginMap] Finished read of: {0}", base.FileName );
                iSource.ReportEvent( SymSource.TEvent.EReadingComplete );
            }
            finally
            {
                base.HandleReadCompleted();
            }
        }

        protected override void OnProgressChanged( int aProgress )
        {
            iSource.ReportEvent( SymSource.TEvent.EReadingProgress, aProgress );
            base.OnProgressChanged( aProgress );
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
                iHarmoniser.Dispose();
            }
        }
		#endregion

        #region Data members
        private readonly SymSource iSource;
        private SymbolCollectionHarmoniser iHarmoniser;
        private uint iGlobalBaseAddress = 0;
		#endregion
	}
}
