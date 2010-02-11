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
using System.IO;
using System.Text;
using SymbianETMLib.Common.Types;
using SymbianETMLib.Common.Exception;
using SymbianETMLib.Common.Buffer;

namespace SymbianETMLib.ETB.Buffer
{
    public class ETBBuffer : ETBufferBase
    {
        #region Constructors
        public ETBBuffer()
        {
        }

        public ETBBuffer( string aFileName )
            : base( aFileName )
        {
        }
        #endregion

        #region API
        public void Reorder( uint aAddressOfFirstByte )
        {
            List<byte> data = base.Data;
            //
            if ( aAddressOfFirstByte > data.Count )
            {
                throw new ETMException( "ERROR - initial write pointer is out-of-bounds" );
            }
            else if ( aAddressOfFirstByte != 0 )
            {
                List<byte> newData = new List<byte>();
                //
                int count = data.Count;
                for ( int i = (int) aAddressOfFirstByte; i < count; i++ )
                {
                    byte b = data[ i ];
                    newData.Add( b );
                }
                for ( int i = 0; i < (int) aAddressOfFirstByte; i++ )
                {
                    byte b = data[ i ];
                    newData.Add( b );
                }
                // 
                base.Data = newData;
            }
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