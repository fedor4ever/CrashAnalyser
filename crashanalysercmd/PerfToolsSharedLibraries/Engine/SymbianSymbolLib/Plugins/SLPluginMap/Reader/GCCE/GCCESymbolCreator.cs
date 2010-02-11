/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SymbianStructuresLib.Debug.Symbols;
using SymbianSymbolLib.SourceManagement.Source;
using SymbianSymbolLib.SourceManagement.Provisioning;
using SymbianUtils;
using SymbianUtils.FileTypes;
using SymbianUtils.Tracer;

namespace SLPluginMap.Reader.GCCE
{
    internal class GCCESymbolCreator
	{
        #region Constructors
        public GCCESymbolCreator( MapReader aReader, SymbolCollection aCollection )
		{
            iReader = aReader;
            iCollection = aCollection;
		}
		#endregion

		#region Properties
		#endregion

		#region API
		public Symbol Parse( string aLine )
		{
            Symbol ret = null;
            //
            Match m = KMapParserRegex.Match( aLine );
            if ( m.Success )
            {
                GroupCollection groups = m.Groups;
                //
                uint globalBaseAddress = iReader.GlobalBaseAddress;
                string symbol = groups[ "Function" ].Value;
                uint offsetAddress = uint.Parse( groups[ "Address" ].Value, System.Globalization.NumberStyles.HexNumber ) - globalBaseAddress;
                //
                if ( symbol != null )
                {
                    symbol = symbol.Trim();
                    //
                    if ( !string.IsNullOrEmpty( symbol ) )
                    {
                        if ( symbol.StartsWith( "PROVIDE" ) )
                        {
                        }
                        else
                        {
                            ret = Symbol.New( iCollection );
                            ret.OffsetAddress = offsetAddress;
                            ret.Size = 0;
                            ret.Object = string.Empty;
                            ret.Name = symbol;
                        }
                    }
                }
            }
			//
            return ret;
		}
        #endregion

        #region Internal constants
        private readonly static Regex KMapParserRegex = new Regex(
              "                0x(?<Address>[A-Fa-f0-9]{8})                (?<Function>.+)",
            RegexOptions.Multiline
            | RegexOptions.CultureInvariant
            | RegexOptions.Compiled
            );
        #endregion

		#region Internal methods
        #endregion

        #region Data members
        private readonly MapReader iReader;
        private readonly SymbolCollection iCollection;
		#endregion
	}
}
