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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.InteropServices;

namespace SymbianUtils.MarshallingUtils
{
    public static class MarshalHelper
    {
        #region Constructors
        #endregion

        #region API
        public static T Read<T>( byte[] aData )
        {
            return Read<T>( aData, 0 );
        }

        public static T Read<T>( byte[] aData, int aOffset )
        {
            int size = Marshal.SizeOf( typeof( T ) );

            byte[] buffer = new byte[ size ];
            for ( int i = 0; i < buffer.Length; i++ )
            {
                int pos = aOffset + i;
                buffer[ i ] = aData[ pos ];
            }

            GCHandle handle = GCHandle.Alloc( buffer, GCHandleType.Pinned );
            T ret = (T) Marshal.PtrToStructure( handle.AddrOfPinnedObject(), typeof( T ) );
            handle.Free();

            return ret;
        }
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        #endregion
    }
}
