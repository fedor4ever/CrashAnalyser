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
using System.ComponentModel;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianUtils.Threading;
using SymbianUtils.FileTypes;
using SymbianUtils.TextUtilities.Readers.Types.Array;
using SymbianStructuresLib.Debug.Symbols;
using SymbianSymbolLib.SourceManagement.Source;
using SymbianSymbolLib.SourceManagement.Provisioning;
using SLPluginSymbol.Data;
using SLPluginSymbol.Source;

namespace SLPluginSymbol.Reader
{
    internal class SymbolFileReader : MultiThreadedProcessor<SymbolFileSegment>
	{
        #region Constructors
        public SymbolFileReader( SymbolSource aSource, SymbolFileData aData )
            : base( aData, System.Threading.ThreadPriority.Lowest )
		{
            iSource = aSource;
            iData = aData;
            
            // Count the total number of lines - this enables us to report progress
            foreach ( SymbolFileSegment segment in iData )
            {
                iTotalNumberOfLines += segment.NumberOfLines;
            }
        }
		#endregion

        #region API
        public void Read( TSynchronicity aSynchronicity )
        {
            base.Start( aSynchronicity );
        }
        #endregion

        #region Properties
		#endregion

        #region Event handlers
        private void SymbolFileSegmentReader_Progress( SymbolFileSegmentReader aReader, long aTotalNumberOfLines, long aChunkSizeProcessed )
        {
            bool report = false;
            int progress = 0;
            //
            lock ( iSyncRoot )
            {
                iProgressSoFar += aChunkSizeProcessed;
                //
                float progressF = (float) iProgressSoFar / (float) iTotalNumberOfLines;
                progress = (int) ( progressF * 100.0f );
                //
                if ( iProgressLastReported != progress )
                {
                    iProgressLastReported = progress;
                    report = true;
                }
                //
                if ( report )
                {
                    System.Diagnostics.Debug.WriteLine( "SymbolFileReader - progress: " + progress );
                    iSource.ReportEvent( SymSource.TEvent.EReadingProgress, progress );
                }
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From MultiThreadedProcessor
        protected override void OnEvent( MultiThreadedProcessor<SymbolFileSegment>.TEvent aEvent )
        {
            base.OnEvent( aEvent );
            //
            switch ( aEvent )
            {
            case MultiThreadedProcessor<SymbolFileSegment>.TEvent.EEventStarting:
                iSource.ReportEvent( SymSource.TEvent.EReadingStarted );
                break;
            case MultiThreadedProcessor<SymbolFileSegment>.TEvent.EEventCompleted:
                iSource.ReportEvent( SymSource.TEvent.EReadingComplete );
                this.Dispose();
                System.Diagnostics.Debug.WriteLine( string.Format( "[Symbol Memory] END   -> {0:d12}, source: {1}", System.GC.GetTotalMemory( true ), iSource.FileName ) );
                break;
            default:
                break;
            }
        }

        protected override bool Process( SymbolFileSegment aItem )
        {
            SymbolFileSegmentReader reader = new SymbolFileSegmentReader( aItem );
            reader.Progress += new SymbolFileSegmentReader.ProgressHandler( SymbolFileSegmentReader_Progress );
            reader.Read( TSynchronicity.ESynchronous );
            return true;
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
                iData.Dispose();
                
                // Since we've just flushed a large portion of file data (symbol files are BIG)
                // ensure that disposed objects are released in order to reduce memory footprint.
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        #endregion

        #region Data members
        private readonly SymbolSource iSource;
        private readonly SymbolFileData iData;
        private readonly long iTotalNumberOfLines;
        private long iProgressSoFar = 0;
        private object iSyncRoot = new object();
        private int iProgressLastReported = -1;
        #endregion
	}
}
