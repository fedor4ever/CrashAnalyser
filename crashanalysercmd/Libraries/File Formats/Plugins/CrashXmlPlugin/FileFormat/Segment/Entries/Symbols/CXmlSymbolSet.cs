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
using SymbianStructuresLib.Debug.Symbols;
using CrashXmlPlugin.FileFormat.Node;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Symbols
{
    internal class CXmlSymbolSet : CXmlNode
	{
		#region Constructors
        public CXmlSymbolSet( SymbolCollection aCollection )
            : base( SegConstants.Symbols_SymbolSet )
		{
            iCollection = aCollection;
		}
		#endregion

        #region API
        public void Add( CISymbol aSymbol )
        {
            if ( !iSymbols.ContainsKey( aSymbol.Id ) )
            {
                iSymbols.Add( aSymbol.Id, aSymbol );
            }
        }
        #endregion

        #region From CXmlNode
        protected override void XmlSerializeContent( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            aParameters.Writer.WriteElementString( SegConstants.Symbols_SymbolSet_Source, iCollection.FileName.FileNameInHost );
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            foreach ( KeyValuePair<CIElementId, CISymbol> kvp in iSymbols )
            {
                CXmlSymbol xmlSymbol = new CXmlSymbol( kvp.Value );
                xmlSymbol.XmlSerialize( aParameters );
            }
        }
        #endregion

        #region Properties
        #endregion

        #region Data members
        private readonly SymbolCollection iCollection;
        private Dictionary<CIElementId, CISymbol> iSymbols = new Dictionary<CIElementId, CISymbol>();
		#endregion
	}
}
