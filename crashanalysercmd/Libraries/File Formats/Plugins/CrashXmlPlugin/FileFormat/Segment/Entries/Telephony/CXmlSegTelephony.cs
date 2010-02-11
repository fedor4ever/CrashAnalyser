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
using CrashItemLib.Crash.Telephony;
using CrashItemLib.Crash.Utils;
using CrashXmlPlugin.FileFormat.Versions;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Telephony
{
	internal class CXmlSegTelephony : CXmlSegBase
	{
		#region Constructors
        public CXmlSegTelephony()
            : base( SegConstants.Telephony )
		{
		}
		#endregion

        #region From CXmlSegBase
        public override int SegmentPriority
        {
            get { return 100; }
        }
        #endregion

        #region From CXmlNode
        public override void XmlSerialize( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CITelephony info = (CITelephony) aParameters.Container.ChildByType( typeof( CITelephony ) );
            if ( info != null )
            {
                base.XmlSerialize( aParameters );
            }
        }

        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CITelephony info = (CITelephony) aParameters.Container.ChildByType( typeof( CITelephony ) );
            if ( info != null )
            {
                string phoneNumber = info.PhoneNumber;
                WriteStringIfNotEmpty( aParameters.Writer, SegConstants.Telephony_PhoneNumber, phoneNumber );

                string imei = info.IMEI;
                WriteStringIfNotEmpty( aParameters.Writer, SegConstants.Telephony_Imei, imei );

                string imsi = info.IMSI;
                WriteStringIfNotEmpty( aParameters.Writer, SegConstants.Telephony_Imsi, imsi );
            }
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CITelephony info = (CITelephony) aParameters.Container.ChildByType( typeof( CITelephony ) );
            if ( info != null )
            {
                CITelephonyNetworkInfo networkInfo = info.NetworkInfo;
                //
                aParameters.Writer.WriteStartElement( SegConstants.Telephony_Network );
                WriteStringIfNotEmpty( aParameters.Writer, SegConstants.Telephony_Network_Country, networkInfo.Country );
                WriteStringIfNotEmpty( aParameters.Writer, SegConstants.Telephony_Network_Identity, networkInfo.Identity );
                WriteStringIfNotEmpty( aParameters.Writer, SegConstants.Telephony_Network_Cell, networkInfo.CellId );
                //
                string regMode = SymbianUtils.Enum.EnumUtils.ToString( networkInfo.RegistrationMode );
                WriteStringIfNotEmpty( aParameters.Writer, SegConstants.Telephony_Network_Registration, regMode );
                //
                aParameters.Writer.WriteEndElement();
            }
        }
        #endregion

        #region Properties
        #endregion

        #region From SegBase
        #endregion

        #region Data members
        #endregion
    }
}
