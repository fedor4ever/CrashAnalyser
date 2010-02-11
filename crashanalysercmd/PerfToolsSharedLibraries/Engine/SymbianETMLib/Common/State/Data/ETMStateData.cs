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
using SymbianUtils.Tracer;
using SymbianUtils.BasicTypes;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Exceptions;
using SymbianStructuresLib.Arm.SecurityMode;
using SymbianStructuresLib.Arm.Instructions;
using SymbianStructuresLib.Debug.Code;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.PluginAPI.Types.Code;
using SymbianETMLib.Common.Types;
using SymbianETMLib.Common.Buffer;
using SymbianETMLib.Common.Engine;
using SymbianETMLib.Common.Config;
using SymbianETMLib.Common.Utilities;

namespace SymbianETMLib.Common.State
{
    public class ETMStateData : ITracer
    {
        #region Constructors
        internal ETMStateData( ETEngineBase aEngine )
        {
            iEngine = aEngine;
            iCurrentState = new ETMDecodeStateUnsynchronized( this );
        }
        #endregion

        #region API
        internal void PushBack( SymByte aByte )
        {
            Buffer.PushBack( aByte );
            iCurrentRawByte = iLastRawByte;
            --iPacketNumber;
        }

        internal void PrepareToHandleByte( SymByte aByte )
        {
            iLastRawByte = iCurrentRawByte;
            iCurrentRawByte = aByte;
            //
            iCurrentState = iCurrentState.PrepareToHandleByte( aByte );
            System.Diagnostics.Debug.Assert( iCurrentState != null );
            //
            ++iPacketNumber;
        }

        internal void IncrementProcessedInstructionCounter()
        {
            ++iNumberOfProcessedInstructions;
        }

        internal uint IncrementPC()
        {
            if ( !iLastBranch.IsUnknown )
            {
                uint instructionSize = (uint) CurrentInstructionSet;
                iPC.Address += instructionSize;
            }

            // Increment the instruction counter irrespective of whether or not we
            // know the program counter address
            IncrementProcessedInstructionCounter();
            return iPC;
        }

        internal ETMInstruction FetchInstruction( uint aAddress )
        {
            ETMInstruction ret = new ETMInstruction( aAddress );
            //
            bool gotCode = false;
            if ( this.LastBranch.IsKnown && Engine.DebugEngineView != null )
            {
                DbgViewCode codeView = Engine.DebugEngineView.Code;

                // In the case where we've been asked to fetch the code from the exception
                // vector, then bypass the rom/rofs code entirely.
                bool isExceptionVector = Config.IsExceptionVector( aAddress );
                if ( isExceptionVector )
                {
                    System.Diagnostics.Debug.Assert( this.CurrentInstructionSet == TArmInstructionSet.EARM );
                    TArmExceptionVector vector = Config.MapToExceptionVector( aAddress );
                    uint rawInstruction = Config.GetExceptionVector( vector );
                    ret.Instruction = codeView.ConvertToInstruction( aAddress, TArmInstructionSet.EARM, rawInstruction );
                }
                else
                {
                    IArmInstruction[] instructions = null;
                    gotCode = codeView.GetInstructions( aAddress, CurrentInstructionSet, 1, out instructions );
                    //
                    if ( gotCode )
                    {
                        System.Diagnostics.Debug.Assert( instructions != null && instructions.Length == 1 );
                        ret.Instruction = instructions[ 0 ];
                    }
                }
            }
            //
            return ret;
        }

        internal void SetUnsynchronized()
        {
            iSynchronized = false;
        }

        internal void SetSynchronized()
        {
            if ( !iSynchronized )
            {
                uint pos = iPacketNumber;
                iEngine.OnSynchronised( pos );
                iSynchronized = true;
            }
        }

        // <summary>
        // Set the known address bits of the program counter. We may not know all
        // bits until an I-sync packet is reached, or then until we see a full 5-byte
        // branch.
        // </summary>
        internal void SetKnownAddressBits( uint aAddress, int aNumberOfValidBits, TETMBranchType aBranchType )
        {
            iLastBranch.SetKnownAddressBits( aAddress, aNumberOfValidBits );
            
            iPC.Address = iLastBranch.Address;
            iPC.KnownBits = iLastBranch.KnownBits;

            // If we know the full branch address, then inform the engine
            if ( iPC.IsKnown )
            {
                iEngine.OnBranch( iPC, iCurrentInstructionSet, iCurrentException, aBranchType );
            }
        }

        internal void SetPC( uint aAddress )
        {
            SetPC( aAddress, this.CurrentInstructionSet );
        }

        internal void SetPC( uint aAddress, TArmInstructionSet aInstructionSet )
        {
            iPC.Address = aAddress;
            iCurrentInstructionSet = aInstructionSet;
            
            // If BBC mode is enabled, i.e. branches are output even when a direct branch is taken
            // then we don't need to emit a branch event when seeing a 'Direct' branch type.
            bool isBBCModeEnabled = Config.BBCModeEnabled;
            if ( isBBCModeEnabled == false && iPC.IsKnown )
            {
                iEngine.OnBranch( iPC, iCurrentInstructionSet, iCurrentException, TETMBranchType.EBranchDirect );
            }
        }

        internal void SetContextID( uint aValue )
        {
            // Tidy up the raw value.
            uint id = ( aValue >> 2 );
            if ( iCurrentContextId != id )
            {
                iCurrentContextId = id;
                iEngine.OnContextSwitch( iCurrentContextId );
            }
        }
        #endregion

        #region Properties
        public uint PacketNumber
        {
            get { return iPacketNumber; }
        }

        public int NumberOfProcessedInstructions
        {
            get { return iNumberOfProcessedInstructions; }
        }

        public SymByte CurrentByte
        {
            get { return iCurrentRawByte; }
        }

        public SymByte LastByte
        {
            get { return iLastRawByte; }
        }

        public ETMDecodeState CurrentState
        {
            get { return iCurrentState; }
        }

        public TArmExceptionType CurrentException
        {
            get { return iCurrentException; }
            set
            {
                if ( iCurrentException != value )
                {
                    iCurrentException = value;
                    Engine.OnExceptionModeChange( iCurrentException );
                }
            }
        }

        public TArmSecurityMode CurrentSecurityMode
        {
            get { return iCurrentSecurityMode; }
            set
            {
                if ( iCurrentSecurityMode != value )
                {
                    iCurrentSecurityMode = value;
                    Engine.OnExceptionModeChange( iCurrentException );
                }
            }
        }

        public TArmInstructionSet CurrentInstructionSet
        {
            get { return iCurrentInstructionSet; }
            set
            {
                if ( iCurrentInstructionSet != value )
                {
                    iCurrentInstructionSet = value;
                }
            }
        }
        #endregion 

        #region Internal properties
        internal SymAddress CurrentAddress
        {
            get { return iPC; }
        }

        internal SymAddressWithKnownBits LastBranch
        {
            get { return iLastBranch; }
        }

        internal ETEngineBase Engine
        {
            get { return iEngine; }
        }

        internal ETMInstruction LastInstruction
        {
            get { return iLastInstruction; }
            set { iLastInstruction = value; }
        }

        internal ETConfigBase Config
        {
            get { return iEngine.Config; }
        }

        private ETBufferBase Buffer
        {
            get { return iEngine.Buffer; }
        }
        #endregion

        #region Internal constants
        #endregion

        #region From ITracer
        public void Trace( string aText )
        {
            iEngine.Trace( aText );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            iEngine.Trace( aFormat, aParams );
        }
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        private readonly ETEngineBase iEngine;
        private bool iSynchronized = false;
        private int iNumberOfProcessedInstructions = 0;
        private uint iPacketNumber = 0;
        private uint iCurrentContextId = uint.MaxValue;
        private ETMInstruction iLastInstruction = new ETMInstruction();
        private SymAddressWithKnownBits iLastBranch = new SymAddressWithKnownBits();
        private SymAddressWithKnownBits iPC = new SymAddressWithKnownBits();
        private SymByte iCurrentRawByte = 0;
        private SymByte iLastRawByte = 0;
        private ETMDecodeState iCurrentState = null;
        private TArmExceptionType iCurrentException = TArmExceptionType.EUnknown;
        private TArmSecurityMode iCurrentSecurityMode = TArmSecurityMode.EUnknown;
        private TArmInstructionSet iCurrentInstructionSet = TArmInstructionSet.EARM;
        #endregion
    }
}
