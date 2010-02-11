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
using System.ComponentModel;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Messages;
using CrashItemLib.Crash.Container;
using CrashItemLib.Crash.Threads;
using SymbianStructuresLib.Arm.Registers;

namespace CrashItemLib.Crash.Registers.Special
{
    public class CIRegisterExcCode : CIRegister
	{
        #region Enumerations
        public enum TExceptionCode
        {
            EExceptionCodeUnknown = -1,

            [Description( "EExcGeneral" )]
            EExceptionCodeGeneral = 0,

            [Description( "EExcIntegerDivideByZero" )]
            EExceptionCodeIntegerDivideByZero = 1,

            [Description( "EExcSingleStep" )]
            EExceptionCodeSingleStep = 2,

            [Description( "EExcBreakPoint" )]
            EExceptionCodeBreakPoint = 3,

            [Description( "EExcIntegerOverflow" )]
            EExceptionCodeIntegerOverflow = 4,

            [Description( "EExcBoundsCheck" )]
            EExceptionCodeBoundsCheck = 5,

            [Description( "EExcInvalidOpCode" )]
            EExceptionCodeInvalidOpCode = 6,

            [Description( "EExcDoubleFault" )]
            EExceptionCodeDoubleFault = 7,

            [Description( "EExcStackFault" )]
            EExceptionCodeStackFault = 8,

            [Description( "EExcAccessViolation" )]
            EExceptionCodeAccessViolation = 9,

            [Description( "EExcPrivInstruction" )]
            EExceptionCodePrivInstruction = 10,

            [Description( "EExcAlignment" )]
            EExceptionCodeAlignment = 11,

            [Description( "EExcPageFault" )]
            EExceptionCodePageFault = 12,

            [Description( "EExcFloatDenormal" )]
            EExceptionCodeFloatDenormal = 13,

            [Description( "EExcFloatDivideByZero" )]
            EExceptionCodeFloatDivideByZero = 14,

            [Description( "EExcFloatInexactResult" )]
            EExceptionCodeFloatInexactResult = 15,

            [Description( "EExcFloatInvalidOperation" )]
            EExceptionCodeFloatInvalidOperation = 16,

            [Description( "EExcFloatOverflow" )]
            EExceptionCodeFloatOverflow = 17,

            [Description( "EExcFloatStackCheck" )]
            EExceptionCodeFloatStackCheck = 18,

            [Description( "EExcFloatUnderflow" )]
            EExceptionCodeFloatUnderflow = 19,

            [Description( "EExcAbort" )]
            EExceptionAbort = 20,

            [Description( "EExcKill" )]
            EExceptionCodeKill = 21,

            [Description( "EExcUserInterrupt" )]
            EExceptionCodeUserInterrupt = 22,

            [Description( "EExcDataAbort" )]
            EExceptionCodeDataAbort = 23,

            [Description( "EExcCodeAbort" )]
            EExceptionCodeAbort = 24,

            [Description( "EExcMaxNumber" )]
            EExceptionCodeMaxNumber = 25,

            [Description( "EExcInvalidVector" )]
            EExceptionCodeInvalidVector = 26,
            //
            EExceptionCodeLast = EExceptionCodeInvalidVector
        }
        #endregion
        
        #region Constructors
        public CIRegisterExcCode( CIRegisterList aCollection, uint aValue )
            : base( aCollection, TArmRegisterType.EArmReg_EXCCODE, aValue )
        {
            iDescription = CreateMessage();
            base.AddChild( iDescription );
        }
        #endregion

        #region API
        /// <summary>
        /// D_EXC is only aware of 3 different exception types.
        ///
        /// 0 = prefetch abort
        /// 1 = data abort
        /// 2 = undefined instruction
        ///
        /// Symbian ELF Core dump does this mapping for us automatically.
        /// </summary>
        public void ExpandToFullExceptionRange()
        {
            switch ( (TBasicExceptionCode) Value )
            {
            case TBasicExceptionCode.EBasicExceptionPrefechAbort:
                iExcCode = TExceptionCode.EExceptionCodeAbort;
                break;
            case TBasicExceptionCode.EBasicExceptionDataAbort:
                iExcCode = TExceptionCode.EExceptionCodeDataAbort;
                break;
            case TBasicExceptionCode.EBasicExceptionUndefinedInstruction:
                iExcCode = TExceptionCode.EExceptionCodeInvalidOpCode;
                break;
            default:
                break;
            }

            // Prod back updated value to underlying register item
            base.Value = (uint) iExcCode;

            // Re-prepare the message
            UpdateMessage( iDescription );
        }
        #endregion

        #region Properties
        public TExceptionCode ExceptionCode
        {
            get
            {
                TExceptionCode ret = TExceptionCode.EExceptionCodeUnknown;
                //
                uint type = base.Value;
                if ( type >= (uint) TExceptionCode.EExceptionCodeGeneral && type <= (uint) TExceptionCode.EExceptionCodeLast )
                {
                    ret = (TExceptionCode) type;
                }
                //
                return ret;
            }
        }

        public string ExceptionCodeDescription
        {
            get
            {
                string type = "Unknown";
                //
                switch ( iExcCode )
                {
                default:
                case TExceptionCode.EExceptionCodeUnknown:
                    break;
                case TExceptionCode.EExceptionCodeGeneral:
                    type = "General Exception";
                    break;
                case TExceptionCode.EExceptionCodeIntegerDivideByZero:
                    type = "Integer Divide by Zero";
                    break;
                case TExceptionCode.EExceptionCodeSingleStep:
                    type = "Single Step";
                    break;
                case TExceptionCode.EExceptionCodeBreakPoint:
                    type = "Break Point";
                    break;
                case TExceptionCode.EExceptionCodeIntegerOverflow:
                    type = "Integer Overflow";
                    break;
                case TExceptionCode.EExceptionCodeBoundsCheck:
                    type = "Bounds Check";
                    break;
                case TExceptionCode.EExceptionCodeInvalidOpCode:
                    type = "Invalid Op. Code";
                    break;
                case TExceptionCode.EExceptionCodeDoubleFault:
                    type = "Double Fault";
                    break;
                case TExceptionCode.EExceptionCodeStackFault:
                    type = "Stack Fault";
                    break;
                case TExceptionCode.EExceptionCodeAccessViolation:
                    type = "Access Violation";
                    break;
                case TExceptionCode.EExceptionCodePrivInstruction:
                    type = "Priv. Instruction";
                    break;
                case TExceptionCode.EExceptionCodeAlignment:
                    type = "Alignment Fault";
                    break;
                case TExceptionCode.EExceptionCodePageFault:
                    type = "Page Fault";
                    break;
                case TExceptionCode.EExceptionCodeFloatDenormal:
                    type = "Float Denormal";
                    break;
                case TExceptionCode.EExceptionCodeFloatDivideByZero:
                    type = "Float Divide by Zero";
                    break;
                case TExceptionCode.EExceptionCodeFloatInexactResult:
                    type = "Inexact Float Result";
                    break;
                case TExceptionCode.EExceptionCodeFloatInvalidOperation:
                    type = "Invalid Float Operation";
                    break;
                case TExceptionCode.EExceptionCodeFloatOverflow:
                    type = "Float Overflow";
                    break;
                case TExceptionCode.EExceptionCodeFloatStackCheck:
                    type = "Float Stack Check";
                    break;
                case TExceptionCode.EExceptionCodeFloatUnderflow:
                    type = "Float Underflow";
                    break;
                case TExceptionCode.EExceptionAbort:
                    type = "Abort";
                    break;
                case TExceptionCode.EExceptionCodeKill:
                    type = "Kill";
                    break;
                case TExceptionCode.EExceptionCodeUserInterrupt:
                    type = "User Interrupt";
                    break;
                case TExceptionCode.EExceptionCodeDataAbort:
                    type = "Data Abort";
                    break;
                case TExceptionCode.EExceptionCodeAbort:
                    type = "Code Abort";
                    break;
                case TExceptionCode.EExceptionCodeMaxNumber:
                    type = "Max Number";
                    break;
                case TExceptionCode.EExceptionCodeInvalidVector:
                    type = "Invalid Vector";
                    break;
                }
                //
                return type;
            }
        }
        #endregion

        #region Operators
        public static implicit operator TExceptionCode( CIRegisterExcCode aReg )
        {
            return aReg.ExceptionCode;
        }
        #endregion

        #region Internal enumerations
        private enum TBasicExceptionCode
        {
            EBasicExceptionPrefechAbort = 0,
            EBasicExceptionDataAbort = 1,
            EBasicExceptionUndefinedInstruction = 2
        }
        #endregion

        #region Internal methods
        private CIMessage CreateMessage()
        {
            CIMessage message = CIMessage.NewMessage( Container );
            //
            UpdateMessage( message );
            //
            return message;
        }

        private void UpdateMessage( CIMessage aMessage )
        {
            SymbianUtils.SymDebug.SymDebugger.Assert( aMessage != null );
            //
            aMessage.Title = "Exception Code";
            aMessage.SetLineFormatted( "The Exception Code register indicates that the processor encountered an exception of type [{0}].", ExceptionCodeDescription );
        }
        #endregion

        #region Data members
        private readonly CIMessage iDescription;
        private TExceptionCode iExcCode = TExceptionCode.EExceptionCodeUnknown;
        #endregion
    }
}
