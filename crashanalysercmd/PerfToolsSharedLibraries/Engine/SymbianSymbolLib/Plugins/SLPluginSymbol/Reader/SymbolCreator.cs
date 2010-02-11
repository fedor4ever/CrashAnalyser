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
using SymbianUtils.Tracer;
using SymbianUtils.FileTypes;

namespace SLPluginSymbol.Reader
{
	internal class SymbolCreator
	{
		#region Constructors
        public SymbolCreator()
		{
		}
        #endregion

        #region API
        public static BasicSymbol Parse( string aLine )
        {
            BasicSymbol ret = null;
            //
            Match m = KSimpleSymbolRegEx.Match( aLine );
            if ( m.Success )
            {
                BasicSymbol symbol = new BasicSymbol();

                symbol.iAddress = uint.Parse( m.Groups[ "Address" ].Value, System.Globalization.NumberStyles.HexNumber );
                symbol.iSize = uint.Parse( m.Groups[ "Size" ].Value, System.Globalization.NumberStyles.HexNumber );

                string symbolAndObject = m.Groups[ "SymbolAndObject" ].Value;
                ParseSymbolText( symbolAndObject, out symbol.iName, out symbol.iObject );

                ret = symbol;
            }
            //
            return ret;
        }

        public Symbol Parse( string aLine, SymbolCollection aCollection )
		{
            Symbol ret = null;
            //
            BasicSymbol basicSymbol = Parse( aLine );
            if ( basicSymbol != null )
            {
                uint baseAddress = aCollection.BaseAddress;
                uint offsetAddress = basicSymbol.iAddress - baseAddress;
                if ( basicSymbol.iAddress < baseAddress )
                {
                    ret = null;
                }
                else
                {
                    ret = Symbol.New( aCollection );
                    //
                    ret.OffsetAddress = offsetAddress;
                    ret.Size = basicSymbol.iSize;
                    ret.Name = basicSymbol.iName;
                    ret.Object = basicSymbol.iObject;

                    // Make sure it's tagged as coming from a symbol file
                    ret.Source = TSymbolSource.ESourceWasSymbolFile;
                }
            }
			//
            return ret;
		}
        #endregion

        #region Classes
        public class BasicSymbol
        {
            #region Data members
            public uint iAddress = 0;
            public uint iSize = 0;
            public string iName = string.Empty;
            public string iObject = string.Empty;
            #endregion
        }
        #endregion

        #region Internal constants
        private static readonly Regex KSimpleSymbolRegEx = new Regex( "(?<Address>[A-Fa-f0-9]{8}) \\s+ (?<Size>[A-Fa-f0-9]{4,}) \\s+ (?<SymbolAndObject>.+)",
            RegexOptions.CultureInvariant
            | RegexOptions.Compiled
            );
        #endregion

        #region Internal methods
        private static void ParseSymbolText( string aText, out string aName, out string aObject )
		{
			int splitPos = aText.LastIndexOf( ' ' );
			if	( splitPos > 0 )
			{
				aName = aText.Substring( 0, splitPos ).TrimEnd();
				aObject = aText.Substring( splitPos ).TrimStart();
			}
			else
			{
				aName = aText;
                aObject = string.Empty;
			}
		}
		#endregion
	}
}
