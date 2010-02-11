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

namespace SymbianParserLib.Utilities
{
    public static class ParserUtils
    {
        #region API
        public static string RemoveLineEndings( string aText )
        {
            StringBuilder ret = new StringBuilder( aText );
            ret = ret.Replace( KCR, string.Empty );
            ret = ret.Replace( KLF, string.Empty );
            return ret.ToString();
        }
        #endregion

        #region Internal constants
        private const string KCR = "\r";
        private const string KLF = "\n";
        #endregion

    }
}
