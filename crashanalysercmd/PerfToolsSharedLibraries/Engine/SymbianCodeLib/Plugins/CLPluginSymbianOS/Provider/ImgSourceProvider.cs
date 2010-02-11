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
using SymbianStructuresLib.Debug.Code;
using SymbianCodeLib.SourceManagement.Source;
using SymbianCodeLib.SourceManagement.Provisioning;
using SymbianUtils.FileTypes;
using SymbianUtils;
using SymbianImageLib.Common.Image;
using SymbianImageLib.Common.Content;
using CLPluginImg.Reader;
using CLPluginImg.Source;

namespace CLPluginImg.Provider
{
    public class ImgSourceProvider : CodeSourceProvider
    {
        #region Constructors
        public ImgSourceProvider( CodeSourceProviderManager aManager )
            : base( aManager )
        {
        }
        #endregion
          
        #region From SymSourceProvider
        public override CodeSourceCollection CreateSources( string aFileName )
        {
            SIImage image = SIImage.New( base.Tracer, aFileName );
            if ( image == null )
            {
                throw new NotSupportedException( "The specified image file is not supported" );
            }

            // We need to make a source and (single) collection for each content object within the image.
            // This enables relocation support when an image is actually used (i.e. read & decompress code
            // on demand, rather than up front). 
            CodeSourceCollection sources = new CodeSourceCollection();
            //
            foreach ( SIContent content in image )
            {
                CodeCollection collection = CodeCollection.New( base.IdAllocator, aFileName, content.FileName );
                collection.IsRelocatable = content.IsRelocationSupported;

                // The URI must be unique
                string uri = string.Format( "{0} [{1}]", aFileName, content.FileName );
                ImgSource source = new ImgSource( uri, this, collection, content );
                sources.Add( source );
            }
            //
            return sources;
        }

        public override SymFileTypeList FileTypes
        {
            get
            {
                SymFileTypeList ret = new SymFileTypeList();
                //
                ret.Add( new SymFileType( ".img", "Symbian OS Image Files" ) );
                //
                return ret;
            }
        }

        public override string Name
        {
            get { return "SYMBIAN OS IMAGE"; }
        }
        #endregion

        #region Properties
        #endregion

        #region Event handlers

        #endregion

        #region Internal constants
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
