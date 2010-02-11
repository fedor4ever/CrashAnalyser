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
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using CrashItemLib.Crash;
using CrashItemLib.Crash.Processes;
using CrashItemLib.Crash.Threads;
using SymbianStructuresLib.Uids;

namespace CrashItemLib.Crash.Utils
{
	public sealed class CIFullNameUtils
	{
        #region Constructors
        public CIFullNameUtils( string aFullName )
        {
            iMatch = KFullNameRegEx.Match( aFullName );
        }
        #endregion

        #region API
        public void GetProcessInfo( CIProcess aProcess )
        {
            if ( IsValid )
            {
                aProcess.Name = Process;
                aProcess.Generation = Generation;
                aProcess.SID = SID;
            }
        }

        public void GetThreadInfo( CIThread aThread )
        {
            if ( IsValid )
            {
                aThread.Name = Thread;
            }
        }
        #endregion

        #region Properties
        public bool IsValid
        {
            get { return iMatch.Success; }
        }

        public string Process
        {
            get
            {
                string ret = string.Empty;
                //
                if ( IsValid )
                {
                    ret = iMatch.Groups[ "ProcessName" ].Value;
                }
                //
                return ret;
            }
        }

        public string Thread
        {
            get
            {
                string ret = string.Empty;
                //
                if ( IsValid )
                {
                    ret = iMatch.Groups[ "ThreadName" ].Value;
                }
                //
                return ret;
            }
        }

        public uint SID
        {
            get
            {
                uint ret = 0;
                //
                if ( IsValid )
                {
                    ret = uint.Parse( iMatch.Groups[ "SID" ].Value, System.Globalization.NumberStyles.AllowHexSpecifier );
                }
                //
                return ret;
            }
        }

        public int Generation
        {
            get
            {
                int ret = 0;
                //
                if ( IsValid )
                {
                    ret = int.Parse( iMatch.Groups[ "Generation" ].Value );
                }
                //
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region Internal regular expressions
        private static readonly Regex KFullNameRegEx = new Regex(
              "(?<ProcessName>.+)\\[(?<SID>[A-Fa-f0-9]{8})\\](?<Generation>" +
              "\\p{N}{4})\\:\\:(?<ThreadName>.+)",
            RegexOptions.CultureInvariant
            | RegexOptions.Compiled
            );
        #endregion

        #region Data members
        private readonly Match iMatch;
        #endregion
    }
}
