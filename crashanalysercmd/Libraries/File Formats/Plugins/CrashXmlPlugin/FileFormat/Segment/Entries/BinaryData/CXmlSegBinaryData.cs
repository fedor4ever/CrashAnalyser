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
using CrashItemLib.Crash.BinaryData;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.BinaryData
{
    internal class CXmlSegBinaryData : CXmlSegBase
	{
		#region Constructors
        public CXmlSegBinaryData()
            : base( SegConstants.BinaryData )
		{
		}
		#endregion

        #region From CXmlSegBase
        public override int SegmentPriority
        {
            get { return 120; }
        }
        #endregion

        #region From CXmlNode
        public override void XmlSerialize( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            iList = aParameters.Container.ChildrenByType<CIBinaryData>( CIElement.TChildSearchType.EEntireHierarchy );
            //
            if ( iList != null && iList.Count > 0 )
            {
                base.XmlSerialize( aParameters );
            }
            //
            iList = null;
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            System.Diagnostics.Debug.Assert( iList != null );
            //
            foreach ( CIBinaryData blob in iList )
            {
                CXmlBlob xmlBlob = new CXmlBlob( blob );
                xmlBlob.XmlSerialize( aParameters );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        private CIElementList<CIBinaryData> iList = null;
		#endregion
	}
}
