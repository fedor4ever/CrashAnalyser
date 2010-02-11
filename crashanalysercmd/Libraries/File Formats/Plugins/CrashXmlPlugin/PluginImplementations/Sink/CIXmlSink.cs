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
using System.Collections.Generic;
using System.Text;
using CrashItemLib.Sink;
using CrashXmlPlugin.FileFormat.Document;

namespace CrashXmlPlugin.PluginImplementations.Sink
{
    public class CrashXmlSink : CISink
    {
        #region Constants
        public const string KXmlSinkName = "Crash XML";
        #endregion

        #region Constructors
        public CrashXmlSink( CISinkManager aManager )
            : base( KXmlSinkName, aManager )
        {
        }
        #endregion
        
        #region From CISink
        public override object Serialize( CISinkSerializationParameters aParams )
        {
            CXmlDocumentRoot document = new CXmlDocumentRoot();
            //
            string fileName = string.Empty;
            using ( CXmlDocumentSerializationParameters parameters = new CXmlDocumentSerializationParameters( aParams, document ) )
            {
                fileName = parameters.FileName;
                document.XmlSerialize( parameters );
            }
            //
            return fileName;
        }
        #endregion
    }
}
