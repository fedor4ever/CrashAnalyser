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
using System.IO;
using System.Collections.Generic;
using System.Text;
using SymbianUtils.Streams;

namespace SymbianStructuresLib.Uids
{
    public class UidType
    {
        #region Constructors
        public UidType()
		{
            iUids.Add( 0 );
            iUids.Add( 0 );
            iUids.Add( 0 );
        }

        public UidType( SymbianStreamReaderLE aReader )
        {
            iUids.Add( aReader.ReadUInt32() );
            iUids.Add( aReader.ReadUInt32() );
            iUids.Add( aReader.ReadUInt32() );
        }
		#endregion

		#region API
        #endregion

		#region Properties
        public int Count
        {
            get { return iUids.Count; }
        }

        public uint this[ int aIndex ]
        {
            get
            {
                return iUids[ aIndex ];
            }
            set
            {
                iUids[ aIndex ] = value;
            }
        }

        public uint MostSignificant
        {
            get
            {
                uint ret = 0;
                //
                if ( Count > 0 )
                {
                    ret = this[ Count - 1 ];
                }
                //
                return ret;
            }
            set
            {
                iUids[ iUids.Count - 1 ] = value;
            }
        }
		#endregion

		#region Internal methods
		#endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            for( int i=0; i<iUids.Count; i++ )
            {
                uint val = iUids[ i ];
                //
                {
                    ret.AppendFormat( "{0:x8}", val );
                    if ( i < 2 )
                    {
                        ret.Append( ", " );
                    }
                }
            }
            //
            return ret.ToString();
        }
        #endregion

        #region Data members
        private List<uint> iUids = new List<uint>();
        #endregion
    }
}
