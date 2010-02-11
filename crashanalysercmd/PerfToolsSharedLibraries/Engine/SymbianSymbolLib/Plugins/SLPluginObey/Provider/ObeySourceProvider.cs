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
using SymbianSymbolLib.SourceManagement.Source;
using SymbianSymbolLib.SourceManagement.Provisioning;
using SymbianUtils.FileTypes;
using SymbianUtils;
using SLPluginObey.Reader;
using SLPluginObey.Source;

namespace SLPluginObey.Provider
{
    public class ObeySourceProvider : SymSourceProvider
    {
        #region Constructors
        public ObeySourceProvider( SymSourceProviderManager aManager )
            : base( aManager )
        {
        }
        #endregion

        #region From SymSourceProvider
        public override bool IsSupported( string aFileName )
        {
            bool ret = base.IsSupported( aFileName );
            if ( ret )
            {
                FindMapProvider();

                // Check that the MAP file source is supported
                bool exists = File.Exists( aFileName );
                ret = ( iMapProvider != null ) && exists;
            }
            //
            return ret;
        }

        public override SymSourceCollection CreateSources( string aFileName )
        {
            System.Diagnostics.Debug.Assert( iMapProvider != null );

            // Read OBY file and resolve to map files in host file system.
            iTransientSources = new SymSourceCollection();
            try
            {
                ObeyFileReader reader = new ObeyFileReader( aFileName );
                reader.EntryRead += new ObeyFileReader.ObyEntryHandler( Reader_EntryRead );
                reader.Read( TSynchronicity.ESynchronous );
            }
            catch ( Exception )
            {
                iTransientSources.Clear();
            }

            // This source provider doesn't directly create any sources
            SymSourceCollection ret = iTransientSources;
            iTransientSources = null;
            return ret;
        }

        public override SymFileTypeList FileTypes
        {
            get
            {
                SymFileTypeList ret = new SymFileTypeList();
                //
                ret.Add( new SymFileType( ".oby", "Symbian OS Obey Files" ) );
                //
                return ret;
            }
        }

        public override string Name
        {
            get { return "OBY"; }
        }
        #endregion

        #region Properties
        #endregion

        #region Event handlers
        private void Reader_EntryRead( ObeyFileReader aReader, string aHost, string aDevice )
        {
            // Check if there is a corresponding map file in the host file system.
            FileInfo mapFile = new FileInfo( aHost + KMapFileExtension );
            if ( mapFile.Exists )
            {
                bool supported = iMapProvider.IsSupported( mapFile.FullName );
                if ( supported )
                {
                    // First get the map provider to create its own concrete list of 
                    // sources for the specified map file. This should just be a single
                    // source and a single collection since that is the logical encapsulating
                    // domain for a given map file.
                    SymSourceCollection mapSources = iMapProvider.CreateSources( mapFile.FullName );

                    // Next, we iterate through any sources (probably just one) and then
                    // extract the collection & provider that was specified for that source.
                    //
                    // We want to effectively sit the OBY source plugin in between the symbol
                    // engine and the map plugin so that it can ensure that we only read a
                    // given map file at the point when the map file symbol collection is activated.
                    foreach ( SymSource mapSource in mapSources )
                    {
                        SymSource obeySource = new ObeySource( mapSource.URI, this, mapSource );
                        iTransientSources.Add( obeySource );
                    }
                }
                else
                {
                    SymbianUtils.SymDebug.SymDebugger.Break();
                }
            }
        }
        #endregion

        #region Internal constants
        private const string KMapFileExtension = ".map";
        #endregion

        #region Internal methods
        private void FindMapProvider()
        {
            if ( iMapProvider == null )
            {
                iMapProvider = base.ProvisioningManager[ "MAP" ];
            }
        }
        #endregion

        #region Data members
        private SymSourceProvider iMapProvider;
        private SymSourceCollection iTransientSources = null;
        #endregion
    }
}
