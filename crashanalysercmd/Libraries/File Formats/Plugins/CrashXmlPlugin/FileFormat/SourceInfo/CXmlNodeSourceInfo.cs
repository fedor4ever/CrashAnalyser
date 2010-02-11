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
using CrashItemLib.Crash.Source;
using CrashXmlPlugin.FileFormat.Node;
using CrashXmlPlugin.FileFormat.Versions;

namespace CrashXmlPlugin.FileFormat.SourceInfo
{
    internal class CXmlNodeSourceInfo : CXmlNode
	{
		#region Constructors
        public CXmlNodeSourceInfo()
            : base( Constants.SourceInfo )
		{
        }
		#endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CISource source = aParameters.Container.Source;

            // File type
            string fileType = source.ImplementorName;
            aParameters.Writer.WriteElementString( Constants.SourceInfo_FileType, fileType );

            // Version
            CXmlVersionExtended version = new CXmlVersionExtended( source.ImplementorVersion );
            version.XmlSerialize( aParameters );

            // Source file (master file)
            string masterFileName = source.MasterFileName;
            aParameters.Writer.WriteElementString( Constants.SourceInfo_MasterFile, masterFileName );

            // Line number (if relevant)
            if ( source.IsLineNumberAvailable )
            {
                aParameters.Writer.WriteElementString( Constants.SourceInfo_LineNumber, source.LineNumber.ToString() );
            }
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            base.XmlSerializeChildren( aParameters );
            
            CISourceElement source = aParameters.Container.Source;

            // Write any raw data
            byte[] rawData = source.InputData;
            if ( rawData != null && rawData.Length > 0 )
            {
                aParameters.Writer.WriteStartElement( Constants.SourceInfo_RawData );
                
                string[] lines = Utilities.ConvertBinaryDataToText( rawData, Constants.KBinaryDataMaxLineLength );
                foreach ( string line in lines )
                {
                    if ( !string.IsNullOrEmpty( line ) )
                    {
                        aParameters.Writer.WriteElementString( Constants.SourceInfo_RawData_Item, line );
                    }
                }

                aParameters.Writer.WriteEndElement();
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
		#endregion
	}
}
