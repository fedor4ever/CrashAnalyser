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
using CrashItemLib.Crash.Processes;
using SymbianStructuresLib.Debug.Symbols;

namespace CrashXmlPlugin.FileFormat.Segment.Entries.Symbols
{
    internal class CXmlSegSymbols : CXmlSegBase
	{
		#region Constructors
        public CXmlSegSymbols()
            : base( SegConstants.Symbols )
		{
		}
		#endregion

        #region From CXmlSegBase
        public override int SegmentPriority
        {
            get { return 10; }
        }
        #endregion

        #region From CXmlNode
        public override void XmlSerialize( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            // Don't write symbols or stack if we were asked to create the summary file.
            if ( aParameters.DetailLevel == CrashItemLib.Sink.CISinkSerializationParameters.TDetailLevel.EFull )
            {
                base.XmlSerialize( aParameters );
            }
        }

        protected override void XmlSerializeChildren( CrashXmlPlugin.FileFormat.Document.CXmlDocumentSerializationParameters aParameters )
        {
            // First partition the symbols into sets
            CISymbolDictionary globalSymbols = aParameters.Container.Symbols;
            Partition( globalSymbols );

            // Also get the dictionaries from the individual processes
            CIElementList<CIProcess> processes = aParameters.Container.ChildrenByType<CIProcess>();
            foreach ( CIProcess process in processes )
            {
                CISymbolDictionary processSymbols = process.Symbols;
                Partition( processSymbols );
            }

            // Then serialize the sets
            foreach ( KeyValuePair<string, CXmlSymbolSet> kvp in iSets )
            {
                CXmlSymbolSet set = kvp.Value;
                set.XmlSerialize( aParameters );
            }
        }
        #endregion

        #region Internal methods
        private void Partition( CISymbolDictionary aDictionary )
        {
            foreach ( CISymbol crashItemSymbol in aDictionary )
            {
                bool serializable = CXmlSymbol.IsSerializable( crashItemSymbol );
                if ( serializable )
                {
                    Symbol symbol = crashItemSymbol.Symbol;
                    SymbolCollection symbolCollection = symbol.Collection;
                    //
                    string collectionName = symbolCollection.FileName.FileNameInHost.ToUpper();
                    CXmlSymbolSet set = null;
                    //
                    if ( !iSets.ContainsKey( collectionName ) )
                    {
                        set = new CXmlSymbolSet( symbolCollection );
                        iSets.Add( collectionName, set );
                    }
                    else
                    {
                        set = iSets[ collectionName ];
                    }
                    //
                    set.Add( crashItemSymbol );
                }
            }
        }
        #endregion

        #region Data members
        private SortedDictionary<string, CXmlSymbolSet> iSets = new SortedDictionary<string, CXmlSymbolSet>();
		#endregion
	}
}
