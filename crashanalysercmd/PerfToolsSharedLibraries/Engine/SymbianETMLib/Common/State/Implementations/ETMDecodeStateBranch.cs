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
using SymbianETMLib.Common.Utilities;
using SymbianETMLib.Common.BranchDecoder;

namespace SymbianETMLib.Common.State
{
    public class ETMDecodeStateBranch : ETMDecodeState
    {
        #region Constructors
        public ETMDecodeStateBranch( ETMStateData aManager )
            : base( aManager )
        {
            iDecoder = ETMBranchDecoder.New( aManager );
        }
        #endregion

        #region API
        public override ETMDecodeState HandleByte( SymByte aByte )
        {
            // Handle the byte
            PerformStateOperation( aByte );

            // Decide what to return based upon current state
            ETMDecodeState nextState = this;
            if ( iState == TState.EStateFinished )
            {
                iDecoder.FlushChanges();
                nextState = new ETMDecodeStateSynchronized( base.StateData );
            }

            // Done.
            return nextState;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal state handler
        private enum TState
        {
            EStateBranch = 0,
            EStateExceptionContinuation,
            EStateFinished
        }

        private void PerformStateOperation( SymByte aByte )
        {
            switch ( iState )
            {
            case TState.EStateBranch:
            {
                iDecoder.Offer( aByte );

                // Decode the branch if we have all the info we need
                if ( iDecoder.IsBranchAddressAvailable )
                {
                    iDecoder.DecodeBranch();

                    // Check if we expect an exception byte
                    if ( iDecoder.IsPacketComplete )
                    {
                        // Nope, we're done
                        iState = TState.EStateFinished;
                    }
                    else
                    {
                        iState = TState.EStateExceptionContinuation;
                    }
                }
                break;
            }
            case TState.EStateExceptionContinuation:
            {
                iDecoder.DecodeException( aByte );
                iState = TState.EStateFinished;
                break;
            }
            default:
            case TState.EStateFinished:
                System.Diagnostics.Debug.Assert( false );
                break;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly ETMBranchDecoder iDecoder;
        private TState iState = TState.EStateBranch;
        #endregion
    }
}
