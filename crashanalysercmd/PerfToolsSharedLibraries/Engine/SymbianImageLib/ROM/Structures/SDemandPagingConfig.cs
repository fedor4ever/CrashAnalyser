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
using System.IO;
using SymbianStructuresLib.Version;

namespace SymbianImageLib.ROM.Structures
{
    internal class SDemandPagingConfig
    {
        #region Constructors
        #endregion

        #region API
        public void Read( BinaryReader aReader )
        {
            iMinPages = aReader.ReadUInt16();
            iMaxPages = aReader.ReadUInt16();
            iYoungOldRatio = aReader.ReadUInt16();
        }
        #endregion

        #region Properties
        public uint Size
        {
            get 
            { 
                int ret = 6 + ( iSpare.Length * 2 );
                return (uint) ret;
            }
        }

        public ushort MinPages
        {
            get { return iMinPages; }
        }

        public ushort MaxPages
        {
            get { return iMaxPages; }
        }

        public ushort YoungOldRatio
        {
            get { return iYoungOldRatio; }
        }
        #endregion

        #region Internal constants
        #endregion

        #region Data members
	    private ushort iMinPages;
        private ushort iMaxPages;
        private ushort iYoungOldRatio;
        private ushort[] iSpare = new ushort[3];
        #endregion
    }
}
