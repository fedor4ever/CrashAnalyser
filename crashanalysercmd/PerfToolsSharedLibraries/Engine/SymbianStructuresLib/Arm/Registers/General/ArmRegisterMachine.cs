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

namespace SymbianStructuresLib.Arm.Registers
{
    public class ArmRegisterMachine : IEnumerable<ArmRegister>
    {
        #region Constructors
        public ArmRegisterMachine()
            : this( null )
        {
        }
        
        public ArmRegisterMachine( IARMBackingStore aBackingStore )
        {
            iBackingStore = aBackingStore;
            //
            Clear();
        }
        #endregion

        #region API
        public void Clear()
        {
            iBanks.Clear();

            // Common registers
            ArmRegisterCollection common = CreateCollection( TArmRegisterBank.ETypeCommon );
            common.AddMany( TArmRegisterType.EArmReg_CPSR,
                            TArmRegisterType.EArmReg_00,
                            TArmRegisterType.EArmReg_01,
                            TArmRegisterType.EArmReg_02,
                            TArmRegisterType.EArmReg_03,
                            TArmRegisterType.EArmReg_04,
                            TArmRegisterType.EArmReg_05,
                            TArmRegisterType.EArmReg_06,
                            TArmRegisterType.EArmReg_07,
                            TArmRegisterType.EArmReg_PC
                          );
            AddBank( common );

            // User regs
            ArmRegisterCollection user = CreateCollection( TArmRegisterBank.ETypeUser, common );
            user.AddMany( TArmRegisterType.EArmReg_08,
                          TArmRegisterType.EArmReg_09,
                          TArmRegisterType.EArmReg_10,
                          TArmRegisterType.EArmReg_11,
                          TArmRegisterType.EArmReg_12,
                          TArmRegisterType.EArmReg_SP,
                          TArmRegisterType.EArmReg_LR
                          );
            AddBank( user );

            // These are all fairly normal. They have their own SP, LR and SPSR
            // The others are common.
            AddBank( TArmRegisterBank.ETypeAbort, user );
            AddBank( TArmRegisterBank.ETypeInterrupt, user );
            AddBank( TArmRegisterBank.ETypeSupervisor, user );
            AddBank( TArmRegisterBank.ETypeSystem, user );
            AddBank( TArmRegisterBank.ETypeUndefined, user );

            // FIQ is special - it has shadows of R8->12
            AddBank( TArmRegisterBank.ETypeFastInterrupt, common,
                     TArmRegisterType.EArmReg_08,
                     TArmRegisterType.EArmReg_09,
                     TArmRegisterType.EArmReg_10,
                     TArmRegisterType.EArmReg_11,
                     TArmRegisterType.EArmReg_12
                     );

            // Don't forget co-processor or exception regs
            ArmRegisterCollection exception = CreateCollection( TArmRegisterBank.ETypeException );
            exception.AddMany( TArmRegisterType.EArmReg_EXCCODE
                          );
            AddBank( exception );
            ArmRegisterCollection coprocessor = CreateCollection( TArmRegisterBank.ETypeCoProcessor );
            coprocessor.AddMany( TArmRegisterType.EArmReg_FAR,
                                 TArmRegisterType.EArmReg_FSR
                          );
            AddBank( coprocessor );
        }

        public bool Available( TArmRegisterBank aType )
        {
            bool ret = iBanks.ContainsKey( aType );
            return ret;
        }

        public ArmRegisterCollection CurrentRegisters()
        {
            // Combine all relevant register values into a complete register collection
            ArmRegisterCollection currentBank = CurrentBank;

            // Standard regs
            ArmRegisterCollection ret = CreateCollection( TArmRegisterBank.ETypeUnknown );
            foreach( ArmRegister reg in currentBank )
            {
                ret.Add( reg.OriginalName, reg.Value );
            }

            // Co-processor
            ArmRegisterCollection cop = this[TArmRegisterBank.ETypeCoProcessor];
            foreach ( ArmRegister reg in cop )
            {
                ret.Add( reg.OriginalName, reg.Value );
            }

            // Exception
            ArmRegisterCollection exc = this[ TArmRegisterBank.ETypeException ];
            foreach ( ArmRegister reg in exc )
            {
                ret.Add( reg.OriginalName, reg.Value );
            }

            return ret;
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iBanks.Count; }
        }

        public ArmRegister CPSR
        {
            get
            {
                ArmRegister ret = this[ TArmRegisterBank.ETypeCommon, TArmRegisterType.EArmReg_CPSR ];
                return ret;
            }
            set
            {
                ArmRegister cpsr = CPSR;
                cpsr.Value = value.Value;
            }
        }

        public TArmRegisterBank CPSRBankType
        {
            get
            { 
                ArmRegister cpsr = CPSR;
                return ArmRegisterBankUtils.ExtractBank( cpsr );
            }
        }

        public ArmRegister CurrentSP
        {
            get
            {
                TArmRegisterBank bank = CPSRBankType;
                ArmRegister ret = this[ bank, TArmRegisterType.EArmReg_SP ];
                return ret;
            }
        }

        public ArmRegister CurrentLR
        {
            get
            {
                TArmRegisterBank bank = CPSRBankType;
                ArmRegister ret = this[ bank, TArmRegisterType.EArmReg_LR ];
                return ret;
            }
        }

        public ArmRegisterCollection CurrentBank
        {
            get
            {
                TArmRegisterBank bank = CPSRBankType;
                return this[ bank ];
            }
        }

        public ArmRegisterCollection this[ TArmRegisterBank aBank ]
        {
            get
            {
                ArmRegisterCollection ret = iBanks[ aBank ];
                return ret;
            }
        }

        public ArmRegister this[ TArmRegisterBank aBank, string aName ]
        {
            get
            {
                ArmRegisterCollection bank = this[ aBank ];
                return bank[ aName ];
            }
        }

        public ArmRegister this[ TArmRegisterBank aBank, TArmRegisterType aType ]
        {
            get
            {
                ArmRegisterCollection bank = this[ aBank ];
                return bank[ aType ];
            }
        }
        #endregion

        #region Internal methods
        private ArmRegisterCollection AddBank( TArmRegisterBank aBank )
        {
            return AddBank( aBank, null );
        }

        private ArmRegisterCollection AddBank( TArmRegisterBank aBank, ArmRegisterCollection aLinkedWith )
        {
            TArmRegisterType[] empty = new TArmRegisterType[] { };
            return AddBank( aBank, aLinkedWith, empty );
        }

        private ArmRegisterCollection AddBank( TArmRegisterBank aBank, ArmRegisterCollection aLinkedWith, params TArmRegisterType[] aExtraRegs )
        {
            ArmRegisterCollection regSet = CreateCollection( aBank, aLinkedWith );
            iBanks.Add( aBank, regSet );

            // Create bank specific registers
            string bankName = ArmRegisterBankUtils.BankAsString( aBank );
            regSet.Add( "R13_" + bankName, 0 );
            regSet.Add( "R14_" + bankName, 0 );
            regSet.Add( "SPSR_" + bankName, 0 );

            // Create custom registers
            foreach( TArmRegisterType custom in aExtraRegs )
            {
                string name = ArmRegister.GetTypeName( custom ) + "_" + bankName;
                regSet.Add( name, 0 );
            }

            return regSet;
        }

        private ArmRegisterCollection AddBank( ArmRegisterCollection aRegSet )
        {
            iBanks.Add( aRegSet.Bank, aRegSet );
            return aRegSet;
        }

        private ArmRegisterCollection CreateCollection( TArmRegisterBank aBank )
        {
            return CreateCollection( aBank, null );
        }

        private ArmRegisterCollection CreateCollection( TArmRegisterBank aBank, ArmRegisterCollection aLinkedWith )
        {
            ArmRegisterCollection ret = null;
            //
            if ( iBackingStore != null )
            {
                ret = iBackingStore.ARMBSCreate( aBank, aLinkedWith );
            }
            else
            {
                ret = new ArmRegisterCollection( aBank, aLinkedWith );
            }
            //
            return ret;
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

        #region From IEnumerable<ArmRegister>
        public IEnumerator<ArmRegister> GetEnumerator()
        {
            SortedList<string, ArmRegister> entries = new SortedList<string, ArmRegister>();

            // Get specific entries - we always take all of these
            foreach ( KeyValuePair<TArmRegisterBank, ArmRegisterCollection> kvp in iBanks )
            {
                ArmRegisterCollection regs = kvp.Value;
                foreach ( ArmRegister reg in regs )
                {
                    string key = reg.Value.ToString( "x8" ) + "_" + reg.OriginalName;
                    if ( !entries.ContainsKey( key ) )
                    {
                        entries.Add( key, reg );
                    }
                }
            }

            // Now we can iterate...
            foreach ( ArmRegister entry in entries.Values )
            {
                yield return entry;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            SortedList<string, ArmRegister> entries = new SortedList<string, ArmRegister>();

            // Get specific entries - we always take all of these
            foreach ( KeyValuePair<TArmRegisterBank, ArmRegisterCollection> kvp in iBanks )
            {
                ArmRegisterCollection regs = kvp.Value;
                foreach ( ArmRegister reg in regs )
                {
                    string key = reg.Value.ToString( "x8" ) + "_" + reg.OriginalName;
                    if ( !entries.ContainsKey( key ) )
                    {
                        entries.Add( key, reg );
                    }
                }
            }

            // Now we can iterate...
            foreach ( ArmRegister entry in entries.Values )
            {
                yield return entry;
            }
        }
        #endregion

        #region Data members
        private IARMBackingStore iBackingStore = null;
        private Dictionary<TArmRegisterBank, ArmRegisterCollection> iBanks = new Dictionary<TArmRegisterBank, ArmRegisterCollection>();
        #endregion
    }

    #region Machine backing store
    public interface IARMBackingStore
    {
        ArmRegisterCollection ARMBSCreate( TArmRegisterBank aBank, ArmRegisterCollection aLinkedWith );
    }
    #endregion
}
