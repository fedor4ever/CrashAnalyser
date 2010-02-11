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

namespace SymbianStructuresLib.Security
{
    public class SSecurityInfo
    {
        #region Constructors
        public SSecurityInfo()
		{
            iCaps = new SCapabilitySet();
        }

        public SSecurityInfo( SymbianStreamReaderLE aReader )
        {
            iSecureId = aReader.ReadUInt32();
            iVendorId = aReader.ReadUInt32();
            iCaps = new SCapabilitySet( aReader );
        }
		#endregion

		#region API
        #endregion

		#region Properties
        public uint SecureId
        {
            get
            {
                return iSecureId;
            }
            set
            {
                iSecureId = value;
            }
        }

        public uint VendorId
        {
            get
            {
                return iVendorId;
            }
            set
            {
                iVendorId = value;
            }
        }

        public SCapabilitySet Capabilities
        {
            get { return iCaps; }
        }
		#endregion

		#region Internal methods
		#endregion

        #region From System.Object
        #endregion

        #region Data members
	    private uint iSecureId = 0;
	    private uint iVendorId = 0;
	    private readonly SCapabilitySet iCaps;
        #endregion
    }
}
