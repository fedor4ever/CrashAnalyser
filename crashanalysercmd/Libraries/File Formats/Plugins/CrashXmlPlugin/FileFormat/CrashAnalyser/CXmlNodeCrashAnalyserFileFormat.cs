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
using System.Xml;
using CrashXmlPlugin.FileFormat.Node;
using CrashXmlPlugin.FileFormat.Versions;

namespace CrashXmlPlugin.FileFormat.CrashAnalyser
{
    internal class CXmlNodeCrashAnalyserFileFormat : CXmlNode
	{
		#region Constructors
        public CXmlNodeCrashAnalyserFileFormat()
            : base( Constants.CrashAnalyser_FileFormat )
		{
		}
		#endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CXmlVersionExtended version = new CXmlVersionExtended( Constants.MasterFileFormatVersionMajor, 
                                                                   Constants.MasterFileFormatVersionMinor );
            version.XmlSerialize( aParameters );
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
		#endregion
	}
}
