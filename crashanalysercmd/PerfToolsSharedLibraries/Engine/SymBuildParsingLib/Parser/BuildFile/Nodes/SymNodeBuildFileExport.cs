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
using SymbianTree;
using SymBuildParsingLib.Utils;
using SymBuildParsingLib.Tree;
using SymBuildParsingLib.Parser.BuildFile.Document;

namespace SymBuildParsingLib.Parser.BuildFile.Nodes
{
	public sealed class SymNodeBuildFileExport : SymNodeAddAsChild
	{
		#region Enumerations
		public enum TType
		{
			ETypeFile = 0,
			ETypeZipFile
		}
		#endregion

		#region Constructors & destructor
		public SymNodeBuildFileExport()
		{
		}

		public SymNodeBuildFileExport( TType aType )
		{
			iType = aType;
		}
		#endregion

		#region API
		public TType Type
		{
			get { return iType; }
			set { iType = value; }
		}

		public string FileName
		{
			get { return iFileName; }
			set
			{
				if	( !( Root is SymBuildFileDocument ) )
				{
					throw new ArgumentException( "Node must be added to tree before using this API" );
				}

				// Get bld.inf file name
				SymBuildFileDocument doc = (SymBuildFileDocument) Root;
				string bldInfPath = Path.GetFullPath( doc.Context.FileName );

				// Try to intepret MMP file name
				bool valid = false;
				string rootPath = Path.GetPathRoot( value );
				bool isRooted = Path.IsPathRooted( value );
				//
				if	( isRooted == false || rootPath == string.Empty || rootPath == @"\" )
				{
					// If a source file is listed with a relative path, the path will be considered relative to the directory containing the bld.inf file. 

					iFileName = SymFileSystemUtils.MergePaths( bldInfPath, value );
					valid = true;
				}
				else if ( isRooted )
				{
					iFileName = SymFileSystemUtils.MergePaths( SymEnvironment.RootPath, value );
					valid = true;
				}

				// Throw errors if we didn't find a valid match
				if	( valid == false )
				{
					throw new ArgumentException( "Invalid file name: " + value );
				}
			}
		}

		public string DestinationPath
		{
			get { return iDestinationPath; }
			set
			{
				// If a destination file is not specified, the source file will be copied to epoc32\include\. 
				bool valid = false;
				string rootPath = Path.GetPathRoot( value );
				//
				if	( Path.IsPathRooted( value ) == false || rootPath == string.Empty || rootPath == @"\" )
				{
					// If a destination file is specified with the relative path, the path will be considered relative to directory epoc32\include\.
					iDestinationPath = SymFileSystemUtils.MergePaths( SymEnvironment.Epoc32IncludePath, value );
					valid = true;
				}
				else if ( rootPath.Length == 2 || rootPath.Length == 3 && rootPath[1] == ':' )
				{
					// If a destination begins with a drive letter, then the file is copied to epoc32\data\<drive_letter>\<path>. For example,
					// 
					// mydata.dat e:\appdata\mydata.dat
					// copies mydata.dat to epoc32\data\e\appdata\mydata.dat.
					// 
					// You can use any driveletter between A and Z.

					string drive = rootPath[ 0 ].ToString().ToLower();
					string pathWithoutDrive = SymFileSystemUtils.DirectoryFromPath( rootPath );

					// Check drive letter is valid
					bool driveIsValid = SymFileSystemUtils.IsDriveValid( drive );
					if	( driveIsValid )
					{
						iDestinationPath = SymEnvironment.Epoc32DataPath + drive + pathWithoutDrive;
						valid = true;
					}
				}

				// Throw errors if we didn't find a valid match
				if	( valid == false )
				{
					throw new ArgumentException( "Invalid destination path: " + value );
				}
			}
		}
		#endregion

		#region Data members
		private TType iType = TType.ETypeFile;
		private string iFileName = string.Empty;
		private string iDestinationPath = SymEnvironment.Epoc32IncludePath;
		#endregion
	}
}
