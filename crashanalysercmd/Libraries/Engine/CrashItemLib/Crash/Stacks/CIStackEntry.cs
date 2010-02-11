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
using SymbianStackLib.Engine;
using SymbianStructuresLib.Uids;
using SymbianStructuresLib.Debug.Symbols;
using SymbianStructuresLib.Arm.Registers;
using SymbianStackLib.Data.Output.Entry;
using SymbianDebugLib.PluginAPI.Types.Symbol;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Symbols;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.Registers;

namespace CrashItemLib.Crash.Stacks
{
	public class CIStackEntry : CIElement
    {
        #region Constructors
        internal CIStackEntry( CIStack aParent, StackOutputEntry aEntry )
            : base( aParent.Container, aParent )
        {
            iEntry = aEntry;

            // If the stack entry references a symbol then associate it with
            // the parent dictionary immediately.
            if ( aEntry.Symbol != null )
            {
                ICISymbolManager symbolManager = this.SymbolManager;
                CISymbol symbol = symbolManager.SymbolDictionary.Register( aEntry.Symbol );
                this.AddChild( symbol );
                base.Trace( string.Format( "[CIStackEntry] address: 0x{0:x8}, symbol: {1}, symId: {2}", iEntry.Data, symbol.Symbol, symbol.Id ) );
            }
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public CIStack Stack
        {
            get { return (CIStack) base.Parent; }
        }

        public CISymbol Symbol
        {
            get { return base.ChildByType( typeof( CISymbol ) ) as CISymbol; }
        }

        public CIRegister Register
        {
            get
            {
                CIRegister ret = null;
                //
                if ( iEntry.IsRegisterBasedEntry )
                {
                    TArmRegisterType type = iEntry.AssociatedRegister;
                    CIStack stack = Stack;
                    CIRegisterList registers = stack.Registers;
                    if ( registers != null )
                    {
                        ret = registers[ type ];
                    }
                }
                //
                return ret;
            }
        }

        public bool MatchesSymbol
        {
            get { return Symbol != null; }
        }

        public uint Address
        {
            get { return iEntry.Address; }
        }

        public uint FunctionOffset
        {
            get { return iEntry.FunctionOffset; }
        }

        public uint Data
        {
            get { return iEntry.Data; }
        }

        public string DataAsString
        {
            get { return iEntry.DataAsString; }
        }

        public bool IsRegisterBasedEntry
        {
            get { return iEntry.IsRegisterBasedEntry; }
        }

        public bool IsAccurate
        {
            get { return iEntry.IsAccurate; }
        }

        public bool IsCurrentStackPointerEntry
        {
            get { return iEntry.IsCurrentStackPointerEntry; }
        }

        public bool IsGhost
        {
            get { return iEntry.IsGhost; }
        }

        public bool IsOutsideCurrentStackRange
        {
            get { return iEntry.IsOutsideCurrentStackRange; }
        }

        internal ICISymbolManager SymbolManager
        {
            get
            {
                ICISymbolManager ret = base.Container;
                CIProcess process = Stack.OwningProcess;
                if ( process != null )
                {
                    ret = process;
                }
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return iEntry.ToString();
        }
        #endregion

        #region Data members
        private readonly StackOutputEntry iEntry;
        #endregion
    }
}
