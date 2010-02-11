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
using System.IO;
using SymbianDebugLib.PluginAPI.Types.Symbol;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Common.FileName;
using SymbianStructuresLib.Debug.Symbols;
using SymbianSymbolLib.QueryAPI;
using SymbianSymbolLib.Relocator;
using SymbianSymbolLib.SourceManagement.Source;
using SymbianUtils.FileSystem.FilePair;

namespace SymbianSymbolLib.DbgEnginePlugin
{
    internal class SymbolView : DbgViewSymbol
    {
        #region Constructors
        public SymbolView( string aName, SymbolPlugin aPlugin )
            : base( aName, aPlugin )
		{
            iRelocator = new SymbolRelocator( aPlugin );
            iQueryAPI = new SymbolQueryAPI( iRelocator );
		}
		#endregion

        #region From DbgPluginView
        public override bool Contains( uint aAddress )
        {
            bool ret = iQueryAPI.Contains( aAddress );
            return ret;
        }

        public override bool Activate( CodeSegDefinition aCodeSegment )
        {
            bool activated = ActivateAndGetCollection( aCodeSegment ) != null;
            return activated;
        }

        public override bool Deactivate( CodeSegDefinition aCodeSegment )
        {
            return iRelocator.Deactivate( aCodeSegment );
        }

        public override bool SerializeTaggedCollections( FileNamePairCollection aFilesToSave )
        {
            int initialFileCount = aFilesToSave.Count;
    
            // Will contain tagged fixed collections
            SymbolCollectionList symColFixed = new SymbolCollectionList();

            // Will contain dynamically relocated collections
            SymbolCollectionList symColDynamicRelocations = new SymbolCollectionList();

            // Find tagged collections
            string fixedSymbolCollectionFileName = string.Empty;
            SymSourceManager sourceManager = iQueryAPI.SourceManager;
            foreach ( SymSource source in sourceManager )
            {
                int count = source.Count;
                for( int i=0; i<count; i++ )
                {
                    SymbolCollection col = source[ i ];
                    if ( col.Tagged )
                    {
                        if ( col.IsFixed )
                        {
                            symColFixed.Add( col );

                            // Save the ROM symbol file name (if present)
                            if ( string.IsNullOrEmpty( fixedSymbolCollectionFileName ) )
                            {
                                fixedSymbolCollectionFileName = source.FileName;
                            }
                        }
                        else
                        {
                            symColDynamicRelocations.Add( col );
                        }
                    }
                }
            }

            // Now save them to needed files. We create one file for all the fixed
            // collections and then individual files for all the dynamically relocated
            // collections.
            //
            // In all cases, we create temporary files which are to be deleted by
            // the client.
            if ( symColFixed.Count > 0 )
            {
                SerializeCollection( aFilesToSave, Path.GetFileNameWithoutExtension( fixedSymbolCollectionFileName ) + ".symbol", symColFixed );
            }
            if ( symColDynamicRelocations.Count > 0 )
            {
                foreach ( SymbolCollection col in symColDynamicRelocations )
                {
                    // For the dynamically relocated collections, we must generate a file name based
                    // upon the collection details.
                    string fileName = Path.GetFileNameWithoutExtension( col.FileName.EitherFullNameButDevicePreferred ) + ".symbol";
                    SerializeCollection( aFilesToSave, fileName, col );
                }
            }
            
            return ( aFilesToSave.Count != initialFileCount );
        }

        public override bool IsReady
        {
            get
            {
                // For a view to be ready we must have at least one
                // activated, i.e. 'ready' symbol source.
                int count = 0;

                // Check with dynamic activations
                foreach ( SymSourceAndCollection pair in iRelocator )
                {
                    if ( pair.Source.TimeToRead == SymSource.TTimeToRead.EReadWhenPriming )
                    {
                        ++count;
                        break; // No need to count anymore
                    }
                }

                // Try to find any fixed activation entries
                if ( count == 0 )
                {
                    SymSourceManager allSources = this.SourceManager;
                    foreach ( SymSource source in allSources )
                    {
                        count += source.CountActivated;
                        if ( count > 0 )
                        {
                            break;
                        }
                    }
                }

                return ( count > 0 );
            }
        }
        #endregion

        #region From DbgViewSymbols
        public override Symbol Lookup( uint aAddress, out SymbolCollection aCollection )
        {
            Symbol ret = iQueryAPI.Lookup( aAddress, out aCollection );
            return ret;
        }

        public override SymbolCollection CollectionByAddress( uint aAddress )
        {
            SymbolCollection ret = iQueryAPI.CollectionByAddress( aAddress );
            return ret;
        }

        public override SymbolCollection ActivateAndGetCollection( CodeSegDefinition aCodeSegment )
        {
            SymbolCollection ret = iRelocator.Activate( aCodeSegment );
            return ret;
        }

        public override SymbolCollection this[ CodeSegDefinition aCodeSeg ]
        {
            get
            {
                SymbolCollection ret = iQueryAPI[ aCodeSeg ];
                return ret;
            }
        }

        public override SymbolCollection this[ PlatformFileName aFileName ]
        {
            get
            {
                SymbolCollection ret = iQueryAPI[ aFileName ];
                return ret;
            }
        }

        protected override IEnumerator<SymbolCollection> GetEnumeratorSymbolCollection()
        {
            return iQueryAPI.GetEnumerator();
        }
        #endregion

        #region API
        #endregion

		#region Properties
        internal SymbolPlugin Plugin
        {
            get { return base.Engine as SymbolPlugin; }
        }

        internal SymbolQueryAPI QueryAPI
        {
            get { return iQueryAPI; }
        }

        internal SymSourceManager SourceManager
        {
            get { return Plugin.SourceManager; }
        }
        #endregion

        #region Internal methods
        private void SerializeCollection( FileNamePairCollection aFilesToSave, string aProposedFileName, SymbolCollection aCollection )
        {
            SymbolCollectionList list = new SymbolCollectionList();
            list.Add( aCollection );
            SerializeCollection( aFilesToSave, aProposedFileName, list );
        }

        private void SerializeCollection( FileNamePairCollection aFilesToSave, string aProposedFileName, SymbolCollectionList aList )
        {
            string tempFileName = Path.GetTempFileName();
            //
            FileNamePair fileNamePair = new FileNamePair( tempFileName );
            fileNamePair.Destination = string.Format( "/Symbols/{0}", Path.GetFileName( aProposedFileName ) );
            fileNamePair.DeleteFile = true;
            
            // Make sure the collections are sorted in order
            aList.SortByCollectionAddress();

            using ( FileStream stream = new FileStream( tempFileName, FileMode.Create ) )
            {
                aList.Serialize( stream );
            }
            //
            aFilesToSave.Add( fileNamePair );
        }
        #endregion

        #region Data members
        private readonly SymbolQueryAPI iQueryAPI;
        private readonly SymbolRelocator iRelocator;
        #endregion
    }
}
