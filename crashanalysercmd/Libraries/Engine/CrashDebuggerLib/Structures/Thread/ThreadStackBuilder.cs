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
using CrashDebuggerLib.Threading;
using SymbianStructuresLib.Debug.Symbols;
using SymbianStructuresLib.Arm.Registers;
using SymbianStructuresLib.CodeSegments;
using SymbianUtils.DataBuffer;
using SymbianUtils.DataBuffer.Entry;
using SymbianUtils.DataBuffer.Primer;
using SymbianDebugLib.Engine;
using SymbianUtils.Utilities;
using CrashDebuggerLib.Structures.KernelObjects;
using CrashDebuggerLib.Structures.Common;
using CrashDebuggerLib.Structures.Process;
using CrashDebuggerLib.Structures.CodeSeg;
using CrashDebuggerLib.Structures.Register;
using CrashDebuggerLib.Structures.Cpu;
using SymbianStackLib.Engine;
using SymbianStackLib.Data.Output;

namespace CrashDebuggerLib.Structures.Thread
{
    internal class ThreadStackBuilder : AsyncOperation
    {
        #region Constructors
        public ThreadStackBuilder( ThreadStackData aInfo )
        {
            iInfo = aInfo;
            this.DoWork += new System.ComponentModel.DoWorkEventHandler( ThreadStackBuilder_DoWork );
        }
        #endregion

        #region API
        public void BuildSync()
        {
            PrimeStackEngine();
            iStackEngine.ReconstructSync();
        }
        #endregion

        #region Properties
        public bool CallStackConstructed
        {
            get
            {
                return iIsReady;
            }
        }

        public StackOutputData CallStackElements
        {
            get
            {
                StackOutputData ret = new StackOutputData();
                //
                if ( iStackEngine != null )
                {
                    ret = iStackEngine.DataOutput;
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        private void PrimeStackEngine()
        {
            // Create new engine (resets old data)
            iStackEngine = new StackEngine( iInfo.CrashDebugger.DebugEngine );

            // Not yet ready
            iIsReady = false;

            // Get the data source
            DThread thread = iInfo.Info.Thread;
            string threadName = thread.Name.ToLower();
            //
            DataBuffer dataSource = iInfo.Data;
            if ( dataSource.Count > 0 )
            {
                // Prime stack engine with data & current stack pointer
                iStackEngine.Primer.Prime( dataSource );
                iStackEngine.AddressInfo.Pointer = iInfo.Info.StackPointer;

                // Set us up so we know when the process finishes
                iStackEngine.EventHandler += new StackEngine.StackEngineEventHandler( StackEngine_EventHandler );

                // Set up registers. First update them taking into account the
                // curent CPU regs.
                RegisterCollection.TType requiredType = RegisterCollection.TType.ETypeUser;
                if ( iInfo.Info.Type == ThreadStackInfo.TType.ETypeSupervisor )
                {
                    requiredType = RegisterCollection.TType.ETypeSupervisor;
                }

                RegisterCollection registers = thread.NThread.GetRegisters( requiredType );
                iStackEngine.Registers = registers;

                // Get code segs
                DProcess process = thread.OwningProcess;
                if ( process != null )
                {
                    iStackEngine.CodeSegments = process.GetCodeSegDefinitions();
                }
            }
            else
            {
            }
        }

        void ThreadStackBuilder_DoWork( object sender, System.ComponentModel.DoWorkEventArgs aArgs )
        {
            PrimeStackEngine();

            // Only reconstruct if we have some data
            if ( iStackEngine.DataSource.Count > 0 )
            {
                iStackEngine.ReconstructSync();
            }
        }
        #endregion

        #region Stack Engine event handler
        private void StackEngine_EventHandler( StackEngine.TEvent aEvent, StackEngine aEngine )
        {
            if ( aEvent == StackEngine.TEvent.EStackBuildingComplete )
            {
                iIsReady = true;
            }
        }
        #endregion

        #region Data members
        private readonly ThreadStackData iInfo;
        private StackEngine iStackEngine = null;
        private bool iIsReady = false;
        #endregion
    }
}
