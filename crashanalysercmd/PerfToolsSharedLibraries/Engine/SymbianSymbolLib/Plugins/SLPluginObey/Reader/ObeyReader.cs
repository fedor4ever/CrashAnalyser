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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SymbianStructuresLib.Debug.Symbols;
using SymbianSymbolLib.SourceManagement.Source;
using SymbianSymbolLib.SourceManagement.Provisioning;
using SymbianUtils.FileTypes;
using SymbianUtils;

namespace SLPluginObey.Reader
{
    internal class ObeyFileReader : AsyncTextFileReader
    {
        #region Delegates & events
        public delegate void ObyEntryHandler( ObeyFileReader aReader, string aHost, string aDevice );
        public event ObyEntryHandler EntryRead;
        #endregion

        #region Constructors
        public ObeyFileReader( string aFileName )
            : base( aFileName )
        {
            // We should identify what kind of prefix to apply to all the (host) file name
            // entries that we read from the OBY file.
            //
            // For example, the OBY might say:
            //
            //  \epoc32\release\ARMV5\urel\somebinary.dll    "..."
            //
            // We must work out where that '\epoc32' directory is relative to, and then
            // use this as the basis of the prefix to apply to every line we read
            // from the OBY itself.
            //
            // Pre-conditions:
            //  * We will only ever look on the host drive that contains the actual OBY file.
            //
            // Algorithm:
            //
            // 1) We will search up the directory tree, starting at the directory containing the OBY
            //    and check whether it contains an \epoc32\ subdirectory. 
            // 2) If an \epoc32 subdirectory exists, then this is our prefix.
            // 3) If no such subdirectory exists, then we pop a level from the directory (i.e. move to parent)
            //    and try again.
            // 4) Eventually, we'll end up with [OBY drive letter]:\epoc32\ - at this point, even if
            //    the specified directory does not exist, we'll just give up.
            //
            
            // Fail safe
            iRootPathToApplyToAllHostFileNames = Path.GetPathRoot( base.FileName );
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo( Path.GetDirectoryName( aFileName ) );
                while( dirInfo.Exists )
                {
                    string path = Path.Combine( dirInfo.FullName, KStandardEpoc32Path );
                    if ( Directory.Exists( path ) )
                    {
                        iRootPathToApplyToAllHostFileNames = dirInfo.FullName;
                        break;
                    }
                    else
                    {
                        dirInfo = new DirectoryInfo( dirInfo.Parent.FullName );
                    }
                }
            }
            catch
            {
            }
        }
        #endregion

        #region API
        public void Read( TSynchronicity aSynchronicity )
        {
            base.StartRead( aSynchronicity );
        }
        #endregion

        #region Properties
        #endregion

        #region From AsyncTextFileReader
        protected override void HandleFilteredLine( string aLine )
        {
            Match m = KMapParserRegex.Match( aLine );
            if ( m.Success )
            {
                GroupCollection groups = m.Groups;
                string type = groups[ "Type" ].Value;
                string host = groups[ "Host" ].Value;
                string device = groups[ "Device" ].Value;
                
                // Fix up names
                try
                {
                    if ( type != "ROFS_HEADER" )
                    {
                        host = CombineWithHostDrive( host );
                        device = CombineWithDeviceDrive( device );

                        if ( EntryRead != null )
                        {
                            EntryRead( this, host, device );
                        }
                    }
                }
                catch ( Exception )
                {
                    base.Trace( "WARNING: exception when trying to parse OBY line: [{0}], file: [{1}]", aLine, base.FileName );
                }
            }
        }
        #endregion

        #region Internal constants
        public static readonly Regex KMapParserRegex = new Regex(
             @"(?<Type>.+)\=(\x22|)(?<Host>.+?)(\x22|)(?:\s+)\x22(?<Device>.+)\x22",
           RegexOptions.CultureInvariant
           | RegexOptions.IgnorePatternWhitespace
           | RegexOptions.Compiled
           );
        private const string KStandardEpoc32Path = @"epoc32\";
        #endregion

        #region Internal methods
        private string CombineWithHostDrive( string aFileAndPath )
        {
            string fileName = aFileAndPath.Trim();
            if ( fileName.StartsWith( @"\" ) )
            {
                fileName = fileName.Substring( 1 );
            }

            string ret = Path.Combine( iRootPathToApplyToAllHostFileNames, fileName );
            return ret;
        }

        private string CombineWithDeviceDrive( string aFileAndPath )
        {
            string ret = Path.Combine( @"Z:\", aFileAndPath );
            return ret;
        }
        #endregion

        #region Data members
        private readonly string iRootPathToApplyToAllHostFileNames;
        #endregion
    }
}
