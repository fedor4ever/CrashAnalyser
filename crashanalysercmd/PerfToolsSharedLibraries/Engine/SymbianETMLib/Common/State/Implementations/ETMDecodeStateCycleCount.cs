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
using SymbianETMLib.Common.Types;
using SymbianETMLib.Common.Packets;
using SymbianETMLib.Common.Utilities;

namespace SymbianETMLib.Common.State
{
    public class ETMDecodeStateCycleCount : ETMDecodeState
    {
        #region Constructors
        public ETMDecodeStateCycleCount( ETMStateData aManager )
            : base( aManager )
        {
        }
        #endregion

        #region API
        public override ETMDecodeState HandleByte( SymByte aByte )
        {
            ETMDecodeState nextState = new ETMDecodeStateSynchronized( base.StateData );

            ETMPcktCycleCount cycle = new ETMPcktCycleCount( aByte );

            // Always save the byte
            iPreviousBytes.Add( aByte );

            // Is the top bit set? if so, then another branch packet follows.
            // If not, we're done.
            if ( iPreviousBytes.Count == 5 )
            {
                // Have obtained 5 bytes or then last byte of smaller run.
                Flush();
            }
            else if ( cycle.MoreToCome )
            {
                nextState = this;
            }
            else
            {
                // Have obtained last byte of smaller run.
                Flush();
            }

            // Done.
            return nextState;
        }
        #endregion

        #region Properties
        protected bool IsFullBranch
        {
            get { return iPreviousBytes.Count == 5; }
        }
        #endregion

        #region Internal methods
        private void Flush()
        {
            // If we have a full 5 bytes, then mask off the top bits
            // from the last byte because these are reserved.
            int count = iPreviousBytes.Count;
            if ( count == 5 )
            {
                iPreviousBytes[ 4 ].Value = (byte) ( iPreviousBytes[ 4 ].Value & 0xF );
            }

            // Build final 32 bit value
            uint counter = 0;
            for ( int shift = 0; shift < count; shift++ )
            {
                int shiftBits = shift * 8;
                uint v = (uint) ( iPreviousBytes[ shift ] << shiftBits );
                counter |= v;
            }
        }

        private void Trace( uint aCounter )
        {
            base.DbgTrace( "CYCLE_COUNT", string.Format( " - {0}", aCounter ) );
        }
        #endregion

        #region Internal constants
        protected const string KFifthByteMask_NormalStateBranchAddressArm               = "00001xxx";
        protected const string KFifthByteMask_NormalStateBranchAddressThumb             = "0001xxxx";
        protected const string KFifthByteMask_NormalStateBranchAddressJazelle           = "001xxxxx";
        protected const string KFifthByteMask_Reserved1                                 = "00000xxx";
        protected const string KFifthByteMask_StateBranchWithFollowingExceptionArm      = "01001xxx";
        protected const string KFifthByteMask_StateBranchWithFollowingExceptionThumb    = "0101xxxx";
        protected const string KFifthByteMask_StateBranchWithFollowingExceptionJazelle  = "011xxxxx";
        protected const string KFifthByteMask_Reserved2                                 = "01000xxx";
        protected const string KFifthByteMask_ExceptionInArmState                       = "1xxxxxxx";
        #endregion

        #region Data members
        private List<SymByte> iPreviousBytes = new List<SymByte>();
        #endregion
    }
}
