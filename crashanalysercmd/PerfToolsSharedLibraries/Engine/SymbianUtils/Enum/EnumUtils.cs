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
using System.ComponentModel;
using System.Reflection;

namespace SymbianUtils.Enum
{
    public static class EnumUtils
    {
        public static string ToString( System.Enum aValue )
        {
            string ret = aValue.ToString();

            Type type = aValue.GetType();
            MemberInfo[] memInfo = type.GetMember( aValue.ToString() );
            if ( memInfo != null && memInfo.Length > 0 )
            {
                object[] attrs = memInfo[ 0 ].GetCustomAttributes( typeof( DescriptionAttribute ), false );

                if ( attrs != null && attrs.Length > 0 )
                {
                    ret = ( (DescriptionAttribute) attrs[ 0 ] ).Description;
                }
            }

            return ret;
        }
    }
}
