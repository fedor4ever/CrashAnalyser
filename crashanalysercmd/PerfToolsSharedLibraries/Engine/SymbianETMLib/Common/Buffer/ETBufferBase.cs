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
using SymbianUtils.BasicTypes;
using SymbianETMLib.Common.Types;
using SymbianETMLib.Common.Exception;

namespace SymbianETMLib.Common.Buffer
{
    public abstract class ETBufferBase
    {
        #region Constructors
        protected ETBufferBase()
        {
        }

        protected ETBufferBase( string aFileName )
        {
            byte[] buffer = File.ReadAllBytes( aFileName );
            AddRange( buffer );
        }
        #endregion

        #region API
        public void Clear()
        {
            iData.Clear();
        }

        public void AddRange( IEnumerable<byte> aBytes )
        {
            iData.AddRange( aBytes );
        }

        public void PushBack( SymByte aByte )
        {
            iData.Insert( 0, aByte.Value );
        }

        public SymByte Dequeue()
        {
            if ( IsEmpty )
            {
                throw new ETMException( "ERROR - buffer is empty" );
            }
            //
            byte head = iData[ 0 ];
            iData.RemoveAt( 0 );
            return new SymByte( head );
        }
        #endregion

        #region Properties
        public bool IsEmpty
        {
            get { return iData.Count == 0; }
        }

        public int Count
        {
            get { return iData.Count; }
        }
        #endregion

        #region Internal methods
        protected List<byte> Data
        {
            get { return iData; }
            set { iData = value; }
        }
        #endregion

        #region Data members
        List<byte> iData = new List<byte>();
        #endregion
    }
}
