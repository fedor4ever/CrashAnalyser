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
using System.Text;
using System.Collections.Generic;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Symbols;
using CrashItemLib.Crash.CodeSegs;
using SymbianStructuresLib.Debug.Symbols;
using CrashXmlPlugin.FileFormat.Node;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Symbols
{
    internal class CXmlSymbol : CXmlNode
	{
		#region Constructors
        public CXmlSymbol( CISymbol aSymbol )
            : base( SegConstants.Symbols_SymbolSet_Symbol )
		{
            iSymbol = aSymbol;
		}
		#endregion

        #region API
        public static bool IsSerializable( CISymbol aSymbol )
        {
            bool ret = true;
            //
            if ( aSymbol.IsNull )
            {
                ret = false;
            }
            else
            {
                Symbol symbol = aSymbol.Symbol;
                TSymbolType type = symbol.Type;
                //
                ret = ( type != TSymbolType.EUnknown );
            }
            //
            return ret;
        }
        #endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            CXmlNode.WriteId( iSymbol, aParameters.Writer );

            aParameters.Writer.WriteElementString( SegConstants.CmnAddress, iSymbol.Symbol.Address.ToString("x8") );
            aParameters.Writer.WriteElementString( SegConstants.CmnSize, iSymbol.Symbol.Size.ToString( "x8" ) );
            aParameters.Writer.WriteElementString( SegConstants.CmnName, iSymbol.Symbol.Name );
            aParameters.Writer.WriteElementString( SegConstants.Symbols_SymbolSet_Symbol_Object, iSymbol.Symbol.Object );

            // If there is a link to a code code segment, then write that too.
            CICodeSeg correspondingCodeSeg = iSymbol.CodeSegment;
            if ( correspondingCodeSeg != null )
            {
                CXmlNode.WriteLink( correspondingCodeSeg.Id, SegConstants.CodeSegs, aParameters.Writer );
            }
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            aParameters.Writer.WriteStartElement( SegConstants.CmnAttributes );
            
            // XIP vs RAM
            bool isLoadedFromRAM = this.IsFromRAMLoadedCode;
            if ( isLoadedFromRAM )
            {
                aParameters.Writer.WriteElementString( SegConstants.CmnRAM, string.Empty );
            }
            else
            {
                aParameters.Writer.WriteElementString( SegConstants.CmnXIP, string.Empty );
            }

            // MAP vs SYMBOL
            TSymbolSource source = this.SymbolSource;
            switch( source )
            {
            case TSymbolSource.ESourceWasMapFile:
                aParameters.Writer.WriteElementString( SegConstants.Symbols_SymbolSet_Symbol_Attribute_Map, string.Empty );
                break;
            case TSymbolSource.ESourceWasSymbolFile:
                aParameters.Writer.WriteElementString( SegConstants.Symbols_SymbolSet_Symbol_Attribute_Symbol, string.Empty );
                break;
            default:
            case TSymbolSource.ESourceWasUnknown:
                break;
            }

            aParameters.Writer.WriteEndElement();
        }
        #endregion

        #region Properties
        public bool IsFromRAMLoadedCode
        {
            get
            {
                bool ret = false;
                //
                if ( iSymbol.Symbol != null )
                {
                    ret = iSymbol.Symbol.IsFromRAMLoadedCode;
                }
                //
                return ret;
            }
        }

        public TSymbolSource SymbolSource
        {
            get
            {
                TSymbolSource ret = TSymbolSource.ESourceWasUnknown;
                //
                if ( iSymbol.Symbol != null )
                {
                    ret = iSymbol.Symbol.Source;
                }
                //
                return ret;
            }
        }
        #endregion

        #region Data members
        private readonly CISymbol iSymbol;
		#endregion
	}
}
