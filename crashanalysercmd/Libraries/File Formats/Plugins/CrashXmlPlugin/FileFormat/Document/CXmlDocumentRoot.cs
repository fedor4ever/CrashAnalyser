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
using CrashXmlPlugin.FileFormat.CrashAnalyser;
using CrashXmlPlugin.FileFormat.SourceInfo;
using CrashXmlPlugin.FileFormat.Segment;
using CrashXmlPlugin.FileFormat.Node;
using SymbianTree;

namespace CrashXmlPlugin.FileFormat.Document
{
    internal class CXmlDocumentRoot : SymDocument
	{
		#region Constructors
        public CXmlDocumentRoot()
		{
            base.Add( new CXmlNodeCrashAnalyser() );
            base.Add( new CXmlNodeSourceInfo() );

            // Segment table must be serialized first - it writes its data to a memory buffer
            base.Add( new CXmlNodeSegmentTable() );

            // ... and then the dictionary writes its data to the underlying file, after which
            // it appends the serialized segment table memory-buffer
            base.Add( new CXmlNodeSegmentDictionary() );
        }
		#endregion

        #region API
        public void XmlSerialize( CXmlDocumentSerializationParameters aParameters )
        {
            string nodeName = XmlNodeName;
            //
            aParameters.Writer.WriteStartDocument();
            aParameters.Writer.WriteDocType(nodeName, null, Constants.DocType, null);


            aParameters.Writer.WriteStartElement( null, nodeName, null );
            XmlSerializeChildren( aParameters );
            aParameters.Writer.WriteEndElement();
            aParameters.Writer.WriteEndDocument();
        }
        #endregion

        #region Properties
        public CXmlNodeCrashAnalyser CrashAnalyser
        {
            get { return (CXmlNodeCrashAnalyser) base.ChildByType( typeof( CXmlNodeCrashAnalyser ) ); }
        }

        public CXmlNodeSourceInfo SourceInfo
        {
            get { return (CXmlNodeSourceInfo) base.ChildByType( typeof( CXmlNodeSourceInfo ) ); }
        }

        public CXmlNodeSegmentDictionary SegmentDictionary
        {
            get { return (CXmlNodeSegmentDictionary) base.ChildByType( typeof( CXmlNodeSegmentDictionary ) ); }
        }

        public CXmlNodeSegmentTable SegmentTable
        {
            get { return (CXmlNodeSegmentTable) base.ChildByType( typeof( CXmlNodeSegmentTable ) ); }
        }
        #endregion

        #region Internal methods
        private void XmlSerializeChildren( CXmlDocumentSerializationParameters aParameters )
        {
            foreach ( SymNode node in this )
            {
                if ( node is CXmlNode )
                {
                    CXmlNode xmlNode = (CXmlNode) node;
                    xmlNode.XmlSerialize( aParameters );
                }
            }
        }
        #endregion

        #region From SymNode
        protected override string XmlNodeName
        {
            get
            {
                return Constants.RootNode;
            }
        }
        #endregion

        #region Data members
		#endregion
	}
}
