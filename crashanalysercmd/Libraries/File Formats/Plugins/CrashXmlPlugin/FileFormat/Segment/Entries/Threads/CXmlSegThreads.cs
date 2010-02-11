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
using CrashItemLib.Crash.Threads;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Threads
{
    internal class CXmlSegThreads : CXmlSegBase
	{
		#region Constructors
        public CXmlSegThreads()
            : base( SegConstants.Threads )
		{
		}
		#endregion

        #region From CXmlSegBase
        public override int SegmentPriority
        {
            get { return 50; }
        }
        #endregion

        #region From CXmlNode
        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CIElementList<CIThread> threads = aParameters.Container.ChildrenByType<CIThread>( CIElement.TChildSearchType.EEntireHierarchy );
            foreach ( CIThread thread in threads )
            {
                CXmlThread xmlThread = new CXmlThread( thread );
                xmlThread.XmlSerialize( aParameters );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
		#endregion
	}
}
