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

namespace SLPluginMap.Reader.RVCT
{
    internal class RVCTSymbolCreator
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

        #region Constructors
        public RVCTSymbolCreator( MapReader aReader, SymbolCollection aCollection )
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
                string typeString = groups[ "Type" ].Value;
                //
                string objectName = groups[ "Binary" ].Value;
                uint size = uint.Parse( groups[ "Size" ].Value );
                string symbol = groups[ "Function" ].Value;
                uint offsetAddress = uint.Parse( groups[ "Address" ].Value, System.Globalization.NumberStyles.HexNumber ) - globalBaseAddress;
                TSymbolType type = TypeByString( typeString );
                //
                ret = Symbol.New( iCollection );
                ret.OffsetAddress = offsetAddress;
                ret.Size = size;
                ret.Object = objectName;
                ret.Name = symbol;
                ret.Type = type;

                TSymbolType tt = ret.Type;

                // If the MAP file indicated thumb code then ensure our symbol library agrees.
                if ( typeString == "Thumb Code" )
                {
                    System.Diagnostics.Debug.Assert( ret.InstructionSet == SymbianStructuresLib.Arm.TArmInstructionSet.ETHUMB );
                }
            }
			//
            return ret;
		}
        #endregion

        #region Internal constants
        // <summary>
        //  Regular expression built for C# on: Fri, Aug 15, 2008, 11:19:15 AM
        //  Using Expresso Version: 3.0.2766, http://www.ultrapico.com
        //  
        //  A description of the regular expression:
        //  
        //  Match expression but don't capture it. [\s+]
        //      Whitespace, one or more repetitions
        //  [Function]: A named capture group. [.+?]
        //      Any character, one or more repetitions, as few as possible
        //  Match expression but don't capture it. [\s+]
        //      Whitespace, one or more repetitions
        //  0x
        //      0x
        //  [Address]: A named capture group. [[A-Fa-f0-9]{8}]
        //      Any character in this class: [A-Fa-f0-9], exactly 8 repetitions
        //  Match expression but don't capture it. [\s+]
        //      Whitespace, one or more repetitions
        //  [Type]: A named capture group. [(?:Data|Section|Number|ARM Code|Thumb Code)]
        //      Match expression but don't capture it. [Data|Section|Number|ARM Code|Thumb Code]
        //          Select from 5 alternatives
        //              Data
        //                  Data
        //              Section
        //                  Section
        //              Number
        //                  Number
        //              ARM Code
        //                  ARM
        //                  Space
        //                  Code
        //              Thumb Code
        //                  Thumb
        //                  Space
        //                  Code
        //  Match expression but don't capture it. [\s+]
        //      Whitespace, one or more repetitions
        //  [Size]: A named capture group. [\d+]
        //      Any digit, one or more repetitions
        //  Match expression but don't capture it. [\s+]
        //      Whitespace, one or more repetitions
        //  [Binary]: A named capture group. [.+]
        //      Any character, one or more repetitions
        //  
        //
        // </summary>
        private static readonly Regex KMapParserRegex = new Regex(
              "(?:\\s*)(?<Function>.+?)(?:\\s+)0x(?<Address>[A-Fa-f0-9]{8})"+
              "(?:\\s+)(?<Type>(?:Data|Section|Number|ARM Code|Thumb Code))"+
              "(?:\\s+)(?<Size>\\d+)(?:\\s+)(?<Binary>.+)",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.Compiled
            );
        #endregion

		#region Internal methods
        private static TSymbolType TypeByString( string aTypeAsString )
        {
            TSymbolType ret = TSymbolType.EUnknown;
            //
            if ( aTypeAsString == "ARM Code" )
                ret = TSymbolType.ECode;
            else if ( aTypeAsString == "Thumb Code" )
                ret = TSymbolType.ECode;
            else if ( aTypeAsString == "Section" )
                ret = TSymbolType.ESection;
            else if ( aTypeAsString == "Data" )
                ret = TSymbolType.EData;
            else if ( aTypeAsString == "Number" )
                ret = TSymbolType.ENumber;
            //
            return ret;
        }
        #endregion

        #region Data members
        private readonly MapReader iReader;
        private readonly SymbolCollection iCollection;
		#endregion
	}
}
