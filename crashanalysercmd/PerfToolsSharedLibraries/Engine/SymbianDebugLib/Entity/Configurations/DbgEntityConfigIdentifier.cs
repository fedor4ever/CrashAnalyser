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
﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace SymbianDebugLib.Entity.Configurations
{
    public class DbgEntityConfigIdentifier
    {
        #region Constructors
        public DbgEntityConfigIdentifier()
        {
        }

        public DbgEntityConfigIdentifier( string aId )
        {
            Add( aId );
        }

        public DbgEntityConfigIdentifier( uint aId )
        {
            Add( aId.ToString( "x8" ) );
        }
        #endregion

        #region API
        public void Add( string aId )
        {
            if ( iIds.ContainsKey( aId ) == false )
            {
                iIds.Add( aId, aId );
            }
        }
 
        public bool Contains( DbgEntityConfigIdentifier aId )
        {
            int matchCount = 0;
            //
            foreach ( string key in aId.iIds.Keys )
            {
                if ( Contains( key ) )
                {
                    ++matchCount;
                }
            }
            //
            bool ret = matchCount > 0 && ( matchCount == this.Count );
            return ret;
        }

        public bool Contains( string aIdText )
        {
            bool ret = iIds.ContainsKey( aIdText );
            return ret;
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iIds.Count; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            foreach ( string key in iIds.Keys )
            {
                ret.Append( key + ", " );
            }

            if ( ret.Length > 0 )
            {
                ret.Remove( ret.Length - 2, 2 );
            }

            return ret.ToString();
        }
        #endregion

        #region Data members
        private StringDictionary iIds = new StringDictionary();
        #endregion
    }
}
