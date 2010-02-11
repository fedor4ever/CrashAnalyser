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
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Base.DataBinding;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Threads;
using CrashItemLib.Crash.Processes;
using SymbianStructuresLib.Arm.Registers;

namespace CrashItemLib.Crash.Registers
{
    #region Attributes
    [CIDBAttributeColumn( "Name", 0 )]
    [CIDBAttributeColumn( "Value", 1 )]
    [CIDBAttributeColumn( "Symbol", 2, true )]
    #endregion
    public class CIRegisterList : CIElement, IARCBackingStore, IEnumerable<CIRegister>
	{
		#region Constructors
        public CIRegisterList( CIContainer aContainer, TArmRegisterBank aBank )
            : base( aContainer )
		{
            iCollection = new ArmRegisterCollection( aBank );
            iCollection.Tag = this;
            iCollection.BackingStore = this;

            // Restrict children
            base.AddSupportedChildType( typeof( CIRegister ) );
            base.AddSupportedChildType( typeof( CrashItemLib.Crash.Messages.CIMessage ) );
        }

        /// <summary>
        /// Internal constructor called by CIRegisterListForThread which sets a thread up as a parent
        /// of the register list.
        /// </summary>
        internal CIRegisterList( CIContainer aContainer, CIElement aParent, TArmRegisterBank aBank )
            : base( aContainer, aParent )
        {
            iCollection = new ArmRegisterCollection( aBank );
            iCollection.Tag = this;
            iCollection.BackingStore = this;

            // Restrict children
            base.AddSupportedChildType( typeof( CIRegister ) );
            base.AddSupportedChildType( typeof( CrashItemLib.Crash.Messages.CIMessage ) );
        }
        #endregion

        #region API
        public CIRegister Add( TArmRegisterType aType, uint aValue )
        {
            // Will cause a call back to create the entries in iRegisters...
            ArmRegister entry = iCollection.Add( aType, aValue );
            //
            System.Diagnostics.Debug.Assert( entry.Tag != null && entry.Tag is CIRegister );
            System.Diagnostics.Debug.Assert( iCollection.Count == base.Count );
            //
            CIRegister ret = (CIRegister) entry.Tag;
            return ret;
        }

        public void Add( ArmRegisterCollection aArmRegisterCollection )
        {
            // Will cause a call back to create the entries in iRegisters...
            iCollection.Copy( aArmRegisterCollection );
            System.Diagnostics.Debug.Assert( iCollection.Count == base.Count );
        }

        public bool Contains( TArmRegisterType aType )
        {
            System.Diagnostics.Debug.Assert( iCollection.Count == base.Count );
            return iCollection.Contains( aType );
        }

        public void Remove( TArmRegisterType aType )
        {
            iCollection.Remove( aType );
        }

        public override void Clear()
        {
            // This calls IARCBackingStore.ARCBSClear(), which in turn calls base.Clear().
            iCollection.Clear();
        }
        #endregion

        #region Properties
        public override int Count
        {
            get
            {
                int ret = base.Count;
                System.Diagnostics.Debug.Assert( iCollection.Count == ret );
                return iCollection.Count; 
            }
        }

        public bool IsCurrentProcessorMode
        {
            get
            {
                bool ret = false;
                //
                if ( Contains( TArmRegisterType.EArmReg_CPSR ) )
                {
                    CIRegister cpsr = this[ TArmRegisterType.EArmReg_CPSR ];
                    TArmRegisterBank currentBank = ArmRegisterBankUtils.ExtractBank( cpsr.Value );
                    //
                    ret = ( currentBank == Bank );
                }
                //
                return ret;
            }
        }

        public override string Name
        {
            get { return BankName; }
        }

        public TArmRegisterBank Bank
        {
            get { return iCollection.Bank; }
        }

        public string BankName
        {
            get { return ArmRegisterBankUtils.BankAsStringLong( Bank ); }
        }

        public string BankAbbreviation
        {
            get { return ArmRegisterBankUtils.BankAsString( Bank ); }
        }

        public CIThread OwningThread
        {
            get
            {
                CIThread ret = base.Parent as CIThread;
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

        public CIRegister[] Registers
        {
            get
            {
                CIElementList<CIRegister> registers = base.ChildrenByType<CIRegister>();
                return registers.ToArray();
            }
        }

        public CIRegister this[ TArmRegisterType aType ]
        {
            get
            {
                System.Diagnostics.Debug.Assert( iCollection.Count == base.Count );
                ArmRegister reg = iCollection[ aType ];
                System.Diagnostics.Debug.Assert( reg.Tag != null && reg.Tag is CIRegister );
                CIRegister ret = (CIRegister) reg.Tag;
                return ret;
            }
        }

        public CIRegister this[ string aName ]
        {
            get
            {
                System.Diagnostics.Debug.Assert( iCollection.Count == base.Count );
                ArmRegister reg = iCollection[ aName ];
                CIRegister ret = (CIRegister) reg.Tag;
                return ret;
            }
        }
        #endregion

        #region Operators
        public static implicit operator ArmRegisterCollection( CIRegisterList aSelf )
        {
            return aSelf.Collection;
        }
        #endregion

        #region Internal methods
        internal ArmRegisterCollection Collection
        {
            get { return iCollection; }
        }
        #endregion

        #region IARCBackingStore Members
        void IARCBackingStore.ARCBSClear()
        {
            base.Clear();
        }

        ArmRegister IARCBackingStore.ARCBSCreate( TArmRegisterType aType, string aName, uint aValue )
        {
            // Go via factory to deal with special registers...
            CIRegister reg = Factory.CIRegisterFactory.New( aType, aValue, aName, this );
            base.AddChild( reg );

            // Set up two-way association
            ArmRegister ret = reg.Register;
            ret.Tag = reg;
            return ret;
        }

        void IARCBackingStore.ARCBSRemove( ArmRegister aRegister )
        {
            CIRegister reg = aRegister.Tag as CIRegister;
            if ( reg != null )
            {
                base.RemoveChild( reg );
            }
        }
        #endregion

        #region From IEnumerable<CIRegister>
        public new IEnumerator<CIRegister> GetEnumerator()
        {
            foreach ( CIElement element in base.Children )
            {
                if ( element is CIRegister )
                {
                    CIRegister reg = (CIRegister) element;
                    yield return reg;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( CIElement element in base.Children )
            {
                if ( element is CIRegister )
                {
                    CIRegister reg = (CIRegister) element;
                    yield return reg;
                }
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return ArmRegisterBankUtils.BankAsString( Bank );
        }
        #endregion

        #region From CIElement
        public override void PrepareRows()
        {
            DataBindingModel.ClearRows();
           
            // Our data binding model is based upon the register object, rather
            // than any key-value-pair properties.
            foreach ( CIRegister reg in this )
            {
                DataBindingModel.Add( reg );
            }
        }
        #endregion

        #region Data members
        private readonly ArmRegisterCollection iCollection;
        #endregion
    }
}
