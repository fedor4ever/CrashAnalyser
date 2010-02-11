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
using SymbianUtils.BasicTypes;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Exceptions;
using SymbianETMLib.Common.Types;
using SymbianETMLib.Common.Packets;

namespace SymbianETMLib.Common.State
{
    public class ETMDecodeStateISync : ETMDecodeState
    {
        #region Constructors
        public ETMDecodeStateISync( ETMStateData aManager )
            : base( aManager )
        {
            iContextIdBytesRequired = aManager.Config.ContextIDSize;
            iBytesRemaining = iContextIdBytesRequired;
        }
        #endregion

        #region API
        public override ETMDecodeState HandleByte( SymByte aByte )
        {
            ETMDecodeState nextState = this;
            //
            switch ( iState )
            {
            case TState.EStateHeader:
                if ( base.StateData.Config.ContextIDSize != 0 )
                {
                    iState = TState.EStateContextId;
                }
                else
                {
                    iState = TState.EStateInformation;
                }
                break;
            case TState.EStateContextId:
                OnContextByte( aByte );
                break;
            case TState.EStateInformation:
                OnInformationByte( aByte );
                break;
            case TState.EStateAddress:
                OnAddress( aByte );
                break;
            default:
            case TState.EStateIdle:
                System.Diagnostics.Debug.Assert( false );
                break;
            }
            //
            if ( iState == TState.EStateIdle )
            {
                nextState = new ETMDecodeStateSynchronized( base.StateData );
            }
            //
            return nextState;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal enumerations
        private enum TState
        {
            EStateHeader = 0,
            EStateContextId,
            EStateInformation,
            EStateAddress,
            EStateIdle
        }
        #endregion

        #region Internal methods
        private void OnContextByte( SymByte aByte )
        {
            int byteNumber = iContextIdBytesRequired - iBytesRemaining;
            uint val = aByte.LShift( byteNumber * 8 );
            iContextId |= val;

            if ( --iBytesRemaining == 0 )
            {
                base.StateData.SetContextID( iContextId );
                iState = TState.EStateInformation;
            }
        }

        private void OnInformationByte( SymByte aByte )
        {
            iInformationByte = new ETMPcktISyncInformation( aByte );
            iState = TState.EStateAddress;

            // We're expecting 4 address bytes to follow.
            iBytesRemaining = 4;
        }

        private void OnAddress( SymByte aByte )
        {
            int byteNumber = 4 - iBytesRemaining;
            uint val = aByte.LShift( byteNumber * 8 );
            iAddress |= val;

            if ( --iBytesRemaining == 0 )
            {
                // Save for tracing purposes
                SymAddress originalAddress = new SymAddress( base.StateData.CurrentAddress.Address );
                TArmInstructionSet originalInstructionSet = base.StateData.CurrentInstructionSet;

                // Set new branch address
                TArmInstructionSet newInstructionSet = iInformationByte.InstructionSet;
                uint address = iAddress;
                if ( ( address & 0x1 ) == 0x1 )
                {
                    // We branched to THUMB, hence change of instruction set...
                    address &= 0xFFFFFFFE;
                    newInstructionSet = TArmInstructionSet.ETHUMB;
                }

                // Store address etc - always 32 bit full address during I-SYNC
                base.StateData.CurrentInstructionSet = newInstructionSet;
                base.StateData.SetKnownAddressBits( address, 32, TETMBranchType.EBranchExplicit );

                // And output debug trace...
                Trace( originalAddress, originalInstructionSet, base.StateData.CurrentAddress, newInstructionSet );

                // We're done
                iState = TState.EStateIdle;
            }
        }

        private void Trace( SymAddress aOriginalAddress, TArmInstructionSet aOriginalISet, SymAddress aNewAddress, TArmInstructionSet aNewISet )
        {
            System.Diagnostics.Debug.Assert( base.StateData.LastBranch.IsKnown );
            //
            StringBuilder lines = new StringBuilder();
            lines.AppendLine( "   I-SYNC" );
            lines.AppendLine( string.Format( "       from: {0} 0x{1:x8} {2}", ETMDecodeState.MakeInstructionSetPrefix( aOriginalISet ), aOriginalAddress, StateData.Engine.LookUpSymbol( aOriginalAddress ) ) );
            lines.AppendLine( string.Format( "         to: {0} 0x{1:x8} {2}", ETMDecodeState.MakeInstructionSetPrefix( aNewISet ), aNewAddress, StateData.Engine.LookUpSymbol( aNewAddress ) ) );
            //
            base.Trace( lines.ToString() );
        }
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        private readonly int iContextIdBytesRequired;
        private TState iState = TState.EStateHeader;
        private uint iContextId = 0;
        private uint iAddress = 0;
        private int iBytesRemaining = 0;
        private ETMPcktISyncInformation iInformationByte = null;
        #endregion
    }
}
