/*
* Copyright (c) 2004-2008 Nokia Corporation and/or its subsidiary(-ies).
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
using CrashDebuggerLib.Structures.KernelObjects;
using CrashDebuggerLib.Structures.Common;

namespace CrashDebuggerLib.Structures.CodeSeg
{
    public class CodeSegEntry : CrashDebuggerAware
    {
        #region Constructors
        public CodeSegEntry( CrashDebuggerInfo aCrashDebugger )
            : this( aCrashDebugger, 0, string.Empty )
        {
        }

        public CodeSegEntry( CrashDebuggerInfo aCrashDebugger, uint aAddress, string aFileName )
            : base( aCrashDebugger )
        {
            KernelAddress = aAddress;
            FileName = aFileName;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint KernelAddress
        {
            get { return iKernelAddress; }
            set { iKernelAddress = value; }
        }

        public uint RunAddress
        {
            get { return iRunAddress; }
            set { iRunAddress = value; }
        }

        public uint RunAddressEnd
        {
            get { return RunAddress + Size; }
        }

        public uint Size
        {
            get { return iSize; }
            set { iSize = value; }
        }

        public string FileName
        {
            get { return iFileName; }
            set { iFileName = value; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            ret.Append( RunAddress.ToString( "x8" ) );
            if ( Size != 0 )
            {
                ret.Append( "-" );
                ret.Append( RunAddressEnd.ToString( "x8" ) );
            }
            ret.Append( " " );
            ret.Append( FileName );
            //
            return ret.ToString();
        }
        #endregion

        #region Data members
        private uint iKernelAddress = 0;
        private uint iRunAddress = 0;
        private uint iSize = 0;
        private string iFileName = string.Empty;
        #endregion
    }
}
