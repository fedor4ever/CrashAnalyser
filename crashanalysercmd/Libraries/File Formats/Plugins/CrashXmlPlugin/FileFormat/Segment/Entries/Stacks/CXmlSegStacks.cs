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

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Stacks
{
    internal class CXmlSegStacks : CXmlSegBase
	{
		#region Constructors
        public CXmlSegStacks()
            : base( SegConstants.Stacks )
		{
		}
		#endregion

        #region From CXmlSegBase
        public override int SegmentPriority
        {
            get { return 20; }
        }
        #endregion

        #region From CXmlNode
        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CIElementList<CIStack> stacks = aParameters.Container.ChildrenByType<CIStack>( CIElement.TChildSearchType.EEntireHierarchy );
            foreach ( CIStack stack in stacks )
            {
                CXmlStack xmlStack = new CXmlStack( stack );
                xmlStack.XmlSerialize( aParameters );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
		#endregion
	}
}
