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
using SymbianStructuresLib.Debug.Symbols.Interfaces;
using SymbianSymbolLib.SourceManagement.Source;
using SymbianSymbolLib.SourceManagement.Provisioning;
using SymbianUtils.FileTypes;
using SymbianUtils;
using SLPluginMap.Utilities;
using SLPluginMap.Reader;
using SLPluginMap.Reader.RVCT;
using SLPluginMap.Reader.GCCE;

namespace SLPluginMap.Provider
{
    public class MapSourceProvider : SymSourceProvider
    {
        #region Constructors
        public MapSourceProvider( SymSourceProviderManager aManager )
            : base( aManager )
        {
        }
        #endregion
          
        #region From SymSourceProvider
        public override SymSourceCollection CreateSources( string aFileName )
        {
            // In order to get the host binary name, we remove the map file extension
            string fileName = aFileName;
            string extension = Path.GetExtension( fileName ).ToUpper();
            if ( extension.EndsWith( KMapFileExtension ) )
            {
                string name = fileName.Substring( 0, fileName.Length - extension.Length );
                fileName = name;
            }

            // A map file source contains just a single collection. We don't know
            // the device-side file name at this point.
            SymbolCollection collection = SymbolCollection.NewByHostFileName( base.IdAllocator, fileName );
            collection.IsRelocatable = true;
            //
            SymSource source = new SymSource( aFileName, this, collection );
            return new SymSourceCollection( source );
        }

        public override void ReadSource( SymSource aSource, TSynchronicity aSynchronicity )
        {
            // Need to work out if it's a GCCE or RVCT map file.
            TMapFileType type = MapFileUtils.Type( aSource.FileName );
            //
            MapReader reader = null;
            switch( type )
            { 
            case TMapFileType.ETypeRVCT:
                reader = new RVCTMapFileReader( aSource, base.Tracer );
                break;
            case TMapFileType.ETypeGCCE:
                reader = new GCCEMapFileReader( aSource, base.Tracer );
                break;
            case TMapFileType.ETypeUnknown:
            default:
                throw new NotSupportedException();
            }
            //
            reader.Read( aSynchronicity );
        }

        public override SymFileTypeList FileTypes
        {
            get
            {
                SymFileTypeList ret = new SymFileTypeList();
                //
                ret.Add( new SymFileType( ".map", "Map Files" ) );
                //
                return ret;
            }
        }

        public override string Name
        {
            get { return "MAP"; }
        }
        #endregion

        #region Properties
        #endregion

        #region Event handlers

        #endregion

        #region Internal constants
        private const string KMapFileExtension = ".MAP";
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
