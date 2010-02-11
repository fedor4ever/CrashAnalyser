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
using SymbianStructuresLib.Arm.Registers;
using CrashItemLib.Crash.Registers;
using CrashXmlPlugin.FileFormat.Node;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Registers
{
    internal class CXmlRegisterSet : CXmlNode
	{
		#region Constructors
        public CXmlRegisterSet( CIRegisterList aList )
            : base( SegConstants.Registers_RegisterSet )
		{
            iList = aList;
		}
		#endregion

        #region API
        #endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CXmlNode.WriteId( iList, aParameters.Writer );
            
            // Abbreviated type
            string typeAbbreviated = ArmRegisterBankUtils.BankAsString( iList.Bank );
            aParameters.Writer.WriteElementString( SegConstants.CmnType, typeAbbreviated );

            // Long type
            string typeLong = ArmRegisterBankUtils.BankAsStringLong( iList.Bank );
            aParameters.Writer.WriteElementString( SegConstants.CmnName, typeLong );

            // Link to thread
            if ( iList.OwningThread != null )
            {
                CXmlNode.WriteLink( iList.OwningThread.Id, SegConstants.Threads, aParameters.Writer );
            }
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            if ( iList.IsCurrentProcessorMode )
            {
                aParameters.Writer.WriteStartElement( SegConstants.CmnAttributes );
                aParameters.Writer.WriteElementString( SegConstants.Registers_RegisterSet_CurrentBank, string.Empty );
                aParameters.Writer.WriteEndElement();
            }

            foreach ( CIRegister register in iList )
            {
                CXmlRegister xmlRegister = new CXmlRegister( register );
                xmlRegister.XmlSerialize( aParameters );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        private readonly CIRegisterList iList;
		#endregion
	}
}
