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
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Code;
using SymbianStructuresLib.Debug.Code.Interfaces;
using SymbianCodeLib.SourceManagement.Source;
using SymbianCodeLib.SourceManagement.Provisioning;
using SymbianUtils;
using SymbianUtils.Range;
using SymbianUtils.FileTypes;
using SymbianImageLib.Common.Image;
using SymbianImageLib.Common.Content;
using SymbianImageLib.ROM.Image;
using CLPluginImg.Provider;
using CLPluginImg.Reader;

namespace CLPluginImg.Source
{
    internal class ImgSource : CodeSource, ICodeCollectionInstructionConverter
    {
        #region Constructors
        public ImgSource( string aURI, CodeSourceProvider aProvider, CodeCollection aCollection, SIContent aImageContent )
            : base( aURI, aProvider )
        {
            iImageContent = aImageContent;

            // Make sure we receive any requests from the collection object for code.
            aCollection.IfaceInstructionConverter = this;
            aCollection.IsRelocatable = aImageContent.IsRelocationSupported;

            // XIP content should be read during priming. 
            TTimeToRead timeToRead = TTimeToRead.EReadWhenPriming;
            if ( aImageContent.IsRelocationSupported )
            {
                timeToRead = TTimeToRead.EReadWhenNeeded;
            }
            else
            {
                // If the image is fixed, then so is the collection base address
                aCollection.Relocate( aImageContent.RelocationAddress );
            }

            // Must add the collection *after* setting it's properties
            base.TimeToRead = timeToRead;
            base.Add( aCollection );
        }
        #endregion
        
        #region From CodeSource
        protected override void DoRead( TSynchronicity aSynchronicity )
        {
            ImgReader reader = new ImgReader( this, Provider.Tracer );
            reader.Read( aSynchronicity );
        }

        protected override void OnReadComplete()
        {
            try
            {
                base.OnReadComplete();
            }
            finally
            {
                System.Diagnostics.Debug.Assert( base.Count == 1 );
                CodeCollection col = this[ 0 ];
                bool codeAvailable = col.IsCodeAvailable;
                //
                if ( iImageContent != null && codeAvailable == false )
                {
                    // Update the underlying collection with it.
                    col.Code = iImageContent.GetAllData();

                    // Don't need this anymore
                    iImageContent.Dispose();
                    iImageContent = null;
                }
            }
        }
        #endregion

        #region Properties
        public SIContent ImageContent
        {
            get { return iImageContent; }
        }

        public new ImgSourceProvider Provider
        {
            get
            {
                ImgSourceProvider provider = (ImgSourceProvider) base.Provider;
                return provider;
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
                if ( iImageContent != null )
                {
                    iImageContent.Dispose();
                    iImageContent = null;
                }
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private SIContent iImageContent;
        #endregion
    }
}
