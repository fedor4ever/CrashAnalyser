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
using CrashDebuggerLib.Structures.Common;
using CrashDebuggerLib.Structures.Thread;
using CrashDebuggerLib.Structures.Register;
using CrashDebuggerLib.Structures.UserContextTable;
using CrashDebuggerLib.Attributes;
using SymbianStructuresLib.Debug.Symbols;
using SymbianStructuresLib.Arm.Registers;
using SymbianUtils.DataBuffer;

namespace CrashDebuggerLib.Structures.NThread
{
    public class NThread : CrashDebuggerAware
    {
        #region Enumerations
        [System.ComponentModel.TypeConverter( typeof( SymbianParserLib.TypeConverters.SymbianEnumConverter ) )]
        public enum TNThreadState
        {
            EUnknown = -1, // Catch all
            EReady = 0,
            ESuspended,
            EWaitFastSemaphore,
            ESleep,
            EBlocked,
            EDead,
            EWaitDfc
        }

        public enum TWaitType
        {
            EWaitTypeNotWaiting = 0,
            EWaitTypeUserWaitForRequest,
            EWaitTypeUserWaitForAnyRequest
        }
        #endregion

        #region Constructors
        public NThread( CrashDebuggerInfo aCrashDebugger, DThread aParentThread )
            : base( aCrashDebugger )
        {
            iParentThread = aParentThread;
            iRegisters = new RegisterCollection( aCrashDebugger, RegisterCollection.TType.ETypeGeneral, aParentThread.OwningProcess );
        }
        #endregion

        #region API
        public static TNThreadState NStateFromString( string aState )
        {
            NThread.TNThreadState ret = NThread.TNThreadState.EUnknown;
            //
            switch ( aState.ToUpper() )
            {
                case "READY":
                    ret = NThread.TNThreadState.EReady;
                    break;
                case "SUSPENDED":
                    ret = NThread.TNThreadState.ESuspended;
                    break;
                case "WAITFSEM":
                    ret = NThread.TNThreadState.EWaitFastSemaphore;
                    break;
                case "SLEEP":
                    ret = NThread.TNThreadState.ESleep;
                    break;
                case "BLOCKED":
                    ret = NThread.TNThreadState.EBlocked;
                    break;
                case "DEAD":
                    ret = NThread.TNThreadState.EDead;
                    break;
                case "WAITDFC":
                    ret = NThread.TNThreadState.EWaitDfc;
                    break;
                default:
                case "??":
                    ret = NThread.TNThreadState.EUnknown;
                    break;
            }
            //
            return ret;
        }

        public RegisterCollection GetRegisters( RegisterCollection.TType aType )
        {
            RegisterCollection ret = null;

            // Are we the currently executing thread?
            bool isCurrent = iParentThread.IsCurrent;
            
            // If we're dealing with the current thread, and we need to supply
            // user-land registers, then we'll try to work with the user context
            // tables so long as we are executing in supervisor mode.
            if ( aType == RegisterCollection.TType.ETypeUser )
            {
                ret = GetUserContextRegisters();
            }
            else
            {
                // Trying to get non-user registers
                Cpu.CpuInfo cpuInfo = CrashDebugger.InfoCpu;
                //
                if ( isCurrent )
                {
                    // Just go entirely with current CPU registers
                    ret = cpuInfo.GetRegisters();
                }
                else
                {
                    // Best we can do :(
                    ret = new RegisterCollection( Registers, aType, iParentThread.OwningProcess );

                    if ( aType == RegisterCollection.TType.ETypeSupervisor )
                    {
                        // We know R13_SP because we explicitly are given it
                        ret[ TArmRegisterType.EArmReg_SP ].Value = SavedSP;

                        // TODO: We also need to get CPSR from somewhere. We just make it
                        // up at the moment, which is really bad...
                        ret[ TArmRegisterType.EArmReg_CPSR ].Value = ret[ TArmRegisterType.EArmReg_SPSR ];
                    }
                }
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        [PropCat("State")]
        public TNThreadState NState
        {
            get { return iNState; }
            set { iNState = value; }
        }

        [PropCat( "Summary", PropCat.TFormatType.EFormatAsHex )]
        public uint Address
        {
            get { return iAddress; }
            set { iAddress = value; }
        }

        public uint RequestSemaphoreAddress
        {
            get
            {
                uint ret = Address;
                ret += (uint) CrashDebuggerLib.Platform.NKernOffsets.KOffsetOf_iRequestSemaphore_In_NThread;
                return ret;
            }
        }

        public uint WaitObj
        {
            get { return iWaitObj; }
            set { iWaitObj = value; }
        }

        [PropCat( "Summary" )]
        public int Priority
        {
            get { return iPriority; }
            set { iPriority = value; }
        }

        [PropCat( "State", PropCat.TFormatType.EFormatAsHex )]
        public uint Attributes
        {
            get { return iAttributes; }
            set { iAttributes = value; }
        }

        [PropCat( "Summary", "Address space" )]
        public uint AddressSpace
        {
            get { return iAddressSpace; }
            set { iAddressSpace = value; }
        }

        [PropCat( "Summary", "Supervisor stack pointer", PropCat.TFormatType.EFormatAsHex )]
        public uint SavedSP
        {
            get { return iSavedSP; }
            set { iSavedSP = value; }
        }

        [PropCat( "Misc", "Return Value" )]
        public int ReturnValue
        {
            get { return iReturnValue; }
            set { iReturnValue = value; }
        }

        [PropCat( "Summary", "Is user thread", PropCat.TFormatType.EFormatAsYesNo )]
        public bool IsUserThread
        {
            get { return iParentThread.IsUserThread; }
        }

        [PropCat( "State", "Is the current thread", PropCat.TFormatType.EFormatAsYesNo )]
        public bool IsCurrent
        {
            get { return iParentThread.IsCurrent; }
        }

        public bool HaveUserContext
        {
            get
            {
                bool haveUserContext = true;
                //
                switch ( UserContextType )
                {
                case TUserContextType.EContextNone:
                case TUserContextType.EContextUndefined:
                case TUserContextType.EContextKernel:
                    haveUserContext = false;
                    break;
                default:
                    break;
                }
                //
                return haveUserContext;
            }
        }

        [PropCat( "State", "User context type" )]
        public TUserContextType UserContextType
        {
            get { return iUserContextType; }
            set { iUserContextType = value; }
        }

        [PropCat( "Linked List Info", PropCat.TFormatType.EFormatRecurseInto )]
        public LinkedListInfo LinkedListInfo
        {
            get { return iLinkedListInfo; }
        }

        [PropCat( "Mutex Info", PropCat.TFormatType.EFormatRecurseInto )]
        public NThreadMutexInfo MutexInfo
        {
            get { return iMutexInfo; }
        }

        [PropCat( "Timing Info", PropCat.TFormatType.EFormatRecurseInto )]
        public NThreadTimeInfo TimeInfo
        {
            get { return iTimeInfo; }
            set { iTimeInfo = value; }
        }

        [PropCat( "Count Info", PropCat.TFormatType.EFormatRecurseInto )]
        public NThreadCountInfo CountInfo
        {
            get { return iCountInfo; }
            set { iCountInfo = value; }
        }

        public NThreadExtraContextInfo ExtraContextInfo
        {
            get { return iExtraContextInfo; }
            set { iExtraContextInfo = value; }
        }

        public RegisterCollection Registers
        {
            get { return iRegisters; }
        }

        public TWaitType WaitType
        {
            get
            {
                TWaitType ret = TWaitType.EWaitTypeNotWaiting;
                //
                RegisterEntry linkReg = Registers[ "R14_USR" ];
                Symbol symbol = linkReg.Symbol;
                if ( symbol != null )
                {
                    if ( symbol.Name.Contains( "User::WaitForAnyRequest" ) )
                    {
                        ret = TWaitType.EWaitTypeUserWaitForAnyRequest;
                    }
                    else if ( symbol.Name.Contains( "User::WaitForRequest(TRequestStatus&)" ) )
                    {
                        ret = TWaitType.EWaitTypeUserWaitForRequest;
                    }
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        // <summary>
        // This property returns the return address of the instruction
        // to resume executing when this thread finishes executing within
        // the context of an exception handler (typically SVC)
        // </summary>
        internal uint UserReturnAddress
        {
            get
            {
                uint retAddr = 0;
                //
                switch ( UserContextType )
                {
                case TUserContextType.EContextNone:
                case TUserContextType.EContextUndefined:
                case TUserContextType.EContextKernel:
                    throw new NotSupportedException();
                default:
                    {
                        Thread.ThreadStackData superStackData = iParentThread.StackInfoSupervisor.Data;
                        SymbianUtils.DataBuffer.Entry.DataBufferUint uintEntry = superStackData.LastRawDataEntry;
                        retAddr = uintEntry.Uint;
                    }
                    break;
                }
                //
                return retAddr;
            }
        }

        private RegisterCollection GetUserContextRegisters()
        {
            bool isCurrent = IsCurrent;
            RegisterCollection ret = new RegisterCollection( Registers, RegisterCollection.TType.ETypeUser, iParentThread.OwningProcess );

            // User-land CPSR is stored in SPSR_SVC
            RegisterEntry spsr = Registers[ TArmRegisterType.EArmReg_SPSR ];
            ret[ TArmRegisterType.EArmReg_CPSR ].Value = spsr.Value;

            // Get the context table that we'll need to work out the reg positions
            UserContextTable.UserContextTable table = CrashDebugger.UserContextTableManager[ UserContextType ];

            // Get SP and stack data for supervisor thread
            uint sp = SavedSP;
            ThreadStackData spData = iParentThread.StackInfoSupervisor.Data;
            if ( spData.Info.Data.Size > 0 )
            {
                // This is the user side address that will be branched back to once we exit SVC mode...
                uint retAddr = UserReturnAddress;
                ret[ TArmRegisterType.EArmReg_PC ].Value = retAddr;
 
                // Now try to get the register values off of the supervisor stack
                DataBuffer superStackData = spData.Data;
                foreach ( ArmRegister reg in ret )
                {
                    if ( UserContextTable.UserContextTable.IsSupported( reg.RegType ) )
                    {
                        UserContextTableEntry uctEntry = table[ reg.RegType ];
                        if ( uctEntry.IsAvailable( isCurrent ) )
                        {
                            ArmRegister savedSp = new ArmRegister( TArmRegisterType.EArmReg_SP, sp );
                            uint newValue = uctEntry.Process( savedSp, superStackData );
                            reg.Value = newValue;
                        }
                    }
                }
            }

            // Getting context of current thread? Some values can be fetched directly
            // from the registers if they are not available from the stack.
            if ( isCurrent && table[ TArmRegisterType.EArmReg_SP ].Type == UserContextTableEntry.TType.EOffsetFromSp )
            {
                RegisterCollection userRegs = CrashDebugger.InfoCpu[ RegisterCollection.TType.ETypeUser ];
                //
                ret[ TArmRegisterType.EArmReg_SP ].Value = userRegs[ TArmRegisterType.EArmReg_SP ];
                ret[ TArmRegisterType.EArmReg_LR ].Value = userRegs[ TArmRegisterType.EArmReg_LR ];
            }

            return ret;
        }
        #endregion

        #region Internal constants
        #endregion

        #region Clipboard Support
        public string ToClipboard()
        {
            StringBuilder ret = new StringBuilder();
            //
            ret.AppendFormat( "NThread Address: 0x{0:x8}, NState: {1}" + System.Environment.NewLine, Address, NState );
            //
            return ret.ToString();
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        private readonly RegisterCollection iRegisters;
        private readonly DThread iParentThread;

        private uint iWaitObj = 0;// object on which this thread is waiting
        private uint iAddress = 0;
        private int iPriority = 0;
        private uint iAttributes = 0;
        private uint iAddressSpace = 0;
        private uint iSavedSP = 0;
        private int iReturnValue = 0;
        
        private TNThreadState iNState = TNThreadState.EDead;
        private TUserContextType iUserContextType = TUserContextType.EContextNone;

        private NThreadMutexInfo iMutexInfo = new NThreadMutexInfo();
        private NThreadCountInfo iCountInfo = new NThreadCountInfo();
        private LinkedListInfo iLinkedListInfo = new LinkedListInfo();
        private NThreadTimeInfo iTimeInfo = new NThreadTimeInfo();
        private NThreadExtraContextInfo iExtraContextInfo = new NThreadExtraContextInfo();
        #endregion
    }
}
