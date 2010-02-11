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
using CrashItemLib.Crash.Memory;
using CrashItemLib.Crash.Utils;
using CrashXmlPlugin.FileFormat.Versions;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Memory
{
    internal class CXmlSegMemoryInfo : CXmlSegBase
	{
		#region Constructors
        public CXmlSegMemoryInfo()
            : base( SegConstants.MemoryInfo )
		{
		}
		#endregion

        #region From CXmlSegBase
        public override int SegmentPriority
        {
            get { return 110; }
        }
        #endregion

        #region From CXmlNode
        public override void XmlSerialize( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            iList = aParameters.Container.ChildrenByType<CIMemoryInfo>( CIElement.TChildSearchType.EEntireHierarchy );
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
            foreach ( CIMemoryInfo info in iList )
            {
                string name = SegConstants.MemoryInfo_Drive;
                if ( info.Type == CIMemoryInfo.TType.ETypeRAM )
                {
                    name = SegConstants.MemoryInfo_RAM;
                }
                //
                CXmlMemoryInfo xmlInfo = new CXmlMemoryInfo( info, name );
                xmlInfo.XmlSerialize( aParameters );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region From SegBase
        #endregion

        #region Data members
        private CIElementList<CIMemoryInfo> iList = null;
        #endregion
    }
}
