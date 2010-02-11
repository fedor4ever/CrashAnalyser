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
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Code;
using SymbianStructuresLib.Debug.Code.Interfaces;
using SymbianCodeLib.SourceManagement.Source;
using SymbianCodeLib.SourceManagement.Provisioning;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianUtils.FileTypes;
using SymbianImageLib.Common.Image;
using SymbianImageLib.Common.Content;
using CLPluginImg.Source;
using CLPluginImg.Provider;

namespace CLPluginImg.Reader
{
    internal class ImgReader : DisposableObject
	{
        #region Constructors
        public ImgReader( ImgSource aSource, ITracer aTracer )
		{
            iSource = aSource;
            iImageContent = aSource.ImageContent;
            iImageContent.DecompressionEvent += new SIContent.DecompressionEventHandler( ImageContent_PreparationEvent );
		}
		#endregion

        #region API
        public void Read( TSynchronicity aSynchronicity )
        {
            iImageContent.PrepareContent( aSynchronicity );
        }
        #endregion

        #region Properties
        protected CodeCollection Collection
        {
            get
            {
                SymbianUtils.SymDebug.SymDebugger.Assert( iSource.Count == 1 );
                return iSource[ 0 ];
            }
        }
		#endregion

        #region Event handlers
        private void ImageContent_PreparationEvent( SIContent.TDecompressionEvent aEvent, SIContent aFile, object aData )
        {
            switch ( aEvent )
            {
            case SIContent.TDecompressionEvent.EEventDecompressionStarting:
                iSource.ReportEvent( CodeSource.TEvent.EReadingStarted );
                break;
            case SIContent.TDecompressionEvent.EEventDecompressionProgress:
                iSource.ReportEvent( CodeSource.TEvent.EReadingProgress,( aData != null && aData is Int32 ) ? (int) aData : 0 );
                break;
            case SIContent.TDecompressionEvent.EEventDecompressionComplete:
                OnComplete();
                break;
            }
        }
        #endregion

        #region Internal methods
        private void OnComplete()
        {
            iSource.ReportEvent( CodeSource.TEvent.EReadingComplete );
            this.Dispose();
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
                iImageContent.DecompressionEvent -= new SIContent.DecompressionEventHandler( ImageContent_PreparationEvent );
            }
        }
        #endregion

        #region Data members
        private readonly CodeSource iSource;
        private readonly SIContent iImageContent;
		#endregion
	}
}
