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
using System.Reflection;

namespace CrashDebuggerLib.Structures.UserContextTable
{
    // <summary>
    // Return table of pointers to user context tables.
    // 
    // Each user context table is an array of UserContextTableEntry objects, one per
    // ARM CPU register, in the order defined in TArmRegisters.
    // 
	// The master table contains pointers to the user context tables in the order
	// defined in TUserContextType.  There are as many user context tables as
	// scenarii leading a user thread to switch to privileged mode.
    // </summary>
    internal class UserContextTableManager
    {
        #region Constructors
        public UserContextTableManager()
        {
            Array vals = Enum.GetValues( typeof( TUserContextType ) );
            foreach ( object val in vals )
            {
                TUserContextType value = (TUserContextType) val;
                iTables.Add( new UserContextTable( value ) );
            }
        }
        #endregion

        #region API
        public void Dump()
        {
            int i =0;
            foreach ( UserContextTable table in iTables )
            {
                TUserContextType type = (TUserContextType) i;
                string text = table.ToString();
                System.Diagnostics.Debug.WriteLine( string.Format( "Table[{0:d2}] = {1} {2}", i, text, type ) );
                i++;
            }
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iTables.Count; }
        }

        public UserContextTable this[ TUserContextType aType ]
        {
            get
            {
                UserContextTable ret = null;
                //
                foreach ( UserContextTable table in iTables )
                {
                    if ( table.Type == aType )
                    {
                        ret = table;
                        break;
                    }
                }
                //
                if ( ret == null )
                {
                    throw new ArgumentException();
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private List<UserContextTable> iTables = new List<UserContextTable>();
        #endregion
    }
}
