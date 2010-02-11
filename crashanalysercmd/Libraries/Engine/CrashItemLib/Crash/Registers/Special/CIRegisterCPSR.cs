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
using System.Text;
using System.Collections.Generic;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Messages;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Threads;
using SymbianUtils.Range;
using SymbianStructuresLib.Arm.Registers;
using CrashItemLib.Crash.Registers.Visualization;
using CrashItemLib.Crash.Registers.Visualization.Bits;
using CrashItemLib.Crash.Registers.Visualization.Utilities;

namespace CrashItemLib.Crash.Registers.Special
{
    public class CIRegisterCPSR : CIRegister, ICIRegisterVisualizerVisitor
	{
		#region Constructors
        public CIRegisterCPSR( CIRegisterList aCollection, uint aValue )
            : base( aCollection, TArmRegisterType.EArmReg_CPSR, aValue )
        {
            PrepareMessage();
            //
            CIRegisterVisualization modeARMv5 = new CIRegisterVisualization( this, this, KVisMode_ARMv5 );
            this.AddChild( modeARMv5 );
            modeARMv5.Refresh();
            //
            CIRegisterVisualization modeARMv6 = new CIRegisterVisualization( this, this, KVisMode_ARMv6 );
            this.AddChild( modeARMv6 );
            modeARMv6.Refresh();
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public TArmRegisterBank ProcessorMode
        {
            get
            {
                TArmRegisterBank bank = ArmRegisterBankUtils.ExtractBank( this.Value );
                return bank;
            }
        }
        #endregion

        #region Internal constants
        private const string KVisMode_ARMv5 = "CPSR (ARMv5)";
        private const string KVisMode_ARMv6 = "CPSR (ARMv6)";
        #endregion

        #region ICIRegisterVisualizerVisitor Members
        void ICIRegisterVisualizerVisitor.Build( CIRegisterVisualization aVisualization )
        {
            aVisualization.Clear();
            //
            switch ( aVisualization.Description )
            {
            default:
            case KVisMode_ARMv5:
                PrepareVisARMv5( aVisualization );
                break;
            case KVisMode_ARMv6:
                PrepareVisARMv6( aVisualization );
                break;
            }
        }
        #endregion

        #region Internal constants
        private const string KReserved = "Reserved";
        #endregion

        #region Internal methods
        private void PrepareMessage()
        {
            CIMessage message = CIMessage.NewMessage( Container );
            message.Title = "Processor Mode";
            message.Description = string.Format( "CPSR (Current Processor Status Register) indicates that the processor was in [{0}] mode.",
                                         ArmRegisterBankUtils.BankAsStringLong( ProcessorMode ) );
            base.AddChild( message );
        }

        private void PrepareVisARMv5( CIRegisterVisualization aVisualization )
        {
            AddBits0To8( aVisualization, true );
            AddEndianness( aVisualization, true );
           
            // These bits are always reserved - "########", "########", "111111##", "########"
            AddReserved( aVisualization, 10, 15 );

            // GE[3:0] bits - reserved in ARMv5 - "########", "####1111", "########", "########"
            AddReserved( aVisualization, 16, 19 );
           
            // These bits are always reserved - "########", "1111####", "########", "########"
            AddReserved( aVisualization, 20, 23 );

            // Jazelle bit
            AddJazelle( aVisualization, true );

            // These bits are always reserved - "#####11#", "########", "########", "########"
            AddReserved( aVisualization, 25, 26 );

            // Reserved bit - Q flag, ARMv5E only. Indicates "overflow and/or saturation has occurred..."
            AddQFlag( aVisualization, true );

            // Condition code flags
            AddConditionCodeFlags( aVisualization );
        }

        private void PrepareVisARMv6( CIRegisterVisualization aVisualization )
        {
            AddBits0To8( aVisualization, false );
            AddEndianness( aVisualization, false );

            // These bits are always reserved - "########", "########", "111111##", "########"
            AddReserved( aVisualization, 10, 15 );

            // GE[3:0] bits - reserved in ARMv5 - "########", "####1111", "########", "########"
            AddBitRange( aVisualization, 16, 19, "Greater than or Equal (SIMD)" );

            // These bits are always reserved - "########", "1111####", "########", "########"
            AddReserved( aVisualization, 20, 23 );

            // Jazelle bit
            AddJazelle( aVisualization, false );

            // These bits are always reserved - "#####11#", "########", "########", "########"
            AddReserved( aVisualization, 25, 26 );

            // Reserved bit - Q flag, ARMv5E only. Indicates "overflow and/or saturation has occurred..."
            AddQFlag( aVisualization, false );

            // Condition code flags
            AddConditionCodeFlags( aVisualization );
        }

        private void AddBits0To8( CIRegisterVisualization aVisualization, bool aIsIABitReserved )
        {
            uint value = aVisualization.Register.Value;

            // Processor mode
            CIRegisterVisBitRange rangeProcMode = new CIRegisterVisBitRange( Container, 0, 4, "Processor Mode" );
            rangeProcMode.Interpretation = ArmRegisterBankUtils.BankAsStringLong( ProcessorMode );
            rangeProcMode.ExtractBits( value, "########", "########", "########", "###11111" );
            aVisualization.AddChild( rangeProcMode );

            // Thumb bit
            CIRegisterVisBit thumbBit = CreateYesNoBit( aVisualization, 5, "########", "########", "########", "##1#####", false, "Thumb Mode", "T" );
            aVisualization.AddChild( thumbBit );

            // FIQ, IRQ bits
            CIRegisterVisBitGroup gpIRQs = new CIRegisterVisBitGroup( Container, "Interrupt Disabled Bits" );
            CIRegisterVisBit fiqBit = CreateYesNoBit( aVisualization, 6, "########", "########", "########", "#1######", false, "FIQ Disabled", "F" );
            gpIRQs.Add( fiqBit );
            CIRegisterVisBit irqBit = CreateYesNoBit( aVisualization, 7, "########", "########", "########", "1#######", false, "IRQ Disabled", "I" );
            gpIRQs.Add( irqBit );

            // Imprecise Abort bit - reserved in non-ARMv5
            CIRegisterVisBit iaBit = CreateYesNoBit( aVisualization, 8, "########", "########", "#######1", "########", aIsIABitReserved, "Imprecise Aborts", "A" );
            gpIRQs.Add( iaBit );

            aVisualization.AddChild( gpIRQs );
        }

        private void AddEndianness( CIRegisterVisualization aVisualization, bool aIsReserved )
        {
            uint value = aVisualization.Register.Value;

            CIRegisterVisBit eBit = CreateBitValue( aVisualization, 9, "###1####", "########", "######1#", "########", aIsReserved, "Endianness", "Big", "Little", "B", "L" );
            aVisualization.AddChild( eBit );
        }

        private void AddJazelle( CIRegisterVisualization aVisualization, bool aIsReserved )
        {
            CIRegisterVisBit bit = CreateYesNoBit( aVisualization, 24, "#######1", "########", "########", "########", aIsReserved, "Jazelle", "J" );
            aVisualization.AddChild( bit );
        }

        private void AddQFlag( CIRegisterVisualization aVisualization, bool aIsReserved )
        {
            CIRegisterVisBit bit = CreateYesNoBit( aVisualization, 27, "####1###", "########", "########", "########", aIsReserved, "Q-Flag (DSP Saturation/Overflow)", "Q" );
            aVisualization.AddChild( bit );
        }

        private void AddConditionCodeFlags( CIRegisterVisualization aVisualization )
        {
            uint value = aVisualization.Register.Value;

            CIRegisterVisBitGroup group = new CIRegisterVisBitGroup( Container );

            CIRegisterVisBit oBit = CreateYesNoBit( aVisualization, 28, "###1####", "########", "########", "########", false, "Overflow (Condition Code)", "O" );
            group.Add( oBit );
            CIRegisterVisBit cBit = CreateYesNoBit( aVisualization, 29, "##1#####", "########", "########", "########", false, "Carry (Condition Code)", "C" );
            group.Add( cBit );
            CIRegisterVisBit zBit = CreateYesNoBit( aVisualization, 30, "#1######", "########", "########", "########", false, "Zero (Condition Code)", "Z" );
            group.Add( zBit );
            CIRegisterVisBit nBit = CreateYesNoBit( aVisualization, 31, "1#######", "########", "########", "########", false, "Negative (Condition Code)", "N" );
            group.Add( nBit );

            aVisualization.AddChild( group );
        }

        private CIRegisterVisBitRange AddBitRange( CIRegisterVisualization aVisualization, uint aStart, uint aEnd, string aCategory )
        {
            // Make the mask
            AddressRange range = new AddressRange( aStart, aEnd );
            StringBuilder mask = new StringBuilder();
            mask.Append( string.Empty.PadLeft( 32, '#' ) );
            for ( int i=31; i>=0; i-- )
            {
                if ( range.Contains( i ) )
                {
                    int index = 31 - i;
                    mask[ index ] = '1';
                }
            }

            // Reserved
            uint value = aVisualization.Register.Value;

            CIRegisterVisBitRange bitRange = new CIRegisterVisBitRange( Container, aStart, aEnd, aCategory );
            bitRange.ExtractBits( value, mask.ToString() );
            aVisualization.AddChild( bitRange );

            return bitRange;
        }

        private void AddReserved( CIRegisterVisualization aVisualization, uint aStart, uint aEnd )
        {
            CIRegisterVisBitRange range = AddBitRange( aVisualization, aStart, aEnd, KReserved );
            range.IsReserved = true;
        }

        private CIRegisterVisBit CreateBitValue( CIRegisterVisualization aVisualization, int aIndex,
            string aMaskByte3, string aMaskByte2, string aMaskByte1, string aMaskByte0,
            bool aReserved, string aCategory, string aInterpretationSet, string aInterpretationClear, string aBitSetCharacter )
        {
            CIRegisterVisBit bit = CreateBitValue( aVisualization, aIndex, aMaskByte3, aMaskByte2, aMaskByte1, aMaskByte0, aReserved, aCategory,aInterpretationSet, aInterpretationClear, aBitSetCharacter, string.Empty );
            return bit;
        }

        private CIRegisterVisBit CreateBitValue( CIRegisterVisualization aVisualization, int aIndex, 
            string aMaskByte3, string aMaskByte2, string aMaskByte1, string aMaskByte0,
            bool aReserved, string aCategory, string aInterpretationSet, string aInterpretationClear, string aBitSetCharacter, string aBitClearCharacter )
        {
            uint value = aVisualization.Register.Value;
            //
            CIRegisterVisBit bit = new CIRegisterVisBit( Container, aIndex, VisUtilities.ExtractBit( value, aMaskByte3, aMaskByte2, aMaskByte1, aMaskByte0 ), aCategory, string.Empty );
            bit.IsReserved = aReserved;
            bit[ TBit.EBitClear ] = aBitClearCharacter;
            bit[ TBit.EBitSet ] = aBitSetCharacter;
            //
            switch ( bit.Value )
            {
            case TBit.EBitSet:
                bit.Interpretation = aInterpretationSet;
                break;
            default:
            case TBit.EBitClear:
                bit.Interpretation = aInterpretationClear;
                break;
            }
            //
            return bit;
        }

        private CIRegisterVisBit CreateYesNoBit( CIRegisterVisualization aVisualization, int aIndex, string aMaskByte3, string aMaskByte2, string aMaskByte1, string aMaskByte0, bool aReserved, string aCategory, string aBitSetCharacter )
        {
            CIRegisterVisBit bit = CreateBitValue( aVisualization, aIndex, aMaskByte3, aMaskByte2, aMaskByte1, aMaskByte0, aReserved, aCategory, "Yes", "No", aBitSetCharacter );
            return bit;
        }
        #endregion
            
        #region Data members
        #endregion
    }
}
