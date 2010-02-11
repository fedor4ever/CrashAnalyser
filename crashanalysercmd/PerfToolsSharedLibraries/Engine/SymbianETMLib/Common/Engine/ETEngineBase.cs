/*
* Copyright (c) 2009 Nokia Corporation and/or its subsidiary(-ies).
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
using SymbianUtils.Tracer;
using SymbianUtils.BasicTypes;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.PluginAPI.Types.Symbol;
using SymbianDebugLib.PluginAPI.Types.Code;
using SymbianStructuresLib.Arm;
using SymbianStructuresLib.Arm.Exceptions;
using SymbianStructuresLib.Arm.Disassembler;
using SymbianStructuresLib.Debug.Symbols;
using SymbianStructuresLib.Debug.Code;
using SymbianETMLib.Common.Buffer;
using SymbianETMLib.Common.State;
using SymbianETMLib.Common.Config;
using SymbianETMLib.Common.Utilities;
using SymbianETMLib.Common.Types;

namespace SymbianETMLib.Common.Engine
{
    public class ETEngineBase : ITracer
    {
        #region Delegates & events
        public delegate void BranchHandler( ETMBranch aBranch );
        public event BranchHandler Branch;

        public delegate void ExceptionModeHandler( TArmExceptionType aExceptionMode );
        public event ExceptionModeHandler Exception;

        public delegate void ContextSwitchHandler( uint aContextId, string aThreadName );
        public event ContextSwitchHandler ContextSwitch;

        public delegate void SynchronizationHandler( uint aPosition );
        public event SynchronizationHandler Synchronized;
        #endregion

        #region Constructors
        public ETEngineBase( ETBufferBase aBuffer, ETConfigBase aConfig )
        {
            iBuffer = aBuffer;
            iConfig = aConfig;
        }
        #endregion

        #region API
        public void Decode()
        {
            int lastBufferCountValue = 0;
            //
            ETMStateData stateManager = new ETMStateData( this );
            while ( !iBuffer.IsEmpty )
            {
                byte b = iBuffer.Dequeue();
                if ( iBuffer.Count != lastBufferCountValue )
                {
                    Trace( "[0x{0:x4}] byte: 0x{1:x2} {2}",
                        stateManager.PacketNumber,
                        b,
                        Convert.ToString( b, 2 ).PadLeft( 8, '0' )
                      );
                }
                //
                lastBufferCountValue = iBuffer.Count;
                stateManager.PrepareToHandleByte( b );
            }
        }

        internal string LookUpSymbol( uint aAddress )
        {
            StringBuilder line = new StringBuilder();
            //
            if ( iDebugEngineView != null )
            {
                SymbolCollection col = null;
                Symbol sym = LookUpSymbol( aAddress, out col );
                //
                if ( sym != null )
                {
                    line.AppendFormat( "0x{0:x8} {1} {2}", sym.Address, sym.ToStringOffset( aAddress ), sym.Name );
                }
                else if ( col != null )
                {
                    line.AppendFormat( "Unknown Symbol from \'{0}\'", col.FileName.FileNameInHost );
                }
                else
                {
                    bool isExceptionVector = Config.IsExceptionVector( aAddress );
                    if ( isExceptionVector )
                    {
                        line.Append( "Exception Vector" );
                    }
                    else
                    {
                        line.Append( "????" );
                    }
                }
            }
            //
            return line.ToString();
        }

        internal Symbol LookUpSymbol( uint aAddress, out SymbolCollection aCollection )
        {
            Symbol ret = null;
            aCollection = null;
            //
            if ( iDebugEngineView != null )
            {
                ret = iDebugEngineView.Symbols.Lookup( aAddress, out aCollection );
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        public ETBufferBase Buffer
        {
            get { return iBuffer; }
        }

        public ETConfigBase Config
        {
            get { return iConfig; }
        }

        public DbgEngineView DebugEngineView
        {
            get { return iDebugEngineView; }
            set { iDebugEngineView = value; }
        }
        #endregion

        #region Internal event propgation methods
        internal void OnExceptionModeChange( TArmExceptionType aNewException )
        {
            if ( Exception != null )
            {
                Exception( aNewException );
            }
        }

        internal void OnBranch( uint aAddress, TArmInstructionSet aInstructionSet, TArmExceptionType aExceptionType, TETMBranchType aBranchType )
        {
            if ( Branch != null )
            {
                ETMBranch branch = new ETMBranch( new SymAddress( aAddress ), iBranchCounter, aBranchType, aInstructionSet, aExceptionType );
                
                // Try to find a symbol
                SymbolCollection col = null;
                Symbol sym = LookUpSymbol( aAddress, out col );
                //
                if ( sym != null )
                {
                    branch.Symbol = sym;
                }
                else if ( col != null )
                {
                    branch.SymbolText = string.Format( "Unknown Symbol from \'{0}\'", col.FileName.FileNameInHost );
                }

                // Cascade event
                Branch( branch );
            }

            ++iBranchCounter;
        }

        internal void OnContextSwitch( uint aNewContextId )
        {
            if ( ContextSwitch != null )
            {
                string threadName = iConfig.GetContextID( aNewContextId );
                ContextSwitch( aNewContextId, threadName );
            }
        }

        internal void OnSynchronised( uint aOffset )
        {
            if ( Synchronized != null )
            {
                Synchronized( aOffset );
            }
        }
        #endregion

        #region From ITracer
        public void Trace( string aText )
        {
            if ( System.Diagnostics.Debugger.IsAttached )
            {
                System.Diagnostics.Debug.WriteLine( aText );
            }
            if ( iConfig.Verbose )
            {
                System.Console.WriteLine( aText );
            }
        }

        public void Trace( string aFormat, params object[] aParams )
        {
            if ( iConfig.Verbose || System.Diagnostics.Debugger.IsAttached )
            {
                string text = string.Format( aFormat, aParams );
                Trace( text );
            }
        }
        #endregion

        #region From System.Object
        #endregion

        #region Data members
        private readonly ETBufferBase iBuffer;
        private readonly ETConfigBase iConfig;
        private int iBranchCounter = 0;
        private DbgEngineView iDebugEngineView;
        #endregion
    }
}
