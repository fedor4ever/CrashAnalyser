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
using System.Text;
using System.Collections.Generic;
using SymbianDebugLib.Engine;
using SymbianDebugLib.PluginAPI.Types;
using SymbianDebugLib.PluginAPI.Types.Symbol;
using SymbianStructuresLib.Uids;
using SymbianUtils.DataBuffer;
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Symbols;
using SymbianUtils;
using SymbianUtils.Range;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Messages;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.Symbols;

namespace CrashItemLib.Crash.CodeSegs
{
    public class CICodeSeg : CIElement
    {
        #region Constructors
        public CICodeSeg( CIProcess aProcess )
            : this( aProcess, new CodeSegDefinition() )
		{
		}
        
        public CICodeSeg( CIProcess aProcess, CodeSegDefinition aCodeSegDef )
            : base( aProcess.Container )
        {
            iOwningProcess = aProcess;
            iCodeSegDef = aCodeSegDef;
        }
        #endregion

        #region API
        public bool Contains( uint aAddress )
        {
            bool ret = Range.Contains( aAddress );
            return ret;
        }

        internal void Resolve( DbgEngineView aDebugEngineView )
        {
            base.Trace( "[CICodeSeg] Resolve() - START - this: {0}", this );
            ResetState();

            // Check whether we have a symbol already loaded for the code segment's base address
            uint baseAddress = this.Base;
            //
            SymbolCollection col = null;
            Symbol symbol = aDebugEngineView.Symbols.Lookup( baseAddress, out col );
            base.Trace( "[CICodeSeg] Resolve() - symbol: {0}, symbolCollection: {1}", symbol, col );
            //
            if ( symbol != null )
            {
                // This must be valid if we found a symbol
                System.Diagnostics.Debug.Assert( col != null );
            }
            else
            {
                // Symbol engine is not aware of the code segment's base address, but we can check by name
                // as well...
                col = aDebugEngineView.Symbols[ iCodeSegDef ];
            }

            // Update state
            IsResolved = ( col != null );
            AssociatedSymbolCollection = col;

            base.Trace( "[CICodeSeg] Resolve() - END - this: {0}, resolved: {1}, iCodeSegDef.FileName: {2}", this, this.IsResolved, iCodeSegDef.FileName );
        }
        #endregion

        #region Properties
        public override string Name
        {
            get { return iCodeSegDef.FileName; }
            set 
            {
                iCodeSegDef.FileName = value;
            }
        }

        public uint Base
        {
            get { return iCodeSegDef.Base; }
            set { iCodeSegDef.Base = value; }
        }

        public uint Limit
        {
            get { return iCodeSegDef.Limit; }
            set
            {
                iCodeSegDef.Limit = value; 
            }
        }

        public uint Size
        {
            get 
            { 
                // The address range contains all-inclusive values.
                //
                // E.g. a range of 0x00 -> 0x10 would include the values...:
                //
                //   0x00, 01, 02, 03, 04, 05, 06, 07, 08, 09, 0A, 0B, 0C, 0D, 0E, 0F, 0x10
                //
                // ...and therefore AddressRange.Size would return 17.
                //
                // Symbian OS treats the range as non-inclusive, so the value is one too large.
                // Hence we subtract one
                uint ret = iCodeSegDef.Size - 1;
                return ret;
            }
        }

        public uint Checksum
        {
            get { return iCodeSegDef.Checksum; }
            set { iCodeSegDef.Checksum = value; }
        }

        public bool IsResolved
        {
            get { return iIsResolved; }
            private set
            { 
                iIsResolved = value;
                base.Trace( "[CICodeSeg] Resolve() - this: {0}, resolved: {1}", this, iIsResolved );
            }
        }

        public bool IsMismatched
        {
            get { return MismatchAddress != KNoSymbolicMismatchAddress; }
        }

        public bool IsSpeculative
        {
            get { return !IsExplicit; }
        }

        public bool IsExplicit
        {
            get { return iIsExplicit; }
            internal set { iIsExplicit = value; }
        }

        public uint MismatchAddress
        {
            get { return iMismatchAddress; }
            private set { iMismatchAddress = value; }
        }

        public AddressRange Range
        {
            get
            {
                // We do this so that calling Range.ToString() won't include a file name!
                AddressRange ret = new AddressRange( iCodeSegDef );
                return ret;
            }
        }

        public CIProcess OwningProcess
        {
            get 
            {
                System.Diagnostics.Debug.Assert( iOwningProcess != null );
                return iOwningProcess; 
            }
        }

        public SymbolCollection AssociatedSymbolCollection
        {
            get { return iAssociatedSymbolCollection; }
            private set
            {
                iAssociatedSymbolCollection = value;
                if ( iAssociatedSymbolCollection != null )
                {
                    bool misMatch = ( iAssociatedSymbolCollection.BaseAddress != this.Base );
                    if ( misMatch )
                    {
                        MismatchAddress = iAssociatedSymbolCollection.BaseAddress;
                    }
                }
            }
        }
        #endregion

        #region Operators
        public static implicit operator CodeSegDefinition( CICodeSeg aCodeSeg )
        {
            return aCodeSeg.iCodeSegDef;
        }
        #endregion

        #region Internal constants
        private const uint KNoSymbolicMismatchAddress = 0;
        #endregion

        #region Internal methods
        private void ResetState()
        {
            IsResolved = false;
            MismatchAddress = 0;
        }

        private Symbol BaseAddressSymbolAndCollection( DbgViewSymbol aSymbolView, out SymbolCollection aCollection )
        {
            Symbol ret = aSymbolView.Lookup( this.Base, out aCollection );
            return ret;
        }
        #endregion

        #region From CIElement
        internal override void OnFinalize( CIElementFinalizationParameters aParams )
        {
            try
            {
                base.OnFinalize( aParams );
            }
            finally
            {
                Resolve( aParams.DebugEngineView );
            }
        }
        #endregion
        
        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.AppendFormat( "{0:x8}-{1:x8} {2}", this.Base, this.Limit, this.Name );
            return ret.ToString();
        }
        #endregion

        #region Data members
        private readonly CIProcess iOwningProcess;
        private bool iIsResolved = false;
        private bool iIsExplicit = false;
        private uint iMismatchAddress = KNoSymbolicMismatchAddress;
        private CodeSegDefinition iCodeSegDef = new CodeSegDefinition();
        private SymbolCollection iAssociatedSymbolCollection = null;
        #endregion
    }
}
