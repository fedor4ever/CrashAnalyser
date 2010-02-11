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

namespace SymbianImageLib.ROM.Structures
{
    internal class TRomLoad
    {
        #region Constructors
        #endregion

        #region API
        public void Read( BinaryReader aReader )
        {
            ReadBytesAsChars( aReader, iName );
            ReadBytesAsChars( aReader, iVersionStr );
            ReadBytesAsChars( aReader, iBuildNumStr );
            iRomSize = aReader.ReadUInt32();
            iWrapSize = aReader.ReadUInt32();
        }
        #endregion

        #region Properties
        public static uint Size
        {
            get
            {
                uint ret = 0;
                //
                ret += KRomNameSize;
                ret += 4;  // iVersionStr
                ret += 4;  // iBuildNumStr
                ret += 4;  // iRomSize
                ret += 4;  // iWrapSize
                //
                return ret;
            }
        }

        public string Name
        {
            get
            {

                StringBuilder ret = new StringBuilder();
                ret.Append( iName );
                return ret.ToString();
            }
        }

        public string Version
        {
            get
            {
                StringBuilder ret = new StringBuilder();
                ret.Append( iVersionStr );
                return ret.ToString();
            }
        }

        public string BuildNumber
        {
            get
            {
                StringBuilder ret = new StringBuilder();
                ret.Append( iBuildNumStr );
                return ret.ToString();
            }
        }
        #endregion

        #region Internal constants
        private const int KRomNameSize = 16;
        #endregion

        #region Internal methods
        private void ReadBytesAsChars( BinaryReader aReader, char[] aDest )
        {
            byte[] bytes = aReader.ReadBytes( aDest.Length );
            //
            int pos = 0;
            foreach ( byte b in bytes )
            {
                aDest[ pos++ ] = System.Convert.ToChar( b );
            }
        }
        #endregion

        #region Data members
        private char[] iName = new char[ KRomNameSize ];
        private char[] iVersionStr = new char[ 4 ];
        private char[] iBuildNumStr = new char[ 4 ];
        private uint iRomSize;
        private uint iWrapSize;
        #endregion
    }
}
