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

namespace SymbianUtils.FileSystem
{
    public abstract class FSEntity
    {
        #region Static constructors
        public static FSEntity New( string aFSEntityName )
        {
            FSEntity ret = null;
            //
            try
            {
                try
                {
                    DirectoryInfo dir = new DirectoryInfo( aFSEntityName );
                    if ( dir.Exists )
                    {
                        ret = new FSEntityDirectory( dir );
                    }
                    else
                    {
                        ret = new FSEntityFile( new FileInfo( aFSEntityName ) );
                    }
                }
                catch ( Exception )
                {
                    ret = new FSEntityFile( new FileInfo( aFSEntityName ) );
                }
            }
            catch ( Exception )
            {
            }
            //
            if ( ret == null )
            {
                throw new FileNotFoundException( "Could not locate suitable entity", aFSEntityName );
            }
            //
            return ret;
        }

        public static FSEntity New( FSEntity aClone )
        {
            FSEntity ret = null;
            //
            if ( aClone is FSEntityFile )
            {
                ret = new FSEntityFile( ( (FSEntityFile) aClone ).File );
            }
            else if ( aClone is FSEntityDirectory )
            {
                ret = new FSEntityDirectory( ( (FSEntityDirectory) aClone ).Directory );
            }
            else
            {
                throw new ArgumentException( "Unsupported File System Entity Type" );
            }
            //
            return ret;
        }
        #endregion

        #region Constructors
        protected FSEntity()
        {
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public bool IsFile
        {
            get { return this is FSEntityFile; }
        }

        public bool IsDirectory
        {
            get { return this is FSEntityDirectory; }
        }

        public abstract string FullName
        {
            get;
        }

        public abstract bool IsValid
        {
            get;
        }

        public abstract bool Exists
        {
            get;
        }
        #endregion

        #region Internal API
        #endregion

        #region Internal methods
        #endregion

        #region Operators
        public static bool operator ==( FSEntity aLeft, FSEntity aRight )
        {
            // If both are null, or both are same instance, return true.
            if ( System.Object.ReferenceEquals( aLeft, aRight ) )
            {
                return true;
            }

            // If one is null, but not both, return false.
            if ( ( (object) aLeft == null ) || ( (object) aRight == null ) )
            {
                return false;
            }

            // Return true if the fields match:
            return aLeft.Equals( aRight );
        }

        public static bool operator !=( FSEntity aLeft, FSEntity aRight )
        {
            return !( aLeft == aRight );
        }
        #endregion

        #region From System.Object
        public override int GetHashCode()
        {
            return FullName.GetHashCode();
        }

        public override bool Equals( object aObject )
        {
            if ( aObject != null )
            {
                if ( aObject is FSEntity )
                {
                    FSEntity other = (FSEntity) aObject;
                    //
                    if ( other.IsFile && this.IsFile )
                    {
                        return other.FullName.Equals( this.FullName );
                    }
                    else if ( other.IsDirectory && this.IsDirectory )
                    {
                        return other.FullName.Equals( this.FullName );
                    }
                }
            }
            //
            return base.Equals( aObject );
        }

        public override string ToString()
        {
            return FullName;
        }
        #endregion

        #region Data members
        #endregion
    }
}
