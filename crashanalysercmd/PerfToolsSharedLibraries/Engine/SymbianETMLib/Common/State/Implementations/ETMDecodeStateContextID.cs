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
    public class ETMDecodeStateContextID : ETMDecodeState
    {
        #region Constructors
        public ETMDecodeStateContextID( ETMStateData aManager )
            : base( aManager )
        {
            iContextIdBytesRequired = aManager.Config.ContextIDSize;
            iBytesRemaining = iContextIdBytesRequired;
        }
        #endregion

        #region API
        public override ETMDecodeState HandleByte( SymByte aByte )
        {
            // TODO: test this
            ETMDecodeState nextState = this;
            //
            int byteNumber = iContextIdBytesRequired - iBytesRemaining;
            uint val = aByte.LShift( byteNumber * 8 );
            iContextId |= val;
            //
            if ( --iBytesRemaining == 0 )
            {
                // Got everything
                base.StateData.SetContextID( iContextId );
                nextState = new ETMDecodeStateSynchronized( base.StateData );
            }
            //
            return nextState;
        }
        #endregion

        #region Properties
        public string ContextIDName
        {
            get
            {
                string ret = base.StateData.Config.GetContextID( iContextId );
                return ret;
            }
        }
        #endregion

        #region Internal methods
        private void Trace()
        {
            base.DbgTrace( "CONTEXT_ID", string.Format( "ID: {0} [{1}]", iContextId, ContextIDName ) );
        }
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        private readonly int iContextIdBytesRequired;
        private uint iContextId = 0;
        private int iBytesRemaining = 0;
        #endregion
    }
}
