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
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI.Types.Symbol;
using SymbianStructuresLib.Arm.Registers;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Base.DataBinding;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Symbols;
using CrashItemLib.Crash.Threads;
using CrashItemLib.Crash.Processes;

namespace CrashItemLib.Crash.Registers
{
    public class CIRegister : CIElement
	{
		#region Constructors
        public CIRegister( CIRegisterList aList, TArmRegisterType aType, uint aValue )
            : this( aList, aType, ArmRegister.GetTypeName( aType ), aValue )
        {
        }

        public CIRegister( CIRegisterList aList, TArmRegisterType aType, string aName, uint aValue )
            : base( aList.Container )
		{
            iList = aList;

            // Create register and observe when it changes value
            iRegister = new ArmRegister( aType, aName, aValue );
            iRegister.Tag = this;

            // Prepare non-resolved symbol. I.e. this saves the address
            // but doesn't actually do any symbolic look up at this stage.
            ICISymbolManager symbolManager = this.SymbolManager;
            CISymbol symbol = symbolManager.SymbolDictionary.Register( iRegister.Value );
            base.AddChild( symbol );
        }
		#endregion

        #region API
        #endregion

        #region Properties
        public override string Name
        {
            get { return TypeName; }
        }

        public TArmRegisterType Type
        {
            get { return Register.RegType; }
        }

        public TArmRegisterBank Bank
        {
            get { return iList.Bank; }
        }

        public string TypeName
        {
            get { return Register.OriginalName; }
        }

        public uint Value
        {
            get { return Register.Value; }
            set 
            {
                // Update register value and also symbol
                iRegister.Value = value;

                // Refresh symbol registration
                ICISymbolManager symbolManager = this.SymbolManager;
                symbolManager.SymbolDictionary.RefreshRegistration( this, value, this.Symbol );
            }
        }

        public ArmRegister Register
        {
            get { return iRegister; }
        }

        public CISymbol Symbol
        {
            get { return base.ChildByType( typeof( CISymbol ) ) as CISymbol; }
        }

        public CIThread OwningThread
        {
            get
            {
                CIThread ret = null;
                //
                if ( iList.OwningThread != null )
                {
                    ret = iList.OwningThread;
                }
                //
                return ret;
            }
        }

        public CIProcess OwningProcess
        {
            get
            {
                CIProcess ret = null;
                //
                if ( OwningThread != null )
                {
                    ret = OwningThread.OwningProcess;
                }
                //
                return ret;
            }
        }

        internal ICISymbolManager SymbolManager
        {
            get
            {
                ICISymbolManager ret = base.Container;
                //
                CIProcess process = this.OwningProcess;
                if ( process != null )
                {
                    ret = process;
                }
                //
                return ret;
            }
        }
        #endregion

        #region Operators
        public static implicit operator ArmRegister( CIRegister aRegister )
        {
            return aRegister.Register;
        }

        public static implicit operator uint( CIRegister aRegister )
        {
            return aRegister.Register.Value;
        }

        public static implicit operator CIDBRow( CIRegister aRegister )
        {
            CIDBRow row = new CIDBRow();
            
            // To ensure that the register and cells are correctly associated
            row.Element = aRegister;

            row.Add( new CIDBCell( aRegister.TypeName ) );
            row.Add( new CIDBCell( aRegister.Value.ToString("x8") ) );
            row.Add( new CIDBCell( aRegister.Symbol.Name ) );
            //
            return row;
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return iRegister.ToString();
        }
        #endregion

        #region From CIElement
        internal override void DoFinalize( CIElementFinalizationParameters aParams, Queue<CIElement> aCallBackLast, bool aForceFinalize )
        {
            base.DoFinalize( aParams, aCallBackLast, aForceFinalize );
        }
        #endregion

        #region Data members
        private readonly CIRegisterList iList;
        private readonly ArmRegister iRegister;
        #endregion
    }
}
