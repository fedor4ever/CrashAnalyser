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
using CrashDebuggerLib.Attributes;

namespace CrashDebuggerLib.Structures.NThread
{
    public class NThreadTimeInfo
    {
        #region Constructors
        public NThreadTimeInfo()
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        [PropCat( "Timing Info" )]
        public int Time
        {
            get { return iTime; }
            set { iTime = value; }
        }

        [PropCat( "Timing Info" )]
        public int Timeslice
        {
            get { return iTimeslice; }
            set { iTimeslice = value; }
        }

        [PropCat( "Timing Info", "Last start time", PropCat.TFormatType.EFormatAsHex )]
        public uint LastStartTime
        {
            get { return iLastStartTime; }
            set { iLastStartTime = value; }
        }

        [PropCat( "Timing Info", "Total CPU time" )]
        public ulong TotalCpuTime
        {
            get { return iTotalCpuTime; }
            set { iTotalCpuTime = value; }
        }

        [PropCat( "Timing Info", PropCat.TFormatType.EFormatAsHex )]
        public uint Tag
        {
            get { return iTag; }
            set { iTag = value; }
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
        private int iTime = 0;              // Time remaining
        private int iTimeslice = 0;         // Timeslice for this thread
        private uint iLastStartTime = 0;    // Last start of execution timestamp
        private ulong iTotalCpuTime = 0;    // Total time spent running, in hi-res timer ticks
        private uint iTag;                  // User defined set of bits which is ANDed with a mask when the thread is scheduled, and indicates if a DFC should be scheduled.
        #endregion
    }
}
