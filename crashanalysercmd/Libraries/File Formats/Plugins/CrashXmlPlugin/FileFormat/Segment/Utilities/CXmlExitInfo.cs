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
using CrashItemLib.Crash.ExitInfo;
using CrashXmlPlugin.FileFormat.Node;

namespace CrashXmlPlugin.FileFormat.Segment.Entries
{
    internal class CXmlExitInfo : CXmlNode
	{
		#region Constructors
        public CXmlExitInfo( CIExitInfo aExitInfo )
            : base( SegConstants.ExitInfo )
		{
            iExitInfo = aExitInfo;
		}
		#endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            // Type
            aParameters.Writer.WriteStartElement( SegConstants.ExitInfo_Type );
            switch ( iExitInfo.Type )
            {
            case CIExitInfo.TExitType.EExitTypeException:
                aParameters.Writer.WriteString( SegConstants.ExitInfo_Type_Exception );
                break;
            case CIExitInfo.TExitType.EExitTypeKill:
                aParameters.Writer.WriteString( SegConstants.ExitInfo_Type_Kill );
                break;
            case CIExitInfo.TExitType.EExitTypePanic:
                aParameters.Writer.WriteString( SegConstants.ExitInfo_Type_Panic );
                break;
            case CIExitInfo.TExitType.EExitTypePending:
                aParameters.Writer.WriteString( SegConstants.ExitInfo_Type_Pending );
                break;
            case CIExitInfo.TExitType.EExitTypeTerminate:
                aParameters.Writer.WriteString( SegConstants.ExitInfo_Type_Terminate );
                break;
            }
            aParameters.Writer.WriteEndElement();

            // For panics or terminates, reason & category
            switch ( iExitInfo.Type )
            {
            case CIExitInfo.TExitType.EExitTypeException:
            case CIExitInfo.TExitType.EExitTypeTerminate:
            case CIExitInfo.TExitType.EExitTypePanic:
                aParameters.Writer.WriteElementString( SegConstants.ExitInfo_Category, iExitInfo.Category );
                aParameters.Writer.WriteElementString( SegConstants.ExitInfo_Reason, iExitInfo.Reason.ToString() );
                break;
            }
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        private readonly CIExitInfo iExitInfo;
		#endregion
	}
}
