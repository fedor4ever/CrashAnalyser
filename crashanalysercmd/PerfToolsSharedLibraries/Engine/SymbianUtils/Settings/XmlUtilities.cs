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
using System.IO;
using System.Xml;
using System.Collections;

namespace SymbianUtils.Settings
{
	internal class XmlUtils
	{
        public static string ReadAttributeValue( XmlReader aReader, string aAttributeName )
        {
            string ret = string.Empty;
            ReadAttributeValue( aReader, aAttributeName, out ret );
            return ret;
        }

        public static void ReadAttributeValue( XmlReader aReader, string aAttributeName, out string aValue )
		{
			aValue = string.Empty;
			//
			bool foundName = aReader.MoveToAttribute(aAttributeName);
            if ( foundName )
            {
                while ( aReader.ReadAttributeValue() )
                {
                    aValue += aReader.Value;
                }
            }
            else
            {
                throw new Exception( "Could not find attribute \'" + aAttributeName + "\' in xml element" );
            }
		}
	}
}
