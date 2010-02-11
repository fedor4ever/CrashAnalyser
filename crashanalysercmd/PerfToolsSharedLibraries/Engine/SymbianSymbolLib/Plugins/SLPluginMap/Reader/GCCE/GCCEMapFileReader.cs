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
    internal class GCCEMapFileReader : MapReader
	{
        #region Delegates & events
        #endregion

        #region Constructors
        public GCCEMapFileReader( SymSource aSource, ITracer aTracer )
            : base( aSource, aTracer )
		{
            base.TrimLine = false;
            iSymbolParser = new GCCESymbolCreator( this, base.Collection );
		}
		#endregion

        #region API
        #endregion

        #region Properties
		#endregion

		#region From AsyncTextReaderBase
		protected override void HandleFilteredLine( string aLine )
		{
            switch ( iStateGlobal )
			{
            case TGlobalState.EWaitingForImage_ER_RO_Base:
                ParseGlobalBaseAddress( aLine );
                break;
            case TGlobalState.EProcessingSymbols:
                ParseSymbols( aLine );
                break;
            default:
                break;
            }
		}

        protected override void HandleReadCompleted()
        {
            try
            {
                SortAndSize();
            }
            finally
            {
                base.HandleReadCompleted();
            }
        }
		#endregion

        #region Internal methods
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

		private void ParseSymbols( string aLine )
		{
			try
			{
                // It's the start of a section...
                Match m = KSectionRegEx.Match( aLine );
                if ( m.Success )
                {
                    iSectionType = GetSection( aLine );
                    iLastSeenObjectName = string.Empty;
                }
                else if ( iSectionType != TSection.ESectionUnsupported )
                {
                    m = KSizeAndObjectRegEx.Match( aLine );
                    if ( m.Success )
                    {
                        iLastSeenObjectName = m.Groups[ "Object" ].Value.Trim();
                    }
                    else
                    {
                        Symbol sym = iSymbolParser.Parse( aLine );
                        if ( sym != null )
                        {
                            // Update object with cached information
                            sym.Object = iLastSeenObjectName;
                            iUnsortedSymbols.Add( sym );
                        }
                    }
                }
			}
			catch( Exception )
			{
			}
		}

        private void ListSymbols( IEnumerable<Symbol> aList )
        {
            foreach ( Symbol sym in aList )
            {
                string line = sym.ToString();
                if ( sym.InstructionSet == SymbianStructuresLib.Arm.TArmInstructionSet.ETHUMB )
                {
                    line = "T " + line;
                }
                else
                {
                    line = "  " + line;
                }
                //
                System.Diagnostics.Debug.WriteLine( line );
            }
        }

        private void SortAndSize()
        {
            // Sort the symbols into ascending address order
            Comparison<Symbol> comparer = delegate( Symbol aLeft, Symbol aRight )
            {
                // compare by address
                int ret = aLeft.Address.CompareTo( aRight.Address );
                return ret;
            };
            iUnsortedSymbols.Sort( comparer );

            // Subsume duplicates
            int count = iUnsortedSymbols.Count;
            for ( int i = count - 1; i >= 1; i-- )
            {
                Symbol sym1 = iUnsortedSymbols[ i - 0];
                Symbol sym2 = iUnsortedSymbols[ i - 1 ];
                //
                if ( sym1.Name == sym2.Name && sym1.Object == sym2.Object )
                {
                    // Since the symbol is the same and since we do not have any
                    // size information for GCCE map files, then we can safely
                    // discard one of the symbols.
                    iUnsortedSymbols.RemoveAt( i );
                }
            }

            // Now calculate sizes
            count = iUnsortedSymbols.Count;
            for ( int i = 0; i < count - 1; i++ )
            {
                Symbol sym1 = iUnsortedSymbols[ i + 0 ];
                Symbol sym2 = iUnsortedSymbols[ i + 1 ];
                
                // Calculate the symbol size based upon the address of the next symbol.
                // This may not be totally accurate, but it's the best we can do
                sym1.Size = ( sym2.Address - sym1.Address );

                // Save the symbol
                base.ReportSymbol( sym1 );
            }
        }

        private static TSection GetSection( string aText )
        {
            TSection ret = TSection.ESectionUnsupported;
            //
            Match m = KSectionRegEx.Match( aText );
            if ( m.Success )
            {
                string sectionName = m.Groups[ 0 ].Value;
                switch ( sectionName )
                {
                case ".text":
                    ret = TSection.ESectionText;
                    break;
                case ".emb_text":
                    ret = TSection.ESectionEmbText;
                    break;
                case ".rodata":
                    ret = TSection.ESectionReadOnlyData;
                    break;
                default:
                    break;
                }
            }
            //
            return ret;
        }
		#endregion

		#region Internal enumerations
		private enum TGlobalState
		{
			EWaitingForImage_ER_RO_Base = 0,
			EProcessingSymbols
		}

        private enum TSection
        {
            ESectionUnsupported = -1,
            ESectionText = 0,
            ESectionEmbText,
            ESectionReadOnlyData
        }
		#endregion

        #region Internal constants
        private readonly static Regex KGlobalBaseAddressRegEx = new Regex(
              @"\s*0x(?<Address>[A-Fa-f0-9]{8})\s+PROVIDE \(Image\$\$ER_RO\$\$Base",
            RegexOptions.Multiline
            | RegexOptions.CultureInvariant
            | RegexOptions.Compiled
            );

        private readonly static Regex KSectionRegEx = new Regex(
              "^\\.([a-zA-Z0-9_\\$\\.]*)",
            RegexOptions.Multiline
            | RegexOptions.CultureInvariant
            | RegexOptions.Compiled
            );
        private readonly static Regex KSizeAndObjectRegEx = new Regex(
              "(?<Prefix>.+?)0x(?<Address>[A-Fa-f0-9]{8})\\s+0x(?<Size>[A-F" +
              "a-f0-9]{1,8})\\s(?<Object>.+)",
            RegexOptions.Multiline
            | RegexOptions.CultureInvariant
            | RegexOptions.Compiled
            );
        #endregion

        #region Data members
		private TGlobalState iStateGlobal = TGlobalState.EWaitingForImage_ER_RO_Base;
        private readonly GCCESymbolCreator iSymbolParser;
        private List<Symbol> iUnsortedSymbols = new List<Symbol>();
        private string iLastSeenObjectName = string.Empty;
        private TSection iSectionType = TSection.ESectionUnsupported;
		#endregion
	}
}
