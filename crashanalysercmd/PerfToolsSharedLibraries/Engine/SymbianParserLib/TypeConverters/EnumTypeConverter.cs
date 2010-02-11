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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.ComponentModel;

namespace SymbianParserLib.TypeConverters
{
    public class SymbianEnumConverter : EnumConverter
    {
        #region Constructors
        public SymbianEnumConverter( Type aType )
            : base( aType )
        {
        }
        #endregion

        #region From EnumConverter
        public override bool CanConvertFrom( ITypeDescriptorContext aContext, Type aSourceType )
        {
            bool ret = base.CanConvertFrom( aContext, aSourceType );
            //
            if ( aSourceType == typeof( uint ) )
            {
                ret = true;
            }
            else if ( aSourceType == typeof( int ) )
            {
                ret = true;
            }
            else if ( aSourceType == typeof( ulong ) )
            {
                ret = true;
            }
            //
            return ret;
        }

        public override object ConvertFrom( ITypeDescriptorContext aContext, System.Globalization.CultureInfo aCulture, object aValue )
        {
            object ret = null;
            //
            if ( ( aValue is uint ) || ( aValue is int ) || ( aValue is ulong ) )
            {
                ret = Enum.ToObject( base.EnumType, aValue );
            }
            else
            {
                ret = base.ConvertFrom( aContext, aCulture, aValue );
            }
            //
            return ret;
        }
        #endregion
    }
}
