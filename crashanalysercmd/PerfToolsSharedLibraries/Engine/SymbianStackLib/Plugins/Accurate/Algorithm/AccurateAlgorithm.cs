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
using SymbianStackAlgorithmAccurate.Engine;
using SymbianStackAlgorithmAccurate.Interfaces;
using SymbianStackAlgorithmAccurate.Stack;
using SymbianStackLib.Algorithms;
using SymbianStackLib.Data.Output;
using SymbianStackLib.Data.Output.Entry;
using SymbianStackLib.Data.Source;
using SymbianStackLib.Interfaces;
using SymbianStructuresLib.Arm.Registers;
using SymbianStructuresLib.Debug.Symbols.Constants;
using SymbianUtils.DataBuffer.Entry;
using SymbianUtils.Range;

namespace SymbianStackAlgorithmAccurate.Algorithm
{
    internal class AccurateAlgorithm : StackAlgorithm, IArmStackInterface
    {
        #region Constructors
        public AccurateAlgorithm( IStackAlgorithmManager aManager, IStackAlgorithmObserver aObserver )
            : base( aManager, aObserver )
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        #endregion

        #region From StackAlgorithm
        public override string Name
        {
            get { return "Accurate"; }
        }

        public override int Priority
        {
            get { return 0; }
        }

        public override bool IsAvailable()
        {
            bool ret = base.IsAvailable();
            //
            if ( ret )
            {
                // We need some registers
                ret = !base.Engine.Registers.IsEmpty;
                //
                if ( ret )
                {
                    // This algorithm can only work if we have code available 
                    // to work with...
                    ret = base.DebugEngineView.Code.IsReady;
                    //
                    if ( ret == false )
                    {
                        base.Trace( "[AccurateAlgorithm] IsAvailable() - code not provided" );
                    }
                }
                else
                {
                    base.Trace( "[AccurateAlgorithm] IsAvailable() - registers not provided" );
                }
            }
            else
            {
                base.Trace( "[AccurateAlgorithm] IsAvailable() - symbols not provided" );
            }
            //
            base.Trace( "[AccurateAlgorithm] IsAvailable() - ret: {0}", ret );
            return ret;
        }
        #endregion

        #region From IArmStackInterface
        uint IArmStackInterface.StackBase
        {
            get { return base.Engine.AddressInfo.Base; }
        }

        uint IArmStackInterface.StackTop
        {
            get { return base.Engine.AddressInfo.Top; }
        }

        uint IArmStackInterface.StackValueAtAddress( uint aAddress )
        {
            DataBufferUint entry = base.SourceData[ aAddress ];
            return entry.Uint;
        }
        #endregion

        #region From AsyncReaderBase
        protected override void HandleReadException( Exception aException )
        {
            // If an exception occurred during frame building or
            // stack reconstruction, then we are aborting the entire operation.
            //
            // However, HandleReadcompleted() is always called irrespective
            // of whether or not an operation succeeded or failed.
            //
            // Therefore, to ensure we do not generate a nested exception in
            // an abort scenario we toggle the 'accurate entry check required'
            // status flag to false so that we will not complain if the
            // algorithm did not locate any accurate frames.
            iAccurateEntryCheckRequired = false;
            base.Trace( "[AccurateAlgorithm] HandleReadException() - aException: {0}, stack: {1}", aException.Message, aException.StackTrace );

            // Now we can propagate the exception onwards.
            base.HandleReadException( aException );
        }

        protected override void HandleReadCompleted()
        {
            // If we didn't identify any accurate entries, then we bail out
            // and let the heuristic algorithm run instead.
            //
            // NB: we don't do this unless we succeeded to reconstruct
            // the entire stack.
            base.Trace( "[AccurateAlgorithm] HandleReadCompleted() - iAccurateEntryCheckRequired: {0}, iAccurateEntryCount: {1}", iAccurateEntryCheckRequired, iAccurateEntryCount );
            if ( iAccurateEntryCheckRequired )
            {
                if ( iAccurateEntryCount == 0 )
                {
                    throw new Exception( "Unable to locate any accurate entries" );
                }
            }

            // We're an accurate algorithm
            base.OutputData.IsAccurate = true;

            base.Trace( "[AccurateAlgorithm] HandleReadCompleted() - read ok!" );
            base.HandleReadCompleted();
        }

        protected override void HandleReadStarted()
        {
            try
            {
                base.Trace( "[AccurateAlgorithm] HandleReadStarted() - START" );

                // Indicate that we will throw an exception if we do not
                // find at least one accurate entry. This is the default behaviour
                // to ensure we don't result in stack data containing just ghost entries.
                // We toggle this to false if we abort stack walking due to an exception,
                // thereby preventing nested exception throwing.
                iAccurateEntryCheckRequired = true;
                
                // Create arm engine
                iArmEngine = new AccurateEngine( base.DebugEngineView, this, base.Manager.Engine );

                // Seed it with the registers
                iArmEngine.CPU.Registers = base.Engine.Registers;

                // Dump the registers in verbose mode
                base.Trace( "STACK ENGINE REGISTERS:" );
                foreach ( ArmRegister reg in base.Engine.Registers )
                {
                    string symbol = base.DebugEngineView.Symbols.PlainText[ reg.Value ];
                    base.Trace( reg.ToString() + " " + symbol );
                }
                //
                base.Trace( System.Environment.NewLine );
                base.Trace( "[AccurateAlgorithm] HandleReadStarted() - setup complete." );
            }
            finally
            {
                base.HandleReadStarted();
            }

            base.Trace( "[AccurateAlgorithm] HandleReadStarted() - END" );
        }

        protected override void PerformOperation()
        {
            System.Diagnostics.Debug.Assert( iArmEngine != null );

            // First step is to read as many valid stack frames as possible
            BuildStackFrames();

            // Second step is to try to fill any gaps in the stack data
            CreateStackOutput();
        }

        protected override long Size
        {
            get
            {
                int bytes = base.SourceData.Count;
                int dWords = bytes / 4;
                return dWords;
            }
        }

        protected override long Position
        {
            get
            {
                return iDWordIndex;
            }
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
                if ( iArmEngine != null )
                {
                    iArmEngine.Dispose();
                    iArmEngine = null;
                }
            }
        }
        #endregion

        #region Internal methods
        private ArmStackFrame FrameByStackAddress( uint aAddress )
        {
            ArmStackFrame ret = null;
            //
            ArmStackFrame[] frames = iArmEngine.StackFrames;
            foreach ( ArmStackFrame frame in frames )
            {
                if ( frame.Address == aAddress )
                {
                    ret = frame;
                    break;
                }
            }
            //
            return ret;
        }

        private ArmStackFrame FrameByRegisterType( TArmRegisterType aRegister )
        {
            ArmStackFrame ret = null;
            //
            ArmStackFrame[] frames = iArmEngine.StackFrames;
            foreach ( ArmStackFrame frame in frames )
            {
                if ( frame.IsRegisterBasedEntry && frame.AssociatedRegister == aRegister )
                {
                    ret = frame;
                    break;
                }
            }
            //
            return ret;
        }

        private void BuildStackFrames()
        {
            bool success = iArmEngine.Process();
            while ( success )
            {
                success = iArmEngine.Process();
            }

            base.Trace( string.Empty );
            base.Trace( string.Empty );
        }

        private void CreateStackOutput()
        {
            // Get the source data we need to reconstruct and signal we're about to start
            StackSourceData sourceData = base.SourceData;

            // Get the output data sink
            StackOutputData outputData = base.OutputData;
            outputData.Clear();
            outputData.AlgorithmName = Name;

            // Get the address range of the stack pointer data
            AddressRange pointerRange = base.Engine.AddressInfo.StackPointerRange;
            AddressRange pointerRangeExtended = base.Engine.AddressInfo.StackPointerRangeWithExtensionArea;

            foreach ( DataBufferUint sourceEntry in sourceData.GetUintEnumerator() )
            {
                // Check if it is within the stack domain, taking into account
                // our extended range
                if ( pointerRangeExtended.Contains( sourceEntry.Address ) )
                {
                    StackOutputEntry outputEntry = new StackOutputEntry( sourceEntry.Address, sourceEntry.Uint, base.DebugEngineView );

                    // Is it the element that corresponds to the current value of SP?
                    bool isCurrentSPEntry = ( outputEntry.AddressRange.Contains( base.Engine.AddressInfo.Pointer ) );

                    // Is it within the pure 'stack pointer' address range?
                    bool outsidePureStackPointerRange = !pointerRange.Contains( sourceEntry.Address );
                    outputEntry.IsOutsideCurrentStackRange = outsidePureStackPointerRange;

                    // Is it a ghost?
                    if ( outputEntry.Symbol != null )
                    {
                        ArmStackFrame realStackFrame = FrameByStackAddress( sourceEntry.Address );
                        outputEntry.IsAccurate = ( realStackFrame != null );
                        outputEntry.IsGhost = ( realStackFrame == null );
                    }

                    // Save entry
                    EmitElement( outputEntry );

                    // If we're inside the stack address range, then poke in the PC and LR values
                    if ( isCurrentSPEntry )
                    {
                        outputEntry.IsCurrentStackPointerEntry = true;

                        // Working bottom up, so LR should go on the stack first
                        ArmStackFrame stackFrameLR = FrameByRegisterType( TArmRegisterType.EArmReg_LR );
                        if ( stackFrameLR != null )
                        {
                            StackOutputEntry entryLR = new StackOutputEntry( 0, stackFrameLR.Data, base.DebugEngineView );
                            entryLR.IsRegisterBasedEntry = true;
                            entryLR.IsOutsideCurrentStackRange = true;
                            entryLR.AssociatedRegister = stackFrameLR.AssociatedRegister;
                            EmitElement( entryLR );
                        }

                        // Then the PC...
                        ArmStackFrame stackFramePC = FrameByRegisterType( TArmRegisterType.EArmReg_PC );
                        if ( stackFramePC != null )
                        {
                            StackOutputEntry entryPC = new StackOutputEntry( 0, stackFramePC.Data, base.DebugEngineView );
                            entryPC.IsRegisterBasedEntry = true;
                            entryPC.IsOutsideCurrentStackRange = true;
                            entryPC.AssociatedRegister = stackFramePC.AssociatedRegister;
                            EmitElement( entryPC );
                        }
                    }
                }
                else
                {
                    // Nope, ignore it...
                }

                NotifyEvent( TEvent.EReadingProgress );
                ++iDWordIndex;
            }
        }

        private void EmitElement( StackOutputEntry aEntry )
        {
            // Debug support
            if ( base.Engine.Verbose )
            {
                StringBuilder line = new StringBuilder( "[AccurateAlgorithm] " );
                //
                if ( aEntry.IsCurrentStackPointerEntry )
                {
                    line.Append( "[C]       " );
                }
                else if ( aEntry.IsRegisterBasedEntry )
                {
                    const int KNormalAddressWidth = 10;
                    string prefix = "[ " + aEntry.AssociatedRegisterName;
                    prefix = prefix.PadRight( KNormalAddressWidth - 2 );
                    prefix += " ]";
                    line.Append( prefix );
                }
                else if ( aEntry.IsGhost )
                {
                    line.Append( "[G]       " );
                }
                else if ( aEntry.IsOutsideCurrentStackRange )
                {
                    line.Append( "          " );
                }
                else
                {
                    line.AppendFormat( "[{0:x8}]", aEntry.Address );
                }

                line.AppendFormat( " {0:x8} {1}", aEntry.Data, aEntry.DataAsString );

                if ( aEntry.Symbol != null )
                {
                    string baseAddressOffset = aEntry.Symbol.ToStringOffset( aEntry.Data );
                    line.AppendFormat( "{0} {1}", baseAddressOffset, aEntry.Symbol.Name );
                }
                else if ( aEntry.AssociatedBinary != string.Empty )
                {
                    line.AppendFormat( "{0} {1}", SymbolConstants.KUnknownOffset, aEntry.AssociatedBinary );
                }

                base.Trace( line.ToString() );
            }

            // Count the number of accurate entries
            if ( aEntry.IsAccurate )
            {
                ++iAccurateEntryCount;
            }

            // Flush entry
            base.StackObserver.StackBuildingElementConstructed( this, aEntry );
        }
        #endregion

        #region Data members
        private AccurateEngine iArmEngine = null;
        private int iDWordIndex = 0;
        private bool iAccurateEntryCheckRequired = true;
        private int iAccurateEntryCount = 0;
        #endregion
    }
}
