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
using CrashItemLib.Crash.Messages;
using CrashXmlPlugin.FileFormat.Node;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Messages
{
    internal class CXmlMessage : CXmlNode
	{
		#region Constructors
        public CXmlMessage( CIMessage aMessage )
            : base( SegConstants.Messages_Message )
		{
            iMessage = aMessage;
		}
		#endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CXmlNode.WriteId( iMessage, aParameters.Writer );
            aParameters.Writer.WriteElementString( SegConstants.CmnType, iMessage.TypeName );
            aParameters.Writer.WriteElementString( SegConstants.Messages_Message_Title, iMessage.Title );
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            foreach ( string line in iMessage )
            {
                aParameters.Writer.WriteElementString( SegConstants.Messages_Message_Line, line );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        private readonly CIMessage iMessage;
		#endregion
	}
}
