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
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Base.DataBinding;
using CrashItemLib.Crash.Utils;
using CrashItemLib.Crash.Container;

namespace CrashItemLib.Crash.Traces
{
	public class CITrace : CIElement
    {
        #region Constructors
        public CITrace( CIContainer aContainer, TraceLine aLine )
            : base( aContainer )
		{
            iLine = aLine;
		}
        #endregion

        #region API
        #endregion

        #region Properties
        public string Payload
        {
            get { return iLine.Payload; }
        }
        
        public string Prefix
        {
            get { return iLine.Prefix; }
        }

        public string Suffix
        {
            get { return iLine.Suffix; }
        }

        public TraceTimeStamp TimeStamp
        {
            get { return iLine.TimeStamp; }
        }
        #endregion

        #region Operators
        public static implicit operator CIDBRow( CITrace aTrace )
        {
            CIDBRow row = new CIDBRow();

            // To ensure that the trace and cells are correctly associated
            row.Element = aTrace;
            row.Add( new CIDBCell( aTrace.Prefix ) );
            row.Add( new CIDBCell( aTrace.Payload ) );
            row.Add( new CIDBCell( aTrace.Suffix ) );
            //
            return row;
        }

        public static implicit operator TraceLine( CITrace aTrace )
        {
            return aTrace.iLine;
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return iLine.ToString();
        }
        #endregion

        #region Data members
        private readonly TraceLine iLine;
        #endregion
    }
}
