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
using CrashItemLib.Crash.Base;
using SymbianTree;
using CrashXmlPlugin.FileFormat.Document;
using CrashXmlPlugin.FileFormat.Segment.Entries;

namespace CrashXmlPlugin.FileFormat.Node
{
	internal class CXmlNode : SymNodeAddAsChild
	{
		#region Constructors
        protected CXmlNode( string aName )
		{
            iName = aName;
		}
		#endregion

        #region API - Framework
        public virtual void XmlSerialize( CXmlDocumentSerializationParameters aParameters )
        {
            string nodeName = XmlNodeName;
            //
            aParameters.Writer.WriteStartElement( null, nodeName, null );
            XmlSerializeContent( aParameters );
            XmlSerializeChildren( aParameters );
            aParameters.Writer.WriteEndElement();
        }

        /// <summary>
        /// Override this virtual function if you have dynamic content to serialize at the
        /// level of this node.
        /// </summary>
        /// <param name="aParameters"></param>
        protected virtual void XmlSerializeContent( CXmlDocumentSerializationParameters aParameters )
        {
        }

        /// <summary>
        /// By default, this method invokes the XmlSerialize() method on all children. Override this
        /// method if you have dynamic children to serialize that are not directly added as child nodes.
        /// </summary>
        /// <param name="aParameters"></param>
        protected virtual void XmlSerializeChildren( CXmlDocumentSerializationParameters aParameters )
        {
            SymNodeEnumeratorChildren iterator = new SymNodeEnumeratorChildren( this );
            foreach ( SymNode node in iterator )
            {
                try
                {
                    ( (CXmlNode) node ).XmlSerialize( aParameters );
                }
                catch ( Exception e )
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine( "CXmlNode Exception: " + this.GetType().Name );
                    System.Diagnostics.Debug.WriteLine( "   Message: " + e.Message );
                    System.Diagnostics.Debug.WriteLine( "   Stack:   " + e.StackTrace );
#endif
                }
            }
        }
        #endregion

        #region Writer Helpers
        public static void WriteId( CIElement aElement, XmlWriter aWriter )
        {
            aWriter.WriteStartElement( SegConstants.CmnId );
            if ( aElement.IsIdExplicit )
            {
                aWriter.WriteAttributeString( SegConstants.CmnType, SegConstants.CmnId_Explicit );
            }
            aWriter.WriteValue( aElement.Id.ToString() );
            aWriter.WriteEndElement();
        }

        public static void WriteLink( CIElementId aLinkTo, string aSegment, XmlWriter aWriter )
        {
            aWriter.WriteStartElement( Constants.CmnLink );
            aWriter.WriteAttributeString( Constants.CmnLink_Seg, aSegment );
            aWriter.WriteValue( aLinkTo.ToString() );
            aWriter.WriteEndElement();
        }

        public static void WriteLinkListStart( XmlWriter aWriter, string aSegment )
        {
            aWriter.WriteStartElement( Constants.CmnLinkList );
            aWriter.WriteAttributeString( Constants.CmnLink_Seg, aSegment );
        }

        public static void WriteLinkListEnd( XmlWriter aWriter )
        {
            aWriter.WriteEndElement();
        }

        public static void WriteStringIfNotEmpty( XmlWriter aWriter, string aName, string aValue )
        {
            if ( !string.IsNullOrEmpty( aValue ) )
            {
                aWriter.WriteElementString( aName, aValue );
            }
        }

        public static void WriteDate( XmlWriter aWriter, DateTime aDate, string aName )
        {
            string date = string.Format( "{0:d4}{1:d2}{2:d2}", aDate.Year, aDate.Month, aDate.Day );
            aWriter.WriteElementString( aName, date );
        }

        public static void WriteTime( XmlWriter aWriter, DateTime aTime, string aName )
        {
            string time = string.Format( "{0:d2}{1:d2}{2:d2}", aTime.Hour, aTime.Minute, aTime.Second );
            aWriter.WriteElementString( aName, time );
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return iName; }
            protected set { iName = value; }
        }
        #endregion

        #region From SymNode
        protected override string XmlNodeName
        {
            get
            {
                return iName;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return iName;
        }
        #endregion

        #region Data members
        private string iName = string.Empty;
		#endregion
	}
}
