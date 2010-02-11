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
using CrashItemLib.Crash.CodeSegs;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Threads;
using CrashItemLib.Crash.Processes;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Processes
{
    internal class CXmlSegProcesses : CXmlSegBase
	{
		#region Constructors
        public CXmlSegProcesses()
            : base( SegConstants.Processes )
		{
		}
		#endregion

        #region From CXmlSegBase
        public override int SegmentPriority
        {
            get { return 60; }
        }
        #endregion

        #region From CXmlNode
        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CIElementList<CIProcess> processes = aParameters.Container.ChildrenByType<CIProcess>( CIElement.TChildSearchType.EEntireHierarchy );
            foreach ( CIProcess process in processes )
            {
                CXmlProcess xmlProcess = new CXmlProcess( process );
                xmlProcess.XmlSerialize( aParameters );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
		#endregion
	}
}
