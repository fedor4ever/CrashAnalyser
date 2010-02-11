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
using CrashItemLib.Engine;
using CrashXmlPlugin.FileFormat.Node;
using CrashXmlPlugin.FileFormat.Document;
using CrashXmlPlugin.FileFormat.Segment.Entries;

namespace CrashXmlPlugin.FileFormat.Segment
{
	internal class CXmlNodeSegmentDictionary : CXmlNode
	{
		#region Constructors
        public CXmlNodeSegmentDictionary()
            : base( Constants.SegmentDictionary )
		{
		}
		#endregion

        #region From CXmlNode
        public override void XmlSerialize( CXmlDocumentSerializationParameters aParameters )
        {
            base.XmlSerialize( aParameters );

            // Now write segment table in-memory buffer
            string segTable = aParameters.Document.SegmentTable.SerializedData;
            if ( segTable.Length > 0 )
            {
                aParameters.Writer.WriteRaw( segTable );
            }
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CXmlDocumentRoot docRoot = ( base.Parent as CXmlDocumentRoot );
            if ( docRoot != null )
            {
                CXmlNodeSegmentTable table = docRoot.SegmentTable;
                if ( table != null )
                {
                    foreach ( CXmlSegBase seg in table )
                    {
                        bool wasSerialized = seg.WasSerialized;
                        if ( wasSerialized )
                        {
                            aParameters.Writer.WriteStartElement( Constants.SegmentDictionary_Segment );
                            //
                            seg.Version.XmlSerialize( aParameters );
                            aParameters.Writer.WriteElementString( SegConstants.CmnName, seg.Name );
                            //
                            aParameters.Writer.WriteEndElement();
                        }
                    }
                }
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
