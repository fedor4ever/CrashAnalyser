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

namespace SymbianETMLib.Common.Packets
{
    public class ETMPcktISyncInformation : ETMPcktBase
    {
        #region Enumerations
        public enum TType
        {
            ENormal = 0,
            ELoadOrStoreInProgress
        }

        public enum TReasonCode
        {
            EPeriodic = 0,
            ETracingEnabled = 1,
            ETracingRestartedAfterOverflow = 2,
            EProcessorExitedFromDebugState = 3
        }
        #endregion

        #region Constructors
        public ETMPcktISyncInformation( SymByte aByte )
            : base( aByte )
        {
        }
        #endregion

        #region API
        #endregion

        #region From PcktBase
        #endregion

        #region Properties
        public TType Type
        {
            get
            {
                TType ret = TType.ENormal;
                SymByte mask = base.CreateMask( "10000000" );
                if ( ( RawByte & mask ) == mask )
                {
                    ret = TType.ELoadOrStoreInProgress;
                }
                return ret;
            }
        }

        public bool IsSecure
        {
            get
            {
                bool ret = RawByte[ 3 ];
                return ret;
            }
        }

        public TArmInstructionSet InstructionSet
        {
            get
            {
                TArmInstructionSet ret = TArmInstructionSet.EARM;
                //
                if ( RawByte[ 4 ] )
                {
                    ret = TArmInstructionSet.EJAZELLE;
                }
                //
                return ret;
            }
        }

        public TReasonCode ReasonCode
        {
            get
            {
                TReasonCode ret = TReasonCode.EPeriodic;
                SymByte mask = base.CreateMask( "01100000" );
                SymByte val = RawByte & mask;
                val = val.HighestBitsShiftedRight( 5 );
                ret = (TReasonCode) val.Value;
                return ret;
            }
        }
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        #endregion
    }
}
