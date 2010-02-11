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
using System.Collections;
using System.Text;
using SymbianStructuresLib.Debug.Common.Interfaces;

namespace SymbianStructuresLib.Debug.Common.Id
{
    public class PlatformId : IComparable<PlatformId>
    {
        #region Constructors
        public PlatformId( uint aId )
        {
            iValue = aId;
        }

        public PlatformId( PlatformId aId )
            : this( aId.Value )
        {
        }
        #endregion

        #region Constants
        public const uint KInitialValue = 0;
        #endregion

        #region API
        #endregion

        #region Properties
        public uint Value
        {
            get { return iValue; }
            set { iValue = value; }
        }
        #endregion

        #region Operators
        public static implicit operator ulong( PlatformId aElement )
        {
            return aElement.Value;
        }
 
        public static implicit operator string( PlatformId aElement )
        {
            return aElement.ToString();
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return iValue.ToString();
        }
        #endregion

        #region From IComparable<PlatformId>
        public int CompareTo( PlatformId aOther )
        {
            int ret = 1;
            //
            if ( aOther != null )
            {
                ret = this.Value.CompareTo( aOther.Value );
            }
            //
            return ret;
        }
        #endregion

        #region Data members
        private uint iValue = KInitialValue;
        #endregion
    }
}