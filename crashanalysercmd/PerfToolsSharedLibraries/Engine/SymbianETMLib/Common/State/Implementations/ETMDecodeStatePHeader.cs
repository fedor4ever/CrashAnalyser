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
using SymbianETMLib.Common.Exception;

namespace SymbianETMLib.Common.State
{
    public class ETMDecodeStatePHeader : ETMDecodeState
    {
        #region Constructors
        public ETMDecodeStatePHeader( ETMStateData aManager )
            : base( aManager )
        {
        }

        static ETMDecodeStatePHeader()
        {
            // ARM branch instructions
            iBranchMask_ARM_BOrBL = new SymMask( "#### 101 # ######## ######## ########" );
            iBranchMask_ARM_BLX_BranchToThumb = new SymMask( "1111 101 # ######## ######## ########" );

            // THUMB branch instructions
            iBranchMask_THUMB_B1 = new SymMask( "1101 #### ########" );
            iBranchMask_THUMB_B2 = new SymMask( "11100  ###########" );
            iBranchMask_THUMB_BLX_Part1 = new SymMask( "11110  ###########" ); // Multi-instruction branch
            iBranchMask_THUMB_BLX_Part2 = new SymMask( "111#1  ###########" ); // Multi-instruction branch
        }
        #endregion

        #region API
        public override ETMDecodeState HandleByte( SymByte aByte )
        {
            ETMDecodeState nextState = new ETMDecodeStateSynchronized( base.StateData );
            //
            ETMPcktBase packet = Packets.Factory.ETMPacketFactory.Create( aByte );
            if ( packet is ETMPcktPHeaderFormat1 )
            {
                ETMPcktPHeaderFormat1 pHeader1 = (ETMPcktPHeaderFormat1) packet;
                ProcessFormat1Conditions( pHeader1 );
            }
            else if ( packet is ETMPcktPHeaderFormat2 )
            {
                ETMPcktPHeaderFormat2 pHeader2 = (ETMPcktPHeaderFormat2) packet;
                ProcessFormat2Conditions( pHeader2 );
            }
            else
            {
                throw new ETMException( "ERROR: P-HEADER is not supported" );
            }
            //
            return nextState;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal enumerations
        private enum TMultiPartThumbBranch
        {
            ETHUMBBL = 0x3,
            ETHUMBBLX = 0x1,
        }
        #endregion

        #region Internal methods
        private void ProcessFormat1Conditions( ETMPcktPHeaderFormat1 aHeader )
        {
            string pass = aHeader.ConditionCountPassed > 0 ? string.Format( "{0} x PASS", aHeader.ConditionCountPassed ) : string.Empty;
            string fail = aHeader.ConditionCountFailed > 0 ? string.Format( ", {0} x FAIL", aHeader.ConditionCountFailed ) : string.Empty;
            base.Trace( "{0} - {1} {2}", base.MakeTracePacketPrefix( "P-HEADER(1)" ), pass, fail );

            // Get the total number of instructions processed
            int totalExecutedInstructionCount = aHeader.ConditionCountPassed + aHeader.ConditionCountFailed;

            // Trace passed instructions
            for ( int i = 0; i < aHeader.ConditionCountPassed; i++ )
            {
                uint address = base.StateData.CurrentAddress;
                ETMInstruction instruction = base.StateData.FetchInstruction( address );
                //
                TraceAtom( ETMPcktPHeaderBase.TAtomType.EAtomE_Passed, instruction );
                
                // We will now check the current instruction to see if it's a branch.
                // If it is, then we don't need to increment the instruction address, because
                // the explicit branch will set a new PC location.
                //
                // Note that in the case of a thumb BLX, i.e. two 16-bit instructions which
                // interpreted together form a +/-4mb branch address, we also need
                // to ensure that the 
                bool branched = CheckForBranch( instruction );
                if ( !branched )
                {
                    base.StateData.IncrementPC();
                }
            }

            // Trace and failed instructions
            if ( aHeader.ConditionCountFailed > 0 )
            {
                uint address = base.StateData.CurrentAddress;
                ETMInstruction instruction = base.StateData.FetchInstruction( address );
                //
                TraceAtom( ETMPcktPHeaderBase.TAtomType.EAtomN_Failed, instruction );
                base.StateData.IncrementPC();
            }

            // Spacer - to make the verbose output easier to read...
            base.Trace( string.Empty );
        }

        private void ProcessFormat2Conditions( ETMPcktPHeaderFormat2 aHeader )
        {
            string atom1TypeName = TraceAtomTypeName( aHeader.Atom1Type );
            string atom2TypeName = TraceAtomTypeName( aHeader.Atom2Type );
            base.Trace( "{0} - 1 x {1}, 1 x {2}", base.MakeTracePacketPrefix( "P-HEADER(2)" ), atom1TypeName, atom2TypeName );

            // First instruction
            {
                uint address1 = base.StateData.CurrentAddress;
                ETMInstruction inst1 = base.StateData.FetchInstruction( address1 );
                TraceAtom( aHeader.Atom1Type, inst1 );
                if ( aHeader.Atom1Type == ETMPcktPHeaderBase.TAtomType.EAtomE_Passed )
                {
                    bool branched = CheckForBranch( inst1 );
                    if ( !branched )
                    {
                        base.StateData.IncrementPC();
                    }
                }
                else
                {
                    base.StateData.IncrementPC();
                }
            }

            // Second instruction
            {
                uint address2 = base.StateData.CurrentAddress;
                ETMInstruction inst2 = base.StateData.FetchInstruction( address2 );
                TraceAtom( aHeader.Atom2Type, inst2 );
                if ( aHeader.Atom2Type == ETMPcktPHeaderBase.TAtomType.EAtomE_Passed )
                {
                    bool branched = CheckForBranch( inst2 );
                    if ( !branched )
                    {
                        base.StateData.IncrementPC();
                    }
                }
                else
                {
                    base.StateData.IncrementPC();
                }
            }

            // Spacer - to make the verbose output easier to read...
            base.Trace( string.Empty );
        }

        private bool HandleTHUMBMultiInstructionBranch( uint aInstruction1, uint aInstruction2 )
        {
            bool branched = false;

            // THUMB multi branch with link exchange instruction - consumes 32 bits
            // of the instruction pipeline, i.e. instructions @ aIndex and aIndex+1 both
            // consumed.
            //
            // Check the signature is correct:
            //
            // NB: 
            //   The first Thumb instruction has H == 10 and supplies the high part of the 
            //   branch offset. This instruction sets up for the subroutine call and is 
            //   shared between the BL and BLX forms.
            //
            //   The second Thumb instruction has H == 11 (for BL) or H == 01 (for BLX). It 
            //   supplies the low part of the branch offset and causes the subroutine call 
            //   to take place.
            //
            //  15   14   13   12 11   10                        0
            // ----------------------------------------------------  
            //  1    1    1      H     <---     offset_11      -->
            //
            // mask1  =      11 00000000000
            // value1 =      10 00000000000
            //           111 11 10111010101
            // mask2  =       1 00000000000
            // value2 =       1 00000000000
            //           111 10 11111111111
            bool inst1Valid = ( ( aInstruction1 & 0x1800 ) == 0x1000 );
            bool inst2Valid = ( ( aInstruction2 & 0x0800 ) == 0x0800 );

            if ( inst1Valid && inst2Valid )
            {
                TArmInstructionSet newInstructionSet = TArmInstructionSet.ETHUMB;
                TMultiPartThumbBranch branchType = TMultiPartThumbBranch.ETHUMBBL;
                if ( ( aInstruction2 & 0x1800 ) == 0x0800 )
                {
                    branchType = TMultiPartThumbBranch.ETHUMBBLX;
                    newInstructionSet = TArmInstructionSet.EARM;
                }

                // We subtract two, because we're already handling the second instruction
                // and the address is relative to the first.
                uint address = base.StateData.CurrentAddress - 2;

                // 111 10 00000000100
                //    100000000000000
                // 
                int instImmediate1 = SignExtend11BitTo32BitTHUMB( aInstruction1 & 0x7FF, 12 );
                address = (uint) ( address + instImmediate1 );
                address += 4;

                // 111 01 11101011010
                //        11111111111 (0x7FF)
                //        11101011010 = 0x75A * 2 = EB4
                //
                // 111 01 11011100010
                //        11011100010 = 0X6E2 * 2 = DC4
                // 
                // 

                // Second instruction
                uint instImmediate2 = ( aInstruction2 & 0x7FF ) * 2;
                address += instImmediate2;

                // For BLX, the resulting address is forced to be word-aligned by 
                // clearing bit[1].
                if ( branchType == TMultiPartThumbBranch.ETHUMBBLX )
                {
                    address = address & 0xFFFFFFFD;
                }

                base.StateData.SetPC( address, newInstructionSet );
                branched = true;
            }
            else
            {
                // Oops. We ran out of instructions. This shouldn't ever happen
                // assuming that the P-HEADER's are synched properly.
                throw new ETMException( "ERROR - synchronisation lost with P-HEADERS - 2nd THUMB BLX instruction missing" );
            }

            return branched;
        }

        private bool CheckForBranch( ETMInstruction aInstruction )
        {
            bool branched = false;
            TArmInstructionSet originalInstructionSet = base.StateData.CurrentInstructionSet;
            SymAddress originalAddress = new SymAddress( base.StateData.CurrentAddress.Address );
            //
            if ( base.StateData.LastBranch.IsKnown )
            {
                uint address = base.StateData.CurrentAddress;
                TArmInstructionSet instructionSet = base.StateData.CurrentInstructionSet;
                //
                if ( instructionSet == TArmInstructionSet.EARM )
                {
                    if ( iBranchMask_ARM_BOrBL.IsMatch( aInstruction ) )
                    {
                        // 1110 101 0 111111111111111111111101
                        int offset = SignExtend24BitTo32BitARM( aInstruction & 0x00FFFFFF );
                        base.StateData.SetPC( (uint) ( address + offset ) );
                        branched = true;
                    }
                    else if ( iBranchMask_ARM_BLX_BranchToThumb.IsMatch( aInstruction ) )
                    {
                        // TODO: verify this - no data to test at the moment
                        int offset = SignExtend24BitTo32BitARM( aInstruction & 0x00FFFFFF );
                        base.StateData.SetPC( (uint) ( address + offset ), TArmInstructionSet.ETHUMB );
                        branched = true;
                    }
                }
                else if ( instructionSet == TArmInstructionSet.ETHUMB )
                {
                    if ( iBranchMask_THUMB_B1.IsMatch( aInstruction ) )
                    {
                        //  15 14 13 12   11 -> 8    7    ->      0
                        // -----------------------------------------
                        //   1  1  0  1     cond     signed_immed_8
                        int offset = SignExtend8BitTo32BitTHUMB( aInstruction & 0xFF );
                        base.StateData.SetPC( (uint) ( address + offset ) );
                        branched = true;
                    }
                    else if ( iBranchMask_THUMB_B2.IsMatch( aInstruction ) )
                    {
                        //  15 14 13 12 11   10        ->         0
                        // -----------------------------------------
                        //   1  1  0  1  1       signed_immed_11
                        int offset = SignExtend11BitTo32BitTHUMB( aInstruction & 0x7FF );
                        base.StateData.SetPC( (uint) ( address + offset ) );
                        branched = true;
                    }
                    else
                    {
                        ETMInstruction inst1 = base.StateData.LastInstruction;
                        bool inst1Match = iBranchMask_THUMB_BLX_Part1.IsMatch( inst1.AIRawValue );
                        ETMInstruction inst2 = aInstruction;
                        bool inst2Match = iBranchMask_THUMB_BLX_Part2.IsMatch( inst2.AIRawValue );
                        //
                        if ( inst1Match && inst2Match )
                        {
                            branched = HandleTHUMBMultiInstructionBranch( inst1.AIRawValue, inst2.AIRawValue );
                            System.Diagnostics.Debug.Assert( branched == true );
                        }
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            if ( branched )
            {
                base.StateData.IncrementProcessedInstructionCounter();
                TraceDirectBranch( originalAddress, originalInstructionSet, base.StateData.CurrentAddress, base.StateData.CurrentInstructionSet );
            }

            // Always cache the last processed instruction
            base.StateData.LastInstruction = aInstruction;
            return branched;
        }

        private void TraceDirectBranch( SymAddress aOriginalAddress, TArmInstructionSet aOriginalISet, SymAddress aNewAddress, TArmInstructionSet aNewISet )
        {
            StringBuilder lines = new StringBuilder();
            lines.AppendLine( "   BRANCH-D" );
            //
            if ( base.StateData.LastBranch.IsKnown )
            {
                lines.AppendLine( string.Format( "       from: {0} 0x{1:x8} {2}", ETMDecodeState.MakeInstructionSetPrefix( aOriginalISet ), aOriginalAddress, StateData.Engine.LookUpSymbol( aOriginalAddress ) ) );
                lines.AppendLine( string.Format( "         to: {0} 0x{1:x8} {2}", ETMDecodeState.MakeInstructionSetPrefix( aNewISet ), aNewAddress, StateData.Engine.LookUpSymbol( aNewAddress ) ) );
            }
            else
            {
            }
            //
            base.Trace( lines.ToString() );
        }

        #region Utilities
        private static string TraceAtomTypeName( ETMPcktPHeaderBase.TAtomType aType )
        {
            switch ( aType )
            {
            default:
            case ETMPcktPHeaderBase.TAtomType.EAtomNotApplicable:
                return "        ";
            case ETMPcktPHeaderBase.TAtomType.EAtomE_Passed:
                return "  PASS  ";
            case ETMPcktPHeaderBase.TAtomType.EAtomN_Failed:
                return "* FAIL *";
            case ETMPcktPHeaderBase.TAtomType.EAtomW_CycleBoundary:
                return " CYBNDR ";
            }
        }

        private void TraceAtom( ETMPcktPHeaderBase.TAtomType aType, ETMInstruction aInstruction )
        {
            string atomTypeName = TraceAtomTypeName( aType );
            //
            StringBuilder text = new StringBuilder();
            text.AppendFormat( "{0}   0x{1}:    {2}         {3}",
                base.StateData.NumberOfProcessedInstructions.ToString().PadLeft( 6, ' ' ),
                base.StateData.CurrentAddress.AddressHex,
                base.StateData.CurrentAddress.AddressBinary,
                atomTypeName
                );
            //
            if ( base.StateData.LastBranch.IsKnown )
            {
                string disasm = aInstruction.ToString();
                text.Append( "    " + disasm );
            }
            //
            base.Trace( text.ToString() );
        }

        private static int SignExtend24BitTo32BitARM( uint aImmediate )
        {
            int offset;
            //
            unchecked
            {
                if ( ( aImmediate & 0x00800000 ) == 0x00800000 )
                {
                    offset = (int) ( 0xff000000 | aImmediate );
                }
                else
                {
                    offset = (int) aImmediate;
                }
            }
            //
            offset <<= 2;
            offset += 8; // pipeline
            return offset;
        }

        private static int SignExtend11BitTo32BitTHUMB( uint aImmediate )
        {
            int offset = SignExtend11BitTo32BitTHUMB( aImmediate, 1 );
            offset += 4; // pipeline
            return offset;
        }

        private static int SignExtend11BitTo32BitTHUMB( uint aImmediate, int aLeftShiftCount )
        {
            int offset;
            //
            unchecked
            {
                //  10  9  8  7  6  5  4  3  2  1  0
                // ----------------------------------
                //   1  0  0  0  0  0  0  0  0  0  0
                if ( ( aImmediate & 0x00000400 ) == 0x00000400 )
                {
                    // 11111111111111111111100000000000
                    //                      10000000000
                    offset = (int) ( 0xFFFFF800 | aImmediate );
                }
                else
                {
                    offset = (int) aImmediate;
                }
            }
            //
            offset <<= aLeftShiftCount;
            return offset;
        }

        private static int SignExtend8BitTo32BitTHUMB( uint aImmediate )
        {
            int offset; 
            //
            unchecked
            {
                //  7  6  5  4  3  2  1  0
                // ------------------------
                //  1  0  0  0  0  0  0  0
                if ( ( aImmediate & 0x00000080 ) == 0x00000080 )
                {
                    // 11111111111111111111111100000000
                    //                         10000000
                    offset = (int) ( 0xFFFFFF00 | aImmediate );
                }
                else
                {
                    offset = (int) aImmediate;
                }
            }
            //
            offset <<= 1;
            offset += 4; // pipeline
            return offset;
        }
        #endregion

        #endregion

        #region From System.Object
        #endregion

        #region Data members
        private static readonly SymMask iBranchMask_ARM_BOrBL;
        private static readonly SymMask iBranchMask_ARM_BLX_BranchToThumb;
        private static readonly SymMask iBranchMask_THUMB_B1;
        private static readonly SymMask iBranchMask_THUMB_B2;
        private static readonly SymMask iBranchMask_THUMB_BLX_Part1;
        private static readonly SymMask iBranchMask_THUMB_BLX_Part2;
        #endregion
    }
}
