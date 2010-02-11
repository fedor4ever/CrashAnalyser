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
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Memory;
using CrashXmlPlugin.FileFormat.Node;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Memory
{
    internal class CXmlMemoryInfo : CXmlNode
	{
		#region Constructors
        public CXmlMemoryInfo( CIMemoryInfo aInfo, string aName )
            : base( aName )
		{
            iInfo = aInfo;
		}
		#endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CXmlNode.WriteId( iInfo, aParameters.Writer );
            WriteIfNotZero( aParameters.Writer, SegConstants.CmnName, iInfo.Free );
            WriteIfNotZero( aParameters.Writer, SegConstants.MemoryInfo_Capacity, iInfo.Capacity );

            if ( iInfo.Type == CIMemoryInfo.TType.ETypeDrive )
            {
                WriteDrivePath( aParameters.Writer, iInfo.DriveNumber );
                WriteIfNotZero( aParameters.Writer, SegConstants.MemoryInfo_UID, iInfo.UID );
                WriteStringIfNotEmpty( aParameters.Writer, SegConstants.MemoryInfo_Drive_Vendor, iInfo.Vendor );
            }
            else if ( iInfo.Type == CIMemoryInfo.TType.ETypeRAM )
            {
            }

            // Reuse "name" tag for volume name
            WriteStringIfNotEmpty( aParameters.Writer, SegConstants.CmnName, iInfo.VolumeName );
        }
        #endregion

        #region Internal methods
        private void WriteIfNotZero( XmlWriter aWriter, string aName, ulong aValue )
        {
            if ( aValue != 0 )
            {
                aWriter.WriteElementString( aName, aValue.ToString() );
            }
        }

        private static void WriteDrivePath( XmlWriter aWriter, int aDriveNumber )
        {
            char driveLetter = ( (char) ( (int) 'A'  + aDriveNumber ) );
            string path = driveLetter + ":";
            //
            aWriter.WriteElementString( SegConstants.MemoryInfo_Drive_Path, path );
        }
        #endregion

        #region Data members
        private readonly CIMemoryInfo iInfo;
		#endregion
	}
}
