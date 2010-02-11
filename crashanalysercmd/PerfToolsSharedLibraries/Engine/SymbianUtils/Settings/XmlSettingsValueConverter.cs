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
using System.Xml;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;

namespace SymbianUtils.Settings
{
    internal static class XmlSettingsValueConverter<T>
	{
        public static T Convert( XmlSettingsValue aValue, T aDefault )
        {
            bool defaultApplied = false;
            return Convert( aValue, aDefault, out defaultApplied );
        }

        public static T Convert( XmlSettingsValue aValue, T aDefault, out bool aDefaultApplied )
        {
            T ret = aDefault;
            aDefaultApplied = true;
            //
            Type srcType = aValue.Value.GetType();
            Type destType = aDefault.GetType();
            //
            try
            {
                // Don't need to convert if source and destination types are
                // identical.
                if ( srcType == destType )
                {
                    ret = (T) aValue.Value;
                }
                else
                {
                    // Don't try to convert empty strings.
                    if ( aValue.Value is string && destType != typeof( string ) )
                    {
                        string val = ( (string) aValue.Value ).Trim();
                        if ( val == string.Empty )
                        {
                            return aDefault;
                        }
                    }

                    // Try the type converter instead
                    TypeConverter converter = TypeDescriptor.GetConverter( destType );
                    if ( converter.CanConvertFrom( srcType ) )
                    {
                        if ( aValue.Value is string )
                        {
                            string text = (string) aValue.Value;
                            ret = (T) converter.ConvertFromInvariantString( text );
                        }
                        else
                        {
                            ret = (T) converter.ConvertFrom( aValue.Value );
                        }
                        aDefaultApplied = false;
                    }
                    else
                    {
                        SymbianUtils.SymDebug.SymDebugger.Break();
                    }
                }
            }
            catch ( System.FormatException )
            {
                SymbianUtils.SymDebug.SymDebugger.Break();
            }
            //
            return ret;
        }
	}
}


