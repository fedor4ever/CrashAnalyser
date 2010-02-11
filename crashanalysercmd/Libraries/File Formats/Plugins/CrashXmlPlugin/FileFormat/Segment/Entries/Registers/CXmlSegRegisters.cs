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
using CrashItemLib.Crash.Symbols;
using CrashItemLib.Crash.Stacks;
using CrashItemLib.Crash.Registers;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Registers
{
    internal class CXmlSegRegisters : CXmlSegBase
	{
		#region Constructors
        public CXmlSegRegisters()
            : base( SegConstants.Registers )
		{
		}
		#endregion

        #region From CXmlSegBase
        public override int SegmentPriority
        {
            get { return 40; }
        }
        #endregion

        #region From CXmlNode
        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CIElementList<CIRegisterListCollection> regListCols = aParameters.Container.ChildrenByType<CIRegisterListCollection>( CIElement.TChildSearchType.EEntireHierarchy );
            foreach ( CIRegisterListCollection regListCol in regListCols )
            {
                foreach ( CIRegisterList regList in regListCol )
                {
                    if ( regList.Count > 0 )
                    {
                        CXmlRegisterSet xmlRegList = new CXmlRegisterSet( regList );
                        xmlRegList.XmlSerialize( aParameters );
                    }
                }
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
		#endregion
	}
}
