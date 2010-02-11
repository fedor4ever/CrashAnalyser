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
using SymbianUtils.Streams;

namespace SymbianStructuresLib.Version
{
    public class TVersion
    {
        #region Constructors
        public TVersion()
        {
            iMajor = 0;
            iMinor = 0;
            iBuild = 0;
        }

        public TVersion( SymbianStreamReaderLE aReader )
        {
            iMajor = aReader.ReadInt8();
            iMinor = aReader.ReadInt8();
            iBuild = aReader.ReadInt16();
        }
        #endregion

        #region API
        public void Read( BinaryReader aReader )
        {
            Major = aReader.ReadSByte();
            Minor = aReader.ReadSByte();
            Build = aReader.ReadInt16();
        }
        #endregion

        #region Properties
        public uint Size
        {
            get { return 4; }
        }

        public sbyte Major
        {
            get { return iMajor; }
            set { iMajor = value; }
        }

        public sbyte Minor
        {
            get { return iMinor; }
            set { iMinor = value; }
        }

        public Int16 Build
        {
            get { return iBuild; }
            set { iBuild = value; }
        }
        #endregion
        
        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.AppendFormat( "{0}.{1}.{2:d4}", iMajor, iMinor, iBuild );
            return ret.ToString();
        }
        #endregion

        #region Internal methods
        #endregion

        #region Data members
        private sbyte iMajor;
        private sbyte iMinor;
        private Int16 iBuild;
        #endregion
    }
}
