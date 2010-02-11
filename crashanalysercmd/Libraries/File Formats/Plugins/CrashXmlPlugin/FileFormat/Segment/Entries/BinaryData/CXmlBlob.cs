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
using System.Xml;
using System.Collections.Generic;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.BinaryData;
using CrashXmlPlugin.FileFormat.Node;
using SymbianUtils.DataBuffer;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.BinaryData
{
    internal class CXmlBlob : CXmlNode
	{
		#region Constructors
        public CXmlBlob( CIBinaryData aBinaryData )
            : base( SegConstants.BinaryData_Blob )
		{
            iBinaryData = aBinaryData;
		}
		#endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CXmlNode.WriteId( iBinaryData, aParameters.Writer );
            aParameters.Writer.WriteElementString( SegConstants.CmnName, iBinaryData.Name );
        }
        
        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            aParameters.Writer.WriteStartElement( SegConstants.BinaryData_Blob_Payload );

            // Convert blob to bytequeue.
            DataBuffer buffer = iBinaryData.DataBuffer;

            string[] lines = Utilities.ConvertBinaryDataToText( buffer, Constants.KBinaryDataMaxLineLength );
            foreach( string line in lines )
            {
                WriteLineOfData( aParameters.Writer, line );
            }

            aParameters.Writer.WriteEndElement();
        }
        #endregion

        #region Internal constants
        #endregion

        #region Properties
        private void WriteLineOfData( XmlWriter aWriter, string aLine )
        {
            if ( !string.IsNullOrEmpty( aLine ) )
            {
                aWriter.WriteElementString( SegConstants.BinaryData_Blob_Payload_Data, aLine );
            }
        }
        #endregion

        #region Data members
        private readonly CIBinaryData iBinaryData;
		#endregion
	}
}
