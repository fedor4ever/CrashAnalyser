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
using System.Text.RegularExpressions;
using SymbianStructuresLib.Debug.Symbols;
using SymbianSymbolLib.SourceManagement.Source;
using SymbianSymbolLib.SourceManagement.Provisioning;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianUtils.FileTypes;
using SLPluginSymbol.Source;

namespace SLPluginSymbol.Data
{
    internal class SymbolFileSegment : DisposableObject, IEnumerable<SymbolCollectionSegment>
    {
        #region Delegates & events
        public delegate void CompletionHandler( SymbolFileSegment aSegment );
        public event CompletionHandler Completed = null;
        #endregion

        #region Constructors
        public SymbolFileSegment( SymbolFileData aData, int aOffset, int aLength )
		{
            iData = aData;
            iOffset = aOffset;
            iLength = aLength;
		}
        #endregion

        #region API
        public void PerformInitialCollectionIdentification()
        {
            SymbolCollectionSegment current = null;
            //
            long lineCount = 0;
            //
            using ( StreamReader reader = NewStream() )
            {
                string line = reader.ReadLine();
                while ( line != null )
                {
                    ++lineCount;
                    if ( line.StartsWith( "From    " ) )
                    {
                        string hostFileName = line.Substring( 4 ).Trim();
                        UpdateLineCount( current );
                        current = NewCollectionSegment( hostFileName );
                    }
                    else if ( current != null && line != string.Empty )
                    {
                        current.AddLine( line );
                    }

                    line = reader.ReadLine();
                }
            }

            UpdateLineCount( current );
            iIsReady = true;

            if ( Completed != null )
            {
                Completed( this );
            }
        }
        #endregion

        #region Properties
        public bool IsReady
        {
            get { return iIsReady; }
        }

        public long NumberOfLines
        {
            get { return iNumberOfLines; }
        }

        public int Count
        {
            get { return iCollectionSegments.Count; }
        }

        public SymbolCollectionSegment this[ int aIndex ]
        {
            get { return iCollectionSegments[ aIndex ]; }
        }

        public SymbolSource Source
        {
            get { return iData.Source; }
        }
        #endregion

        #region Internal constants
        #endregion

        #region Internal methods
        private void UpdateLineCount( SymbolCollectionSegment aFinalisedSegment )
        {
            if ( aFinalisedSegment != null )
            {
                iNumberOfLines += aFinalisedSegment.Count;
            }
        }

        private SymbolCollectionSegment NewCollectionSegment( string aFileNameInHost )
        {
            SymbolCollectionSegment ret = new SymbolCollectionSegment( iData.Source.Provider.IdAllocator, aFileNameInHost );
            iCollectionSegments.Add( ret );
            return ret;
        }

        private StreamReader NewStream()
        {
            return new StreamReader( new MemoryStream( iData, iOffset, iLength ) );
        }
        #endregion

        #region From IEnumerable<SymbolCollectionSegment>
        public IEnumerator<SymbolCollectionSegment> GetEnumerator()
        {
            foreach ( SymbolCollectionSegment segment in iCollectionSegments )
            {
                yield return segment;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( SymbolCollectionSegment segment in iCollectionSegments )
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
                foreach ( SymbolCollectionSegment segment in iCollectionSegments )
                {
                    segment.Dispose();
                }
                iCollectionSegments.Clear();
                iCollectionSegments = null;
            }
        }
        #endregion

        #region Data members
        private readonly SymbolFileData iData;
        private readonly int iOffset;
        private readonly int iLength;
        private bool iIsReady = false;
        private long iNumberOfLines = 0;
        private List<SymbolCollectionSegment> iCollectionSegments = new List<SymbolCollectionSegment>();
        #endregion
    }
}
