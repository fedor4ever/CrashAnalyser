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
using SymbianUtils;
using SymbianUtils.FileSystem.FilePair;
using SymbianStructuresLib.CodeSegments;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.PluginAPI.Types.Code;
using SymbianDebugLib.PluginAPI.Types.Symbol;

namespace SymbianDebugLib.Engine
{
    public class DbgEngineView : DisposableObject
    {
        #region Constructors
        internal DbgEngineView( DbgEngine aEngine, string aName, CodeSegDefinitionCollection aCodeSegments, TDbgViewDeactivationType aDeactivationType )
        {
            iName = aName;
            iEngine = aEngine;
            iDeactivationType = aDeactivationType;
            iCodeSegments = new CodeSegDefinitionCollection( aCodeSegments );
            //
            iViewCode = (DbgViewCode) EngineCode.CreateView( iName );
            if ( iViewCode != null )
            {
                iViewCode.Activate( aCodeSegments );
            }
            //
            iViewSymbols = (DbgViewSymbol) EngineSymbols.CreateView( iName );
            if ( iViewSymbols != null )
            {
                iViewSymbols.Activate( aCodeSegments );
            }
        }
        #endregion

        #region API
        public bool SerializeTaggedCollections( FileNamePairCollection aFilesToSave )
        {
            bool savedSyms = Symbols.SerializeTaggedCollections( aFilesToSave );
            bool savedCode = Code.SerializeTaggedCollections( aFilesToSave );
            //
            return ( savedCode || savedSyms );
        }
        #endregion

        #region Properties
        public string Name
        {
            get
            {
                return iName;
            }
        }

        public DbgViewCode Code
        {
            get
            {
                if ( iViewCode == null )
                {
                    throw new NotSupportedException();
                }
                //
                return iViewCode; 
            }
        }

        public DbgViewSymbol Symbols
        {
            get
            {
                if ( iViewSymbols == null )
                {
                    throw new NotSupportedException();
                }
                //
                return iViewSymbols; 
            }
        }

        public DbgEngineCode EngineCode
        {
            get { return iEngine.Code; }
        }

        public DbgEngineSymbol EngineSymbols
        {
            get { return iEngine.Symbols; } 
        }
        #endregion

        #region Operators
        public static implicit operator DbgViewCode( DbgEngineView aView )
        {
            return aView.Code;
        }

        public static implicit operator DbgViewSymbol( DbgEngineView aView )
        {
            return aView.Symbols;
        }
        #endregion

        #region From DisposableObject
        protected override void CleanupManagedResources()
        {
            try
            {
                base.CleanupManagedResources();
            }
            finally
            {
                if ( iViewCode != null )
                {
                    if ( iDeactivationType == TDbgViewDeactivationType.EDeactivateWhenDisposed )
                    {
                        iViewCode.Deactivate( iCodeSegments );
                    }
                    //
                    iViewCode.Dispose();
                    iViewCode = null;
                }
                if ( iViewSymbols != null )
                {
                    if ( iDeactivationType == TDbgViewDeactivationType.EDeactivateWhenDisposed )
                    {
                        iViewSymbols.Deactivate( iCodeSegments );
                    }
                    //
                    iViewSymbols.Dispose();
                    iViewSymbols = null;
                }
            }
        }
        #endregion

        #region From System.Object
        public override int GetHashCode()
        {
            return iName.GetHashCode();
        }

        public override string ToString()
        {
            return iName;
        }
        #endregion

        #region Data members
        private readonly string iName;
        private readonly DbgEngine iEngine;
        private readonly CodeSegDefinitionCollection iCodeSegments;
        private readonly TDbgViewDeactivationType iDeactivationType;
        private DbgViewCode iViewCode;
        private DbgViewSymbol iViewSymbols;
        #endregion
    }
}
