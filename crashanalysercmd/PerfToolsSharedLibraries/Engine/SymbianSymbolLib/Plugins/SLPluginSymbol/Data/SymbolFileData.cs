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
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianUtils.FileTypes;
using SymbianUtils.Threading;
using SLPluginSymbol.Source;

namespace SLPluginSymbol.Data
{
    internal class SymbolFileData : DisposableObject, IEnumerable<SymbolFileSegment>
    {
        #region Delegates & events
        public delegate void DataPreparedHandler( SymbolFileData aData );
        public event DataPreparedHandler DataPrepared = delegate { };
        #endregion

        #region Constructors
        public SymbolFileData( SymbolSource aSource )
		{
            iSource = aSource;
            iData = File.ReadAllBytes( aSource.URI );
        }
        #endregion

        #region API
        public void Split( int aSegmentCount )
        {
            int chunkSize = (int) iData.Length / aSegmentCount;
            int blockPosStart = 0;
            int length = 0;
            //
            for ( int i = 0; i < aSegmentCount; i++ )
            {
                int pos = 0;
                int blockPosEnd = Math.Min( iData.Length - 1, blockPosStart + chunkSize );
                while ( pos >= 0 )
                {
                    pos = Array.IndexOf( iData, KPattern[ 0 ], blockPosEnd );
                    if ( pos > 0 )
                    {
                        if ( pos + 8 >= iData.Length )
                        {
                            break;
                        }
                        else if ( pos + 8 < iData.Length && iData[ pos + 7 ] == KPattern[ 7 ] )
                        {
                            bool isMatch = CompareByteArrays( KPattern, iData, pos );
                            if ( isMatch )
                            {
                                length = pos - blockPosStart;
                                //System.Diagnostics.Debug.WriteLine( string.Format( "Block {0:d2} @ 0x{1:x8}, length: {2:d8}", i, blockPosStart, length ) );
                                //
                                iSegments.Add( new SymbolFileSegment( this, blockPosStart, length ) );
                                blockPosStart = pos;
                                break;
                            }
                            else
                            {
                                // Didn't find a match, move forwards
                                blockPosEnd = pos + 1;
                            }
                        }
                        else
                        {
                            // Didn't find a match, move forwards
                            blockPosEnd = pos + 1;
                        }
                    }
                    else
                    {
                        // Searched to end of file and didn't find another block, so just create
                        // a new reader for everything that remains.
                        length = iData.Length - blockPosStart;
                        //System.Diagnostics.Debug.WriteLine( string.Format( "Block {0:d2} @ 0x{1:x8}, length: {2:d8}", i, blockPosStart, length ) );
                        iSegments.Add( new SymbolFileSegment( this, blockPosStart, length ) );
                        //
                        break;
                    }
                }
            }

            iCollectionIdentifier = new MultiThreadedProcessor<SymbolFileSegment>( this );
        }

        public void FindCollections()
        {
            System.Diagnostics.Debug.Assert( iCollectionIdentifier != null );
            iCollectionIdentifier.ProcessItem += new MultiThreadedProcessor<SymbolFileSegment>.ItemProcessor( CollectionIdentifier_ProcessItem );
            iCollectionIdentifier.EventHandler += new MultiThreadedProcessor<SymbolFileSegment>.ProcessorEventHandler( CollectionIdentifier_EventHandler );
            iCollectionIdentifier.Start( TSynchronicity.EAsynchronous );
        }
        #endregion

        #region Properties
        public SymbolSource Source
        {
            get { return iSource; }
        }
        #endregion

        #region Operators
        public static implicit operator byte[]( SymbolFileData aData )
        {
            return aData.iData;
        }
        #endregion

        #region Internal constants
        private static readonly byte[] KPattern = new byte[] { (byte) 'F', (byte) 'r', (byte) 'o', (byte) 'm', (byte) ' ', (byte) ' ', (byte) ' ', (byte) ' ' };
        #endregion

        #region Event handlers
        private void CollectionIdentifier_EventHandler( MultiThreadedProcessor<SymbolFileSegment>.TEvent aEvent )
        {
            switch ( aEvent )
            {
            default:
                break;
            case MultiThreadedProcessor<SymbolFileSegment>.TEvent.EEventStarting:
                break;
            case MultiThreadedProcessor<SymbolFileSegment>.TEvent.EEventCompleted:
                iCollectionIdentifier.ProcessItem -= new MultiThreadedProcessor<SymbolFileSegment>.ItemProcessor( CollectionIdentifier_ProcessItem );
                iCollectionIdentifier.EventHandler -= new MultiThreadedProcessor<SymbolFileSegment>.ProcessorEventHandler( CollectionIdentifier_EventHandler );
                iCollectionIdentifier.Dispose();
                iCollectionIdentifier = null;
                DataPrepared( this );
                break;
            }
        }

        private void CollectionIdentifier_ProcessItem( SymbolFileSegment aItem )
        {
            aItem.PerformInitialCollectionIdentification();
        }
        #endregion

        #region Internal methods
        private static bool CompareByteArrays( byte[] aSearchFor, byte[] aSearchIn, int aStartPos )
        {
            bool areEqual = true;
            //
            for ( int i = 0; i < aSearchFor.Length; i++ )
            {
                byte c = aSearchFor[ i ];

                int bigBufferIndex = aStartPos + i;
                if ( bigBufferIndex > aSearchIn.Length )
                {
                    // We ran out of data
                    areEqual = false;
                    break;
                }
                else
                {
                    byte b = aSearchIn[ bigBufferIndex ];
                    if ( b != c )
                    {
                        areEqual = false;
                        break;
                    }
                }
            }
            //
            return areEqual;
        }
        #endregion
        
        #region From IEnumerable<SymbolFileSegment>
        public IEnumerator<SymbolFileSegment> GetEnumerator()
        {
            foreach( SymbolFileSegment segment in iSegments )
            {
                yield return segment;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( SymbolFileSegment segment in iSegments )
            {
                yield return segment;
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
                foreach ( SymbolFileSegment seg in iSegments )
                {
                    seg.Dispose();
                }
                //
                if ( iCollectionIdentifier != null )
                {
                    iCollectionIdentifier.Dispose();
                    iCollectionIdentifier = null;
                }
                //
                iSegments.Clear();
                iData = null;
            }
        }
        #endregion

        #region Data members
        private readonly SymbolSource iSource;
        private byte[] iData = null;
        private List<SymbolFileSegment> iSegments = new List<SymbolFileSegment>();
        private MultiThreadedProcessor<SymbolFileSegment> iCollectionIdentifier;
        #endregion
    }
}
