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

namespace CrashDebuggerLib.Structures.DebugMask
{
    public class DebugMaskInfo : CrashDebuggerAware
    {
        #region Enumerations
        public enum TDebugMaskArea : byte
        {
            EAreaKernelDebug = 63,       // Bits   0 -> 63
            EAreaKernelConfig = 127,     // Bits  64 -> 127
            EAreaFileSystem = 191,       // Bits 128 -> 191
            EAreaLicensee = 255,         // Bits 192 -> 255
        }

        [Flags]
        public enum TDebugMaskKernel : ulong
        {
            KHARDWARE = 0x00000001,
            KBOOT = 0x00000002,
            KSERVER = 0x00000004,
            KMMU = 0x00000008,
            KSEMAPHORE = 0x00000010,
            KSCHED = 0x00000020,
            KPROC = 0x00000040,
            KEXEC = 0x00000080,
            KDEBUGGER = 0x00000100,
            KTHREAD = 0x00000200,
            KDLL = 0x00000400,
            KIPC = 0x00000800,
            KPBUS1 = 0x00001000,
            KPBUS2 = 0x00002000,
            KPBUSDRV = 0x00004000,
            KPOWER = 0x00008000,
            KTIMING = 0x00010000,
            KEVENT = 0x00020000,
            KOBJECT = 0x00040000,
            KDFC = 0x00080000,
            KEXTENSION = 0x00100000,
            KSCHED2 = 0x00200000,
            KLOCDRV = 0x00400000,
            KFAIL = 0x00800000,
            KTHREAD2 = 0x01000000,
            KDEVICE = 0x02000000,
            KMEMTRACE = 0x04000000,
            KDMA = 0x08000000,
            KMMU2 = 0x10000000,
            KNKERN = 0x20000000,
            KSCRATCH = 0x40000000,
            KPANIC = 0x80000000,
            KUSB = 1u << 32,
            KUSBPSL = 1u << 33,
            KNETWORK1 = 1u << 34,
            KNETWORK2 = 1u << 35,
            KSOUND1 = 1u << 36,
            KREALTIME = 1u << 63,
            KPAGING = 1u << 62,
            KLOCDPAGING = 1u << 61
        }

        [Flags]
        public enum TDebugMaskKernelConfig : ulong
        {
            KALLTHREADSSYSTEM = 0x00000001,
            KTESTFAST = 0x00000002,
            KTESTLATENCY = 0x00000004
        }
        #endregion

        #region Constructors
        public DebugMaskInfo( CrashDebuggerInfo aCrashDebugger )
            : base( aCrashDebugger )
        {
        }
        #endregion

        #region API
        public void Clear()
        {
            iDebugMaskFileSystem = 0;
            iDebugMaskKernel = TDebugMaskKernel.KPANIC;
            iDebugMaskKernelConfig = 0;
            iDebugMaskReservedForLicensees = 0;
        }

        public void SetValueByWordIndex( ulong aValue, int aWord )
        {
            uint areaByte = ( (byte) aWord ) * 32u;
            TDebugMaskArea area = TDebugMaskArea.EAreaKernelDebug;
            if ( areaByte < (byte) TDebugMaskArea.EAreaKernelDebug )
            {
                area = TDebugMaskArea.EAreaKernelDebug;
            }
            else if ( areaByte < (byte) TDebugMaskArea.EAreaKernelConfig )
            {
                area = TDebugMaskArea.EAreaKernelConfig;
            }
            else if ( areaByte < (byte) TDebugMaskArea.EAreaFileSystem )
            {
                area = TDebugMaskArea.EAreaFileSystem;
            }
            else if ( areaByte < (byte) TDebugMaskArea.EAreaLicensee )
            {
                area = TDebugMaskArea.EAreaLicensee;
            }
            SetValueByArea( aValue, area );
        }

        public void SetValueByArea( ulong aValue, TDebugMaskArea aArea )
        {
            switch ( aArea )
            {
                default:
                case TDebugMaskArea.EAreaKernelDebug:
                    DebugMaskKernel = (TDebugMaskKernel) aValue;
                    break;
                case TDebugMaskArea.EAreaKernelConfig:
                    DebugMaskKernelConfig = (TDebugMaskKernelConfig) aValue;
                    break;
                case TDebugMaskArea.EAreaFileSystem:
                    iDebugMaskFileSystem = aValue;
                    break;
                case TDebugMaskArea.EAreaLicensee:
                    iDebugMaskReservedForLicensees = aValue;
                    break;
            }
        }
        #endregion

        #region Properties
        public TDebugMaskKernel DebugMaskKernel
        {
            get { return iDebugMaskKernel; }
            set { iDebugMaskKernel = value; }
        }

        public TDebugMaskKernelConfig DebugMaskKernelConfig
        {
            get { return iDebugMaskKernelConfig; }
            set { iDebugMaskKernelConfig = value; }
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
        private TDebugMaskKernel iDebugMaskKernel = TDebugMaskKernel.KPANIC;
        private TDebugMaskKernelConfig iDebugMaskKernelConfig = 0;
        private ulong iDebugMaskFileSystem = 0;
        private ulong iDebugMaskReservedForLicensees = 0;
        #endregion
    }
}
