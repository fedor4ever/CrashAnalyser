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
using SymbianUtils.PluginManager;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Disassembler;
using SymbianStructuresLib.Arm.Instructions;
using SymbianInstructionLib.Arm.Instructions.Arm;
using SymbianInstructionLib.Arm.Instructions.Thumb;
using SymbianInstructionLib.Arm.Instructions.Common;
using System.Reflection;

namespace SymbianInstructionLib.Arm.Library
{
    public class ArmLibrary
    {
        #region Constructors
        public ArmLibrary()
        {
        }

        static ArmLibrary()
        {
            // Load all the ARM instructions
            Comparison<ArmInstruction> armInstructionComparer = delegate( ArmInstruction aLeft, ArmInstruction aRight )
            {
                // We sort in reverse order, hence right compared to left
                return aRight.SortOrder.CompareTo( aLeft.SortOrder );
            };
            iArmInstructions = new PluginManager<ArmInstruction>();
            iArmInstructions.LoadFromCallingAssembly();
            iArmInstructions.Sort( armInstructionComparer );

            // Load all the THUMB instructions
            Comparison<ThumbInstruction> thumbInstructionComparer = delegate( ThumbInstruction aLeft, ThumbInstruction aRight )
            {
                // We sort in reverse order, hence right compared to left
                return aRight.SortOrder.CompareTo( aLeft.SortOrder );
            };
            iThumbInstructions = new PluginManager<ThumbInstruction>();
            iThumbInstructions.LoadFromCallingAssembly();
            iThumbInstructions.Sort( thumbInstructionComparer );

            // Load disassembler if present
            iDisassembler = new PluginManager<IArmDisassembler>();
            iDisassembler.Load( null );
        }
        #endregion

        #region API
        public IArmInstruction[] ConvertToInstructions( TArmInstructionSet aInstructionSet, uint[] aRawInstructions, uint aStartingAddress )
        {
            IArmInstruction[] ret = null;
            //
            switch ( aInstructionSet )
            {
            case TArmInstructionSet.EARM:
                ret = ConvertToArm( aRawInstructions, aStartingAddress );
                break;
            case TArmInstructionSet.ETHUMB:
                ret = ConvertToThumb( aRawInstructions, aStartingAddress );
                break;
            default:
                throw new NotSupportedException();
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private void Disassemble( ArmBaseInstruction aInstruction )
        {
            if ( iDisassembler.Count > 0 )
            {
                IArmDisassembler disassembler = iDisassembler[ 0 ];
                string disassembly = disassembler.Disassemble( aInstruction );
                aInstruction.AIDisassembly = disassembly;
            }
        }

        private IArmInstruction[] ConvertToArm( uint[] aRawInstructions, uint aStartingAddress )
        {
            // TODO: optimise this
            List<IArmInstruction> ret = new List<IArmInstruction>();
            //
            uint address = aStartingAddress;
            for ( int i = 0; i < aRawInstructions.Length; i++, address += 4 )
            {
                uint raw = aRawInstructions[ i ];
                foreach ( ArmInstruction inst in iArmInstructions )
                {
                    if ( inst.Matches( raw ) )
                    {
                        Type type = inst.GetType();
                        ConstructorInfo ctor = type.GetConstructor( new Type[] { } );
                        ArmInstruction copy = (ArmInstruction) ctor.Invoke( new object[] { } );
                        copy.AIAddress = address;
                        copy.AIRawValue = raw;
                        ret.Add( copy );
                        Disassemble( copy );
                        break;
                    }
                }
            }
            //
            return ret.ToArray();
        }

        private IArmInstruction[] ConvertToThumb( uint[] aRawInstructions, uint aStartingAddress )
        {
            // TODO: optimise this
            List<IArmInstruction> ret = new List<IArmInstruction>();
            //
            uint address = aStartingAddress;
            for ( int i = 0; i < aRawInstructions.Length; i++, address += 2 )
            {
                uint raw = aRawInstructions[ i ];
                foreach ( ThumbInstruction inst in iThumbInstructions )
                {
                    if ( inst.Matches( raw ) )
                    {
                        Type type = inst.GetType();
                        ConstructorInfo ctor = type.GetConstructor( new Type[] { } );
                        ThumbInstruction copy = (ThumbInstruction) ctor.Invoke( new object[] { } );
                        copy.AIAddress = address;
                        copy.AIRawValue = raw;
                        ret.Add( copy );
                        Disassemble( copy );
                        break;
                    }
                }
            }
            //
            return ret.ToArray();
        }
        #endregion

        #region Data members
        private static readonly PluginManager<ArmInstruction> iArmInstructions;
        private static readonly PluginManager<ThumbInstruction> iThumbInstructions;
        private static readonly PluginManager<IArmDisassembler> iDisassembler;
        #endregion
    }
}
