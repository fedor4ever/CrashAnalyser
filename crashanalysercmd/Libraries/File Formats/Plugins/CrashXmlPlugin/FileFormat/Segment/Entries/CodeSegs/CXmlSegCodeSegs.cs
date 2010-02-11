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
using CrashItemLib.Crash.CodeSegs;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.CodeSegs
{
    internal class CXmlSegCodeSegs : CXmlSegBase
	{
		#region Constructors
        public CXmlSegCodeSegs()
            : base( SegConstants.CodeSegs )
		{
		}
		#endregion

        #region From CXmlSegBase
        public override int SegmentPriority
        {
            get { return 30; }
        }
        #endregion

        #region From CXmlNode
        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            // Get the code segments
            CIElementList<CICodeSeg> codeSegs = aParameters.Container.ChildrenByType<CICodeSeg>( CIElement.TChildSearchType.EEntireHierarchy );
            
            // Sort them
            Comparison<CICodeSeg> comparer = delegate( CICodeSeg aLeft, CICodeSeg aRight )
            {
                return string.Compare( aLeft.Name, aRight.Name, true );
            };
            codeSegs.Sort( comparer );

            // List them
            foreach ( CICodeSeg codeSeg in codeSegs )
            {
                CXmlCodeSeg xmlCodeSeg = new CXmlCodeSeg( codeSeg );
                xmlCodeSeg.XmlSerialize( aParameters );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
		#endregion
	}
}
