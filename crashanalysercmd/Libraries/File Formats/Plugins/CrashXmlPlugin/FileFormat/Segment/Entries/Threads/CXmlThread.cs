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
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Threads;
using CrashItemLib.Crash.Stacks;
using CrashXmlPlugin.FileFormat.Node;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Threads
{
    internal class CXmlThread : CXmlNode
	{
		#region Constructors
        public CXmlThread( CIThread aThread )
            : base( SegConstants.Threads_Thread )
		{
            iThread = aThread;
		}
		#endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CXmlNode.WriteId( iThread, aParameters.Writer );

            aParameters.Writer.WriteElementString( SegConstants.CmnName, iThread.Name );
            aParameters.Writer.WriteElementString( SegConstants.Threads_Thread_FullName, iThread.FullName );

            // Parent process
            if ( iThread.OwningProcess != null )
            {
                CXmlNode.WriteLink( iThread.OwningProcess.Id, SegConstants.Processes, aParameters.Writer );
            }

            // Thread priority
            if ( iThread.Priority != 0 )
            {
                aParameters.Writer.WriteElementString( SegConstants.CmnPriority, iThread.Priority.ToString() );
            }

            // Write any messages
            CXmlSegBase.XmlSerializeMessages( aParameters, iThread );
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            // Exit info
            CXmlExitInfo xmlExitInfo = new CXmlExitInfo( iThread.ExitInfo );
            xmlExitInfo.XmlSerialize( aParameters );

            // Stacks
            XmlSerializeStacks( aParameters );

            // Registers
            XmlSerializeRegisters( aParameters );
        }
        #endregion

        #region Internal methods
        private void XmlSerializeRegisters( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            // Must obtain the registers in advance to avoid creating an empty list.
            List<CIRegisterList> regs = new List<CIRegisterList>();

            // Find register lists
            CIElementList<CIRegisterListCollection> allRegs = iThread.ChildrenByType<CIRegisterListCollection>();
            foreach ( CIRegisterListCollection registerListCol in allRegs )
            {
                foreach ( CIRegisterList registerList in registerListCol )
                {
                    if ( registerList.Count > 0 )
                    {
                        regs.Add( registerList );
                    }
                }
            }

            // Only write something if we have some entries
            if ( regs.Count > 0 )
            {
                CXmlNode.WriteLinkListStart( aParameters.Writer, SegConstants.Registers );
                foreach ( CIRegisterList registerList in regs )
                {
                    CXmlNode.WriteLink( registerList.Id, SegConstants.Registers, aParameters.Writer );
                }
                CXmlNode.WriteLinkListEnd( aParameters.Writer );
            }
        }

        private void XmlSerializeStacks( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            // Find stacks
            CIElementList<CIStack> stacks = iThread.ChildrenByType<CIStack>();

            // Only write something if we have some entries
            if ( stacks.Count > 0 )
            {
                CXmlNode.WriteLinkListStart( aParameters.Writer, SegConstants.Stacks );
                foreach ( CIStack item in stacks )
                {
                    CXmlNode.WriteLink( item.Id, SegConstants.Stacks, aParameters.Writer );
                }
                CXmlNode.WriteLinkListEnd( aParameters.Writer );
            }
        }
        #endregion

        #region Data members
        private readonly CIThread iThread;
		#endregion
	}
}
