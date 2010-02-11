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

namespace CrashXmlPlugin.FileFormat
{
    internal static class Utilities
    {
        public static string[] ConvertBinaryDataToText( IEnumerable<byte> aData, int aLineLength )
        {
            List<string> lines = new List<string>();

            StringBuilder line = new StringBuilder();
            foreach ( byte b in aData )
            {
                line.AppendFormat( "{0:x2}", b );

                if ( line.Length >= aLineLength )
                {
                    lines.Add( line.ToString() );
                    line.Length = 0;
                }
            }

            // Flush leftovers
            if ( line.Length > 0 )
            {
                lines.Add( line.ToString() );
            }

            return lines.ToArray();
        }
    }
}
