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
using CrashDebuggerLib.Structures.KernelObjects;
using CrashDebuggerLib.Structures.MessageQueue;
using CrashDebuggerLib.Structures.CodeSeg;
using SymbianParserLib.Elements;
using SymbianParserLib.Enums;
using SymbianParserLib.BaseStructures;

namespace CrashDebuggerLib.Parsers.State.Implementation.Helpers
{
    internal class HelperDMsgQueue : HelperDObject
    {
        #region Constructors
        public HelperDMsgQueue()
        {
        }
        #endregion

        #region API
        public void CreateMonitorMessageQueue( ParserEngine aEngine, string aName, DMsgQueue aQueue )
        {
            ParserParagraph para0 = base.CreateMonitorObjectParagraph( aName, aQueue );
            aEngine.Add( para0 );
            ParserParagraph para1 = CreateMessageQueueCommon( aName, aQueue );
            aEngine.Add( para1 );
        }
        #endregion

        #region Call-back methods
        public void SetMessageQueueState( ParserLine aLine, ParserFieldName aFieldName, string aState )
        {
            System.Diagnostics.Debug.Assert( aLine.Tag is DMsgQueue );
            DMsgQueue queue = (DMsgQueue) aLine.Tag;
            DMsgQueue.TQueueState state = DMsgQueue.StateByString( aState );
            queue.State = state;
        }
        #endregion

        #region Internal methods
        private ParserParagraph CreateMessageQueueCommon( string aName, DMsgQueue aQueue )
        {
            ParserParagraph para = new ParserParagraph( aName );
            //
            ParserLine l1 = ParserLine.NewSymFormat( "StartOfPool %08x, EndOfPool %08x\r\n" );
            l1.SetTargetProperties( aQueue.PoolInfo, "Start", "End" );
            //
            ParserLine l2 = ParserLine.NewSymFormat( "FirstFullSlot %08x, FirstFreeSlot %08x\r\n" );
            l2.SetTargetProperties( aQueue.SlotInfo, "FirstFull", "FirstFree" );
            //
            ParserLine l3 = ParserLine.NewSymFormat( "MaxMsgLength %d\r\n" );
            l3.SetTargetProperties( aQueue, "MaxMessageLength" );
            //
            ParserLine l4 = ParserLine.NewSymFormat( "MessageQueue state %S" );
            l4.Tag = aQueue;
            l4.SetTargetMethod( this, "SetMessageQueueState" );
            //
            ParserLine l5 = ParserLine.NewSymFormat( "ThreadWaitingForData %08x, DataAvailStatPtr %08x\r\n" );
            l5.SetTargetProperties( new object[] { aQueue.WaitData, aQueue.WaitData.RequestStatus }, "WaitingThreadAddress", "Address" );
            //
            ParserLine l6 = ParserLine.NewSymFormat( "ThreadWaitingForSpace %08x, SpaceAvailStatPtr %08x\r\n" );
            l6.SetTargetProperties( new object[] { aQueue.WaitSpace, aQueue.WaitSpace.RequestStatus }, "WaitingThreadAddress", "Address" );

            para.Add( l1, l2, l3, l4, l5, l6 );
            return para;
        }
        #endregion
    }
}
