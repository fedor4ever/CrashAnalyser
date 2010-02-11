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
using SymbianStructuresLib.Arm.Registers;
using SymbianStructuresLib.Debug.Symbols;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI.Types.Symbol;
using CrashDebuggerLib.Structures.KernelObjects;
using CrashDebuggerLib.Structures.Common;
using CrashDebuggerLib.Structures.Process;

namespace CrashDebuggerLib.Structures.Register
{
    public class RegisterCollection : CrashDebuggerAware, IEnumerable<RegisterEntry>, IARCBackingStore, IComparer<RegisterEntry>
    {
        #region Enumerations
        public enum TType
        {
            // Special cases
            ETypeGeneral        = -1,       // CPU specific
            ETypeCommonBank     = 0,        // R00 -> R07 inclusive

            // Linked into to CPSR values
            ETypeUser           = 0x10,     // 0b10000
            ETypeFastInterrupt  = 0x11,     // 0b10001
            ETypeInterrupt      = 0x12,     // 0b10010
            ETypeSupervisor     = 0x13,     // 0b10011
            ETypeAbort          = 0x17,     // 0b10111
            ETypeUndefined      = 0x1B,     // 0b11011

            // Not used in Symbian OS
            ETypeSystem         = 0x1F      // 0b11111
        }
        #endregion

        #region Constructors
        internal RegisterCollection( CrashDebuggerInfo aCrashDebugger, TType aType )
            : this( aCrashDebugger, aType, null, null )
        {
        }

        internal RegisterCollection( CrashDebuggerInfo aCrashDebugger, TType aType, DProcess aProcess )
            : this( aCrashDebugger, aType, aProcess, null )
        {
        }

        internal RegisterCollection( RegisterCollection aCopy, TType aType, DProcess aProcess )
            : this( aCopy.CrashDebugger, aType, aProcess, null )
        {
            foreach ( RegisterEntry entry in aCopy )
            {
                Add( entry.OriginalName, entry.Value );
            }
        }

        internal RegisterCollection( CrashDebuggerInfo aCrashDebugger, TType aType, DProcess aProcess, RegisterCollection aLinkedWith )
            : base( aCrashDebugger )
        {
            iType = aType;
            iProcess = aProcess;

            iEntries = new ArmRegisterCollection();
            iEntries.BackingStore = this as IARCBackingStore;

            iLinkedWith = aLinkedWith;
        }
        #endregion

        #region API
        public RegisterEntry Add( string aName, uint aValue )
        {
            ArmRegister added = iEntries.Add( aName, aValue );
            RegisterEntry ret = added as RegisterEntry;
            return ret;
        }

        public void AddMany( params TArmRegisterType[] aTypes )
        {
            foreach ( TArmRegisterType reg in aTypes )
            {
                string name = ArmRegister.GetTypeName( reg );
                Add( name, 0 );
            }
        }

        public bool Exists( string aName )
        {
            // Need this to map the specified name into a common name.
            // I.e. convert R14_USR to R14
            RegisterEntry temp = new RegisterEntry( this, aName, 0 );

            // First try concrete entries
            bool ret = iEntries.Contains( temp.Name );
            if ( !ret && iLinkedWith != null )
            {
                // Try linked
                ret = iLinkedWith.Exists( temp.Name );
            }

            return ret;
        }

        public void Clear()
        {
            iEntries.Clear();
        }

        public static string BankName( TType aType )
        {
            string ret = string.Empty;
            //
            switch ( aType )
            {
            default:
            case TType.ETypeGeneral:
            case TType.ETypeCommonBank:
                ret = string.Empty;
                break;
            case TType.ETypeAbort:
                ret = "ABT";
                break;
            case TType.ETypeFastInterrupt:
                ret = "FIQ";
                break;
            case TType.ETypeInterrupt:
                ret = "IRQ";
                break;
            case TType.ETypeSupervisor:
                ret = "SVC";
                break;
            case TType.ETypeSystem:
                ret = "SYS";
                break;
            case TType.ETypeUndefined:
                ret = "UND";
                break;
            case TType.ETypeUser:
                ret = "USR";
                break;
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return iName; }
            set { iName = value; }
        }

        public RegisterEntry this[ string aName ]
        {
            get
            {
                RegisterEntry ret = new RegisterEntry( this, aName, 0 );
                
                // First try concrete entries in this object
                if ( iEntries.Contains( ret.Name ) )
                {
                    ret = (RegisterEntry) iEntries[ ret.Name ];
                }
                else if ( iLinkedWith != null && iLinkedWith.Exists( ret.Name ) )
                {
                    // Try linked entries
                    ret = iLinkedWith[ ret.Name ];
                }
                else
                {
                    // Not found
                    Add( aName, 0 );
                }
                //
                return ret;
            }
        }

        public RegisterEntry this[ TArmRegisterType aType ]
        {
            get
            {
                RegisterEntry ret = new RegisterEntry( this, aType );
                return this[ ret.Name ];
            }
        }

        public int Count
        {
            get
            {
                int ret = iEntries.Count;
                //
                if ( iLinkedWith != null )
                {
                    ret += iLinkedWith.Count;
                }
                //
                return ret;
            }
        }
        #endregion

        #region Operators
        public static implicit operator ArmRegisterCollection( RegisterCollection aSelf )
        {
            return aSelf.iEntries;
        }
        #endregion

        #region Internal methods
        public void Dump()
        {
            System.Diagnostics.Debug.WriteLine( "REGISTERS [" + iType + "]" );
            System.Diagnostics.Debug.WriteLine( string.Empty );
            //
            string regs = this.ToString();
            System.Diagnostics.Debug.WriteLine( regs );
        }

        internal Symbol LookUpSymbol( uint aAddress )
        {
            Symbol ret = null;
            //
            if ( iProcess != null )
            {
                ret = iProcess.LookUpSymbol( aAddress );
            }
            else
            {
                ret = base.CrashDebugger.LookUpSymbol( aAddress );
            }
            //
            return ret;
        }
        #endregion

        #region IArmRegisterBackingStore Members
        void IARCBackingStore.ARCBSClear()
        {
            // Nothing to do - our entries are derived from ArmRegister and 
            // are owned by iEntries.
        }

        void IARCBackingStore.ARCBSRemove( ArmRegister aRegister )
        {
            // Nothing to do - our entries are derived from ArmRegister and 
            // are owned by iEntries.
        }

        ArmRegister IARCBackingStore.ARCBSCreate( TArmRegisterType aType, string aName, uint aValue )
        {
            RegisterEntry entry = null;
            //            
            if ( aType == TArmRegisterType.EArmReg_CPSR )
            {
                // CPSR is a bit special...
                entry = new RegisterEntryCPSR( this, aValue );
            }
            else
            {
                entry = new RegisterEntry( this, aName, aValue );
                entry.RegType = aType;
            }
            //
            return entry;
        }
        #endregion

        #region From IComparer<RegisterEntry>
        public int Compare( RegisterEntry aLeft, RegisterEntry aRight )
        {
            int ret = -1;

            // Try to order the registers so that Rnn register names come first
            // then CPSR, then the other stuff.
            TArmRegisterType leftType = aLeft.RegType;
            TArmRegisterType rightType = aRight.RegType;
            //
            if ( leftType != TArmRegisterType.EArmReg_Other && rightType == TArmRegisterType.EArmReg_Other )
            {
                // Left is smaller since it's a standard register
                ret = -1;
            }
            else if ( leftType == TArmRegisterType.EArmReg_Other && rightType != TArmRegisterType.EArmReg_Other )
            {
                // Right is smaller since it's a standard register
                ret = 1;
            }
            else if ( leftType == TArmRegisterType.EArmReg_Other && rightType == TArmRegisterType.EArmReg_Other )
            {
                // Must compare names since both are non-standard registers
                ret = aLeft.OriginalName.CompareTo( aRight.OriginalName );
            }
            else 
            {
                // Registers are not non-standard, compare based upon numerical value
                if ( leftType == rightType )
                {
                    ret = 0;
                }
                else if ( leftType > rightType )
                {
                    ret = 1;
                }
            }
            //
            return ret;
        }
        #endregion

        #region From IEnumerable<RegisterEntry>
        public IEnumerator<RegisterEntry> GetEnumerator()
        {
            SortedList<string, RegisterEntry> entries = new SortedList<string, RegisterEntry>();

            // Get specific entries - we always take all of these
            foreach ( ArmRegister reg in iEntries )
            {
                entries.Add( reg.Name, (RegisterEntry) reg );
            }

            // And also common entries
            if ( iLinkedWith != null )
            {
                foreach( RegisterEntry reg in iLinkedWith )
                {
                    // Make sure that the concrete entries override
                    // any common values
                    if ( entries.ContainsKey( reg.Name ) == false )
                    {
                        entries.Add( reg.Name, reg );
                    }
                }
            }

            // For some reason, sorted list isn't actually sorting the entries
            // by key properly. We must do this ourselves... Ugh :(
            List<RegisterEntry> ret = new List<RegisterEntry>();
            foreach ( KeyValuePair<string, RegisterEntry> pair in entries )
            {
                ret.Add( pair.Value );
            }
            ret.Sort( this );

            // Now we can iterate...
            foreach ( RegisterEntry entry in ret )
            {
                yield return entry;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            IEnumerator<RegisterEntry> self = (IEnumerator<RegisterEntry>) this;
            System.Collections.IEnumerator ret = (System.Collections.IEnumerator) self;
            return ret;
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            foreach ( RegisterEntry entry in this )
            {
                ret.AppendLine( entry.ToString() );
            }
            //
            string text = ret.ToString();
            return text;
        }
        #endregion

        #region Data members
        private readonly TType iType;
        private readonly DProcess iProcess;
        private readonly RegisterCollection iLinkedWith;
        private readonly ArmRegisterCollection iEntries;
        private string iName = string.Empty;
        #endregion
    }
}
