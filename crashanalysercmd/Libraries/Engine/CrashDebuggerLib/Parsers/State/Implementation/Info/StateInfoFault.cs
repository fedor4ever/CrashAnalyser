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
using CrashDebuggerLib.Structures.Fault;
using SymbianParserLib.Elements;
using SymbianParserLib.Enums;

namespace CrashDebuggerLib.Parsers.State.Implementation
{
    internal class StateInfoFault : State
    {
        #region Constructors
        public StateInfoFault( CrashDebuggerParser aParser )
            : base( aParser )
        {
        }
        #endregion

        #region API
        public override void Prepare()
        {
            PrepareMandatoryParagraph();
            PrepareOptionalParagraph();
        }

        public override void Finalise()
        {
            
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        private void PrepareMandatoryParagraph()
        {
            ParserParagraph para = new ParserParagraph( "FaultInfo");
            //
            ParserLine l1 = ParserLine.NewSymFormat( "Fault Category: %S  Fault Reason: %08x\r\n" );
            l1.SetTargetProperties( CrashDebugger.InfoFault, "Category", "Reason" );
            //
            ParserLine l2 = ParserLine.NewSymFormat( "ExcId %08x CodeAddr %08x DataAddr %08x Extra %08x\r\n" );
            l2.SetTargetProperties( CrashDebugger.InfoFault, "ExceptionId", "CodeAddress", "DataAddress", "ExtraInfo" );
            //
            para.Add( l1, l2 );
            ParserEngine.Add( para );
        }

        private void PrepareOptionalParagraph()
        {
            ParserParagraph para = new ParserParagraph( "CpuFaultInfo" );
            para.SetTargetMethod( CrashDebugger.InfoFault.Registers, "Add", TValueStoreMethodArguments.EValueStoreMethodArgumentNameAsString, TValueStoreMethodArguments.EValueStoreMethodArgumentValue );
            //
            ParserLine l1 = ParserLine.NewSymFormat( "Exc %1d Cpsr=%08x FAR=%08x FSR=%08x\r\n" );
            l1[ 0 ].SetTargetProperties( CrashDebugger.InfoFault, "ExcCode" );
            //
            ParserLine l2 = ParserLine.NewSymFormat( " R0=%08x  R1=%08x  R2=%08x  R3=%08x\r\n" );
            ParserLine l3 = ParserLine.NewSymFormat( " R4=%08x  R5=%08x  R6=%08x  R7=%08x\r\n" );
            ParserLine l4 = ParserLine.NewSymFormat( " R8=%08x  R9=%08x R10=%08x R11=%08x\r\n" );
            ParserLine l5 = ParserLine.NewSymFormat( "R12=%08x R13=%08x R14=%08x R15=%08x\r\n" );
            ParserLine l6 = ParserLine.NewSymFormat( "R13Svc=%08x R14Svc=%08x SpsrSvc=%08x\r\n" );
            //
            para.Add( l1, l2, l3, l4, l5, l6 );
            ParserEngine.Add( para );
        }
        #endregion

        #region Data members
        #endregion
    }
}
