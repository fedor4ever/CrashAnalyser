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
using CrashDebuggerLib.Structures.Register;
using CrashDebuggerLib.Structures.Common;

namespace CrashDebuggerLib.Structures.Fault
{
    public class FaultInfo : CrashDebuggerAware
    {
        #region Enumerations
        public enum TFaultType
        {
            EFaultPanic = 0,
            EFaultException
        }
        #endregion

        #region Constructors
        public FaultInfo( CrashDebuggerInfo aCrashDebugger )
            : base( aCrashDebugger )
        {
            iRegisters = new RegisterCollection( aCrashDebugger, RegisterCollection.TType.ETypeGeneral );
        }
        #endregion

        #region API
        public void Clear()
        {
            iCategory = string.Empty;
            iReason = 0;
            iCodeAddress = 0;
            iDataAddress = 0;
            iExceptionId = 0;
            iExtraInfo = 0;
            iExcCode = 0;
            iRegisters.Clear();
        }
        #endregion

        #region Properties
        public string Category
        {
            get { return iCategory; }
            set { iCategory = value; }
        }

        public uint Reason
        {
            get { return iReason; }
            set { iReason = value; }
        }

        public uint CodeAddress
        {
            get { return iCodeAddress; }
            set { iCodeAddress = value; }
        }

        public uint DataAddress
        {
            get { return iDataAddress; }
            set { iDataAddress = value; }
        }

        public int ExcCode
        {
            get { return iExcCode; }
            set { iExcCode = value; }
        }

        public uint ExceptionId
        {
            get { return iExceptionId; }
            set { iExceptionId = value; }
        }

        public uint ExtraInfo
        {
            get { return iExtraInfo; }
            set { iExtraInfo = value; }
        }

        public RegisterCollection Registers
        {
            get { return iRegisters; }
        }

        public TFaultType FaultType
        {
            get
            {
                TFaultType ret = TFaultType.EFaultPanic;
                //
                if ( Category == KExceptionFaultReason )
                {
                    ret = TFaultType.EFaultException;
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        private const string KExceptionFaultReason = "Exception";
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Data members
        private string iCategory = string.Empty;
        private uint iReason = 0;
        private uint iCodeAddress = 0;
        private uint iDataAddress = 0;
        private uint iExceptionId = 0;
        private uint iExtraInfo = 0;
        private int iExcCode = 0;
        private readonly RegisterCollection iRegisters;
        #endregion
    }
}
