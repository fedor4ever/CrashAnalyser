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
using CrashXmlPlugin.FileFormat.Node;

namespace CrashXmlPlugin.FileFormat.Versions
{
    internal class CXmlVersionText : CXmlNode
	{
		#region Constructors
        public CXmlVersionText()
            : this( string.Empty )
		{
		}

        public CXmlVersionText( string aText )
            : base( Constants.Version_Text )
        {
            iText = aText;
        }
        #endregion

        #region API
        public static void WriteVersionTextListStart( XmlWriter aWriter )
        {
            aWriter.WriteStartElement( Constants.CmnLinkList );
        }

        public static void WriteVersionTextListEnd( XmlWriter aWriter )
        {
            aWriter.WriteEndElement();
        }
        #endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            aParameters.Writer.WriteString( this.ToString() );
        }
        #endregion

        #region Properties
        public string Text
        {
            get { return iText; }
            set { iText = value; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return iText;
        }
        #endregion

        #region Data members
        private string iText = string.Empty;
		#endregion
	}
}
