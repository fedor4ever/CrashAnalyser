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
using CrashDebuggerLib.Structures.Scheduler;
using CrashDebuggerLib.Structures.Register;
using SymbianParserLib.Elements;
using SymbianParserLib.Enums;

namespace CrashDebuggerLib.Parsers.State.Implementation
{
    internal class StateInfoScheduler : State
    {
        #region Constructors
        public StateInfoScheduler( CrashDebuggerParser aParser )
            : base( aParser )
        {
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
        private void PrepareMandatoryParagraph()
        {
            {
                SchedulerInfo info = CrashDebugger.InfoScheduler;

                ParserParagraph para = new ParserParagraph( "SCHEDULER_INFO" );
                //
                ParserLine l1 = ParserLine.NewSymFormat( "SCHEDULER @%08x: CurrentThread %08x\r\n" );
                l1.SetTargetProperties( info, "Address", "CurrentNThreadAddress" );
                //
                ParserLine l2 = ParserLine.NewSymFormat( "RescheduleNeeded=%02x DfcPending=%02x KernCSLocked=%08x\r\n" );
                l2.SetTargetProperties( info, "RescheduleNeeded", "DfcPending", "KernCSLocked" ); 
                //
                ParserLine l3 = ParserLine.NewSymFormat( "DFCS: next %08x prev %08x\r\n" );
                l3.SetTargetProperties( info.DFCs, "Next", "Previous" );
                //
                ParserLine l4 = ParserLine.NewSymFormat( "ProcessHandler=%08x, AddressSpace=%08x\r\n" );
                l4.SetTargetProperties( info, "ProcessHandlerAddress", "AddressSpace" );
                //
                ParserLine l5 = ParserLine.NewSymFormat( "SYSLOCK: HoldingThread %08x iWaiting %08x\r\n" );
                l5.SetTargetProperties( info.SysLockInfo, "HoldingThreadAddress", "WaitingThreadAddress" );
                //                
                ParserLine l6 = ParserLine.NewSymFormat( "Extras 0: %08x 1: %08x 2: %08x 3: %08x\r\n" );
                l6.SetTargetMethod( info.ExtraRegisters, "Add" );
                //
                ParserLine l7 = ParserLine.NewSymFormat( "Extras 4: %08x 5: %08x 6: %08x 7: %08x\r\n" );
                l7.SetTargetMethod( info.ExtraRegisters, "Add" );
                //
                ParserLine l8 = ParserLine.NewSymFormat( "Extras 8: %08x 9: %08x A: %08x B: %08x\r\n" );
                l8.SetTargetMethod( info.ExtraRegisters, "Add" );
                //
                ParserLine l9 = ParserLine.NewSymFormat( "Extras C: %08x D: %08x E: %08x F: %08x\r\n" );
                l9.SetTargetMethod( info.ExtraRegisters, "Add" );
                //
                para.Add( l1, l2, l3, l4, l5, l6, l7, l8, l9 );
                ParserEngine.Add( para );
            }
        }
        #endregion

        #region Data members
        #endregion
    }
}
