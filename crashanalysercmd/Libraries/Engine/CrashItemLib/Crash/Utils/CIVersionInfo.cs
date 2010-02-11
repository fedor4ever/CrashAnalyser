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

namespace CrashItemLib.Crash.Utils
{
	public sealed class CIVersionInfo
	{
        #region Constructors
        internal CIVersionInfo()
            : this( string.Empty )
        {
        }

        internal CIVersionInfo( string aValue )
            : this( string.Empty, aValue )
        {
        }

        internal CIVersionInfo( string aName, string aValue )
        {
            Name = aName;
            Value = aValue;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public bool IsValid
        {
            get { return !String.IsNullOrEmpty( Value ); }
        }

        public string Value
        {
            get { return iValue; }
            set
            { 
                iValue = value;
                if ( value == null )
                {
                    iValue = string.Empty;
                }
            }
        }

        public string Name
        {
            get { return iName; }
            set
            {
                iName = value;
                if ( value == null )
                {
                    iName = string.Empty;
                }
            }
        }
        #endregion

        #region Operators
        public static implicit operator string( CIVersionInfo aVersion )
        {
            return aVersion.Value;
        }

        public static implicit operator CIVersionInfo( string aText )
        {
            return new CIVersionInfo( aText );
        }

        public static implicit operator CIDBRow( CIVersionInfo aVersion )
        {
            CIDBRow row = new CIDBRow();
            //
            row.Add( new CIDBCell( aVersion.Name ) );
            row.Add( new CIDBCell( aVersion.Value ) );
            //
            return row;
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            if ( string.IsNullOrEmpty( iName ) )
            {
                ret.Append( iValue );
            }
            else
            {
                ret.AppendFormat( "{0} = {1}", iName, iValue );
            }
            //
            return ret.ToString();
        }
        #endregion

        #region Data members
        private string iValue = string.Empty;
        private string iName = string.Empty;
        #endregion
    }
}
