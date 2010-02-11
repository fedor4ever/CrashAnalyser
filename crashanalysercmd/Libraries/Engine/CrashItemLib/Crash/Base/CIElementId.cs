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
using SymbianDebugLib.Engine;
using CrashItemLib.Crash.Messages;

namespace CrashItemLib.Crash.Base
{
	public class CIElementId : IComparable<CIElementId>
	{
		#region Constructors
        public CIElementId()
            : this( KDefaultValue )
        {
        }

        public CIElementId( long aId )
		{
            iId = aId;
		}

        public CIElementId( CIElementId aId )
		{
            iId = aId.Id;
		}
		#endregion

        #region API
        #endregion

        #region Properties
        public long Id
        {
            get { return iId; }
            set { iId = value; }
        }
        #endregion

        #region Operators
        public static implicit operator long( CIElementId aElement )
        {
            return aElement.Id;
        }

        public static implicit operator CIElementId( long aValue )
        {
            return new CIElementId( aValue );
        }

        public static implicit operator string( CIElementId aElement )
        {
            return aElement.ToString();
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return iId.ToString();
        }
        #endregion

        #region From IComparable<CIElementId>
        public int CompareTo( CIElementId aOther )
        {
            int ret = 1;
            //
            if ( aOther != null )
            {
                ret = iId.CompareTo( aOther.Id );
            }
            //
            return ret;
        }
        #endregion

        #region Internal constants
        private const long KDefaultValue = -1;
        #endregion

        #region Data members
        private long iId = KDefaultValue;
		#endregion
	}
}
