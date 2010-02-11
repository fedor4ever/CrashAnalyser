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
    public class TCheckedUid
    {
        #region Constructors
        public TCheckedUid()
		{
        }

        public TCheckedUid( SymbianStreamReaderLE aReader )
        {
            iType = new UidType( aReader );
            iCheck = aReader.ReadUInt32();
        }
		#endregion

		#region API
        #endregion

		#region Properties
        public uint Check
        {
            get
            {
                return iCheck;
            }
            set
            {
                iCheck = value;
            }
        }

        public UidType Type
        {
            get { return iType; }
            set { iType = value; }
        }
		#endregion

		#region Internal methods
		#endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.AppendFormat( "{0} [{1:x8}]", iType, iCheck );
            return ret.ToString();
        }
        #endregion

        #region Data members
        private UidType iType = new UidType();
        private uint iCheck = 0;
        #endregion
    }
}
