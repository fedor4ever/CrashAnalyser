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
using System.Collections;
using System.IO;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbolLib.Generics;
using SymbolLib.Sources.Map.File;
using SymbolLib.Sources.Map.Symbol;
using SymbolLib.Sources.Map.Engine;

namespace SymbolLib.Sources.Map.Parser
{
    #region SymbolEntryCreator interface
    internal interface SymbolEntryCreator
    {
        MapSymbol CreateSymbol();
    }
    #endregion

    internal class MapFileParser : AsyncTextFileReader
	{
        #region Delegates & events
        public delegate void SymbolCreatedHandler( MapSymbol aSymbol );
        public event SymbolCreatedHandler SymbolCreated;

        public delegate void MapFileBaseAddressHandler( uint aBaseAddress );
        public event MapFileBaseAddressHandler BaseAddressHandler;
        #endregion

        #region Constructors & destructor
        public MapFileParser( SymbolEntryCreator aEntryCreator, string aMapFileName, ITracer aTracer )
        :   base( aMapFileName, aTracer )
		{
            iEntryCreator = aEntryCreator;
		}
		#endregion

        #region API
        public void Read( TSynchronicity aSynchronicity )
        {
            base.StartRead( aSynchronicity );
        }
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
			try
			{
				// Image$$ER_RO$$Base                       0x00008000   Number         0  anon$$obj.o(linker$$defined$$symbols)
                MapSymbol baseOffsetEntry = iEntryCreator.CreateSymbol();

                // Keep trying to parse until we are successful. First time we succeed
                // we use the symbol address as the global offset address within the map
                // file (typically 0x8000)
                bool parsedOkay = baseOffsetEntry.Parse( aLine );
                if ( parsedOkay && baseOffsetEntry.Address > 0 && BaseAddressHandler != null )
                {
                    BaseAddressHandler( (uint) baseOffsetEntry.Address );
                    iStateGlobal = TGlobalState.EProcessingSymbols;
                }
			}
			catch(GenericSymbolicCreationException)
			{
			}
		}

		private void ParseGlobalSymbol( string aLine )
		{
			try
			{
				// Image$$ER_RO$$Base                       0x00008000   Number         0  anon$$obj.o(linker$$defined$$symbols)
                MapSymbol symbol = iEntryCreator.CreateSymbol();
				bool parsedOkay = symbol.Parse( aLine );
                if ( parsedOkay && SymbolCreated != null )
				{
                    SymbolCreated( symbol );
				}
			}
			catch(GenericSymbolicCreationException)
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

		#region Data members
		private readonly SymbolEntryCreator iEntryCreator;
		private TState iState = TState.EInUnknownRegion;
		private TGlobalState iStateGlobal = TGlobalState.EWaitingForImage_ER_RO_Base;
		#endregion
	}
}
