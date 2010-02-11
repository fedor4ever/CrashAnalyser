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
    public class ETMDecodeStateIgnore : ETMDecodeState
    {
        #region Constructors
        public ETMDecodeStateIgnore( ETMStateData aManager )
            : base( aManager )
        {
        }
        #endregion

        #region API
        public override ETMDecodeState HandleByte( SymByte aByte )
        {
            ETMDecodeState nextState = new ETMDecodeStateSynchronized( base.StateData );
            Trace();
            return nextState;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private void Trace()
        {
            StringBuilder lines = new StringBuilder();
            lines.AppendLine( "   IGNORE" );
            base.Trace( lines.ToString() );
        }
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        #endregion
    }
}
