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
using CrashDebuggerLib.Structures.KernelObjects;
using CrashDebuggerLib.Structures.Common;
using CrashDebuggerLib.Structures.Process;

namespace CrashDebuggerLib.Structures.MessageQueue
{
    public class DMsgQueue : DObject
    {
        #region Enumerations
        [System.ComponentModel.TypeConverter( typeof( SymbianParserLib.TypeConverters.SymbianEnumConverter ) )]
        public enum TQueueState
        {
            EUnknown = -1,
            EEmpty,
            EPartial,
            EFull
        }
        #endregion

        #region Constructors
        public DMsgQueue( CrashDebuggerInfo aCrashDebugger )
            : base( aCrashDebugger, TObjectType.EMsgQueue )
        {
            iWaitSpaceInfo = new MsgQueueWaitInfo( MsgQueueWaitInfo.TType.EWaitTypeSpace, CrashDebugger );
            iWaitDataInfo = new MsgQueueWaitInfo( MsgQueueWaitInfo.TType.EWaitTypeData, CrashDebugger );
        }
        #endregion

        #region API
        public static TQueueState StateByString( string aState )
        {
            TQueueState ret = TQueueState.EUnknown;
            //
            switch ( aState.ToUpper() )
            {
            case "EMPTY":
                ret = TQueueState.EEmpty;
                break;
            case "PARTIAL":
                ret = TQueueState.EPartial;
                break;
            case "FULL":
                ret = TQueueState.EFull;
                break;
            default:
                break;
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        public MsgQueuePoolInfo PoolInfo
        {
            get { return iPoolInfo; }
        }

        public MsgQueueSlotInfo SlotInfo
        {
            get { return iSlotInfo; }
        }

        public int MaxMessageLength
        {
            get { return iMaxMessageLength; }
            set { iMaxMessageLength = value; }
        }

        public TQueueState State
        {
            get { return iState; }
            set { iState = value; }
        }

        public string StateString
        {
            get
            {
                string ret = "Unknown";
                //
                switch ( iState )
                {
                default:
                case TQueueState.EUnknown:
                    break;
                case TQueueState.EEmpty:
                    ret = "Empty";
                    break;
                case TQueueState.EPartial:
                    ret = "Partial";
                    break;
                case TQueueState.EFull:
                    ret = "Full";
                    break;
                }
                //
                return ret;
            }
        }

        public MsgQueueWaitInfo WaitSpace
        {
            get { return iWaitSpaceInfo; }
        }

        public MsgQueueWaitInfo WaitData
        {
            get { return iWaitDataInfo; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        private readonly MsgQueueWaitInfo iWaitSpaceInfo;
        private readonly MsgQueueWaitInfo iWaitDataInfo;
        private MsgQueuePoolInfo iPoolInfo = new MsgQueuePoolInfo();
        private MsgQueueSlotInfo iSlotInfo = new MsgQueueSlotInfo();
        private int iMaxMessageLength = 0;
        private TQueueState iState = TQueueState.EUnknown;
        #endregion
    }
}
