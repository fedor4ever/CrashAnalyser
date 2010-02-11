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
using CrashItemLib.Crash.Traces;
using CrashItemLib.Crash.Utils;
using CrashXmlPlugin.FileFormat.Versions;
using SymbianStructuresLib.Debug.Trace;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Traces
{
    internal class CXmlSegTraces : CXmlSegBase
	{
		#region Constructors
        public CXmlSegTraces()
            : base( SegConstants.Traces )
		{
		}
		#endregion

        #region From CXmlSegBase
        public override int SegmentPriority
        {
            get { return 150; }
        }
        #endregion

        #region From CXmlNode
        public override void XmlSerialize( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            iTraceData = aParameters.Container.Traces;
            //
            if ( iTraceData != null && iTraceData.Lines.Length > 0 )
            {
                base.XmlSerialize( aParameters );
            }
            //
            iTraceData = null;
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            System.Diagnostics.Debug.Assert( iTraceData != null );
            foreach ( CITrace ciTrace in iTraceData )
            {
                aParameters.Writer.WriteStartElement( SegConstants.Traces_Line );
                //
                TraceLine trace = ciTrace;

                // Type
                string type = string.Empty;
                switch ( trace.Type )
                {
                case TraceLine.TType.ETypeBinary:
                    type = SegConstants.Traces_Type_Binary;
                    break;
                case TraceLine.TType.ETypeRaw:
                    type = SegConstants.Traces_Type_Raw;
                    break;
                case TraceLine.TType.ETypeText:
                    type = SegConstants.Traces_Type_Text;
                    break;
                default:
                    type = SegConstants.Traces_Type_Unknown;
                    break;
                }
                if ( string.IsNullOrEmpty( type ) == false )
                {
                    aParameters.Writer.WriteAttributeString( SegConstants.CmnType, type );
                }

                // Context id
                if ( trace.ContextId != 0 )
                {
                    aParameters.Writer.WriteAttributeString( SegConstants.Traces_ContextId, "0x" + trace.ContextId.ToString( "x8" ) );
                }

                // Time stamp
                aParameters.Writer.WriteAttributeString( SegConstants.Traces_TimeStamp, trace.TimeStamp.ToString() );

                // Prefix
                string prefix = trace.Prefix;
                if ( string.IsNullOrEmpty( prefix ) == false )
                {
                    aParameters.Writer.WriteAttributeString( SegConstants.Traces_Prefix, prefix );
                }

                // Suffix
                string suffix = trace.Suffix;
                if ( string.IsNullOrEmpty( suffix ) == false )
                {
                    aParameters.Writer.WriteAttributeString( SegConstants.Traces_Suffix, suffix );
                }

                if ( trace.HasIdentifier )
                {
                    // Component/group/id triplet
                    TraceIdentifier identifier = trace.Identifier;
                    aParameters.Writer.WriteAttributeString( SegConstants.Traces_ComponentId, "0x" + identifier.Component.ToString( "x8" ) );
                    aParameters.Writer.WriteAttributeString( SegConstants.Traces_GroupId, identifier.Group.ToString() );
                    aParameters.Writer.WriteAttributeString( SegConstants.Traces_InstanceId, identifier.Id.ToString() );

                    // File & line
                    TraceLocation location = identifier.Location;
                    //
                    string file = location.File;
                    string lineNumber = location.Line.ToString();
                    //
                    if ( string.IsNullOrEmpty( file ) == false && string.IsNullOrEmpty( lineNumber ) == false )
                    {
                        aParameters.Writer.WriteAttributeString( SegConstants.Traces_File, file );
                        aParameters.Writer.WriteAttributeString( SegConstants.Traces_LineNumber, lineNumber );
                    }
                }

                // Payload
                string payload = trace.Payload;
                aParameters.Writer.WriteValue( payload );

                aParameters.Writer.WriteEndElement();
            }
        }
        #endregion

        #region Properties
        #endregion

        #region From SegBase
        #endregion

        #region Data members
        private CITraceData iTraceData = null;
        #endregion
    }
}
