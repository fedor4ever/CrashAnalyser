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
using CrashXmlPlugin.FileFormat.Segment;
using CrashXmlPlugin.FileFormat.Versions;
using CrashXmlPlugin.FileFormat.Node;
using CrashXmlPlugin.FileFormat.Document;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Messages;

namespace CrashXmlPlugin.FileFormat.Segment.Entries
{
    internal abstract class CXmlSegBase : CXmlNode
	{
		#region Constructors
        protected CXmlSegBase( string aName )
            : base( aName )
		{
		}
		#endregion

        #region Framework API
        public abstract int SegmentPriority
        {
            get;
        }
        #endregion

        #region API
        public static void XmlSerializeMessages( CXmlDocumentSerializationParameters aParameters, CIElement aElement )
        {
            CIElementList<CIMessage> messages = aElement.ChildrenByType<CIMessage>();
            foreach ( CIMessage msg in messages )
            {
                CXmlNode.WriteLink( msg.Id, SegConstants.Messages, aParameters.Writer );
            }
        }
        #endregion

        #region From CXmlNode
        public override void XmlSerialize( CXmlDocumentSerializationParameters aParameters )
        {
            iWasSerialized = true;
            base.XmlSerialize( aParameters );
        }
        #endregion

        #region Properties
        public CXmlVersionExtended Version
        {
            get { return iVersion; }
        }

        public bool WasSerialized
        {
            get { return iWasSerialized; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private CXmlVersionExtended iVersion = new CXmlVersionExtended();
        private bool iWasSerialized = false;
		#endregion
	}
}
