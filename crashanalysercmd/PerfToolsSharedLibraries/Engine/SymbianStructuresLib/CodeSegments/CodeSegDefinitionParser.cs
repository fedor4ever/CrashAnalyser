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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace SymbianStructuresLib.CodeSegments
{
	public static class CodeSegDefinitionParser
	{
        #region API
        public static CodeSegDefinition ParseDefinition( string aLine )
        {
            CodeSegDefinition ret = null;
            //
            Match m = KCodeSegRegEx.Match( aLine );
            if ( m.Success )
            {
                ret = new CodeSegDefinition();
                //
                string gpAddressStart = m.Groups[ "StartAddress" ].Value;
                string gpAddressEnd = m.Groups[ "EndAddress" ].Value;
                string gpBinary = m.Groups[ "Binary" ].Value;
                //
                ret.Base = uint.Parse( gpAddressStart, System.Globalization.NumberStyles.HexNumber );
                ret.Limit = uint.Parse( gpAddressEnd, System.Globalization.NumberStyles.HexNumber );
                ret.FileName = gpBinary;
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal constants
        private static readonly Regex KCodeSegRegEx = new Regex(
            @"(?<StartAddress>[a-fA-F0-9]{8})-(?<EndAddress>[a-fA-F0-9]{8})\s{1}(?<Binary>.+)",
            RegexOptions.IgnoreCase
            );
        private const int KBaseHex = 16;
        #endregion

        #region Data members
        #endregion
    }
}
