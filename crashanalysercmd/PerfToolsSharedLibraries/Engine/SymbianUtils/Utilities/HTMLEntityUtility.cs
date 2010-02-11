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

namespace SymbianUtils
{
	public class HTMLEntityUtility
	{
		public static string Entitize(string text)
		{
			return Entitize(text, true);
		}

		public static string Entitize( string aText, bool aEntitizeQuotAmpAndLtGt)
		{
			StringBuilder ret = new StringBuilder( aText.Length );

			for(int i=0;i<aText.Length;i++)
			{
				int code = (int) aText[i];
				if ( (code > 127 || code < 32 ) || (aEntitizeQuotAmpAndLtGt && ((code == 34) || (code == 38) || (code == 60) || (code == 62))))
				{
					ret.Append( "&#" + code + ";" );
				}
				else if ( aText[i] == '\'' || aText[i] == '@' || aText[i] == '\"' )
				{
					ret.Append( "." );
				}
				else
				{
					ret.Append( aText[i] );
				}
			}

			return ret.ToString();
		}
	}
}
