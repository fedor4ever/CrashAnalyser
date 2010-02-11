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
using CrashItemLib.Crash.InfoSW;
using CrashItemLib.Crash.Utils;
using CrashXmlPlugin.FileFormat.Versions;
using CrashXmlPlugin.FileFormat.Node;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.InfoSW
{
	internal class CXmlSegInfoSW : CXmlSegBase
	{
		#region Constructors
        public CXmlSegInfoSW()
            : base( SegConstants.SWInfo )
		{
		}
		#endregion

        #region From CXmlSegBase
        public override int SegmentPriority
        {
            get { return 80; }
        }
        #endregion

        #region From CXmlNode
        public override void XmlSerialize( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CIInfoSW info = (CIInfoSW) aParameters.Container.ChildByType( typeof( CIInfoSW ) );
            if ( info != null )
            {
                base.XmlSerialize( aParameters );
            }
        }

        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CIInfoSW info = (CIInfoSW) aParameters.Container.ChildByType( typeof( CIInfoSW ) );
            if ( info != null )
            {
                // Checksum
                if ( info.ImageCheckSum != 0 )
                {
                    aParameters.Writer.WriteElementString( SegConstants.CmnChecksum, info.ImageCheckSum.ToString( "x8" ) );
                }

                // Date and time
                CXmlNode.WriteDate( aParameters.Writer, info.ImageTimeStamp, SegConstants.CmnDate );
                CXmlNode.WriteTime( aParameters.Writer, info.ImageTimeStamp, SegConstants.CmnTime );

                // Platform
                string platform = info.Platform;
                WriteStringIfNotEmpty( aParameters.Writer, SegConstants.SWInfo_Platform, platform );

                // Language
                string language = info.Language;
                WriteStringIfNotEmpty( aParameters.Writer, SegConstants.SWInfo_Language, language );
            }
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CIInfoSW info = (CIInfoSW) aParameters.Container.ChildByType( typeof( CIInfoSW ) );
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
