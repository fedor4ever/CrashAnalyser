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

namespace SymbolLib.CodeSegDef
{
	public class CodeSegResolverEntry
	{
		#region Enumerations
		public enum TBuildType
		{
			EReleaseTypeRelease = 0,
			EReleaseTypeDebug
		}

        public enum TSourceType
        {
            ESourceNotDefined = 0,
            ESourceWasMapFile,
            ESourceWasSymbolFile
        }
		#endregion

		#region Constructors & destructor
		public CodeSegResolverEntry()
		{
		}

		public CodeSegResolverEntry( string aFileName )
		{
			EnvironmentFileNameAndPath = aFileName;
            ImageFileNameAndPath = Path.Combine( CodeSegResolver.KROMBinaryPath, Path.GetFileName( aFileName ) ).ToLower();
		}

        public CodeSegResolverEntry( string aEnvFileName, string aImageFileName )
		{
			EnvironmentFileNameAndPath = aEnvFileName.ToLower();
            ImageFileNameAndPath = aImageFileName.ToLower();
		}
		#endregion

        #region API
        #endregion

        #region Properties
        public TSourceType Source
        {
            get { return iSource; }
            set { iSource = value; }
        }

        public TBuildType BuildType
        {
            get { return iBuildType; }
            set { iBuildType = value; }
        }

		public string EnvironmentFileName
		{
			get { return Path.GetFileName( EnvironmentFileNameAndPath ); }
		}

        public string EnvironmentFileNameAndPath
        {
			get { return iFileNameAndPath_InEnvironment; }
			set
			{
				iFileNameAndPath_InEnvironment = value;
				
				// Try to identify the release type (build) based upon the
				// filename directory information. We'll default to UREL.
				if	( value.Length > 0 )
				{
					iBuildType = BuildTypeByFileName( value );
				}
			}
        }

		public string ImageFileName
		{
			get
            {
                string ret = string.Empty;
                string val = ImageFileNameAndPath;
                //
                if ( !string.IsNullOrEmpty( val ) )
                {
                    // Try to get rid of all file extensions
                    while ( ret == string.Empty )
                    {
                        try
                        {
                            ret = Path.GetFileName( val );
                        }
                        catch ( ArgumentException )
                        {
                            if ( val.Length > 0 )
                            {
                                val = val.Substring( 0, val.Length - 1 );
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                //
                return ret;
            }
		}

        public string ImageFileNameAndPath
        {
			get { return iFileNameAndPath_InImage; }
			set
			{
				iFileNameAndPath_InImage = value;
			}
        }

        public string ImageFileNameAndPathWithoutDrive
        {
			get
            {
                string withoutDrive = Path.GetDirectoryName( ImageFileNameAndPath );
                //
                if ( withoutDrive.Length > 2 && withoutDrive[ 1 ] == ':' && withoutDrive[ 2 ] == '\\' )
                {
                    withoutDrive = withoutDrive.Substring( 2 );
                }
                if ( withoutDrive.Length > 0 && withoutDrive[ withoutDrive.Length - 1 ] != Path.DirectorySeparatorChar )
                {
                    withoutDrive += Path.DirectorySeparatorChar;
                }
                //
                withoutDrive += Path.GetFileName( ImageFileNameAndPath );
                return withoutDrive;
            }
        }
		#endregion

        #region From System.Object
		public override string ToString()
		{
			return ImageFileNameAndPath.ToLower();
		}

        public override bool Equals( object aObject )
        {
            bool isEqual = false;
            //
            if ( aObject != null )
            {
                if ( aObject is CodeSegResolverEntry )
                {
                    CodeSegResolverEntry otherEntry = aObject as CodeSegResolverEntry;
                    //
                    isEqual = ( otherEntry.ImageFileNameAndPathWithoutDrive.ToLower() == ImageFileNameAndPathWithoutDrive.ToLower() );
                }
            }
            //
            return isEqual;
        }

		public override int GetHashCode()
		{
			return ImageFileNameAndPath.GetHashCode();
		}
        #endregion

        #region Internal methods
        private static TBuildType BuildTypeByFileName( string aFileName )
		{
			TBuildType ret = TBuildType.EReleaseTypeRelease;
			string path = "UREL";
			try
			{
				path = Path.GetDirectoryName( aFileName ).ToUpper();
			}
			finally
			{
			}
			//
			if	( path == "UDEB" )
			{
			}
			else
			{
				ret = TBuildType.EReleaseTypeRelease;
			}
			//
			return ret;
		}
		#endregion

		#region Data members
		private string iFileNameAndPath_InEnvironment = string.Empty;
		private string iFileNameAndPath_InImage = string.Empty;
        private TBuildType iBuildType = TBuildType.EReleaseTypeRelease;
        private TSourceType iSource = TSourceType.ESourceNotDefined;
		#endregion
	}
}
