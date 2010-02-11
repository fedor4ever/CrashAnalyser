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
using SymbianUtils;
using CrashItemLib;

namespace CrashItemLib.PluginAPI
{
    public class CFFTraceLine
    {
        #region Constructors
        public CFFTraceLine( string aLine, long aLineNumber, CFFSource aSource )
		{
            iLine = aLine;
            iLineNumber = aLineNumber;
            iSource = aSource;
        }
        #endregion

        #region API
        public byte[] ToBinary()
        {
            List<byte> ret = new List<byte>();

            // We just want to map the raw unicode character onto a single byte.
            // ASCII range is probably not sufficient (guess?) so this is why we
            // do not use System.Text.ASCIIEncoding, but rather roll our own.
            string line = iLine + System.Environment.NewLine;
            foreach ( char c in line )
            {
                byte b = System.Convert.ToByte( c );
                ret.Add( b );
            }

            return ret.ToArray();
        }
        #endregion

        #region Properties
        public string Line
        {
            get { return iLine; }
        }

        public long LineNumber
        {
            get { return iLineNumber; }
        }

        public CFFSource Descriptor
        {
            get { return iSource; }
        }
        #endregion

        #region Operators
        public static implicit operator string( CFFTraceLine aLine )
        {
            return aLine.Line;
        }
        #endregion

		#region Data members
        private readonly string iLine;
        private readonly long iLineNumber;
        private readonly CFFSource iSource;
        #endregion
    }
}
