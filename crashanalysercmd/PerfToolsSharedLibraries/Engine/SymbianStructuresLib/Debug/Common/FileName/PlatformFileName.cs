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
using System.Text;
using System.Collections.Generic;
using SymbianUtils;

namespace SymbianStructuresLib.Debug.Common.FileName
{
    public class PlatformFileName
    {
        #region Static constructor
        public static PlatformFileName New( PlatformFileName aCopy )
        {
            PlatformFileName ret = new PlatformFileName();
            ret.FileNameInDevice = aCopy.FileNameInDevice;
            ret.FileNameInHost = aCopy.FileNameInHost;
            return ret;
        }

        public static PlatformFileName NewByHostName( string aFileName )
        {
            PlatformFileName ret = new PlatformFileName();

            ret.FileNameInHost = aFileName;

            // Seed the device file name based upon the host info
            string justFileName = Path.GetFileName( aFileName );
            ret.FileNameInDevice = Path.Combine( PlatformFileNameConstants.Device.KPathWildcardSysBin, justFileName );

            return ret;
        }

        public static PlatformFileName NewByDeviceName( string aFileName )
        {
            PlatformFileName ret = new PlatformFileName();

            ret.FileNameInDevice = aFileName;

            // Seed the device file name based upon the host info
            string justFileName = Path.GetFileName( ret.FileNameInDevice );
            ret.FileNameInHost = Path.Combine( PlatformFileNameConstants.Host.KPathEpoc32ReleaseArmv5Urel, justFileName );

            return ret;
        }
        #endregion

        #region Constructors
        private PlatformFileName()
        {
        }
        #endregion

        #region API
        public bool Contains( string aText )
        {
            string text = aText.ToUpper();
            bool ret = iFileNameInDevice.ToUpper().Contains( text ) ||
                       iFileNameInHost.ToUpper().Contains( text );
            return ret;
        }
        #endregion

        #region Properties
        public bool ContainsWildcard
        {
            get { return iFileNameInDevice.StartsWith( PlatformFileNameConstants.Device.KPathWildcardRoot ); }
        }

        public string EitherFullNameButDevicePreferred
        {
            get
            {
                // Prefer the in-device file name if possible
                string ret = iFileNameInDevice;
                //
                if ( string.IsNullOrEmpty( ret ) )
                {
                    ret = iFileNameInHost;
                    if ( string.IsNullOrEmpty( ret ) )
                    {
                        ret = string.Empty;
                    }
                }
                //
                return ret;
            }
        }

        public string FileNameInDevice
        {
            get { return iFileNameInDevice; }
            set
            {
                string name = value;
                if ( name.Length < 2 )
                {
                    throw new ArgumentException( "File name is invalid" );
                }

                // If the specified filename doesn't enclude a drive letter, then add one.
                bool needsDrive = false;
                if ( name.StartsWith( @"/" ) || name.StartsWith( @"\" ) )
                {
                    needsDrive = true;
                }
                else if ( name[ 1 ] != ':' )
                {
                    needsDrive = true;
                }

                if ( needsDrive )
                {
                    StringBuilder fileName = new StringBuilder( name );
                    if ( fileName[ 0 ] != Path.DirectorySeparatorChar )
                    {
                        fileName.Insert( 0, @"\" );
                    }
                    fileName.Insert( 0, PlatformFileNameConstants.Device.KPathWildcardRoot );
                    iFileNameInDevice = string.Intern( fileName.ToString() );
                }
                else
                {
                    iFileNameInDevice = string.Intern( name );
                }
            }
        }

        public string FileNameInHost
        {
            get { return iFileNameInHost; }
            set
            {
                iFileNameInHost = string.Intern( value );
            }
        }
        #endregion

        #region From System.Object
        public override int GetHashCode()
        {
            return iFileNameInDevice.GetHashCode();
        }

        public override string ToString()
        {
            return EitherFullNameButDevicePreferred;
        }

        public override bool Equals( object aObject )
        {
            bool ret = false;
            //
            if ( aObject is PlatformFileName )
            {
                PlatformFileName other = (PlatformFileName) aObject;

                // These are the strings we'll compare - we'll assume exact comparison
                // required by default...
                string pathHostThis = this.FileNameInHost;
                string pathHostOther = other.FileNameInHost;
                string pathDeviceThis = this.FileNameInDevice;
                string pathDeviceOther = other.FileNameInDevice;

                // We must find out if this object (or aObject) contains a wildcard.
                if ( this.ContainsWildcard || other.ContainsWildcard )
                {
                    // Compare just the in-device paths, not the drives, since one or
                    // other of the drive letters is unknown.
                    pathDeviceThis = this.FileNameInDeviceWithoutRoot;
                    pathDeviceOther = other.FileNameInDeviceWithoutRoot;
                }

                // Now we can compare the two...
                bool sameDevice = string.Compare( pathDeviceThis, pathDeviceOther, StringComparison.CurrentCultureIgnoreCase ) == 0;
                bool sameHost = string.Compare( pathHostThis, pathHostOther, StringComparison.CurrentCultureIgnoreCase ) == 0;
                //
                ret = ( sameDevice || sameHost );
            }
            else
            {
                ret = base.Equals( aObject );
            }
            //
            return ret;
        }
        #endregion

        #region Internal methods
        internal string FileNameInDeviceWithoutRoot
        {
            get
            {
                string ret = iFileNameInDevice;
                //
                if ( ret.Length > 2 )
                {
                    if ( ret[ 1 ] == ':' )
                    {
                        ret = ret.Substring( 2 );
                    }
                }
                //
                return ret;
            }
        }
        #endregion

        #region Data members
        private string iFileNameInDevice = string.Empty;
        private string iFileNameInHost = string.Empty;
        #endregion
    }
}