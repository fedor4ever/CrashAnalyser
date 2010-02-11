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
using SymbianStructuresLib.Arm.SecurityMode;
using SymbianETMLib.Common.Types;
using SymbianETMLib.Common.Config;
using SymbianETMLib.Common.State;
using SymbianETMLib.Common.Exception;

namespace SymbianETMLib.Common.BranchDecoder
{
    internal class ETMBranchDecoderOriginal : ETMBranchDecoder
    {
        #region Constructors
        public ETMBranchDecoderOriginal( ETMStateData aManager )
            : base( aManager )
        {
        }

        static ETMBranchDecoderOriginal()
        {
            iMaskByte5_ARM = new SymMask( KMaskByte5_ARM );
            iMaskByte5_THUMB = new SymMask( KMaskByte5_THUMB );
            iMaskByte5_JAZELLE = new SymMask( KMaskByte5_JAZELLE );
        }
        #endregion

        #region From ETBBranchDecoder
        public override void Offer( SymByte aByte )
        {
            base.Save( aByte );
        }

        protected override void DecodeFull()
        {
            // We have full 5 bytes of address info.
            SymMask maskFirst = new SymMask( 0x7E, SymMask.TShiftDirection.ERight, 1 );
            SymMask maskMiddle = new SymMask( 0x7F );

            SymByte b;
            SymAddressWithKnownBits address = new SymAddressWithKnownBits();
            uint masked = 0;
            
            // First byte
            b = base[ 0 ];
            address.KnownBits += 6;
            address.Address |= maskFirst.Apply( b );

            // Bytes 1, 2, 3
            for ( int i = 1; i <= 3; i++ )
            {
                b = base[ i ];
                b = (byte) maskMiddle.Apply( b );
                masked = (uint) ( b << address.KnownBits );
                address.KnownBits += 7;
                address.Address |= masked;
            }

            // Last byte - mask depends on instruction set.
            b = base[ 4 ];
            int lastByteAddressBits = 0;
            if ( this.ContainsInlineException || iMaskByte5_ARM.IsMatch( b ) )
            {
                // Inline exception indicates ARM.
                base.InstructionSet = TArmInstructionSet.EARM;
                lastByteAddressBits = 3;
            }
            else if ( iMaskByte5_THUMB.IsMatch( b ) )
            {
                base.InstructionSet = TArmInstructionSet.ETHUMB;
                lastByteAddressBits = 4;
            }
            else if ( iMaskByte5_JAZELLE.IsMatch( b ) )
            {
                base.InstructionSet = TArmInstructionSet.EJAZELLE;
                lastByteAddressBits = 5;
            }
            else
            {
                throw new ETMException( "ERROR: branch type unknown" );
            }

            // Now we can process the last byte
            masked = b.LowestBits( lastByteAddressBits );
            masked <<= address.KnownBits;
            address.KnownBits += lastByteAddressBits;
            address.Address |= masked;

            // Shift entire address by shift count based upon current instruction set.
            int isShift = ETMBranchDecoder.CompressionLeftShiftCount( base.InstructionSet );
            System.Diagnostics.Debug.Assert( address.KnownBits + isShift == 32 );
            address.KnownBits = 32;
            address.Address <<= isShift;

            // Save address
            base.BranchAddress = address;

            // We may also need to decode the inline exception
            if ( ContainsInlineException )
            {
                // Yup, line exception...
                DecodeInlineException( base.LastByte );
            }
        }

        protected override void DecodePartial()
        {
            // According to Table 7-13, "Missing components of exception Branch address packets"
            // then after a partial branch, we're always in "no exception" state
            base.ExceptionType = TArmExceptionType.ENone;

            // We have full 5 bytes of address info.
            SymMask maskFirst = new SymMask( 0x7E, SymMask.TShiftDirection.ERight, 1 );
            SymMask maskMiddle = new SymMask( 0x7F );

            SymByte b;
            SymAddressWithKnownBits address = new SymAddressWithKnownBits();
            uint masked = 0;
            int count = base.Count;

            // First byte
            b = base[ 0 ];
            address.KnownBits += 6;
            address.Address |= maskFirst.Apply( b );

            // Bytes 1, 2, 3
            for ( int i = 1; i < count; i++ )
            {
                b = base[ i ];
                b = (byte) maskMiddle.Apply( b );
                masked = (uint) ( b << address.KnownBits );
                address.KnownBits += 7;
                address.Address |= masked;
            }

            // Shift entire address by shift count based upon current instruction set.
            int isShift = ETMBranchDecoder.CompressionLeftShiftCount( base.InstructionSet );
            address.KnownBits += isShift;
            address.Address <<= isShift;

            // Save address
            base.BranchAddress = address;
        }

        public override void DecodeException( SymByte aByte )
        {
            // In a continuation exception byte, bit 7 is always supposed
            // to be clear, irrespective of whether original or alternative
            // compression schemes are in use.
            System.Diagnostics.Debug.Assert( aByte[ 7 ] == false );

            // Instruction cancellation
            base.IsLastInstructionCancelled = aByte[ 5 ];

            // Security
            base.SecurityMode = TArmSecurityMode.ESecure;
            if ( aByte[ 0 ] )
            {
                base.SecurityMode = TArmSecurityMode.ENotSecure;
            }

            // Exception type
            TArmExceptionType exceptionType = TArmExceptionType.EUnknown;
            aByte = (byte) ( ( (byte) ( aByte & 0x1E ) ) >> 1 );
            switch ( aByte )
            {
            case 0:
                exceptionType = TArmExceptionType.ENone;
                break;
            case 1:
                exceptionType = TArmExceptionType.EHaltingDebug;
                break;
            case 2:
                exceptionType = TArmExceptionType.ESecureMonitorCall;
                break;
            default:
            case 3:
                throw new ETMException( "ERROR - reserved exception code during continuation byte" );
            case 4:
                exceptionType = TArmExceptionType.EAsyncDataAbort;
                break;
            case 5:
                exceptionType = TArmExceptionType.EJazelle;
                break;
            case 6:
            case 7:
                throw new ETMException( "ERROR - reserved exception code during continuation byte" );
            case 8:
                exceptionType = TArmExceptionType.EProcessorReset;
                break;
            case 9:
                exceptionType = TArmExceptionType.EUndefinedInstruction;
                break;
            case 10:
                exceptionType = TArmExceptionType.ESVC;
                break;
            case 11:
                exceptionType = TArmExceptionType.EPrefetchAbortOrSWBreakpoint;
                break;
            case 12:
                exceptionType = TArmExceptionType.ESyncDataAbortOrSWWatchpoint;
                break;
            case 13:
                exceptionType = TArmExceptionType.EGeneric;
                break;
            case 14:
                exceptionType = TArmExceptionType.EIRQ;
                break;
            case 15:
                exceptionType = TArmExceptionType.EFIQ;
                break;
            }
            base.ExceptionType = exceptionType;
        }
        #endregion

        #region Properties
        protected virtual bool ContainsInlineException
        {
            get
            {
                bool ret = false;
                //
                bool completeBranch = IsBranchAddressAvailable;
                if ( completeBranch )
                {
                    ret = ( LastByte[ 7 ] );
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal constants
        protected const string KMaskByte5_ARM       = "0#001###";
        protected const string KMaskByte5_THUMB     = "0#01####";
        protected const string KMaskByte5_JAZELLE   = "0#1#####";
        #endregion

        #region Internal methods
        private void DecodeInlineException( SymByte aByte )
        {
            // b1CEEExxx Exception executed in ARM state.
            // 
            // The C bit is set to 1 if the exception cancels the last traced instruction.
            // The EEE bits, bits [5:3], indicate the type of exception as shown in Table 7-9.
            // Use of this format is deprecated in favor of using an Exception information byte.

            // Inline exception packets always indicate ARM mode
            base.InstructionSet = TArmInstructionSet.EARM;

            // Was it a cancelling branch?
            base.IsLastInstructionCancelled = aByte[ 6 ];

            // Set back to no exception unless changed explicitly below.
            base.ExceptionType = TArmExceptionType.ENone;

            // Get exception type
            SymMask mask = new SymMask( "## 111 ###", SymMask.TShiftDirection.ERight, 3 );
            SymByte exceptionInfo = (byte) mask.Apply( aByte );

            if ( exceptionInfo == 0 )
            {
                // Have to work this out from the branch address
                TArmExceptionVector vector = base.Config.MapToExceptionVector( base.BranchAddress.Address );
                switch ( vector )
                {
                case TArmExceptionVector.EReset:
                    base.ExceptionType = TArmExceptionType.EProcessorReset;
                    break;
                case TArmExceptionVector.EUndefinedInstruction:
                    base.ExceptionType = TArmExceptionType.EUndefinedInstruction;
                    break;
                case TArmExceptionVector.ESVC:
                    base.ExceptionType = TArmExceptionType.ESVC;
                    break;
                case TArmExceptionVector.EPrefetchAbort:
                    base.ExceptionType = TArmExceptionType.EPrefetchAbortOrSWBreakpoint;
                    break;
                case TArmExceptionVector.EDataAbort:
                    base.ExceptionType = TArmExceptionType.ESyncDataAbortOrSWWatchpoint;
                    break;
                default:
                    throw new ETMException( "ERROR - unable to extract exception type from branch address: 0x" + base.BranchAddress );
                }
            }
            else
            {
                switch ( exceptionInfo )
                {
                case 1:
                    base.ExceptionType = TArmExceptionType.EIRQ;
                    break;
                default:
                case 2:
                case 3:
                    throw new NotSupportedException( "Reserved exception type" );
                case 4:
                    base.ExceptionType = TArmExceptionType.EJazelle;
                    break;
                case 5:
                    base.ExceptionType = TArmExceptionType.EFIQ;
                    break;
                case 6:
                    base.ExceptionType = TArmExceptionType.EAsyncDataAbort;
                    break;
                case 7:
                    base.ExceptionType = TArmExceptionType.EHaltingDebug;
                    break;
                }
            }
        }
        #endregion

        #region Data members
        private static readonly SymMask iMaskByte5_ARM;
        private static readonly SymMask iMaskByte5_THUMB;
        private static readonly SymMask iMaskByte5_JAZELLE;
        #endregion
    }
}
