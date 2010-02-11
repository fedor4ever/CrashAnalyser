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

namespace CrashDebuggerLib.Structures.Thread
{
    public class ThreadStackInfo : CrashDebuggerAware
    {
        #region Enumerations
        public enum TType
        {
            ETypeSupervisor = 0,
            ETypeUser
        }
        #endregion

        #region Constructors
        public ThreadStackInfo( CrashDebuggerInfo aCrashDebugger, DThread aThread, TType aType )
            : base( aCrashDebugger )
        {
            iType = aType;
            iThread = aThread;
            iData = new ThreadStackData( aCrashDebugger, this );
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public TType Type
        {
            get { return iType; }
        }

        public uint BaseAddress
        {
            get { return iBaseAddress; }
            set { iBaseAddress = value; }
        }

        public uint Size
        {
            get { return iSize; }
            set { iSize = value; }
        }

        public uint StackPointer
        {
            get { return iStackPointer; }
            set { iStackPointer = value; }
        }

        public ThreadStackData Data
        {
            get { return iData; }
        }

        public DThread Thread
        {
            get { return iThread; }
        }

        public DProcess Process
        {
            get
            {
                DThread thread = this.Thread;
                return thread.OwningProcess;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region Clipboard Support
        public string ToClipboard()
        {
            StringBuilder ret = new StringBuilder();
            //
            string type = "Supervisor";
            if ( iType == TType.ETypeUser )
            {
                type = "User";
            }
            //
            ret.AppendFormat( "{0} Stack @ 0x{1:x8}, Stack Pointer: 0x{2:x8}" + System.Environment.NewLine, type.ToUpper(), BaseAddress, StackPointer );
            ret.AppendLine( iData.CallStackToString() );
            //
            return ret.ToString();
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        private readonly TType iType;
        private uint iBaseAddress = 0;
        private uint iSize = 0;
        private uint iStackPointer = 0;
        private readonly DThread iThread;
        private readonly ThreadStackData iData;
        #endregion
    }
}
