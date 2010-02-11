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
using System.Collections.Generic;
using System.Text;
using SymbianStructuresLib.Debug.Symbols;
using CrashDebuggerLib.Structures.KernelObjects;
using SymbianStructuresLib.Arm.Registers;

namespace CrashDebuggerLib.Structures.Register
{
    public class RegisterEntry : ArmRegister
    {
        #region Constructors
        internal RegisterEntry( RegisterCollection aParent, TArmRegisterType aType )
            : base( aType )
        {
            iParent = aParent;
        }

        internal RegisterEntry( RegisterCollection aParent, string aName, uint aValue )
            : base( aName, aValue )
        {
            iParent = aParent;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public string SymbolString
        {
            get
            {
                Symbol symbol = Symbol;
                if ( symbol != null )
                {
                    return symbol.Name;
                }
                return "0x" + Value.ToString( "x8" );
            }
        }

        public Symbol Symbol
        {
            get
            {
                if ( iSymbol == null )
                {
                    uint address = this.Value;
                    iSymbol = iParent.LookUpSymbol( address );
                }
                return iSymbol;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            ret.Append( base.ToString() );
            Symbol symbol = Symbol;
            if ( symbol != null )
            {
                ret.Append( " " + symbol.Name );
            }
            //
            return ret.ToString();
        }
        #endregion

        #region Data members
        private readonly RegisterCollection iParent;
        private Symbol iSymbol = null;
        #endregion
    }
}
