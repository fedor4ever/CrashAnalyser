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
using CrashItemLib.Crash.Events;
using CrashItemLib.Crash.Utils;
using CrashXmlPlugin.FileFormat.Versions;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Events
{
	internal class CXmlSegEventLog : CXmlSegBase
	{
		#region Constructors
        public CXmlSegEventLog()
            : base( SegConstants.EventLog )
		{
		}
		#endregion

        #region From CXmlSegBase
        public override int SegmentPriority
        {
            get { return 90; }
        }
        #endregion

        #region From CXmlNode
        public override void XmlSerialize( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CIEventList events = aParameters.Container.Events;
            if ( events.Count > 0 )
            {
                base.XmlSerialize( aParameters );
            }
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CIEventList events = aParameters.Container.Events;
            foreach( CIEvent e in events )
            {
                CXmlEvent xmlEvent = new CXmlEvent( e );
                xmlEvent.XmlSerialize( aParameters );
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
