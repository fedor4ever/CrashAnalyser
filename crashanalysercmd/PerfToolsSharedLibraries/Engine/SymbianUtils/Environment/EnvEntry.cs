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
using System.Collections;
using System.IO;
using System.Text;

namespace SymbianUtils.Environment
{
    public class EnvEntry : EnvEntryBase
    {
        #region Constructors
        internal EnvEntry( DriveInfo aDriveInfo, out bool aIsValid )
            : base( aDriveInfo )
		{
            aIsValid = false;

			// Check whether drive contains Symbian OS environment
			try
			{
                bool gotSymbianVersion = BuildSymbianOSVersion();
                bool gotS60Version = BuildS60Version();

                // Check whether we have a valid Symbian OS version string.
                // If we do, then we don't care if the S60 version is missing
                if ( gotSymbianVersion )
                {
                    aIsValid = true;
                }
                else
                {
                    // Not valid - we didn't get a Symbian OS version
                }
			}
            catch
			{
                // Not valid
			}
		}
		#endregion

		#region Internal methods
        private bool BuildSymbianOSVersion()
		{
			string ver = string.Empty;
			string fileName = Path.Combine( DriveName, KSymbianBuildInfoFileAndPath );

            try
            {
                if ( File.Exists( fileName ) )
                {
                    //
                    using ( StreamReader reader = new StreamReader( fileName ) )
                    {
                        string line = reader.ReadLine();
                        while ( line != null )
                        {
                            if ( line.IndexOf( KSymbianBuildInfoMarkerText ) >= 0 )
                            {
                                ver = line.Replace( KSymbianBuildInfoMarkerText, "" ).Trim();
                                break;
                            }
                            line = reader.ReadLine();
                        }
                    }
                }
                else
                {
                    // Try to use a variant file instead.
                    string variantPath = Path.Combine( DriveName, KSymbianVariantPath );
                    DirectoryInfo dir = new DirectoryInfo( variantPath );
                    if ( dir.Exists )
                    {
                        FileInfo[] files = dir.GetFiles();

                        // Find the most recent file
                        FileInfo newest = null;
                        foreach ( FileInfo file in files )
                        {
                            if ( newest == null || file.LastWriteTimeUtc > newest.LastWriteTimeUtc )
                            {
                                newest = file;
                            }
                        }

                        if ( newest != null )
                        {
                            ver = Path.GetFileNameWithoutExtension( newest.FullName );
                        }
                    }
                }
            }
            catch
            {
            }

            // Update version
            bool valid = string.IsNullOrEmpty( ver ) == false;
            if ( valid )
            {
                base.VersionStringSymbian = ver;
            }
            return valid;
		}

		private bool BuildS60Version()
		{
			string ver = string.Empty;
			string fileName = Path.Combine( DriveName, KS60BuildInfoFileAndPath );
			//
            try
            {
                if ( File.Exists( fileName ) )
                {
                    using ( StreamReader reader = new StreamReader( fileName ) )
                    {
                        string line = reader.ReadLine();
                        while ( line != null )
                        {
                            if ( line.IndexOf( KS60BuildInfoMarkerText ) == 0 )
                            {
                                int breakPos = line.IndexOf( "\\n" );
                                if ( breakPos > 0 )
                                {
                                    ver = line.Substring( KS60BuildInfoMarkerText.Length, breakPos - KS60BuildInfoMarkerText.Length );
                                    break;
                                }
                            }
                            line = reader.ReadLine();
                        }
                    }
                }
            }
            catch
            {
            }

            // Update version
            bool valid = string.IsNullOrEmpty( ver ) == false;
            if ( valid )
            {
                base.VersionStringS60 = ver;
			}
            return valid;
		}
		#endregion

		#region Internal constants
		private const string KSymbianBuildInfoMarkerText = "ManufacturerSoftwareBuild";
		private const string KS60BuildInfoMarkerText = "V ";
		private const string KSymbianBuildInfoFileAndPath = @"epoc32\data\buildinfo.txt";
        private const string KSymbianVariantPath = @"epoc32\include\variant\";
        private const string KS60BuildInfoFileAndPath = @"epoc32\data\z\resource\versions\sw.txt";
		#endregion

		#region Data members
		#endregion
	}
}
