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
    internal class CSROpScanSymbol : CodeSegResolverOperation
    {
        #region Constructors
        public CSROpScanSymbol( CodeSegResolver aResolver, bool aResetResolver, FileInfo aFile, IEnumerable<string> aBinaryNames )
            : base( aResolver, aResetResolver )
        {
            iFile = aFile;
            //
            foreach ( string binary in aBinaryNames )
            {
                iBinaries.Add( binary );
            }
        }
        #endregion

        #region From CSROperation
        protected override void Scan()
        {
            base.DriveLetter = Path.GetPathRoot( iFile.FullName );
            foreach ( string binary in iBinaries )
            {
                string envFileName = binary;
                if ( envFileName[ 0 ] == Path.DirectorySeparatorChar )
                {
                    envFileName = envFileName.Substring( 1 );
                }
                envFileName = CombineWithDriveLetter( envFileName );
                string imageFileName = CodeSegResolver.KROMBinaryPath + Path.GetFileName( envFileName );
                //
                CodeSegResolverEntry entry = new CodeSegResolverEntry( envFileName, imageFileName );
                //
                base.Add( entry );
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
        private List<string> iBinaries = new List<string>();
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
