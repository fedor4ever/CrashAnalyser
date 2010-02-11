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
using CrashDebuggerLib.Structures.KernelObjects;
using CrashDebuggerLib.Structures.Register;
using CrashDebuggerLib.Structures.Common;
using SymbianStructuresLib.Arm.Registers;

namespace CrashDebuggerLib.Structures.Register
{
    public class RegisterSet : CrashDebuggerAware
    {
        #region Constructors
        internal RegisterSet( CrashDebuggerInfo aCrashDebugger )
            : base( aCrashDebugger )
        {
            Clear();
        }
        #endregion

        #region API
        public void Clear()
        {
            iBanks.Clear();

            // General regs don't share a bank - they're just a dumping
            // ground for CPU specific entries.
            AddBank( RegisterCollection.TType.ETypeGeneral );

            // The rest use banking. CPSR goes in the common bank.
            RegisterCollection common = new RegisterCollection( CrashDebugger, RegisterCollection.TType.ETypeCommonBank );
            iBanks.Add( RegisterCollection.TType.ETypeCommonBank, common );
            common.AddMany( TArmRegisterType.EArmReg_CPSR,
                             TArmRegisterType.EArmReg_00,
                             TArmRegisterType.EArmReg_01,
                             TArmRegisterType.EArmReg_02,
                             TArmRegisterType.EArmReg_03,
                             TArmRegisterType.EArmReg_04,
                             TArmRegisterType.EArmReg_05,
                             TArmRegisterType.EArmReg_06,
                             TArmRegisterType.EArmReg_07 );

            // These are all fairly normal. They have their own SP, LR and SPSR
            // The others are common.
            AddBank( RegisterCollection.TType.ETypeUser, common );
            AddBank( RegisterCollection.TType.ETypeAbort, common );
            AddBank( RegisterCollection.TType.ETypeInterrupt, common );
            AddBank( RegisterCollection.TType.ETypeSupervisor, common );
            AddBank( RegisterCollection.TType.ETypeSystem, common );
            AddBank( RegisterCollection.TType.ETypeUndefined, common );

            // FIQ is special - it has shadows of R8->12
            AddBank( RegisterCollection.TType.ETypeFastInterrupt, common,
                     TArmRegisterType.EArmReg_08,
                     TArmRegisterType.EArmReg_09,
                     TArmRegisterType.EArmReg_10,
                     TArmRegisterType.EArmReg_11,
                     TArmRegisterType.EArmReg_12
                     );
        }

        public bool Available( RegisterCollection.TType aType )
        {
            bool ret = iBanks.ContainsKey( aType );
            return ret;
        }
        #endregion

        #region Properties
        public RegisterEntryCPSR CPSR
        {
            get
            {
                RegisterEntryCPSR cpsr = (RegisterEntryCPSR) this[ RegisterCollection.TType.ETypeCommonBank, TArmRegisterType.EArmReg_CPSR ];
                return cpsr;
            }
        }

        public RegisterCollection.TType CPSRBankType
        {
            get { return CPSR.CurrentBank; }
        }

        public RegisterEntry CurrentSP
        {
            get
            {
                RegisterCollection.TType bank = CPSRBankType;
                RegisterEntry ret = this[ bank, TArmRegisterType.EArmReg_SP ];
                return ret;
            }
        }

        public RegisterEntry CurrentLR
        {
            get
            {
                RegisterCollection.TType bank = CPSRBankType;
                RegisterEntry ret = this[ bank, TArmRegisterType.EArmReg_LR ];
                return ret;
            }
        }

        public RegisterCollection this[ RegisterCollection.TType aBank ]
        {
            get
            {
                RegisterCollection ret = iBanks[ aBank ];
                return ret;
            }
        }

        public RegisterEntry this[ RegisterCollection.TType aBank, string aName ]
        {
            get
            {
                RegisterCollection bank = this[ aBank ];
                return bank[ aName ];
            }
        }

        public RegisterEntry this[ RegisterCollection.TType aBank, TArmRegisterType aType ]
        {
            get
            {
                RegisterCollection bank = this[ aBank ];
                return bank[ aType ];
            }
        }
        #endregion

        #region Internal methods
        private RegisterCollection AddBank( RegisterCollection.TType aType )
        {
            return AddBank( aType, null );
        }

        private RegisterCollection AddBank( RegisterCollection.TType aType, RegisterCollection aLinkedWith )
        {
            TArmRegisterType[] empty = new TArmRegisterType[] { };
            return AddBank( aType, aLinkedWith, empty );
        }

        private RegisterCollection AddBank( RegisterCollection.TType aType, RegisterCollection aLinkedWith, params TArmRegisterType[] aExtraRegs )
        {
            RegisterCollection bank = new RegisterCollection( CrashDebugger, aType, null, aLinkedWith );
            iBanks.Add( aType, bank );

            // Create bank specific registers
            string bankName = RegisterCollection.BankName( aType );
            if ( bankName != string.Empty )
            {
                bankName = "_" + bankName;
            }

            bank.Add( "R13" + bankName, 0 );
            bank.Add( "R14" + bankName, 0 );
            bank.Add( "SPSR" + bankName, 0 );

            // Create custom registers
            foreach( TArmRegisterType custom in aExtraRegs )
            {
                string name = ArmRegister.GetTypeName( custom ) + bankName;
                bank.Add( name, 0 );
            }

            return bank;
        }
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        private Dictionary<RegisterCollection.TType, RegisterCollection> iBanks = new Dictionary<RegisterCollection.TType, RegisterCollection>();
        #endregion
    }
}
