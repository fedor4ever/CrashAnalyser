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
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Events;
using CrashXmlPlugin.FileFormat.Node;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Events
{
    internal class CXmlEvent : CXmlNode
	{
		#region Constructors
        public CXmlEvent( CIEvent aEvent )
            : base( SegConstants.EventLog_Event )
		{
            iEvent = aEvent;
		}
		#endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            aParameters.Writer.WriteAttributeString( SegConstants.CmnType, iEvent.TypeName );
            aParameters.Writer.WriteString( iEvent.Value.ToString() );
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        private readonly CIEvent iEvent;
		#endregion
	}
}
