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
using SymbianStructuresLib.Debug.Symbols;
using SymbianSymbolLib.SourceManagement.Source;
using SymbianSymbolLib.SourceManagement.Provisioning;
using SymbianUtils.FileTypes;
using SymbianUtils;
using SLPluginSymbol.Data;
using SLPluginSymbol.Reader;
using SLPluginSymbol.Source;
using SLPluginSymbol.Utilities;

namespace SLPluginSymbol.Provider
{
    public class SymbolSourceProvider : SymSourceProvider
    {
        #region Constructors
        public SymbolSourceProvider( SymSourceProviderManager aManager )
            : base( aManager )
        {
        }
        #endregion

        #region From SymSourceProvider
        public override SymSourceCollection CreateSources( string aFileName )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "[Symbol Memory] START -> {0:d12}, source: {1}", System.GC.GetTotalMemory( true ), aFileName ) );
            SymbolSource source = new SymbolSource( aFileName, this );
            return new SymSourceCollection( source );
        }

        public override void ReadSource( SymSource aSource, TSynchronicity aSynchronicity )
        {
            SymbolSource source = (SymbolSource) aSource;
            //
            SymbolFileData data = source.ExcavateData();
            SymbolFileReader reader = new SymbolFileReader( source, data );
            reader.Read( aSynchronicity );
        }

        public override SymFileTypeList FileTypes
        {
            get
            {
                SymFileTypeList ret = new SymFileTypeList();
                //
                ret.Add( new SymFileType( ".symbol", "Symbian OS Symbolic Information File" ) );
                //
                return ret;
            }
        }

        public override string Name
        {
            get { return "SYMBOL"; }
        }
        #endregion

        #region Properties
        #endregion

        #region Event handlers

        #endregion

        #region Internal constants
        private const string KSymbolFileExtension = ".SYMBOL";
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
