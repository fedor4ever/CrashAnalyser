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

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Messages
{
    internal class CXmlSegMessages : CXmlSegBase
	{
		#region Constructors
        public CXmlSegMessages()
            : base( SegConstants.Messages )
		{
		}
		#endregion

        #region From CXmlSegBase
        public override int SegmentPriority
        {
            get { return 140; }
        }
        #endregion

        #region From CXmlNode
        public override void XmlSerialize( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            iMessages = aParameters.Container.ChildrenByType<CIMessage>( CIElement.TChildSearchType.EEntireHierarchy );
            if ( iMessages.Count > 0 )
            {
                base.XmlSerialize( aParameters );
            }
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            foreach ( CIMessage message in iMessages )
            {
                CXmlMessage xmlMessage = new CXmlMessage( message );
                xmlMessage.XmlSerialize( aParameters );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        private CIElementList<CIMessage> iMessages = null;
        #endregion
	}
}
