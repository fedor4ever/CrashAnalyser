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
using SymbianUtils;
using SymbianUtils.Range;
using SymbianUtils.DataBuffer;
using SymbianUtils.DataBuffer.Entry;
using SymbianUtils.DataBuffer.Primer;
using SymbianStackLib.Engine;
using SymbianStackLib.Data.Source;
using SymbianStackLib.Data.Output;
using SymbianStackLib.Data.Output.Entry;
using SymbianStructuresLib.Uids;
using SymbianStructuresLib.Arm.Registers;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Registers;
using CrashItemLib.Crash.Threads;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.CodeSegs;
using CrashItemLib.Crash.Messages;

namespace CrashItemLib.Crash.Stacks
{
	public class CIStack : CIElement, IEnumerable<CIStackEntry>
    {
        #region Static constructors
        public static CIStack NewStandAlone( CIRegisterList aRegisters, byte[] aData, uint aAddressOfFirstByte, AddressRange aRange )
        {
            CIStack ret = new CIStack( aRegisters, aData, aAddressOfFirstByte, aRange );
            return ret;
        }

        internal static CIStack NewThreadStack( CIThread aThread, CIRegisterList aRegisters, byte[] aData, uint aAddressOfFirstByte, AddressRange aRange )
        {
            CIStack ret = new CIStack( aRegisters, aData, aAddressOfFirstByte, aRange );
            return ret;
        }
        #endregion

        #region Constructors
        private CIStack( CIRegisterList aRegisters, byte[] aData, uint aAddressOfFirstByte, AddressRange aRange )
            : base( aRegisters.Container )
        {
            base.AddSupportedChildType( typeof( CIStackEntry ) );
            base.AddSupportedChildType( typeof( CIMessage ) );

            iRegisters = aRegisters;
            iStackAddressRange = aRange;

            // Prepare data
            DataBufferPrimer primer = new DataBufferPrimer( iStackData );
            primer.Prime( aData, aAddressOfFirstByte );
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public bool IsStackOutputAvailable
        {
            get { return iStackOutput != null; }
        }

        public bool IsThreadAvailable
        {
            get { return OwningThread != null; }
        }

        public bool IsProcessAvailable
        {
            get { return OwningProcess != null; }
        }

        public bool IsOverflow
        {
            get
            {
                bool ret = false;
                //
                bool pointerAvailable = this.Registers.Contains( TArmRegisterType.EArmReg_SP );
                if ( pointerAvailable )
                {
                    CIRegister regSP = this.Pointer;
                    AddressRange stackRange = this.Range;
                    //
                    ret = ( regSP <= stackRange.Min );
                }
                //
                return ret;
            }
        }

        public TArmRegisterBank Type
        {
            get { return Registers.Bank; }
        }

        public uint Base
        {
            // NB: CIStack stores addresses differently to the stack
            // engine, but these are correct so no need to panic.
            get
            {
                return iStackAddressRange.Min; 
            }
        }

        public uint Limit
        {
            // NB: CIStack stores addresses differently to the stack
            // engine, but these are correct so no need to panic.
            get
            {
                return iStackAddressRange.Max;
            }
        }

        public uint Size
        {
            get
            {
                // The address range contains all-inclusive values.
                //
                // E.g. a range of 0x00 -> 0x10 would include the values...:
                //
                //   0x00, 01, 02, 03, 04, 05, 06, 07, 08, 09, 0A, 0B, 0C, 0D, 0E, 0F, 0x10
                //
                // ...and therefore AddressRange.Size would return 17.
                //
                // Symbian OS treats the range as non-inclusive, so the value is one too large.
                // Hence we subtract one
                uint ret = iStackAddressRange.Size - 1;
                return ret;
            }
        }

        public int EntryCount
        {
            get { return iStackOutput.Count; }
        }

        public string Algorithm
        {
            get
            {
                string ret = "Unknown Algorithm";
                if ( iStackOutput != null )
                {
                    ret = iStackOutput.AlgorithmName;
                }
                return ret;
            }
        }

        public byte[] RawStackData
        {
            get { return iStackData.ToArray(); }
        }

        public int RawDataLength
        {
            get { return iStackData.Count; }
        }

        public AddressRange RawDataRange
        {
            get { return iStackData.Range; }
        }

        public CIRegister Pointer
        {
            get
            {
                CIRegister ret = Registers[ TArmRegisterType.EArmReg_SP ];
                return ret;
            }
        }

        public uint PointerValue
        {
            get
            {
                uint ret = 0;
                
                // We do it this way so as to avoid calling operator[] on the 
                // register list if the register does not exist. The default behaviour
                // of the register list class is to create a new register in such a situation
                // and we don't really want to do that.
                CIRegisterList regs = this.Registers;
                if ( regs.Contains( TArmRegisterType.EArmReg_SP ) )
                {
                    ret = this.Pointer.Value;
                }
                //
                return ret;
            }
        }

        public AddressRange Range
        {
            get { return iStackAddressRange; }
            set
            {
                iStackAddressRange = value;
            }
        }

        public CIThread OwningThread
        {
            get { return iRegisters.OwningThread; }
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

        public CICodeSegList CodeSegments
        {
            get
            {
                CICodeSegList list = null;
                CIProcess process = OwningProcess;
                if ( process != null )
                {
                    list = process.CodeSegments;
                }
                return list;
            }
        }

        public CIRegisterList Registers
        {
            get { return iRegisters; }
        }

        public StackOutputData RawOutputData
        {
            get { return iStackOutput; }
        }

        internal StackSourceData RawSourceData
        {
            get { return iStackData; }
        }
        #endregion

        #region Internal methods
        private void CreateEntries()
        {
            if ( iStackOutput != null )
            {
                int count = iStackOutput.Count;
                //
                for ( int i = 0; i < count; i++ )
                {
                    StackOutputEntry dataOutputEntry = iStackOutput[ i ];
                    //
                    CIStackEntry entry = new CIStackEntry( this, dataOutputEntry );
                    base.AddChild( entry );
                }
            }
        }
        #endregion

        #region From CIElement
        internal override void OnFinalize( CIElementFinalizationParameters aParams )
        {
            try
            {
                base.OnFinalize( aParams );
            }
            finally
            {
                CIStackBuilder builder = new CIStackBuilder( this, aParams.DebugEngine );
                builder.Build( TSynchronicity.ESynchronous );
                //
                iStackOutput = builder.StackEngine.DataOutput;
                //
                CreateEntries();
            }
        }
        #endregion

        #region From IEnumerable<CIStackEntry>
        public new IEnumerator<CIStackEntry> GetEnumerator()
        {
            CIElementList<CIStackEntry> entries = base.ChildrenByType<CIStackEntry>();
            return entries.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            CIElementList<CIStackEntry> entries = base.ChildrenByType<CIStackEntry>();
            return entries.GetEnumerator();
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            if ( OwningThread != null )
            {
                ret.Append( "[Stack] " );
                ret.Append( OwningThread.ToString() );
                ret.Append( " " );
            }
            else
            {

            }
            //
            ret.Append( Registers.BankName );
            return ret.ToString();
        }
        #endregion

        #region Data members
        private readonly CIRegisterList iRegisters;
        private StackSourceData iStackData = new StackSourceData();
        private AddressRange iStackAddressRange = new AddressRange();
        private StackOutputData iStackOutput = null;
        #endregion
    }
}
