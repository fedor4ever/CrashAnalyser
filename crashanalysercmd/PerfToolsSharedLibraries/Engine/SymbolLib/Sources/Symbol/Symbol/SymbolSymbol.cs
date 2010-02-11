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
using System.Text.RegularExpressions;
using SymbolLib.Generics;
using SymbolLib.Sources.Symbol.File;

namespace SymbolLib.Sources.Symbol.Symbol
{
	public class SymbolSymbol : GenericSymbol
	{
		#region Constructors
        public static SymbolSymbol New( GenericSymbolCollection aCollection )
        {
            return new SymbolSymbol( aCollection );
        }

        public static SymbolSymbol NewUnknown()
        {
            return NewUnknown( new SymbolsForBinary( string.Empty ) );
        }

        public static SymbolSymbol NewUnknown( uint aAddress, uint aSize, string aObject )
        {
            SymbolSymbol ret = NewUnknown();
            //
            ret.OffsetAddress = aAddress;
            ret.Size = aSize;
            ret.Object = aObject;
            //
            return ret;
        }

        public static SymbolSymbol NewUnknown( GenericSymbolCollection aCollection )
        {
            SymbolSymbol symbol = new SymbolSymbol( aCollection );
            //
            symbol.OffsetAddress = GenericSymbol.KNullEntryAddress;
            symbol.Symbol = GenericSymbol.KNonMatchingSymbolName;
            if ( aCollection.HostBinaryFileName.Length > 0 )
            {
                symbol.Object = Path.GetFileName( aCollection.HostBinaryFileName );
            }
            else
            {
                symbol.Object = GenericSymbol.KNonMatchingObjectName;
            }
            //
            return symbol;
        }
        #endregion

        #region Internal constructors
        private SymbolSymbol( GenericSymbolCollection aCollection )
            : base( aCollection )
		{
		}

        private SymbolSymbol( string aSymbolName )
            : this( aSymbolName, 0 )
        {
		}

        private SymbolSymbol( string aSymbolName, uint aAddress )
            : base( new SymbolsForBinary( string.Empty )  )
        {
            Symbol = aSymbolName;
            OffsetAddress = aAddress;
        }
        #endregion

        #region API
        public void ResetOffsetAddress( long aOffset )
        {
            // This resets the offset address
            OffsetAddress = aOffset;

            // This resets the OffsetEndAddress
            Size = Size;
        }
        #endregion

        #region From GenericSymbol
        public override bool Parse( string aLine )
		{
            bool ret = false;
            //
            Match m = KSimpleSymbolRegEx.Match( aLine );
            if ( m.Success )
            {
                ret = ExtractFromMatch( m );
            }
            //
            return ret;
		}

        public override TSourceType SourceType
        {
            get { return GenericSymbol.TSourceType.ESourceTypeFileSymbol; }
        }
        #endregion

        #region Internal methods
        private bool ExtractFromMatch( Match aMatch )
        {
            bool ret = false;
            //
            long baseAddress = Collection.BaseAddress;
            long address = uint.Parse( aMatch.Groups[ "Address" ].Value, System.Globalization.NumberStyles.HexNumber );
            long offsetAddress = address - baseAddress;
            if ( address < baseAddress )
            {
                ret = false;
            }
            else
            {
                OffsetAddress = offsetAddress;
                Size = uint.Parse( aMatch.Groups[ "Size" ].Value, System.Globalization.NumberStyles.HexNumber );

                string symbolAndObject = aMatch.Groups[ "SymbolAndObject" ].Value;
                ParseSymbolText( symbolAndObject );

                System.Diagnostics.Debug.Assert( Address >= 0 );
                System.Diagnostics.Debug.Assert( Address <= EndAddress );

                ret = true;
            }
            //
            return ret;
        }
        #endregion

        #region Internal constants
        private static readonly Regex KSimpleSymbolRegEx = new Regex( "(?<Address>[A-Fa-f0-9]{8}) \\s+ (?<Size>[A-Fa-f0-9]{4,}) \\s+ (?<SymbolAndObject>.+)",
            RegexOptions.CultureInvariant
            | RegexOptions.Compiled
            );
        #endregion

        #region Internal methods
        protected void ParseSymbolText( string aText )
		{
			int splitPos = aText.LastIndexOf( ' ' );
			if	( splitPos > 0 )
			{
				Symbol = aText.Substring( 0, splitPos ).TrimEnd();
				Object = aText.Substring( splitPos ).TrimStart();
			}
			else
			{
				Symbol = aText;
                Object = string.Empty;
			}
		}
		#endregion
	}
}
