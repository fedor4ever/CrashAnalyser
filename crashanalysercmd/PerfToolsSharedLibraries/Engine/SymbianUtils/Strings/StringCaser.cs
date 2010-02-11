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

namespace SymbianUtils.Strings
{
	public class StringCaser
	{
		public static string PrettyCase( string aValue )
		{
            string ret = string.Empty;
            //
            if  ( aValue.Length > 0 )
            {
                ret = aValue.Substring( 0, 1 ).ToUpper() + aValue.Substring( 1 ).ToLower();
            }
            //
            return ret;
		}
	}
}
