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
    public abstract class EnvEntryBase
    {
        #region Static factory function
        internal static EnvEntryBase New( DriveInfo aDriveInfo )
        {
            bool valid;
            //
            EnvEntryBase ret = new EnvEntry( aDriveInfo, out valid );
            if ( valid == false )
            {
                ret = new EnvEntryNull();
            }
            return ret;
        }
        #endregion

        #region Constructors
        protected EnvEntryBase( DriveInfo aDriveInfo )
        {
            iDriveInfo = aDriveInfo;
        }
        #endregion

        #region API
        public string CombineWithFile( string aFileName )
        {
            string ret = aFileName;
            //
            if ( iDriveInfo != null )
            {
                string drive = DriveName;

                if ( aFileName.Length > 2 )
                {
                    bool isDrive = aFileName[ 1 ] == ':';
                    if ( isDrive )
                    {
                        // Remove drive before combining with environment drive
                        aFileName = aFileName.Substring( 2 );
                    }

                    // Strip trailing backslash from drive name if already part
                    // of file name prefix
                    if ( aFileName.StartsWith( "\\" ) && drive.EndsWith( "\\" ) )
                    {
                        drive = drive.Substring( 0, drive.Length - 1 );
                    }
                }

                ret = drive + aFileName;
            }
            //
            return ret;
        }
        #endregion

        #region Properties
        public bool IsValid
        {
            get
            {
                bool ret = ( iDriveInfo != null ) && ( string.IsNullOrEmpty( iVersionStringSymbian ) == false );
                return ret;
            }
        }

        public DriveInfo DriveInfo
        {
            get { return iDriveInfo; }
        }

        public string DriveName
        {
            get
            {
                string ret = string.Empty;
                if ( iDriveInfo != null )
                {
                    ret = iDriveInfo.Name;
                }
                return ret;
            }
        }

        public string VolumeName
        {
            get
            {
                string ret = string.Empty;
                if ( iDriveInfo != null )
                {
                    ret = string.Format( "[ {0} ]", iDriveInfo.VolumeLabel );
                }
                return ret;
            }
        }

        public string VersionStringSymbian
        {
            get { return iVersionStringSymbian; }
            protected set { iVersionStringSymbian = value; }
        }

        public string VersionStringS60
        {
            get { return iVersionStringS60; }
            protected set { iVersionStringS60 = value; }
        }

        public bool IsS60Environment
        {
            get { return string.IsNullOrEmpty( iVersionStringS60 ) == false; }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.Append( DriveName );
            ret.Append( " " );
            ret.Append( VolumeName );
            ret.Append( " " );

            // Try to use S60 version string if we have it...
            if ( VersionStringS60.Length > 0 )
            {
                ret.Append( VersionStringS60 );
            }
            else
            {
                ret.Append( VersionStringSymbian );
            }
            //
            return ret.ToString();
        }
        #endregion

        #region Data members
        private readonly DriveInfo iDriveInfo;
        private string iVersionStringSymbian = string.Empty;
        private string iVersionStringS60 = string.Empty;
        #endregion
    }
}
