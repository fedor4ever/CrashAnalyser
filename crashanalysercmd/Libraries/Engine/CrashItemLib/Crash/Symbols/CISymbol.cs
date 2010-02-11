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
using SymbianUtils.Range;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.PluginAPI.Types.Symbol;
using SymbianStructuresLib.Debug.Symbols;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Symbols;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Stacks;
using CrashItemLib.Crash.CodeSegs;
using CrashItemLib.Crash.Threads;
using CrashItemLib.Crash.Summarisable;

namespace CrashItemLib.Crash.Symbols
{
    public class CISymbol : CIElement
    {
        #region Constructors
        internal CISymbol( CIContainer aContainer, uint aAddress )
            : base( aContainer )
        {
            iAddress = aAddress;
        }

        internal CISymbol( CIContainer aContainer, Symbol aSymbol )
            : this( aContainer, aSymbol.Address )
        {
            AssignPermanentSymbol( aSymbol );
        }
        #endregion

        #region API
        internal CISymbolDictionary Dictionary
        {
            get
            {
                CISymbolDictionary ret = base.Container.Symbols;
                CIProcess owningProcess = this.OwningProcess;
                //
                if ( owningProcess != null )
                {
                    ret = owningProcess.Symbols;
                }
                //
                return ret;
            }
        }

        internal void AssignPermanentSymbol( Symbol aSymbol )
        {
            iSymbol = aSymbol;
            iSymbolLocked = true;
        }

        internal int ReferenceCountIncrement()
        {
            return ++iRefCount;
        }

        internal int ReferenceCountDecrement()
        {
            return --iRefCount;
        }
        #endregion

        #region Properties
        public override string Name
        {
            get 
            {
                string ret = string.Empty;
                //
                if ( !IsNull )
                {
                    ret = Symbol.Name;
                }
                //
                return ret;
            }
            set { }
        }

        public uint Address
        {
            get { return iAddress; }
        }

        public bool IsNull
        {
            get
            {
                return iSymbol == null;
            }
        }

        public CICodeSeg CodeSegment
        {
            get
            {
                CICodeSeg ret = null;
                //
                CIProcess process = OwningProcess;
                if ( process != null )
                {
                    ret = process.CodeSegments[ Address ];
                }
                //
                return ret;
            }
        }

        public Symbol Symbol
        {
            get { return iSymbol; }
        }
        #endregion

        #region Operators
        public static implicit operator Symbol( CISymbol aSelf )
        {
            return aSelf.Symbol;
        }
        #endregion

        #region From CIElement
        internal override void OnFinalize( CIElementFinalizationParameters aParams )
        {
            try
            {
                base.OnFinalize( aParams );
            }
            finally
            {
                if ( iAddress != 0 )
                {
                    if ( iSymbolLocked == false )
                    {
                        iSymbol = aParams.DebugEngineView.Symbols[ iAddress ];
                        base.Trace( string.Format( "[CISymbol] OnFinalize() - address: 0x{0:x8}, id: {1}, iSymbol: {2}", iAddress, base.Id, iSymbol ) );
                    }
                }
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            if ( iSymbol != null )
            {
                return iSymbol.ToString();
            }
            //
            return "0x" + iAddress.ToString( "x8" );
        }
        #endregion

        #region Internal methods
        private CIProcess OwningProcess
        {
            get
            {
                CIProcess ret = null;
                //
                CIElement parent = this.Parent;
                while ( parent != null )
                {
                    if ( parent is CIProcess )
                    {
                        ret = (CIProcess) parent;
                        break;
                    }
                    else if ( parent is CIStack )
                    {
                        CIStack entity = (CIStack) parent;
                        ret = entity.OwningProcess;
                        break;
                    }
                    else if ( parent is CIThread )
                    {
                        CIThread entity = (CIThread) parent;
                        ret = entity.OwningProcess;
                        break;
                    }
                    else if ( parent is CISummarisableEntity )
                    {
                        CISummarisableEntity entity = (CISummarisableEntity) parent;
                        ret = entity.Process;
                        break;
                    }
                    else
                    {
                        parent = parent.Parent;
                    }
                }
                //
                return ret;
            }
        }
        #endregion

        #region Data members
        private readonly uint iAddress;
        private int iRefCount = 0;
        private bool iSymbolLocked = false;
        private Symbol iSymbol = null;
        #endregion
    }
}
