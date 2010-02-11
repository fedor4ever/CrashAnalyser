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
using SymbolLib.Generics;
using SymbolLib.Sources.Map.File;
using SymbolLib.Sources.Map.Engine;

namespace SymbolLib.Sources.Map.Symbol
{
	internal class MapSymbol : GenericSymbol
	{
		#region Enumerations
		public enum TType
		{
			EUnknown = 0,
			EARMCode,
			EThumbCode,
			EData,
			ENumber,
			ESection
		}
		#endregion

		#region Static Constructors
        public static MapSymbol New( MapFile aFile )
        {
            return new MapSymbol( aFile );
        }

        public static MapSymbol NewUnknown( MapFile aFile )
        {
            MapSymbol symbol = new MapSymbol( aFile );
            //
            symbol.iType = TType.EARMCode;
            symbol.Size = 0;
            symbol.OffsetAddress = GenericSymbol.KNullEntryAddress;
            symbol.Symbol = GenericSymbol.KNonMatchingSymbolName;
            //
            if ( aFile.HostBinaryFileName.Length > 0 )
            {
                symbol.Object = Path.GetFileName( aFile.HostBinaryFileName );
            }
            else
            {
                symbol.Object = GenericSymbol.KNonMatchingObjectName;
            }
            //
            return symbol;
        }
        #endregion

        #region Constructors & destructor
        private MapSymbol( MapFile aFile )
            : base( aFile )
		{
		}
		#endregion

		#region Properties
		public TType Type
		{
			get { return iType; }
            private set { iType = value; }
        }
		#endregion

		#region From GenericSymbol
		public override bool Parse( string aLine )
		{
            Match m = KMapParserRegex.Match( aLine );
            if ( m.Success )
            {
                GroupCollection groups = m.Groups;
                //
                uint globalBaseAddress = MapFile.GlobalBaseAddress;
                //
                Object = groups[ "Binary" ].Value;
                Type = TypeByString( groups[ "Type" ].Value );
                Symbol = groups[ "Function" ].Value;
                OffsetAddress = long.Parse( groups[ "Address" ].Value, System.Globalization.NumberStyles.HexNumber ) - globalBaseAddress;
                Size = long.Parse( groups[ "Size" ].Value );
            }
			//
            return m.Success;
		}

        public override TSourceType SourceType
        {
            get { return GenericSymbol.TSourceType.ESourceTypeFileMap; }
        }
        #endregion

        #region Internal constants
        private const string KDummySymbolName = "[Unknown Symbol]";
        private static readonly Regex KMapParserRegex = new Regex(
              "(?:\\s*)(?<Function>.+?)(?:\\s+)0x(?<Address>[A-Fa-f0-9]{8})"+
              "(?:\\s+)(?<Type>(?:Data|Section|Number|ARM Code|Thumb Code))"+
              "(?:\\s+)(?<Size>\\d+)(?:\\s+)(?<Binary>.+)",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            );
        #endregion

		#region Internal methods
		private static TType TypeByString( string aTypeAsString )
		{
			TType ret = TType.EUnknown;
			//
			if	( aTypeAsString == "ARM Code" )
				ret = TType.EARMCode;
			else if ( aTypeAsString == "Thumb Code" )
				ret = TType.EThumbCode;
			else if ( aTypeAsString == "Section" )
				ret = TType.ESection;
			else if ( aTypeAsString == "Data" )
				ret = TType.EData;
			else if ( aTypeAsString == "Number" )
				ret = TType.ENumber;
			//
			return ret;
		}
		#endregion

        #region Internal properties
        private MapFile MapFile
        {
            get { return (Sources.Map.File.MapFile) Collection; }
        }
        #endregion

        #region Data members
        private TType iType = TType.EUnknown;
		#endregion
	}
}
