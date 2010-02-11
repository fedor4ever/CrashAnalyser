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
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Symbols;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI.Types.Symbol;
using CrashDebuggerLib.Structures.KernelObjects;
using CrashDebuggerLib.Structures.Common;
using CrashDebuggerLib.Structures.Chunk;
using CrashDebuggerLib.Structures.CodeSeg;

namespace CrashDebuggerLib.Structures.Process
{
    public class DProcess : DObject
    {
        #region Enumerations
        [Flags, System.ComponentModel.TypeConverter( typeof( SymbianParserLib.TypeConverters.SymbianEnumConverter ) )]
        public enum TProcessFlags : uint
        {
            EProcessFlagNone                = 0x00000000,
            EProcessFlagSystemCritical      = 0x00000004,	// process panic reboots entire system
            EProcessFlagSystemPermanent     = 0x00000008,	// process exit of any kind reboots entire system
            EProcessFlagPriorityControl     = 0x40000000,
            EProcessFlagJustInTime			= 0x80000000,
        }

        [Flags, System.ComponentModel.TypeConverter( typeof( SymbianParserLib.TypeConverters.SymbianEnumConverter ) )]
        public enum TProcessAttributes : uint
        {  
            ENone                           = 0x00000000,
            EPrivate                        = 0x00000002,
            EResumed                        = 0x00010000,
            EBeingLoaded                    = 0x08000000,
            ESupervisor                     = 0x80000000,
        }
        #endregion

        #region Constructors
        public DProcess( CrashDebuggerInfo aCrashDebugger )
            : base( aCrashDebugger, TObjectType.EProcess )
        {
        }
        #endregion

        #region API
        public CodeSegDefinitionCollection GetCodeSegDefinitions()
        {
            CodeSegDefinitionCollection ret = new CodeSegDefinitionCollection();
            //
            foreach ( ProcessCodeSeg codeSeg in CodeSegments )
            {
                CodeSegDefinition codeSegDef = new CodeSegDefinition();
                codeSegDef.Set( codeSeg.ProcessLocalRunAddress, codeSeg.ProcessLocalRunAddressEnd );
                codeSegDef.FileName = codeSeg.FileName;
                ret.Add( codeSegDef );
            }
            //
            return ret;
        }

        internal void PrepareDebugView()
        {
            iDebugEngineView = base.CrashDebugger.DebugEngine.CreateView( string.Format( "DProcess [{0}]", base.Name ) );
        }

        internal Symbol LookUpSymbol( uint aAddress )
        {
            Symbol ret = null;
            //
            if ( iDebugEngineView != null )
            {
                ret = iDebugEngineView.Symbols[ aAddress ];
            }
            else
            {
                ret = base.CrashDebugger.LookUpSymbol( aAddress );
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        public ExitInfo ExitInfo
        {
            get { return iExitInfo; }
        }

        public TProcessFlags Flags
        {
            get { return iFlags; }
            set { iFlags = value; }
        }

        public uint Handles
        {
            get { return iHandles; }
            set { iHandles = value; }
        }

        public TProcessAttributes Attributes
        {
            get { return iAttributes; }
            set { iAttributes = value; }
        }

        public uint DataBssStackChunkAddress
        {
            get { return iDataBssStackChunkAddress; }
            set { iDataBssStackChunkAddress = value; }
        }

        public DChunk DataBssStackChunk
        {
            get { return CrashDebugger.ChunkByAddress( DataBssStackChunkAddress ); }
        }

        public uint CodeSegAddress
        {
            get { return iCodeSegAddress; }
            set
            {
                iCodeSegAddress = value;

                // If the address specified isn't already part of the
                // list of codesegs for the process, then add it.
                if ( !CodeSegments.Contains( value ) )
                {
                    ProcessCodeSeg codeSeg = new ProcessCodeSeg( CrashDebugger, value, 0 );
                    CodeSegments.Add( codeSeg );
                }
            }
        }

        public CodeSegEntry CodeSeg
        {
            get { return CrashDebugger.CodeSegByAddress( CodeSegAddress ); }
        }

        public uint TempCodeSegAddress
        {
            get { return iTempCodeSegAddress; }
            set { iTempCodeSegAddress = value; }
        }

        public CodeSegEntry TempCodeSeg
        {
            get { return CrashDebugger.CodeSegByAddress( TempCodeSegAddress ); }
        }

        public ProcessLockInfo LockInfo
        {
            get { return iLockInfo; }
        }

        public uint SID
        {
            get { return iSID; }
            set { iSID = value; }
        }

        public ProcessCodeSegCollection CodeSegments
        {
            get { return iCodeSegs; }
        }

        public ProcessPageDir PageDirInfo
        {
            get { return iPageDirInfo; }
        }

        public ProcessChunkCollection Chunks
        {
            get { return iChunks; }
        }

        public long Id
        {
            get { return iId; }
            set { iId = value; }
        }

        public Capability Capabilities
        {
            get { return iCapability; }
        }

        public int OSASID
        {
            get { return iOSASID; }
            set { iOSASID = value; }
        }

        public uint LPD
        {
            get { return iLPD; }
            set { iLPD = value; }
        }

        public uint GPD
        {
            get { return iGPD; }
            set { iGPD = value; }
        }

        public bool IsCurrent
        {
            get
            {
                bool ret = CrashDebugger.IsCurrentProcess( this );
                return ret;
            }
        }

        internal DbgEngineView DbgEngineView
        {
            get { return iDebugEngineView; }
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
        private uint iHandles = 0;
        private int iOSASID = 0;
        private uint iCodeSegAddress = 0;
        private uint iDataBssStackChunkAddress = 0;
        private uint iTempCodeSegAddress = 0;
        private uint iSID = 0;
        private long iId = 0;
        private uint iLPD = 0;
        private uint iGPD = 0;
        private DbgEngineView iDebugEngineView = null;
        private ExitInfo iExitInfo = new ExitInfo();
        private Capability iCapability = new Capability();
        private TProcessFlags iFlags = TProcessFlags.EProcessFlagNone;
        private ProcessPageDir iPageDirInfo = new ProcessPageDir();
        private ProcessLockInfo iLockInfo = new ProcessLockInfo();
        private TProcessAttributes iAttributes = TProcessAttributes.ENone;
        private ProcessChunkCollection iChunks = new ProcessChunkCollection();
        private ProcessCodeSegCollection iCodeSegs = new ProcessCodeSegCollection();
        #endregion
    }
}
