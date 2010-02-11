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
using SymbianUtils.Range;
using SymbianStructuresLib.Uids;
using SymbianStructuresLib.Arm.Registers;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Base.DataBinding;
using CrashItemLib.Crash.Stacks;
using CrashItemLib.Crash.ExitInfo;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Summarisable;

namespace CrashItemLib.Crash.Threads
{
    #region Attributes
    [CIDBAttributeColumn( "Name", 0 )]
    [CIDBAttributeColumn( "Value", 1, true )]
    #endregion
    public class CIThread : CIElement
	{
		#region Constructors
        public CIThread( CIProcess aProcess )
            : base( aProcess.Container, aProcess )
		{
            AddChild( new CIExitInfo( this ) );
            AddChild( new CIThreadRegisterListCollection( this ) );
		}
		#endregion

        #region API
        public CIStack CreateStack( CIRegisterList aRegisters, byte[] aData, uint aAddressOfFirstByte, AddressRange aRange )
        {
            // The registers given must be a child of this thread (in some way)
            System.Diagnostics.Debug.Assert( aRegisters.OwningThread == this );

            // Add it to the overall item too
            CIStack stack = CIStack.NewThreadStack( this, aRegisters, aData, aAddressOfFirstByte, aRange );
            AddChild( stack );

            return stack;
        }

        internal bool Contains( CIStack aStack )
        {
            return iStacks.ContainsKey( aStack.Type );
        }
        #endregion

        #region Properties
        [CIDBAttributeCell( "Id", 2 )]
        public override CIElementId Id
        {
            get { return base.Id; }
            set { base.Id = value; }
        }

        [CIDBAttributeCell( "Priority", 3, 0 )]
        public int Priority
        {
            get { return iPriority; }
            set { iPriority = value; }
        }

        [CIDBAttributeCell("Full Name", 1 )]
        public string FullName
        {
            get
            {
                StringBuilder ret = new StringBuilder();
                //
                if ( OwningProcess.Name.Length > 0 )
                {
                    ret.AppendFormat( "{0}[{1:x8}]{2:d4}::{3}",
                        OwningProcess.Name,
                        OwningProcess.SID,
                        OwningProcess.Generation,
                        Name
                        );
                }
                else
                {
                    ret.Append( Name );
                }
                //
                return ret.ToString();
            }
        }

        public override string Name
        {
            get
            {
                return iName;
            }
            set
            {
                iName = value;
            }
        }

        public CIProcess OwningProcess
        {
            get
            {
                CIProcess ret = (CIProcess) base.Parent;
                return ret;
            }
        }

        [CIDBAttributeCell( "Exit Info", 4 )]
        public CIExitInfo ExitInfo
        {
            get
            {
                // We ensure there is only ever one, so this is okay
                return (CIExitInfo) base.ChildByType( typeof( CIExitInfo ) );
            }
        }

        public CIStack[] Stacks
        {
            get
            {
                CIElementList<CIStack> stacks = base.ChildrenByType<CIStack>();
                return stacks.ToArray();
            }
        }

        public CISummarisableEntity Summary
        {
            get
            {
                CISummarisableEntityList list = Container.Summaries;
                CISummarisableEntity ret = list[ this ];
                return ret;
            }
        }

        public bool IsAbnormalTermination
        {
            get
            {
                bool ret = ExitInfo.IsAbnormalTermination;
                return ret;
            }
        }

        public CIRegisterList CurrentProcessorModeRegisters
        {
            get { return Registers.CurrentProcessorModeRegisters; }
        }

        public CIThreadRegisterListCollection Registers
        {
            get
            {
                // We ensure there is only ever one, so this is okay
                return (CIThreadRegisterListCollection) base.ChildByType( typeof( CIThreadRegisterListCollection ) );
            }
        }
        #endregion

        #region From CIElement
        public override void AddChild( CIElement aChild )
        {
            // We support 4 types of children at the moment.
            // Registers, stacks and exit info, messages.
            if ( aChild.GetType() == typeof( CIExitInfo ) )
            {
                if ( base.ChildrenByType<CIExitInfo>().Count != 0 )
                {
                    throw new ArgumentException( "Exit Info already associated with the thread" );
                }
            }
            else if ( aChild.GetType() == typeof( CIThreadRegisterListCollection ) )
            {
                if ( base.ChildrenByType<CIThreadRegisterListCollection>().Count != 0 )
                {
                    throw new ArgumentException( "Registers already associated with the thread" );
                }
            }
            else if ( aChild.GetType() == typeof( CIStack ) )
            {
                CIStack stack = (CIStack) aChild;

                // We must ensure we don't already have a stack of the specified mode
                // associated with the thread.
                bool exists = iStacks.ContainsKey( stack.Type );
                if ( exists )
                {
                    throw new ArgumentException( ArmRegisterBankUtils.BankAsStringLong( stack.Type ) + " mode stack already registered with thread" );
                }
                else
                {
                    iStacks.Add( stack.Type, stack );
                }
            }
            //
            base.AddChild( aChild );
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        private const string KUnknownThreadName = "UnknownThread";
        #endregion

        #region Data members
        private int iPriority = 0;
        private string iName = KUnknownThreadName;
        private Dictionary<TArmRegisterBank, CIStack> iStacks = new Dictionary<TArmRegisterBank, CIStack>();
        #endregion
    }
}
