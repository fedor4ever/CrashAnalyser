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
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI.Types.Symbol;
using SymbianStackAlgorithmAccurate.Code;
using SymbianStackAlgorithmAccurate.CPU;
using SymbianStackAlgorithmAccurate.Interfaces;
using SymbianStackAlgorithmAccurate.Prologue;
using SymbianStackAlgorithmAccurate.Stack;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Registers;
using SymbianUtils;
using SymbianUtils.Tracer;

namespace SymbianStackAlgorithmAccurate.Engine
{
    internal class AccurateEngine : DisposableObject, ITracer
    {
        #region Constructors
        public AccurateEngine( DbgEngineView aDebugEngineView, IArmStackInterface aStackInterface, ITracer aTracer )
        {
            iTracer = aTracer;
            iDebugEngineView = aDebugEngineView;
            iStackInterface = aStackInterface;
            iCodeHelper = new ArmCodeHelper( aDebugEngineView, aTracer );
        }
        #endregion

        #region API
        public bool Process()
        {
            // We need SP, LR, PC, CPSR
            CheckRequiredRegistersAvailable();

            // Debug info
            PrintInitialInfo();

            // Make initial stack frames for seed registers
            MakeInitialSeedStackFramesFromRegisterValues();

            // Get sp
            ArmRegister sp = CPU[ TArmRegisterType.EArmReg_SP ];
            uint initialSPValue = sp.Value;

            // Create Prologue object that will establish the instructions for the
            // function and also identify operations that might affect SP and LR.
            ArmPrologueHelper Prologue = new ArmPrologueHelper( this );
            Prologue.Build();
            
            // Create a new stack frame for this function call
            ArmStackFrame stackFrame = new ArmStackFrame( Prologue );

            // We save the stack address which contained the popped link register
            // during the previous cycle. If possible, use that value. If it
            // hasn't been set, then assume we obtained the link register from the
            // previous 4 bytes of stack (according to the current value of SP).
            long stackAddressAssociatedWithCurrentFrame = iLastLinkRegisterStackAddress;
            if ( stackAddressAssociatedWithCurrentFrame == KLinkRegisterWasNotPushedOnStack )
            {
                // We're always four bytes behind the current SP
                stackAddressAssociatedWithCurrentFrame = sp - 4;
            }
            stackFrame.Address = (uint) stackAddressAssociatedWithCurrentFrame;
            stackFrame.Data = iStackInterface.StackValueAtAddress( stackFrame.Address );

            Trace( "Creating Stack Frame [" + stackFrame.Address.ToString( "x8" ) + "] = 0x" + stackFrame.Data.ToString( "x8" ) + " = " + SymbolString ( stackFrame.Data ) );

            // Can now adjust stack pointer based upon the number of stack-adjusting
            // instructions during the Prologue phase.
            uint stackAdjustment = (uint) ( Prologue.NumberOfWordsPushedOnStack * 4 );
            sp.Value += stackAdjustment;
            Trace( "stack adjusted by: 0x" + stackAdjustment.ToString( "x8" ) );

            // We're hoping that the link register was pushed on the stack somewhere
            // during the function preamble. If that was the case, then as we processed
            // each instruction, we'll have updated the register offsets so that we know
            // the offset to the link register from the perspective of the starting stack
            // address for the function.
            uint lrOffsetInWords = Prologue.OffsetValues[ TArmRegisterType.EArmReg_LR ];
            Trace( string.Format( "LR offset on stack is: 0x{0:x8}", lrOffsetInWords * 4 ) );
            GetNewLRValue( lrOffsetInWords, Prologue );

            // Update the PC to point to the new function address (which we obtain
            // from LR)
            uint oldPCValue = CPU.PC;
            ChangePCToLRAddress();
            uint newPCValue = CPU.PC;
            Trace( string.Format( "oldPCValue: 0x{0:x8}, newPCValue: 0x{1:x8}, fn: {2}", oldPCValue, newPCValue, SymbolViewText[ newPCValue ] ) );

            // Decide if we are in thumb or ARM mode after switching functions
            UpdateInstructionSet( newPCValue );

            // Return true if we moved to a new function
            bool gotNewFunction = ( oldPCValue != CPU.PC );
            Trace( "gotNewFunction: " + gotNewFunction );

            // Save stack frame
            SaveStackFrames( stackFrame );

            // Increment iteration
            ++iIterationNumber;

            // Do we have more to do?
            bool moreToDo = gotNewFunction && ( CPU.PC > 0 );

            // Done
            Trace( "moreToDo: " + moreToDo );
            return moreToDo;
        }
        #endregion

        #region Properties
        public ArmCpu CPU
        {
            get { return iCPU; }
        }

        public ArmStackFrame[] StackFrames
        {
            get
            {
                return iStackFrames.ToArray();
            }
        }

        internal ArmCodeHelper CodeHelper
        {
            get { return iCodeHelper; }
        }

        internal DbgEngineView DebugEngineView
        {
            get { return iDebugEngineView; }
        }

        internal DbgViewSymbol SymbolView
        {
            get { return DebugEngineView.Symbols; }
        }

        internal DbgSymbolViewText SymbolViewText
        {
            get { return SymbolView.PlainText; }
        }
        #endregion

        #region Internal methods
        private void PrintInitialInfo()
        {
            ArmRegister sp = CPU[ TArmRegisterType.EArmReg_SP ];
            //
            ArmRegister lr = CPU[ TArmRegisterType.EArmReg_LR ];
            string lrSymbol = SymbolViewText[ lr ];
            //
            ArmRegister pc = CPU[ TArmRegisterType.EArmReg_PC ];
            string pcSymbol = SymbolViewText[ pc ];
            //
            ArmRegister cpsr = CPU[ TArmRegisterType.EArmReg_CPSR ];
            //
            Trace( System.Environment.NewLine );
            Trace( string.Format( "[{5:d2}] SP: 0x{0:x8}, LR: 0x{1:x8} [{2}], PC: 0x{3:x8} [{4}], isThumb: {6}", sp.Value, lr.Value, lrSymbol, pc.Value, pcSymbol, iIterationNumber, CPU.CurrentProcessorMode == TArmInstructionSet.ETHUMB ) );
        }

        private void CheckRequiredRegistersAvailable()
        {
            ArmRegisterCollection regs = CPU.Registers;

            // We need SP, LR, PC, CPSR
            bool sp = regs.Contains( TArmRegisterType.EArmReg_SP );
            bool lr = regs.Contains( TArmRegisterType.EArmReg_LR );
            bool pc = regs.Contains( TArmRegisterType.EArmReg_PC );
            bool cpsr = regs.Contains( TArmRegisterType.EArmReg_CPSR );
            //
            bool available = ( sp && lr && pc && cpsr );
            if ( !available )
            {
                SymbianUtils.SymDebug.SymDebugger.Break();
                throw new ArgumentException( "One or more registers is unavailable" );
            }
        }

        private void MakeInitialSeedStackFramesFromRegisterValues()
        {
            if ( !iAlreadySavedInitialRegisterFrames )
            {
                ArmRegisterCollection regs = CPU.Registers;

                // Make PC stack frame
                ArmStackFrame framePC = new ArmStackFrame( TArmRegisterType.EArmReg_PC );
                framePC.Data = regs[ TArmRegisterType.EArmReg_PC ].Value;

                // Make LR stack frame
                ArmStackFrame frameLR = new ArmStackFrame( TArmRegisterType.EArmReg_LR );
                frameLR.Data = regs[ TArmRegisterType.EArmReg_LR ].Value;

                // Save 'em
                SaveStackFrames( framePC, frameLR );

                // Don't do this again
                iAlreadySavedInitialRegisterFrames = true;
            }
        }

        private void GetNewLRValue( uint aLinkRegOffsetInWords, ArmPrologueHelper aPrologue )
        {
            uint sp = CPU[ TArmRegisterType.EArmReg_SP ];
            Trace( string.Format( "GetNewLRValue - stack DWORD offset to LR: 0x{0:x8}, sp: 0x{1:x8}", aLinkRegOffsetInWords, sp ) );

            // If the link register was pushed onto the stack, then get the new value
            // now...
            if ( aLinkRegOffsetInWords != uint.MaxValue )
            {
                ArmRegisterCollection regs = aPrologue.OffsetValues;
                foreach ( ArmRegister reg in regs )
                {
                    Trace( "GetNewLRValue - reg offsets - " + reg.ToString() );
                }

                long offsetOnStackToLR = aLinkRegOffsetInWords * 4;
                long stackBase = iStackInterface.StackBase;
                long stackOffsetToLR = sp - 4 - stackBase - offsetOnStackToLR;
                iLastLinkRegisterStackAddress = stackBase + stackOffsetToLR;
                uint newLRValue = iStackInterface.StackValueAtAddress( (uint) iLastLinkRegisterStackAddress );
                Trace( string.Format( "GetNewLRValue - Fetching LR from stack address: 0x{0:x8} (0x{1:x8})", iLastLinkRegisterStackAddress, newLRValue ) );

                uint temp;
                string sym;
                SymbolViewText.Lookup( newLRValue, out temp, out sym );
                Trace( "GetNewLRValue - LR changed to: 0x" + newLRValue.ToString( "x8" ) + " => [ " + sym + " ]" );

                CPU[ TArmRegisterType.EArmReg_LR ].Value = newLRValue;
            }
            else
            {
                iLastLinkRegisterStackAddress = KLinkRegisterWasNotPushedOnStack;
                Trace( "GetNewLRValue - LR not pushed on stack!" );
            }
        }

        private void UpdateInstructionSet( uint aPC )
        {
            uint isThumb = aPC & 0x1;
            if ( isThumb != 0 )
            {
                CPU.CurrentProcessorMode = TArmInstructionSet.ETHUMB;
            }
            else
            {
                CPU.CurrentProcessorMode = TArmInstructionSet.EARM;
            }

            // Twiddle the first bit to clear any possible non-address value
            CPU.PC.Value &= 0xFFFFFFFE;
        }

        private void ChangePCToLRAddress()
        {
            CPU.PC.Value = CPU.LR.Value;

            // Zero out the LR value as it has just been promoted to PC
            CPU.LR.Value = 0;
        }

        private void SaveStackFrames( params ArmStackFrame[] aFrames )
        {
            foreach ( ArmStackFrame frame in aFrames )
            {
                iStackFrames.Add( frame );
            }
        }

        private string SymbolString( uint aAddress )
        {
            // Used for debugging
            uint fnStartAddr = 0;
            string symbolName = string.Empty;
            SymbolViewText.Lookup( aAddress, out fnStartAddr, out symbolName );
            return symbolName;
        }
        #endregion

        #region Internal constants
        private const long KLinkRegisterWasNotPushedOnStack = -1;
        #endregion

        #region From ITracer
        public void Trace( string aMessage )
        {
            StringBuilder t = new StringBuilder();
            //
            string pad = string.Empty;
            //
            if ( iTraceIndentLevel != 0 )
            {
                pad = pad.PadLeft( iTraceIndentLevel * 4, ' ' );
            }
            //
            t.AppendFormat( "[AIE] {0}{1}", pad, aMessage );
            iTracer.Trace( t.ToString() );
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            string t = string.Format( aFormat, aParams );
            Trace( t );
        }

        internal void SetIndent( int aIndentLevel )
        {
            iTraceIndentLevel = aIndentLevel;
        }
        #endregion

        #region From DisposableObject
        protected override void CleanupManagedResources()
        {
            try
            {
                base.CleanupManagedResources();
            }
            finally
            {
                iStackFrames.Clear();
                iStackFrames = null;
            }
        }
        #endregion

        #region Data members
        private readonly ITracer iTracer;
        private readonly DbgEngineView iDebugEngineView;
        private readonly ArmCodeHelper iCodeHelper;
        private readonly IArmStackInterface iStackInterface;
        private long iLastLinkRegisterStackAddress = KLinkRegisterWasNotPushedOnStack;
        private int iTraceIndentLevel = 0;
        private int iIterationNumber = 0;
        private bool iAlreadySavedInitialRegisterFrames = false;
        private ArmCpu iCPU = new ArmCpu();
        private List<ArmStackFrame> iStackFrames = new List<ArmStackFrame>();
        #endregion
    }
}
