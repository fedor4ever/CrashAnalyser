/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using SymbianParserLib.Engine;
using SymbianParserLib.BaseStructures;
using CrashDebuggerLib.Structures.Cpu;
using CrashDebuggerLib.Structures.Register;
using SymbianParserLib.Elements;
using SymbianParserLib.Enums;
using SymbianStructuresLib.Arm.Registers;

namespace CrashDebuggerLib.Parsers.State.Implementation
{
    internal class StateInfoCpu : State
    {
        #region Constructors
        public StateInfoCpu( CrashDebuggerParser aParser )
            : base( aParser )
        {
            iInfo = aParser.CrashDebugger.InfoCpu;
        }
        #endregion

        #region API
        public override void Prepare()
        {
            PrepareMandatoryParagraph();
        }

        public override void Finalise()
        {

        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private void SwitchBank( ParserParagraph aPara, ParserLine aLine )
        {
        }

        private void AddRegister( ParserParagraph aPara, ParserField aField, ParserFieldName aFieldName, uint aValue )
        {
            RegisterCollection.TType type = (RegisterCollection.TType) aPara.Tag;
            string regName = aFieldName.Name;

            // USR registers are a bit tricky since they are largely shared. Only R13 and R14 are
            // really USR specific.
            if ( type == RegisterCollection.TType.ETypeUser )
            {
                ArmRegister reg = new ArmRegister( regName, aValue );
                //
                switch ( reg.RegType )
                {
                default:
                    type = RegisterCollection.TType.ETypeCommonBank;
                    break;
                case TArmRegisterType.EArmReg_SP:
                case TArmRegisterType.EArmReg_LR:
                    break;
                }
            }

            RegisterCollection regCollection = iInfo[ type ];
            regCollection.Add( regName, aValue );
        }

        private void PrepareMandatoryParagraph()
        {
            {
                ParserParagraph para = CreateParagraph( "MODE_USR:", RegisterCollection.TType.ETypeUser );
                //
                ParserLine l2 = ParserLine.NewSymFormat( " R0=%08x  R1=%08x  R2=%08x  R3=%08x\r\n" );
                ParserLine l3 = ParserLine.NewSymFormat( " R4=%08x  R5=%08x  R6=%08x  R7=%08x\r\n" );
                ParserLine l4 = ParserLine.NewSymFormat( " R8=%08x  R9=%08x R10=%08x R11=%08x\r\n" );
                ParserLine l5 = ParserLine.NewSymFormat( "R12=%08x R13=%08x R14=%08x R15=%08x\r\n" );
                para.Add( l2, l3, l4, l5 );
                ParserEngine.Add( para );
            }

            {
                ParserParagraph para = CreateParagraph( string.Empty, RegisterCollection.TType.ETypeCommonBank );
                ParserLine l2 = ParserLine.NewSymFormat( "CPSR=%08x\r\n" );
                para.Add( l2 );
                ParserEngine.Add( para );
            }

            {
                ParserParagraph para = CreateParagraph( "MODE_FIQ:", RegisterCollection.TType.ETypeFastInterrupt );
                //
                ParserLine l2 = ParserLine.NewSymFormat( " R8=%08x  R9=%08x R10=%08x R11=%08x\r\n" );
                ParserLine l3 = ParserLine.NewSymFormat( "R12=%08x R13=%08x R14=%08x SPSR=%08x\r\n" );
                para.Add( l2, l3 );
                ParserEngine.Add( para );
            }

            {
                ParserParagraph para = CreateParagraph( "MODE_IRQ:", RegisterCollection.TType.ETypeInterrupt );
                ParserLine l2 = ParserLine.NewSymFormat( "R13=%08x R14=%08x SPSR=%08x\r\n" );
                para.Add( l2 );
                ParserEngine.Add( para );
            }

            {
                ParserParagraph para = CreateParagraph( "MODE_SVC:", RegisterCollection.TType.ETypeSupervisor );
                ParserLine l2 = ParserLine.NewSymFormat( "R13=%08x R14=%08x SPSR=%08x\r\n" );
                para.Add( l2 );
                ParserEngine.Add( para );
            }

            {
                ParserParagraph para = CreateParagraph( "MODE_ABT:", RegisterCollection.TType.ETypeAbort );
                ParserLine l2 = ParserLine.NewSymFormat( "R13=%08x R14=%08x SPSR=%08x\r\n" );
                para.Add( l2 );
                ParserEngine.Add( para );
            }

            {
                ParserParagraph para = CreateParagraph( "MODE_UND:", RegisterCollection.TType.ETypeUndefined );
                ParserLine l2 = ParserLine.NewSymFormat( "R13=%08x R14=%08x SPSR=%08x\r\n" );
                para.Add( l2 );
                ParserEngine.Add( para );
            }

            {
                ParserParagraph para = CreateParagraph( string.Empty, RegisterCollection.TType.ETypeGeneral );
                ParserLine l2 = ParserLine.NewSymFormat( "DACR %08x\r\n" );
                ParserLine l3 = ParserLine.NewSymFormat( "CAR %08x\r\n" );
                ParserLine l4 = ParserLine.NewSymFormat( "MMUID %08x\r\n" );
                ParserLine l5 = ParserLine.NewSymFormat( "MMUCR %08x\r\n" );
                ParserLine l6 = ParserLine.NewSymFormat( "AUXCR %08x\r\n" );
                ParserLine l7 = ParserLine.NewSymFormat( "FPEXC %08x\r\n" );
                ParserLine l8 = ParserLine.NewSymFormat( "CTYPE %08x\r\n" );
                para.Add( l2, l3, l4, l5, l6, l7, l8 );
                ParserEngine.Add( para );
            }
        }

        private ParserParagraph CreateParagraph( string aName, RegisterCollection.TType aType )
        {
            RegisterCollection registers = CrashDebugger.InfoCpu[ aType ];
            //
            ParserParagraph para = new ParserParagraph( aName );
            para.Tag = aType;
            para.SetTargetMethod( this, "AddRegister" );
            //
            if ( aName.Length > 0 )
            {
                ParserLine header = ParserLine.New( aName + "\r\n" );
                header.SetTargetMethod( this, "SwitchBank" );
            }
            return para;
        }
        #endregion

        #region Data members
        private readonly CpuInfo iInfo;
        #endregion
    }
}
