/*
* Copyright (c) 2009 Nokia Corporation and/or its subsidiary(-ies). 
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
using System.Text;
using System.Collections.Generic;
using CrashItemLib.Crash.InfoHW;
using CrashItemLib.Crash.Utils;
using CrashXmlPlugin.FileFormat.Versions;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.InfoHW
{
	internal class CXmlSegInfoHW : CXmlSegBase
	{
		#region Constructors
        public CXmlSegInfoHW()
            : base( SegConstants.HWInfo )
		{
		}
		#endregion

        #region From CXmlSegBase
        public override int SegmentPriority
        {
            get { return 70; }
        }
        #endregion

        #region From CXmlNode
        public override void XmlSerialize( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CIInfoHW info = (CIInfoHW) aParameters.Container.ChildByType( typeof( CIInfoHW ) );
            if ( info != null )
            {
                base.XmlSerialize( aParameters );
            }
        }

        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CIInfoHW info = (CIInfoHW) aParameters.Container.ChildByType( typeof( CIInfoHW ) );
            if ( info != null )
            {
                // Product type
                string productType = info.ProductType;
                WriteStringIfNotEmpty( aParameters.Writer, SegConstants.HWInfo_ProductType, productType );

                // Product code
                string productCode = info.ProductCode;
                WriteStringIfNotEmpty( aParameters.Writer, SegConstants.HWInfo_ProductCode, productCode );

                // Serial number
                string serialNumber = info.SerialNumber;
                WriteStringIfNotEmpty( aParameters.Writer, SegConstants.HWInfo_SerialNumber, serialNumber );
            }
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CIInfoHW info = (CIInfoHW) aParameters.Container.ChildByType( typeof( CIInfoHW ) );
            if ( info != null )
            {
                int count = info.VersionCount;
                if ( count > 0 )
                {
                    CXmlVersionText.WriteVersionTextListStart( aParameters.Writer );
                    foreach ( CIVersionInfo version in info )
                    {
                        if ( version.IsValid )
                        {
                            CXmlVersionText xmlVersion = new CXmlVersionText( version.Value );
                            xmlVersion.XmlSerialize( aParameters );
                        }
                    }
                    CXmlVersionText.WriteVersionTextListEnd( aParameters.Writer );
                }
            }
        }
        #endregion

        #region Properties
        #endregion

        #region From SegBase
        #endregion

        #region Data members
        #endregion
    }
}
