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
using SymbianETMLib.Common.Types;
using SymbianETMLib.Common.State;
using SymbianETMLib.Common.Config;

namespace SymbianETMLib.Common.BranchDecoder
{
    internal abstract class ETMBranchDecoder : ITracer
    {
        #region Factory
        public static ETMBranchDecoder New( ETMStateData aStateData )
        {
            ETMBranchDecoder ret = null;
            //
            switch ( aStateData.Config.BranchCompressionScheme )
            {
            default:
            case TETMBranchCompressionScheme.EOriginal:
                ret = new ETMBranchDecoderOriginal( aStateData );
                break;
            case TETMBranchCompressionScheme.EAlternative:
                ret = new ETMBranchDecoderAlternative( aStateData );
                break;
            }
            //
            return ret;
        }
        #endregion

        #region Constructors
        protected ETMBranchDecoder( ETMStateData aStateData )
        {
            iStateData = aStateData;
            iInstructionSet = aStateData.CurrentInstructionSet;
        }
        #endregion

        #region API
        public void FlushChanges()
        {
            TArmInstructionSet originalInstructionSet = iStateData.CurrentInstructionSet;
            SymAddress originalAddress = new SymAddress( iStateData.CurrentAddress.Address );

            //if ( !IsLastInstructionCancelled )
            {
                // Set known address
                iStateData.SetKnownAddressBits( iBranchAddress.Address,
                                              iBranchAddress.KnownBits,
                                              TETMBranchType.EBranchExplicit );

                // Handle a change in security mode
                if ( iSecurityMode != TArmSecurityMode.EUnknown )
                {
                    iStateData.CurrentSecurityMode = iSecurityMode;
                }

                if ( iExceptionType != TArmExceptionType.EUnknown )
                {
                    iStateData.CurrentException = iExceptionType;
                }

                // Handle a change in instruction set
                if ( iStateData.CurrentInstructionSet != iInstructionSet )
                {
                    iStateData.CurrentInstructionSet = iInstructionSet;
                }
            }

            DbgTrace( originalAddress, originalInstructionSet );
        }

        public void DecodeBranch()
        {
            System.Diagnostics.Debug.Assert( IsBranchAddressAvailable );
            System.Diagnostics.Debug.Assert( Count > 0 && Count <= 5 );
            //
            if ( Count == 5 )
            {
                DecodeFull();
            }
            else
            {
                DecodePartial();
            }
        }
        #endregion

        #region Framework API
        public abstract void Offer( SymByte aByte );

        public abstract void DecodeException( SymByte aByte );

        protected abstract void DecodeFull();

        protected abstract void DecodePartial();

        public virtual bool IsPacketComplete
        {
            get
            {
                // If the 7th bit is clear, then this might be
                // the last byte.
                //
                // If:
                //
                // a) the number of bytes forming the branch packet
                //    is less than 5, then this is the last byte.
                //
                // b) the number of bytes forming the branch packet
                //    is 5, and
                //    i)  bit 6 is clear => normal branch, this is the last byte
                //    ii) bit 7 is set => inline exception, but this is still the last byte.
                //
                bool ret = false;
                bool bit7 = LastByte[ 7 ];
                //
                switch ( iBytes.Count )
                {
                default:
                case 0:
                    ret = false;
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                    ret = ( bit7 == false ); // (a)
                    break;
                case 5:
                    if ( bit7 )
                    {
                        // (b), part (ii)
                        ret = true;
                    }
                    else
                    {
                        // (b), part (i) - i.e. bit 6 and 7 are both clear => normal branch byte
                        ret = ( LastByte[ 6 ] == false );
                    }
                    break;
                }
                return ret;
            }
        }

        public virtual bool IsBranchAddressAvailable
        {
            get
            {
                // The branch address is only available once 
                // bit seven is clear, or then once we've reached
                // an entire run of 5 bytes.
                bool ret = false;
                //
                if ( Count == 5 )
                {
                    ret = true;
                }
                else
                {
                    ret = ( LastByte[ 7 ] == false );
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal API
        protected void Save( SymByte aByte )
        {
            iBytes.Add( aByte );
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iBytes.Count; }
        }

        public bool IsLastInstructionCancelled
        {
            get { return iLastInstructionCancelled; }
            protected set { iLastInstructionCancelled = value; }
        }

        public SymByte this[ int aIndex ]
        {
            get { return iBytes[ aIndex ]; }
        }

        public ETConfigBase Config
        {
            get { return iStateData.Config; }
        }

        public TArmExceptionType ExceptionType
        {
            get { return iExceptionType; }
            protected set { iExceptionType = value; }
        }

        public TArmSecurityMode SecurityMode
        {
            get { return iSecurityMode; }
            protected set { iSecurityMode = value; }
        }

        public TArmInstructionSet InstructionSet
        {
            get { return iInstructionSet; }
            protected set { iInstructionSet = value; }
        }

        public SymAddressWithKnownBits BranchAddress
        {
            get { return iBranchAddress; }
            protected set { iBranchAddress = value; }
        }
        #endregion

        #region Internal methods
        protected SymByte LastByte
        {
            get
            {
                SymByte ret = new SymByte( 0 );
                //
                int count = iBytes.Count;
                if ( count > 0 )
                {
                    ret = iBytes[ count - 1 ];
                }
                //
                return ret;
            }
        }

        protected static int CompressionLeftShiftCount( TArmInstructionSet aIS )
        {
            int ret = 2;

            // Interpret the bytes to form an address based upon current
            // instruction set.
            switch ( aIS )
            {
            default:
            case TArmInstructionSet.EARM:
                break;
            case TArmInstructionSet.ETHUMB:
                ret = 1;
                break;
            case TArmInstructionSet.EJAZELLE:
                ret = 0;
                break;
            }
            //
            return ret;
        }

        private void DbgTrace( SymAddress aOriginalAddress, TArmInstructionSet aOriginalISet )
        {
            if ( Count == 5 )
            {
                DbgTrace( "BRANCH-F", aOriginalAddress, aOriginalISet );
            }
            else
            {
                DbgTrace( "BRANCH-P", aOriginalAddress, aOriginalISet );
            }
        }

        private void DbgTrace( string aType, SymAddress aOriginalAddress, TArmInstructionSet aOriginalISet )
        {
            StringBuilder lines = new StringBuilder();
            lines.AppendLine( "   " + aType );
            //
            if ( iStateData.LastBranch.IsKnown )
            {
                lines.AppendLine( string.Format( "      using: {0} 0x{1} to go...",
                    iBranchAddress.AddressBinary,
                    iBranchAddress.AddressHex )
                    );
                lines.AppendLine( string.Format( "       from: {0} 0x{1} {2} 0x{3:x8} {4}", 
                    aOriginalAddress.AddressBinary,
                    aOriginalAddress.AddressHex,
                    ETMDecodeState.MakeInstructionSetPrefix( aOriginalISet ),
                    aOriginalAddress,
                    iStateData.Engine.LookUpSymbol( aOriginalAddress ) ) 
                    );
                lines.AppendLine( string.Format( "         to: {0} 0x{1} {2} 0x{3:x8} {4}", 
                    iStateData.CurrentAddress.AddressBinary,
                    iStateData.CurrentAddress.AddressHex,
                    ETMDecodeState.MakeInstructionSetPrefix( iStateData.CurrentInstructionSet ),
                    iStateData.CurrentAddress, 
                    iStateData.Engine.LookUpSymbol( iStateData.CurrentAddress ) ) 
                    );
            }
            //
            Trace( lines.ToString() );
        }
        #endregion

        #region From ITracer
        public void Trace( string aText )
        {
            iStateData.Trace( aText );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            iStateData.Trace( aFormat, aParams );
        }
        #endregion

        #region Data members
        private readonly ETMStateData iStateData;
        private bool iLastInstructionCancelled = false;
        private List<SymByte> iBytes = new List<SymByte>();
        private TArmExceptionType iExceptionType = TArmExceptionType.EUnknown;
        private TArmSecurityMode iSecurityMode = TArmSecurityMode.EUnknown;
        private TArmInstructionSet iInstructionSet = TArmInstructionSet.EARM;
        private SymAddressWithKnownBits iBranchAddress = new SymAddressWithKnownBits();
        #endregion
    }
}
