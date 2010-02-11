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
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Registers.Visualization;
using CrashItemLib.Crash.Registers.Visualization.Bits;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.ValueInterpretations
{
    internal class CXmlSegValueInterpretations : CXmlSegBase
	{
		#region Constructors
        public CXmlSegValueInterpretations()
            : base( SegConstants.ValueInterpretation )
		{
		}
		#endregion

        #region From CXmlSegBase
        public override int SegmentPriority
        {
            get { return 130; }
        }
        #endregion

        #region From CXmlNode
        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CIElementList<CIRegisterVisualization> visList = aParameters.Container.ChildrenByType<CIRegisterVisualization>( CIElement.TChildSearchType.EEntireHierarchy );
            foreach ( CIRegisterVisualization visualisation in visList )
            {
                CXmlValueInterpretation xmlVis = new CXmlValueInterpretation( visualisation );
                xmlVis.XmlSerialize( aParameters );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
		#endregion
	}
}
