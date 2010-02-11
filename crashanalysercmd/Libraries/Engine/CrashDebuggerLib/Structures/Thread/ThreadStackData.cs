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
using SymbianUtils.Utilities;
using CrashDebuggerLib.Structures.KernelObjects;
using CrashDebuggerLib.Structures.Common;
using CrashDebuggerLib.Structures.Process;
using CrashDebuggerLib.Structures.CodeSeg;
using CrashDebuggerLib.Structures.Register;
using SymbianStackLib.Engine;
using SymbianStackLib.Data.Output;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Symbols;
using SymbianStructuresLib.Arm.Registers;
using SymbianUtils.DataBuffer;
using SymbianUtils.DataBuffer.Entry;
using SymbianUtils.DataBuffer.Primer;

namespace CrashDebuggerLib.Structures.Thread
{
    public class ThreadStackData : CrashDebuggerAware
    {
        #region Constructors
        public ThreadStackData( CrashDebuggerInfo aCrashDebugger, ThreadStackInfo aInfo )
            : base( aCrashDebugger )
        {
            iInfo = aInfo;
            iStackBuilder = new ThreadStackBuilder( this);
        }
        #endregion

        #region API
        public void Add( uint aValue )
        {
            byte val = (byte) aValue;
            iDataSource.Add( val );
        }

        public void BuildCallStackSync()
        {
            if ( iStackBuilder.IsBusy )
            {
                // Wait for it
                while ( iStackBuilder.IsBusy )
                {
                    System.Threading.Thread.Sleep( 100 );
                }
            }
            else
            {
                iStackBuilder.BuildSync();
            }
        }

        public void BuildCallStackAsync()
        {
            if ( iStackBuilder.IsBusy )
            {
                throw new ArgumentException( "Stack builder is busy!" );
            }

            // Request call back when queue spot available. If it's the current
            // thread then we jump it to the start of the queue
            CrashDebugger.AsyncOperationManager.Queue( iStackBuilder, Info.Thread.IsCurrent );
        }
        #endregion

        #region String Conversion Support
        public string CallStackToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            ret.Append( CallStackElements.ToString() );
            //
            return ret.ToString();
        }
        #endregion

        #region Properties
        public int Size
        {
            get { return iDataSource.Count; }
        }

        public StackOutputData CallStackElements
        {
            get
            {
                if ( !iStackBuilder.CallStackConstructed )
                {
                    BuildCallStackSync();
                }
                //
                return iStackBuilder.CallStackElements;
            }
        }
        #endregion

        #region Internal methods
        internal void SetStartingAddress( uint aAddress )
        {
            if ( iDataSource.AddressOffset == 0 )
            {
                iDataSource.AddressOffset = aAddress;
            }
        }
        
        internal DataBuffer Data
        {
            get { return iDataSource; }
        }

        internal ThreadStackInfo Info
        {
            get { return iInfo; }
        }

        internal DataBufferUint LastRawDataEntry
        {
            get
            {
                if ( Size < 4 )
                {
                    throw new ArgumentException();
                }
                //
                DataBufferUint ret = null;
                //
                foreach ( DataBufferUint entry in iDataSource.GetUintEnumerator() )
                {
                    // The enumerator works from bottom up, so just return the first one.
                    ret = entry;
                    break;
                }
                //
                return ret;
            }
        }
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
        private readonly ThreadStackInfo iInfo;
        private readonly ThreadStackBuilder iStackBuilder;
        private DataBuffer iDataSource = new DataBuffer();
        private StackOutputData iDataOutput = new StackOutputData();
        #endregion
    }
}
