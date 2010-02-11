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
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using SymbianUtils.DataBuffer.Entry;
using SymbianStructuresLib.Arm.Registers;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Symbols;
using SymbianUtils;
using SymbianUtils.Range;
using SymbianUtils.Utilities;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.Engine;
using SymbianStackLib.Data.Source;
using SymbianStackLib.Data.Output;
using SymbianStackLib.Data.Output.Entry;
using SymbianStackLib.Interfaces;
using SymbianStackLib.Algorithms;

namespace SymbianStackAlgorithmHeuristic
{
    internal class StackAlgHeuristic : StackAlgorithm
	{
		#region Constructors
        public StackAlgHeuristic( IStackAlgorithmManager aManager, IStackAlgorithmObserver aObserver )
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
            get { return "Heuristic"; }
        }

        public override int Priority
        {
            get { return 100; } 
        }
        #endregion

        #region From AsyncReaderBase
        protected override void PerformOperation()
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

            // Indicates if we added LR and PC to the call stack
            bool addedLRandPC = false;

            // Get registers
            ArmRegisterCollection regs = base.Engine.Registers;

            foreach( DataBufferUint sourceEntry in sourceData.GetUintEnumerator() )
            {
                // Check if it is within the stack domain, taking into account
                // our extended range
                if ( pointerRangeExtended.Contains( sourceEntry.Address ) )
                {
                    StackOutputEntry outputEntry = new StackOutputEntry( sourceEntry.Address, sourceEntry.Uint, base.DebugEngineView );

                    // Is it the element tht corresponds to the current value of SP?
                    bool isCurrentSPEntry = ( outputEntry.AddressRange.Contains( base.Engine.AddressInfo.Pointer ) );
                    outputEntry.IsCurrentStackPointerEntry = isCurrentSPEntry;

                    // Is it within the pure 'stack pointer' address range?
                    bool outsidePureStackPointerRange = !pointerRange.Contains( sourceEntry.Address );
                    outputEntry.IsOutsideCurrentStackRange = outsidePureStackPointerRange;

                    // These are never accurate, but neither are they ghosts
                    outputEntry.IsAccurate = false;
                    outputEntry.IsGhost = false;
                    
                    // Save entry
                    EmitElement( outputEntry );

                    // If we're inside the stack address range, then poke in the PC and LR values
                    if ( isCurrentSPEntry )
                    {
                        System.Diagnostics.Debug.Assert( !addedLRandPC );
                        addedLRandPC = AddLRAndPC();
                    }
                }
                else
                {
                    // Nope, ignore it...
                }

                NotifyEvent( TEvent.EReadingProgress );
                ++iDWordIndex;
            }

            // If the stack overflowed, then SP might be outside of the stack range. Therefore
            // LR and PC will not be added yet.
            if ( !addedLRandPC )
            {
                AddLRAndPC();
            }
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

		#region Internal methods
        private bool AddLRAndPC()
        {
            bool addedLRandPC = false;
            //
            ArmRegisterCollection regs = base.Engine.Registers;
            
            // If we're inside the stack address range, then poke in the PC and LR values
            if ( regs.Count > 0 )
            {
                // Working bottom up, so LR should go on the stack first
                if ( regs.Contains( TArmRegisterType.EArmReg_LR ) )
                {
                    ArmRegister regLR = regs[ TArmRegisterType.EArmReg_LR ];

                    StackOutputEntry entryLR = new StackOutputEntry( 0, regLR.Value, base.DebugEngineView );
                    entryLR.IsRegisterBasedEntry = true;
                    entryLR.IsOutsideCurrentStackRange = true;
                    entryLR.AssociatedRegister = regLR.RegType;
                    EmitElement( entryLR );
                }

                // Then the PC...
                if ( regs.Contains( TArmRegisterType.EArmReg_PC ) )
                {
                    ArmRegister regPC = regs[ TArmRegisterType.EArmReg_PC ];

                    StackOutputEntry entryPC = new StackOutputEntry( 0, regPC.Value, base.DebugEngineView );
                    entryPC.IsRegisterBasedEntry = true;
                    entryPC.IsOutsideCurrentStackRange = true;
                    entryPC.AssociatedRegister = regPC.RegType;
                    EmitElement( entryPC );
                }

                // Even if they weren't added, we at least attempted to addd them
                addedLRandPC = true;
            }

            return addedLRandPC;
        }

        private void EmitElement( StackOutputEntry aEntry )
        {
            // Flush entry
            base.StackObserver.StackBuildingElementConstructed( this, aEntry );
        }
        #endregion

		#region Data members
        private int iDWordIndex = 0;
		#endregion
    }
}
