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
using CrashItemLib.Crash;
using CrashItemLib.Crash.Base;
using CrashItemLib.Crash.Base.DataBinding;
using SymbianUtils.Range;
using SymbianStructuresLib.Uids;
using SymbianUtils.DataBuffer;
using CrashItemLib.Crash.Container;

namespace CrashItemLib.Crash.Reports
{
    public class CIReportParameter : CIElement
    {
        #region Constructors
        internal CIReportParameter( CIReportInfo aReport, string aName, uint aValue )
            : base( aReport.Container )
		{
            iName = string.IsNullOrEmpty( aName ) ? string.Empty : aName;
            iValue = aValue;
		}
		#endregion

        #region API
        #endregion

        #region Properties
        public override string Name
        {
            get { return iName; }
        }

        public uint Value
        {
            get { return iValue; }
        }
        #endregion

        #region Operators
        public static implicit operator CIDBRow( CIReportParameter aObject )
        {
            CIDBRow row = new CIDBRow();
            //
            row.Add( new CIDBCell( aObject.Name ) );
            row.Add( new CIDBCell( aObject.Value.ToString() ) );
            //
            return row;
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private readonly string iName;
        private readonly uint iValue;
        #endregion
    }
}
