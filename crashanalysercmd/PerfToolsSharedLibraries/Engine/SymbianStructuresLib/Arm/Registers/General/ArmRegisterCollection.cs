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
    public class ArmRegisterCollection : IEnumerable<ArmRegister>, IComparer<ArmRegister>
    {
        #region Constructors
        public ArmRegisterCollection()
            : this( TArmRegisterBank.ETypeUnknown )
        {
        }

        public ArmRegisterCollection( TArmRegisterBank aBank )
            : this( aBank, null )
        {
        }

        public ArmRegisterCollection( TArmRegisterBank aBank, ArmRegisterCollection aLinkedWith )
        {
            iBank = aBank;
            iLinkedWith = aLinkedWith;
        }

        public ArmRegisterCollection( ArmRegisterCollection aCopy )
            : this( aCopy.Bank, aCopy.iLinkedWith )
        {
            foreach ( ArmRegister reg in aCopy )
            {
                ArmRegister copy = new ArmRegister( reg );
                DoAdd( copy );
            }
        }
        #endregion

        #region API
        public ArmRegister Add( ArmRegister aRegister )
        {
            ArmRegister entry = null;
            //
            if ( iBackingStore != null )
            {
                string name = aRegister.OriginalName;
                entry = iBackingStore.ARCBSCreate( aRegister.RegType, name, aRegister.Value );
                entry = DoAdd( entry );
            }
            else
            {
                entry = DoAdd( aRegister );
            }
            //
            return entry;
        }

        public ArmRegister Add( TArmRegisterType aType, uint aValue )
        {
            ArmRegister entry = null;
            if ( iBackingStore != null )
            {
                string name = ArmRegister.GetTypeName( aType );
                entry = iBackingStore.ARCBSCreate( aType, name, aValue );
            }
            else
            {
                entry = new ArmRegister( aType, aValue );
            }
            //
            entry = DoAdd( entry );
            return entry;
        }

        public ArmRegister Add( string aName, uint aValue )
        {
            TArmRegisterType type = ArmRegister.GetTypeByName( aName );

            ArmRegister entry = null;
            if ( iBackingStore != null )
            {
                entry = iBackingStore.ARCBSCreate( type, aName, aValue );
            }
            else
            {
                entry = new ArmRegister( aName, aValue );
            }
            //
            entry = DoAdd( entry );
            return entry;
        }

        public void Copy( ArmRegisterCollection aCollection )
        {
            foreach ( ArmRegister reg in aCollection )
            {
                Add( reg.OriginalName, reg.Value );
            }
        }

        public virtual void AddDefaults()
        {
            Clear();
            //
            Array items = Enum.GetValues( typeof( TArmRegisterType ) );
            foreach ( object enumEntry in items )
            {
                TArmRegisterType value = (TArmRegisterType) enumEntry;
                if ( value >= TArmRegisterType.EArmReg_00 && value <= TArmRegisterType.EArmReg_EXCPC )
                {
                    Add( value, 0 );
                }
            }
        }

        public void AddMany( params TArmRegisterType[] aTypes )
        {
            foreach ( TArmRegisterType reg in aTypes )
            {
                string name = ArmRegister.GetTypeName( reg );
                Add( name, 0 );
            }
        }

        public void SetAll( uint aValue )
        {
            foreach ( ArmRegister reg in this )
            {
                reg.Value = aValue;
            }
        }

        public bool Contains( TArmRegisterType aType )
        {
            if ( aType == TArmRegisterType.EArmReg_Other )
            {
                throw new ArgumentException( "Type must be unique" );
            }
            //
            bool ret = false;
            //
            foreach ( ArmRegister reg in this )
            {
                if ( reg.RegType == aType )
                {
                    ret = true;
                    break;
                }
            }
            //
            return ret;
        }

        public bool Contains( string aName )
        {
            // Need this to map the specified name into a common name.
            // I.e. convert R14_USR to R14
            ArmRegister temp = new ArmRegister( aName, 0 );

            // First try concrete entries
            bool ret = iEntries.ContainsKey( temp.Name );
            if ( !ret && iLinkedWith != null )
            {
                // Try linked
                ret = iLinkedWith.Contains( temp.Name );
            }

            return ret;
        }

        public void Clear()
        {
            if ( iBackingStore != null )
            {
                iBackingStore.ARCBSClear();
            }

            iEntries.Clear();
        }

        public void Remove( TArmRegisterType aType )
        {
            ArmRegister temp = new ArmRegister( aType, 0 );
            bool contains = iEntries.ContainsKey( temp.Name );
            if ( contains )
            {
                temp = iEntries[ temp.Name ];
                iEntries.Remove( temp.Name );
                if ( iBackingStore != null )
                {
                    iBackingStore.ARCBSRemove( temp );
                }
            }
        }
        #endregion

        #region Properties
        public TArmRegisterBank Bank
        {
            get { return iBank; }
        }

        public ArmRegister this[ ArmRegister aRegister ]
        {
            get
            {
                ArmRegister ret = this[ aRegister.Name ];
                return ret;
            }
        }

        public ArmRegister this[ TArmRegisterType aType ]
        {
            get
            {
                // We can only find entries which have a well known name
                if ( aType == TArmRegisterType.EArmReg_Other )
                {
                    throw new ArgumentException( "Type must be unique" );
                }

                // Check for existing entry
                foreach ( ArmRegister entry in this )
                {
                    if ( entry.RegType == aType )
                    {
                        return entry;
                    }
                }
                
                // The specified entry did not exist so silently
                // add it and return a reference to the new entry
                ArmRegister ret = Add( aType, 0 );
                return ret;
            }
        }

        public ArmRegister this[ string aName ]
        {
            get
            {
                ArmRegister ret = new ArmRegister( aName, 0 );

                // First try concrete entries in this object
                if ( iEntries.ContainsKey( ret.Name ) )
                {
                    ret = iEntries[ ret.Name ];
                }
                else if ( iLinkedWith != null && iLinkedWith.Contains( ret.Name ) )
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

        public bool IsEmpty
        {
            get
            {
                bool empty = true;
                //
                foreach ( ArmRegister reg in this )
                {
                    if ( reg.Value != 0 )
                    {
                        empty = false;
                        break;
                    }
                }
                //
                return empty;
            }
        }

        public object Tag
        {
            get { return iTag; }
            set { iTag = value; }
        }

        public IARCBackingStore BackingStore
        {
            get { return iBackingStore; }
            set { iBackingStore = value; }
        }
        #endregion

        #region Internal methods
        private void MakeDefaultEntries()
        {
            Add( TArmRegisterType.EArmReg_00, 0 );
            Add( TArmRegisterType.EArmReg_01, 0 );
            Add( TArmRegisterType.EArmReg_02, 0 );
            Add( TArmRegisterType.EArmReg_03, 0 );
            Add( TArmRegisterType.EArmReg_04, 0 );
            Add( TArmRegisterType.EArmReg_05, 0 );
            Add( TArmRegisterType.EArmReg_06, 0 );
            Add( TArmRegisterType.EArmReg_07, 0 );
            Add( TArmRegisterType.EArmReg_08, 0 );
            Add( TArmRegisterType.EArmReg_09, 0 );
            Add( TArmRegisterType.EArmReg_10, 0 );
            Add( TArmRegisterType.EArmReg_11, 0 );
            Add( TArmRegisterType.EArmReg_12, 0 );
            Add( TArmRegisterType.EArmReg_SP, 0 );
            Add( TArmRegisterType.EArmReg_LR, 0 );
            Add( TArmRegisterType.EArmReg_PC, 0 );
            Add( TArmRegisterType.EArmReg_CPSR, 0 );
        }

        private ArmRegister DoAdd( ArmRegister aRegister )
        {
            ArmRegister ret = aRegister;
            //
            bool exists = iEntries.ContainsKey( aRegister.Name );
            if ( exists )
            {
                ret = iEntries[ aRegister.Name ];
                ret.Value = aRegister.Value;
            }
            else
            {
                // Associate entry with this collection
                aRegister.Parent = this;
                iEntries.Add( aRegister.Name, aRegister );
            }
            //
            return ret;
        }
        #endregion

        #region From IComparer<ArmRegister>
        public int Compare( ArmRegister aLeft, ArmRegister aRight )
        {
            int ret = -1;
            //
            if ( aLeft == null || aRight == null )
            {
                if ( aRight == null )
                {
                    ret = 1;
                }
            }
            else
            {
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
            }
            //
            return ret;
        }
        #endregion

        #region From IEnumerable<ArmRegister>
        public IEnumerator<ArmRegister> GetEnumerator()
        {
            SortedList<string, ArmRegister> entries = new SortedList<string, ArmRegister>();

            // Get specific entries - we always take all of these
            foreach ( KeyValuePair<string, ArmRegister> kvp in iEntries )
            {
                ArmRegister reg = kvp.Value;
                entries.Add( reg.Name, reg );
            }

            // And also common entries
            if ( iLinkedWith != null )
            {
                foreach ( ArmRegister reg in iLinkedWith )
                {
                    // Make sure that the concrete entries override
                    // any common values
                    if ( entries.ContainsKey( reg.Name ) == false )
                    {
                        entries.Add( reg.Name, reg );
                    }
                }
            }

            // Ensure list is really sorted.
            List<ArmRegister> ret = new List<ArmRegister>();
            foreach ( KeyValuePair<string, ArmRegister> pair in entries )
            {
                ret.Add( pair.Value );
            }
            ret.Sort( this );

            // Now we can iterate...
            foreach ( ArmRegister entry in ret )
            {
                yield return entry;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            SortedList<string, ArmRegister> entries = new SortedList<string, ArmRegister>();

            // Get specific entries - we always take all of these
            foreach ( KeyValuePair<string, ArmRegister> kvp in iEntries )
            {
                ArmRegister reg = kvp.Value;
                entries.Add( reg.Name, reg );
            }

            // And also common entries
            if ( iLinkedWith != null )
            {
                foreach ( ArmRegister reg in iLinkedWith )
                {
                    // Make sure that the concrete entries override
                    // any common values
                    if ( entries.ContainsKey( reg.Name ) == false )
                    {
                        entries.Add( reg.Name, reg );
                    }
                }
            }

            // Ensure list is really sorted.
            List<ArmRegister> ret = new List<ArmRegister>();
            foreach ( KeyValuePair<string, ArmRegister> pair in entries )
            {
                ret.Add( pair.Value );
            }
            ret.Sort( this );

            // Now we can iterate...
            foreach ( ArmRegister entry in ret )
            {
                yield return entry;
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            foreach ( ArmRegister entry in this )
            {
                ret.AppendLine( entry.ToString() );
            }
            return ret.ToString();
        }
        #endregion

        #region Data members
        private object iTag = null;
        private readonly TArmRegisterBank iBank;
        private readonly ArmRegisterCollection iLinkedWith;
        private string iName = string.Empty;
        private IARCBackingStore iBackingStore = null;
        private Dictionary<string, ArmRegister> iEntries = new Dictionary<string, ArmRegister>();
        #endregion
    }

    #region Backing store interface
    public interface IARCBackingStore
    {
        void ARCBSClear();

        ArmRegister ARCBSCreate( TArmRegisterType aType, string aName, uint aValue );
        void ARCBSRemove( ArmRegister aRegister );
    }
    #endregion
}
