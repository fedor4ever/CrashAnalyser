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
using SymbianUtils.Enum;
using SymbianUtils.Streams;

namespace SymbianStructuresLib.Security
{
    public class SCapabilitySet : IEnumerable<TCapability>
    {
        #region Constructors
        public SCapabilitySet()
		{
        }

        public SCapabilitySet( SymbianStreamReaderLE aReader )
        {
            ulong caps = aReader.ReadUInt32();
            uint high = aReader.ReadUInt32();
            caps += ( high << 32 );
            //
            for ( int i = 0; i < KMaxBitIndex; i++ )
            {
                ulong checkVal = (ulong) 1 << i;
                if ( ( checkVal & caps ) == checkVal )
                {
                    TCapability cap = (TCapability) i;
                    iCapabilities.Add( cap );
                }
            }
        }
		#endregion

		#region API
        #endregion

		#region Properties
        public bool this[ TCapability aCapability ]
        {
            get
            {
                return iCapabilities.Contains( aCapability );
            }
        }
		#endregion

		#region Internal constants
        private const int KMaxBitIndex = 64;
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            foreach( TCapability cap in iCapabilities )
            {
                string name = EnumUtils.ToString( cap );
                //
                if ( ret.Length > 0 )
                {
                    ret.Append( ", " );
                }
                //
                ret.Append( name );
            }
            //
            return ret.ToString();
        }
        #endregion

        #region From IEnumerable<TCapability>
        public IEnumerator<TCapability> GetEnumerator()
        {
            foreach ( TCapability cap in iCapabilities )
            {
                yield return cap;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( TCapability cap in iCapabilities )
            {
                yield return cap;
            }
        }
        #endregion

        #region Data members
        private List<TCapability> iCapabilities = new List<TCapability>();
        #endregion
    }
}
