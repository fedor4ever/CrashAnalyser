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
    internal class RVCTMapFileReader : MapReader
	{
        #region Delegates & events
        #endregion

        #region Constructors
        public RVCTMapFileReader( SymSource aSource, ITracer aTracer )
            : base( aSource, aTracer )
		{
            iSymbolParser = new RVCTSymbolCreator( this, base.Collection );
		}
		#endregion

        #region API
        #endregion

        #region Properties
		#endregion

		#region From AsyncTextReaderBase
		protected override void HandleFilteredLine( string aLine )
		{
			if	( aLine == "Global Symbols" )
			{
				iState = TState.EInGlobalRegion;
			}
			else if ( aLine == "Local Symbols" )
			{
				iState = TState.EInLocalRegion;
			}
			else
			{
				switch( iState )
				{
				case TState.EInUnknownRegion:
					break;
				case TState.EInLocalRegion:
					ParseLineRegionLocal( aLine );
					break;
				case TState.EInGlobalRegion:
					ParseLineRegionGlobal( aLine );
					break;
				case TState.EComplete:
					System.Diagnostics.Debug.Assert(false);
					break;
				}
			}
		}
		#endregion

		#region Local & global (high level) line handlers
		private void ParseLineRegionLocal( string aLine )
		{
		}

		private void ParseLineRegionGlobal( string aLine )
		{
			switch( iStateGlobal )
			{
			case TGlobalState.EWaitingForImage_ER_RO_Base:
                ParseGlobalBaseAddress( aLine );
				break;
			case TGlobalState.EProcessingSymbols:
				ParseGlobalSymbol( aLine );
				break;
			default:
				break;
			}
		}
		#endregion

		#region Global section line parse methods
		private void ParseGlobalBaseAddress( string aLine )
		{
            Match m = KGlobalBaseAddressRegEx.Match( aLine );
            if ( m.Success )
            {
                string value = m.Groups[ 1 ].Value;
                base.GlobalBaseAddress = uint.Parse( value, System.Globalization.NumberStyles.HexNumber );
                iStateGlobal = TGlobalState.EProcessingSymbols;
            }
		}

		private void ParseGlobalSymbol( string aLine )
		{
			try
			{
                Symbol sym = iSymbolParser.Parse( aLine );
                if ( sym != null )
                {
                    base.ReportSymbol( sym );
                }
			}
			catch( Exception )
			{
			}
		}
		#endregion

		#region Internal enumerations
		private enum TState
		{
			EInUnknownRegion = 0,
			EInLocalRegion,
			EInGlobalRegion,
			EComplete
		}

		private enum TGlobalState
		{
			EWaitingForImage_ER_RO_Base = 0,
			EProcessingSymbols
		}
		#endregion

        #region Internal constants
        private readonly static Regex KGlobalBaseAddressRegEx = new Regex(
              "\\s*Image\\$\\$ER_RO\\$\\$Base\\s+0x([A-Fa-f0-9]{8})\\s"+
              "+Number",
             RegexOptions.CultureInvariant
            | RegexOptions.Compiled
            );
        #endregion

        #region Data members
        private TState iState = TState.EInUnknownRegion;
		private TGlobalState iStateGlobal = TGlobalState.EWaitingForImage_ER_RO_Base;
        private readonly RVCTSymbolCreator iSymbolParser;
		#endregion
	}
}
