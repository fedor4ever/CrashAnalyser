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
using System.IO;
using System.Collections.Generic;
using System.Text;
using SymbianUtils.Tracer;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Instructions;
using SymbianStackAlgorithmAccurate.CPU;
using SymbianStackAlgorithmAccurate.Interfaces;
using SymbianStackAlgorithmAccurate.Instructions.Types;

namespace SymbianStackAlgorithmAccurate.Instructions
{
    internal class AccInstructionList : IEnumerable<AccInstruction>
    {
        #region Constructors
        public AccInstructionList()
        {
        }
        #endregion

        #region API
        public void AddRange( IEnumerable<IArmInstruction> aInstructions )
        {
            foreach ( IArmInstruction bi in aInstructions )
            {
                AccInstruction instruction = null;
                //
                switch ( bi.AIGroup )
                {
                case TArmInstructionGroup.EGroupBranch:
                    instruction = new AccInstBranch( bi );
                    break;
                case TArmInstructionGroup.EGroupDataProcessing:
                    instruction = new AccInstDataProcessing( bi );
                    break;
                case TArmInstructionGroup.EGroupDataTransfer:
                    instruction = new AccInstDataTransfer( bi );
                    break;
                default:
                    instruction = new AccInstUnknown( bi );
                    break;
                }
                //
                iInstructions.Add( instruction );
            }
        }

        public AccInstruction Deque()
        {
            if ( Count == 0 )
            {
                throw new InvalidOperationException( "No instructions to deque" );
            }

            AccInstruction head = iInstructions[ 0 ];
            iInstructions.RemoveAt( 0 );
            return head;
        }

        internal void Prefilter( int aInstructionCountOffsetToPC )
        {
            int count = this.Count;
            for ( int i = 0; i < count; i++ )
            {
                AccInstruction instruction = iInstructions[ i ];
                //
                if ( instruction.Ignored == false )
                {
                    instruction.Prefilter( this, i, aInstructionCountOffsetToPC );
                }
            }
        }

        internal void DebugPrint( ITracer aTracer )
        {
            if ( aTracer != null )
            {
                uint address = iInstructions.Count > 0 ? iInstructions[ 0 ].Instruction.AIAddress : uint.MaxValue;
                //
                aTracer.Trace( "" );
                aTracer.Trace( "INSTRUCTIONS @ 0x{0:x8}", address );
                aTracer.Trace( "=========================" );
                //
                foreach ( AccInstruction instruction in this )
                {
                    string line = instruction.ToString();
                    aTracer.Trace( line );
                }
                //
                aTracer.Trace( "" );
            }
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iInstructions.Count; }
        }

        public TArmInstructionSet InstructionSet
        {
            get
            {
                TArmInstructionSet ret = TArmInstructionSet.EARM;
                //
                if ( iInstructions.Count > 0 )
                {
                    ret = iInstructions[ 0 ].Instruction.AIType;
                }
                //
                return ret;
            }
        }

        public AccInstruction this[ int aIndex ]
        {
            get { return iInstructions[ aIndex ]; }
        }
        #endregion

        #region From IEnumerable<AccInstruction>
        public IEnumerator<AccInstruction> GetEnumerator()
        {
            foreach ( AccInstruction instruction in iInstructions )
            {
                yield return instruction;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( AccInstruction instruction in iInstructions )
            {
                yield return instruction;
            }
        }
        #endregion

        #region Data members
        private List<AccInstruction> iInstructions = new List<AccInstruction>();
        #endregion
    }
}

