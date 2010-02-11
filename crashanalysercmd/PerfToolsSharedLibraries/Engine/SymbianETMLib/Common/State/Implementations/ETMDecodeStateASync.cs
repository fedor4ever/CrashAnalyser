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
using SymbianETMLib.Common.Exception;

namespace SymbianETMLib.Common.State
{
    public class ETMDecodeStateASync : ETMDecodeState
    {
        #region Constructors
        public ETMDecodeStateASync( ETMStateData aManager )
            : base( aManager )
        {
        }
        #endregion

        #region API
        public override ETMDecodeState HandleByte( SymByte aByte )
        {
            ETMDecodeState nextState = this;
            //
            if ( aByte == 0 )
            {
                ++iASyncConsecutiveNullByteCount;
            }
            else if ( aByte == 0x80 && iASyncConsecutiveNullByteCount == 5 )
            {
                // That's the start of the a-sync packet
                iASyncConsecutiveNullByteCount = 0;
                base.StateData.SetSynchronized();
                nextState = new ETMDecodeStateSynchronized( base.StateData );
                Trace();
            }
            else
            {
                // Invalid byte
                throw new ETMException( "ERROR - data is corrupt - expected null byte or end marker during A-SYNC" );
            }
            //
            return nextState;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private void Trace()
        {
            base.DbgTrace( "A-SYNC", string.Empty );
        }
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        private int iASyncConsecutiveNullByteCount = 0;
        #endregion
    }
}
