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
using System.Text.RegularExpressions;
using System.Collections.Generic;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Stacks;
using CrashItemLib.Crash.ExitInfo;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.Registers;
using SymbianStructuresLib.Uids;
using SymbianStructuresLib.Arm.Registers;

namespace CrashItemLib.Crash.Registers
{
	public class CIRegisterListCollection : CIElement, IEnumerable<CIRegisterList>
	{
		#region Constructors
        [CIElementAttributeMandatory()]
        public CIRegisterListCollection( CIContainer aContainer )
            : this( aContainer, null )
		{
        }
        
        internal CIRegisterListCollection( CIContainer aContainer, CIElement aParent )
            : base( aContainer, aParent )
        {
        }
        #endregion

        #region API
        public void Add( TArmRegisterBank aType )
        {
            this[ aType ] = new CIRegisterList( Container, aType );
        }

        public void Add( CIRegisterList aCollection )
        {
            // Make sure the dictionary doesn't already contain this bank type!
            bool exists = Contains( aCollection.Bank );
            if ( exists )
            {
                throw new Exception( aCollection.BankName + " already exists within collection" );
            }
            else
            {
                iRegisters.Add( aCollection.Bank, aCollection );

                // Register as child also
                base.AddChild( aCollection );
            }
        }

        public void Add( params TArmRegisterBank[] aTypes )
        {
            foreach ( TArmRegisterBank type in aTypes )
            {
                Add( type );
            }
        }

        public bool Contains( TArmRegisterBank aType )
        {
            return iRegisters.ContainsKey( aType );
        }

        public override void Clear()
        {
            iRegisters.Clear();
            base.Clear();
        }
        #endregion

        #region Properties
        public virtual uint CPSR
        {
            get
            {
                uint ret = 0;
                //
                CIRegisterList list = CurrentProcessorModeRegisters;
                if ( list != null )
                {
                    ret = list[ TArmRegisterType.EArmReg_CPSR ].Value;
                }
                else
                {
                    // CPSR is not yet defined
                }
                //
                return ret;
            }
            set
            {
                TArmRegisterBank bank = ArmRegisterBankUtils.ExtractBank( value );
                
                // Remove CPSR from current register list
                CIRegisterList list = CurrentProcessorModeRegisters;
                if ( list != null )
                {
                    list.Remove( TArmRegisterType.EArmReg_CPSR );
                }

                // Now we can add it without fear of creating a duplicate
                CIRegisterList cpsrRegList = this[ bank ];
                if ( cpsrRegList != null )
                {
                    cpsrRegList.Add( TArmRegisterType.EArmReg_CPSR, value );
                }
            }
        }

        public virtual CIRegisterList CurrentProcessorModeRegisters
        {
            get
            {
                // Try to find the bank that contains CPSR (if any) and
                // return that.
                CIRegisterList ret = null;
                //
                foreach ( CIRegisterList col in this )
                {
                    if ( col.Contains( TArmRegisterType.EArmReg_CPSR ) )
                    {
                        // Get CPSR value
                        ArmRegister cpsr = col[ TArmRegisterType.EArmReg_CPSR ];
                        TArmRegisterBank cpsrBank = ArmRegisterBankUtils.ExtractBank( cpsr );

                        // Try to get the corresponding bank (should be the same as col...)
                        if ( Contains( cpsrBank ) )
                        {
                            CIRegisterList list = this[ cpsrBank ];
                            System.Diagnostics.Debug.Assert( list is CIRegisterList );
                            ret = (CIRegisterList) list;
                        }
                        break;
                    }
                }
                //
                return ret;
            }
        }

        public CIRegisterList this[ TArmRegisterBank aType ]
        {
            get { return iRegisters[ aType ]; }
            set
            {
                if ( iRegisters.ContainsKey( aType ) )
                {
                    iRegisters[ aType ] = value;
                }
                else
                {
                    Add( value );
                }
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region From IEnumerable<CIRegisterCollection>
        public new IEnumerator<CIRegisterList> GetEnumerator()
        {
            foreach ( KeyValuePair<TArmRegisterBank, CIRegisterList> kvp in iRegisters )
            {
                yield return kvp.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( KeyValuePair<TArmRegisterBank, CIRegisterList> kvp in iRegisters )
            {
                yield return kvp.Value;
            }
        }
        #endregion

        #region Data members
        private Dictionary<TArmRegisterBank, CIRegisterList> iRegisters = new Dictionary<TArmRegisterBank, CIRegisterList>();
        #endregion
    }
}
