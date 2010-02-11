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

namespace SLPluginObey.Source
{
    internal class ObeySource : SymSource, ISymbolCollectionRelocationHandler
    {
        #region Constructors
        public ObeySource( string aURI, SymSourceProvider aProvider, SymSource aOriginalMapSource )
            : base( aURI, aProvider )
        {
            foreach ( SymbolCollection mapCollection in aOriginalMapSource )
            {
                mapCollection.IfaceRelocationHandler = this as ISymbolCollectionRelocationHandler;
                base.Add( mapCollection );
            }
            iOriginalMapSource = aOriginalMapSource;
        }
        #endregion

        #region API
        #endregion

        #region From SymSource
        public override void Read( TSynchronicity aSynchronicity )
        {
            // This method is typically called when the engine is primed with a list of files.
            // We've already read the OBY file content, so there's no need to do anything there
            // at all. In fact, this method is never invoked in that scenario because the OBY
            // file is never registered as a SymSource.
            //
            // The usual invocation context for this method is when a map file prime request is
            // received. Because we associated every MAP file [that we were able to locate from the 
            // OBY data] with the OBY provider, we can intercept the requests to read the MAP content.
            //
            // This allows us to ignore those read requests that occur during priming, and thereby 
            // allow "on demand" reading of MAP file content only when an explicit code segment 
            // activation (relocation/fixup) API call is made to the SymbolView class.
            //
            // When the symbol collection is activated (relocated/fixed up) we will be notified by
            // way of the SymbolCollection's "relocated" event. This will then allow us to syncronously
            // read the MAP file content and update the collection with a list of real symbols. All of
            // this is managed by the ObeySource class.
            //
            // This therefore explains why this method is implemented, but is empty.
            // Also see Reader_EntryRead for further details.
            base.ReportEvent( TEvent.EReadingComplete );
        }
        #endregion

        #region Properties
        public SymSource OriginalMapSource
        {
            get { return iOriginalMapSource; }
        }
        #endregion

        #region Event handlers
        #endregion

        #region Internal constants
        #endregion

        #region Internal methods
        #endregion

        #region ISymbolCollectionRelocationHandler Members
        void ISymbolCollectionRelocationHandler.PrepareForRelocation( SymbolCollection aCollection, uint aOldBase, uint aNewBase )
        {
            // This is invoked when a map file is activated. Because we've probably not yet read the
            // source content, we will synchronously parse the map file now so that we have a full set
            // of symbols available to the client.
            if ( aCollection.IsEmptyApartFromDefaultSymbol )
            {
                // This will read into the original map source and original map collection.
                iOriginalMapSource.Read( TSynchronicity.ESynchronous );

                // If the specified aCollection is a clone, i.e. not the original, then we may need
                // to copy symbols over.
                if ( iOriginalMapSource.Count > 0 )
                {
                    SymbolCollection primaryMapCollection = iOriginalMapSource[ 0 ];
                    if ( aCollection != primaryMapCollection )
                    {
                        aCollection.Clone( primaryMapCollection );
                    }
                }
            }
        }
        #endregion

        #region Data members
        private readonly SymSource iOriginalMapSource;
        #endregion
    }
}
