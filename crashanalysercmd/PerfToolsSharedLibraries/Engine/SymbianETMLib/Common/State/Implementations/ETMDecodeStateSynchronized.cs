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

namespace SymbianETMLib.Common.State
{
    public class ETMDecodeStateSynchronized : ETMDecodeState
    {
        #region Constructors
        public ETMDecodeStateSynchronized( ETMStateData aManager )
            : base( aManager )
        {
        }
        #endregion

        #region API
        public override ETMDecodeState HandleByte( SymByte aByte )
        {
            ETMDecodeState nextState = this;
            //
            ETMPcktBase packet = Packets.Factory.ETMPacketFactory.Create( aByte );
            if ( packet != null )
            {
                if ( packet is ETMPcktBranch )
                {
                    nextState = new ETMDecodeStateBranch( base.StateData );
                    base.StateData.PushBack( aByte );
                }
                else if ( packet is ETMPcktIgnore )
                {
                    nextState = new ETMDecodeStateIgnore( base.StateData );
                    base.StateData.PushBack( aByte );
                }
                else if ( packet is ETMPcktISync )
                {
                    nextState = new ETMDecodeStateISync( base.StateData );
                    base.StateData.PushBack( aByte );
                }
                else if ( packet is ETMPcktPHeaderFormat1 || packet is ETMPcktPHeaderFormat2 )
                {
                    nextState = new ETMDecodeStatePHeader( base.StateData );
                    base.StateData.PushBack( aByte );
                }
                else if ( packet is ETMPcktASync )
                {
                    nextState = new ETMDecodeStateASync( base.StateData );
                    base.StateData.PushBack( aByte );
                }
                else if ( packet is ETMPcktOutOfOrderData )
                {
                    nextState = new ETMDecodeStateOutOfOrderData( base.StateData );
                    base.StateData.PushBack( aByte );
                }
                else if ( packet is ETMPcktCycleCount )
                {
                    nextState = new ETMDecodeStateCycleCount( base.StateData );
                }
                else if ( packet is ETMPcktContextID )
                {
                    nextState = new ETMDecodeStateContextID( base.StateData );
                }
                else
                {
                    base.Trace( string.Format( "OP NOT HANDLED: {0:x2}", aByte ) );
                    System.Diagnostics.Debug.Assert( false );
                }
            }
            else
            {
                base.Trace( string.Format( "WARNING: OP NOT RECOGNISED: {0:x2}", aByte ) );
                System.Diagnostics.Debug.Assert( false );
            }
            //
            return nextState;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        #endregion
    }
}
