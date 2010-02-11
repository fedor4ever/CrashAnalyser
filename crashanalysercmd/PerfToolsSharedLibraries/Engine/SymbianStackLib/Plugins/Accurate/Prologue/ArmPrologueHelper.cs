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
using SymbianDebugLib.PluginAPI.Types.Symbol;
using SymbianStructuresLib.Debug.Symbols;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Registers;
using SymbianStructuresLib.Arm.Registers;
using SymbianStackAlgorithmAccurate.CPU;
using SymbianStackAlgorithmAccurate.Code;
using SymbianStackAlgorithmAccurate.Engine;
using SymbianStackAlgorithmAccurate.Interfaces;
using SymbianStackAlgorithmAccurate.Instructions;

namespace SymbianStackAlgorithmAccurate.Prologue
{
    internal class ArmPrologueHelper
    {
        #region Constructors
        public ArmPrologueHelper( AccurateEngine aEngine )
        {
            iEngine = aEngine;

            // Make a new PC register, since we're going to manipulate it...
            iPC = new ArmRegister( aEngine.CPU.PC );

            // Create offsets
            iOffsetValues.AddDefaults();
            iOffsetValues.SetAll( uint.MaxValue );
        }
        #endregion

        #region API
        public void Build()
        {
            // First, work out how many instructions we need to read from
            // the code data in order to reach the current PC value.
            // We currently cap this at 20 instructions.
            CalculatePrologueInstructionCount();

            // Get Prologue instructions
            GetPrologueInstructions();

            // Update iPC with Prologue starting address - needed for PC-relative
            // instructions
            PrepareInitialPCValue();

            // Process the instructions until exhausted
            ProcessPrologueInstructions();
        }

        public int IncrementNumberOfWordsPushedOnStack( TArmRegisterType aRegister )
        {
            uint offset = (uint) iNumberOfWordsPushedOnStack * 4;
            int ret = iNumberOfWordsPushedOnStack++;
            iEngine.Trace( "[PLG] IncrementNumberOfWordsPushedOnStack - register: {0}, offset: 0x{1:x4}, DWORDs now on stack: {2:d2}", aRegister, offset, NumberOfWordsPushedOnStack );
            return ret;
        }

        public int AddToNumberOfWordsPushedOnStack( int aExtraWords )
        {
            iNumberOfWordsPushedOnStack += aExtraWords;
            iEngine.Trace( "[PLG] AddToNumberOfWordsPushedOnStack     - DWORDs added: {0}, prior SP adjustment: 0x{1:x8} ({2} x DWORDs), new SP adjustment: 0x{3:x8} ({4} x DWORDs)",
                aExtraWords, 
                ( iNumberOfWordsPushedOnStack - aExtraWords ) * 4, 
                iNumberOfWordsPushedOnStack - aExtraWords,
                iNumberOfWordsPushedOnStack * 4,
                iNumberOfWordsPushedOnStack  
            );
            //
            return iNumberOfWordsPushedOnStack;
        }
        #endregion

        #region Properties
        public int PrologueInstructionCount
        {
            get
            { 
                int ret = iPrologueInstructionCount;
                //
                if ( ret > KMaxPrologueInstructionCount )
                {
                    ret = KMaxPrologueInstructionCount;
                    iEngine.Trace( "[PLG] Capping the amount of Prologue instructions to read to: " + ret ); 
                }
                //
                return ret;
            }
            set
            {
                iPrologueInstructionCount = value;
            }
        }

        public string FunctionName
        {
            get { return iFunctionName; }
        }

        public TArmInstructionSet FunctionInstructionSet
        {
            get
            {
                TArmInstructionSet ret = TArmInstructionSet.EARM;
                //
                if ( ( FunctionStartingAddress & 0x1 ) == 0x1 )
                {
                    ret = TArmInstructionSet.ETHUMB;
                }
                //
                return ret;
            }
        }

        public uint FunctionStartingAddress
        {
            get { return iFunctionStartAddress; }
        }

        public uint FunctionStartingAddressWithoutType
        {
            get { return iFunctionStartAddress & KInstructionSetMask; }
        }

        public uint FunctionOffsetToPC
        {
            get
            {
                uint funcAddrWithoutInstructionSetType = FunctionStartingAddressWithoutType;
                uint offset = iPC - funcAddrWithoutInstructionSetType;
                return offset;
            }
        }

        public ArmRegister ProloguePC
        {
            get { return iPC; }
        }

        public int NumberOfWordsPushedOnStack
        {
            get { return iNumberOfWordsPushedOnStack; }
            set { iNumberOfWordsPushedOnStack = value; }
        }
        #endregion

        #region Internal methods
        private void CalculatePrologueInstructionCount()
        {
            DbgViewSymbol symbolView = iEngine.DebugEngineView.Symbols;

            // Get the PC and try to match it to a function
            SymbolCollection collection = null;
            Symbol symbol = symbolView.Lookup( iPC, out collection );
            if ( symbol != null )
            {
                iFunctionStartAddress = symbol.Address;
                iFunctionName = symbol.Name;
                //
                uint offset = FunctionOffsetToPC;
                uint instructionSize = SingleInstructionSize;
                uint prologueInstructionCount = ( offset / instructionSize );
                //
                iEngine.Trace( "[PLG] Prologue function: 0x{0:x8} = {1} [+{2:x4}], {3} instructions", iPC.Value, iFunctionName, offset, PrologueInstructionCount );
                PrologueInstructionCount = (int) prologueInstructionCount;
            }
            else
            {
                // We could not locate the symbol for the corresponding program counter address.
                // In this situation, there's nothing we can do - if we cannot work out the offset
                // within the function, then we cannot identify how many Prologue instructions to 
                // attempt to read.
                //
                // If the symbol was not found because no code segment claims ownership of this address
                // then that might indicate premature dll unload or bad crash data (missing code segments)
                if ( collection == null )
                {
                    throw new APESymbolNotFoundCodeSegmentUnavailable( iPC );
                }
                else
                {
                    throw new APESymbolNotFound( iPC, string.Format( "Code segment \'{0}\' should describe symbol, but none was found for requested program counter address", collection.FileName ) );
                }
            }
        }

        private void GetPrologueInstructions()
        {
            TArmInstructionSet instSet = CPU.CurrentProcessorMode;
            uint address = FunctionStartingAddressWithoutType;

            // Let's get unadulterated instruction counts
            int instCount = iPrologueInstructionCount;
            if ( address > 0 && instCount > 0 )
            {
                iInstructions = CodeHelper.LoadInstructions( address, instCount, instSet );
            }
            else
            {
                iInstructions = new AccInstructionList();
            }

            // Verify that we have the expected number of instructions.
            // If, for some reason, the code provider does not supply
            // any Prologue instructions, then we should bail out.
            int actual = iInstructions.Count;
            if ( actual != instCount )
            {
                throw new Exception( string.Format( "Prologue instructions unavailable or insufficient @ address: 0x{0:x8} - expected: {1}, received: {2}", FunctionStartingAddressWithoutType, instCount, actual ) );
            }

            // Since we fetch all the instructions from a function (leading up to the current address)
            // we may have lots more instructions that we'd ideally normally expect to see form part
            // of the function prologue. Normally, we cap the prologue instruction count at ~19 instructions,
            // so therefore we should disable any instructions beyond this maximum.
            for ( int i = KMaxPrologueInstructionCount - 1; i < iInstructions.Count; i++ )
            {
                iInstructions[ i ].Ignored = true;
            }

            // Run the instructions through the pre-filter. We tell the 
            // instruction list how many instructions through the current function
            // we are because this helps to identify whether a branch has been
            // executed as the last instruction, or whether we artificially limited
            // the preamble, in which case the branch was "probably" not taken.
            iInstructions.Prefilter( iPrologueInstructionCount );
            iInstructions.DebugPrint( iEngine as ITracer );
        }

        private void PrepareInitialPCValue()
        {
            // Update the program counter so that we skip past the start of
            // the function. According to Tom G, this is two instructions past
            // the function entry address
            uint newPC = FunctionStartingAddress;
            newPC += (uint) ( 2 * SingleInstructionSize );

            // Zero the non-address bits for sanity
            uint clearBitMask = (uint) ( SingleInstructionSize - 1 );
            newPC &= ~clearBitMask;
            iPC.Value = newPC;

            string sym = iEngine.DebugEngineView.Symbols.PlainText[ iPC.Value ];
            iEngine.Trace( "[PLG] PrepareInitialPCValue - new PC value: 0x{0:x8} = {1}", iPC.Value, sym );
        }

        private void ProcessPrologueInstructions()
        {
            uint sp = iEngine.CPU[ TArmRegisterType.EArmReg_SP ];
            iEngine.Trace( "[PLG] ProcessPrologueInstructions - initial PC: 0x{0:x8}, SP: 0x{1:x8}", iPC.Value, sp );

            // We've got the necessary instructions so continue as normal...
            int actual = iInstructions.Count;
            while ( actual > 0 )
            {
                // Get instruction
                AccInstruction inst = iInstructions.Deque();

                // Don't process any ignored instructions
                if ( inst.Ignored == false )
                {
                    iEngine.Trace( "[PLG] ProcessPrologueInstructions - PC: 0x{0:x8}, SP: 0x{1:x8}, I: {2}", iPC.Value, sp + ( iNumberOfWordsPushedOnStack * 4 ), inst.ToString() );
                    iEngine.SetIndent( 1 );
 
                    // Process it to update offsets & register values
                    inst.Process( this );

                    // Update Prologue program counter value
                    iPC.Value += SingleInstructionSize;

                    // Finished with indentation
                    iEngine.SetIndent( 0 );
                }

                // Update count
                actual = iInstructions.Count;
            }
        }
        #endregion

        #region Internal properties
        internal ArmCpu CPU
        {
            get { return iEngine.CPU; }
        }

        internal ArmCodeHelper CodeHelper
        {
            get { return iEngine.CodeHelper; }
        }

        internal ArmRegisterCollection OffsetValues
        {
            get { return iOffsetValues; }
        }

        private uint SingleInstructionSize
        {
            get
            {
                uint size = ArmCpuUtils.InstructionSize( CPU.CurrentProcessorMode );
                return size;
            }
        }
        #endregion

        #region Internal constants
        private const int KMaxPrologueInstructionCount = 19;
        private const uint KInstructionSetMask = 0xFFFFFFFE;
        #endregion

        #region Data members
        private readonly AccurateEngine iEngine;
        private readonly ArmRegister iPC;
        private int iPrologueInstructionCount = 0;
        private string iFunctionName = string.Empty;
        private uint iFunctionStartAddress = 0;
        private int iNumberOfWordsPushedOnStack = 0;
        private AccInstructionList iInstructions = new AccInstructionList();
        private ArmRegisterCollection iOffsetValues = new ArmRegisterCollection();
        #endregion
    }
}
