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
using SymbianStructuresLib.Debug.Trace;
using SymbianDebugLib.PluginAPI.Types.Trace;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Base.DataBinding;
using CrashItemLib.Crash.Utils;
using CrashItemLib.Crash.Container;

namespace CrashItemLib.Crash.Traces
{
    [CIDBAttributeColumn( "Prefix", 0 )]
    [CIDBAttributeColumn( "Payload", 1, true )]
    [CIDBAttributeColumn( "Suffix", 2 )]
    public class CITraceData : CIElement, IEnumerable<CITrace>
    {
        #region Constructors
        [CIElementAttributeMandatory()]
        public CITraceData( CIContainer aContainer )
            : base( aContainer )
        {
            // Restrict children to traces
            base.AddSupportedChildType( typeof( CITrace ) );

            // Must be done at the end because it creates elements
            base.IsToBeFinalizedLast = true;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public byte[] RawData
        {
            get { return iRawData != null ? iRawData : new byte[] { }; }
            set
            {
                iRawData = value;
            }
        }

        public TraceLine[] Lines
        {
            get
            {
                TraceLine[] ret = iLines;
                if ( ret == null )
                {
                    ret = new TraceLine[] { };
                }
                return ret;
            }
        }
        #endregion

        #region From IEnumerable<CITrace>
        public new IEnumerator<CITrace> GetEnumerator()
        {
            foreach ( CIElement element in base.Children )
            {
                if ( element is CITrace )
                {
                    CITrace reg = (CITrace) element;
                    yield return reg;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( CIElement element in base.Children )
            {
                if ( element is CITrace )
                {
                    CITrace reg = (CITrace) element;
                    yield return reg;
                }
            }
        }
        #endregion

        #region From CIElementBase
        public override void PrepareRows()
        {
            DataBindingModel.ClearRows();

            // Our data binding model is based upon the trace object, rather
            // than any key-value-pair properties.
            foreach ( CITrace t in this )
            {
                DataBindingModel.Add( t );
            }
        }

        internal override void OnFinalize( CIElementFinalizationParameters aParams )
        {
            base.OnFinalize( aParams );
            //
            DecodeTraces( aParams.DebugEngine.TraceDictionaries );
        }
        #endregion

        #region Internal methods
        private void DecodeTraces( DbgEngineTrace aTraceDecoder )
        {
            if ( iRawData != null )
            {
                iLines = aTraceDecoder.Decode( iRawData );
                if ( iLines != null && iLines.Length > 0 )
                {
                    foreach ( TraceLine line in iLines )
                    {
                        CITrace entry = new CITrace( base.Container, line );
                        base.AddChild( entry );
                    }
                }
            }
        }
        #endregion

        #region Data members
        private byte[] iRawData = null;
        private TraceLine[] iLines = null;
        #endregion
    }
}
