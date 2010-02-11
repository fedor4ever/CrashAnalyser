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
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using SymbianUtils;
using SymbianUtils.SerializedOperations;
using SymbolLib.Sources.Map.Parser;

namespace SymbolLib.CodeSegDef
{
    internal class CSROpScanObey : CodeSegResolverOperation
    {
        #region Constructors
        public CSROpScanObey( CodeSegResolver aResolver, bool aResetResolver, FileInfo aFile )
            : base( aResolver, aResetResolver )
        {
            iFile = aFile;
        }
        #endregion

        #region From CSROperation
        protected override void Scan()
        {
            base.DriveLetter = Path.GetPathRoot( iFile.FullName );
            if ( iFile.Exists )
            {
                using ( StreamReader reader = new StreamReader( iFile.FullName ) )
                {
                    string line = reader.ReadLine();
                    //
                    while ( line != null )
                    {
                        Match m = KMapParserRegex.Match( line );
                        if ( m.Success )
                        {
                            string nameHost = base.CombineWithDriveLetter( m.Groups[ "Host]" ].Value );
                            string nameDevice = Path.Combine( KRomDriveFileLetter, m.Groups[ "Device" ].Value );
                            //
                            CodeSegResolverEntry entry = new CodeSegResolverEntry( nameHost, nameDevice );
                            //
                            base.Add( entry );
                        }
                        //
                        line = reader.ReadLine();
                    }
                }
            }
        }
        #endregion

        #region API
        #endregion

        #region Properties
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        private const string KRomDriveFileLetter = "Z:\\";
        #endregion

        #region Data members
        private readonly FileInfo iFile;
        public static readonly Regex KMapParserRegex = new Regex(
             "(?<Type>.+)\r\n\\=\r\n(?<Host>.+?)(?:\\s+)\\x22(?<Device>.+)\\x2" +
             "2",
           RegexOptions.CultureInvariant
           | RegexOptions.IgnorePatternWhitespace
           | RegexOptions.Compiled
           );
        #endregion
    }
}
