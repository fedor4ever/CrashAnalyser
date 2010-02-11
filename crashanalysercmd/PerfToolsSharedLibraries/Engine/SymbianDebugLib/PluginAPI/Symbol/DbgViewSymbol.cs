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
using System.Collections.Generic;
using System.Text;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Symbols;
using SymbianStructuresLib.Debug.Common.FileName;

namespace SymbianDebugLib.PluginAPI.Types.Symbol
{
    public abstract class DbgViewSymbol : DbgPluginView, IEnumerable<SymbolCollection>
    {
        #region Constructors
        protected DbgViewSymbol( string aName, DbgPluginEngine aEngine )
            : base( aName, aEngine )
        {
            iPlainTextAPI = new DbgSymbolViewText( this );
        }
        #endregion

        #region Framework API
        public abstract SymbianStructuresLib.Debug.Symbols.Symbol Lookup( uint aAddress, out SymbolCollection aCollection );

        public abstract SymbolCollection CollectionByAddress( uint aAddress );

        public abstract SymbolCollection ActivateAndGetCollection( CodeSegDefinition aCodeSegment );

        public abstract SymbolCollection this[ CodeSegDefinition aCodeSeg ]
        {
            get;
        }

        public abstract SymbolCollection this[ PlatformFileName aFileName ]
        {
            get;
        }

        protected abstract IEnumerator<SymbolCollection> GetEnumeratorSymbolCollection();
        #endregion

        #region Properties
        public SymbianStructuresLib.Debug.Symbols.Symbol this[ uint aAddress ]
        {
            get
            {
                SymbolCollection col = null;
                return Lookup( aAddress, out col );
            }
        }

        public DbgSymbolViewText PlainText
        {
            get { return iPlainTextAPI; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From IEnumerable<SymbolCollection>
        public IEnumerator<SymbolCollection> GetEnumerator()
        {
            return GetEnumeratorSymbolCollection();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumeratorSymbolCollection();
        }
        #endregion

        #region Data members
        private readonly DbgSymbolViewText iPlainTextAPI;
        #endregion
    }

    #region Plain Text API helper
    public sealed class DbgSymbolViewText
    {
        #region Constructors
        internal DbgSymbolViewText( DbgViewSymbol aView )
        {
            iView = aView;
        }
        #endregion

        #region API
        public bool Lookup( uint aAddress, out uint aStartingAddress, out string aSymbolName )
        {
            uint addrEnd = 0;
            bool found = Lookup( aAddress, out aStartingAddress, out addrEnd, out aSymbolName );
            return found;
        }

        public bool Lookup( uint aAddress, out uint aStartingAddress, out uint aEndingAddress, out string aSymbolName )
        {
            aStartingAddress = 0;
            aEndingAddress = 0;
            aSymbolName = string.Empty;
            //
            SymbianStructuresLib.Debug.Symbols.Symbol sym = iView[ aAddress ];
            //
            if ( sym != null )
            {
                aStartingAddress = sym.Address;
                aEndingAddress = sym.EndAddress;
                aSymbolName = sym.Name;
            }
            //
            return ( sym != null );
        }
        #endregion

        #region Properties
        public string this[ uint aAddress ]
        {
            get
            {
                uint addrStart = 0;
                string symName = string.Empty;
                bool found = Lookup( aAddress, out addrStart, out symName );
                return symName;
            }
        }
        #endregion

        #region Data members
        private readonly DbgViewSymbol iView;
        #endregion
    }
    #endregion
}
