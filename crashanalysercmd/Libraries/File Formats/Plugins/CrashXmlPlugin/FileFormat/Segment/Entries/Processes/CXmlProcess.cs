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
using CrashItemLib.Crash.CodeSegs;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.Threads;
using CrashXmlPlugin.FileFormat.Node;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Processes
{
    internal class CXmlProcess : CXmlNode
	{
		#region Constructors
        public CXmlProcess( CIProcess aProcess )
            : base( SegConstants.Processes_Process )
		{
            iProcess = aProcess;
		}
		#endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CXmlNode.WriteId( iProcess, aParameters.Writer );
            aParameters.Writer.WriteElementString( SegConstants.CmnName, iProcess.Name );

            // UIDs
            if ( iProcess.Uids[ 0 ] != 0 )
            {
                aParameters.Writer.WriteElementString( SegConstants.Processes_Process_UID1, iProcess.Uids[ 0 ].ToString( "x8" ) );
            }
            if ( iProcess.Uids[ 1 ] != 0 )
            {
                aParameters.Writer.WriteElementString( SegConstants.Processes_Process_UID2, iProcess.Uids[ 1 ].ToString( "x8" ) );
            }
            if ( iProcess.Uids[ 2 ] != 0 )
            {
                aParameters.Writer.WriteElementString( SegConstants.Processes_Process_UID3, iProcess.Uids[ 2 ].ToString( "x8" ) );
            }

            // SID
            if ( iProcess.SID != 0 )
            {
                aParameters.Writer.WriteElementString( SegConstants.Processes_Process_SID, iProcess.SID.ToString( "x8" ) );
            }

            // Generation
            aParameters.Writer.WriteElementString( SegConstants.Processes_Process_Generation, iProcess.Generation.ToString() );

            // Priority
            if ( iProcess.Priority != 0 )
            {
                aParameters.Writer.WriteElementString( SegConstants.CmnPriority, iProcess.Priority.ToString() );
            }
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            // Stacks
            CXmlNode.WriteLinkListStart( aParameters.Writer, SegConstants.Threads );
            foreach ( CIThread thread in iProcess.Threads )
            {
                CXmlNode.WriteLink( thread.Id, SegConstants.Threads, aParameters.Writer );
            }
            CXmlNode.WriteLinkListEnd( aParameters.Writer );

            // Code segments
            CXmlNode.WriteLinkListStart( aParameters.Writer, SegConstants.CodeSegs );
            foreach ( CICodeSeg codeSeg in iProcess.CodeSegments )
            {
                CXmlNode.WriteLink( codeSeg.Id, SegConstants.CodeSegs, aParameters.Writer );
            }
            CXmlNode.WriteLinkListEnd( aParameters.Writer );
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        private readonly CIProcess iProcess;
		#endregion
	}
}
