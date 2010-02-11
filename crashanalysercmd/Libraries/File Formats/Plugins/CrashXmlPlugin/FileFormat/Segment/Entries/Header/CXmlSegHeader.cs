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
using CrashItemLib.Crash.Header;
using CrashXmlPlugin.FileFormat.Versions;
using CrashXmlPlugin.FileFormat.Node;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Header
{
	internal class CXmlSegHeader : CXmlSegBase
	{
		#region Constructors
        public CXmlSegHeader()
            : base( SegConstants.Header )
		{
		}
		#endregion

        #region From CXmlSegBase
        public override int SegmentPriority
        {
            get { return 0; }
        }
        #endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            if ( aParameters.Container.Status != CrashItemLib.Crash.Container.CIContainer.TStatus.EStatusErrorContainer )
            {
                CIHeader header = aParameters.Container.Header;
                DateTime crashTime = header.CrashTime;

                // Date and time
                CXmlNode.WriteDate( aParameters.Writer, crashTime, SegConstants.CmnDate );
                CXmlNode.WriteTime( aParameters.Writer, crashTime, SegConstants.CmnTime );

                // Uptime
                double uptime = header.UpTime.TotalSeconds;
                if ( uptime > 0 )
                {
                    aParameters.Writer.WriteElementString( SegConstants.Header_Uptime, uptime.ToString() );
                }

                // Underlying version information from crash file (if available)
                if ( header.FileFormatVersion.IsValid )
                {
                    CXmlVersionText version = new CXmlVersionText( header.FileFormatVersion );
                    version.XmlSerialize( aParameters );
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
