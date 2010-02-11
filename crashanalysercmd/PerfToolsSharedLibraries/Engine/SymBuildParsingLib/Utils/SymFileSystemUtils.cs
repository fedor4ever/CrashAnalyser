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

namespace SymBuildParsingLib.Utils
{
	public class SymFileSystemUtils
	{
		public static bool FileExists( string aFileName )
		{
			FileInfo info = new FileInfo( aFileName );
			return info.Exists;
		}

		public static bool IsDriveValid( string aDrive )
		{
			bool valid = false;
			//
			if	( aDrive.Length > 0 )
			{
				valid = IsDriveValid( aDrive[ 0 ] );
			}
			//
			return valid;
		}

		public static bool IsDriveValid( char aDrive )
		{
			string driveAsLowerCaseString = new string( aDrive, 1 ).ToLower();
			int drive = (int) driveAsLowerCaseString[ 0 ];
			//
			int firstDrive = (int) 'a';
			int lastDrive = (int) 'z';
			//
			bool valid = ( drive >= firstDrive && drive <= lastDrive );
			return valid;
		}

		public static string AddPathSeparator( string aDir )
		{
			string ret = aDir.Trim();
			//
			int length = ret.Length;
			if	( length > 0 )
			{
				if	( ret[ length - 1 ] != Path.DirectorySeparatorChar )
				{
					ret += Path.DirectorySeparatorChar;
				}
			}
			//
			return ret;
		}

		public static string DirectoryFromPath( string aName )
		{
			// C:\system\data\wibble.txt
			// Returns C:\System\Data\
			string ret = Path.GetDirectoryName( aName ) + Path.DirectorySeparatorChar;
			return ret;
		}

		public static string DriveFromPath( string aName )
		{
			// C:\\system\\data\\wibble.txt => C:
			string drive = "C:";
			//
			if	( Path.IsPathRooted( aName ) == true )
			{
				drive = Path.GetPathRoot( aName );
				
				// Check its got a drive
				if	( drive.Length < 2 || drive[ 1 ] != ':' )
				{
					throw new ArgumentException( "Path \'" + aName + "\' does not have a drive as its root" );
				}

				drive = drive.Substring( 0, 2 );
			}
			else
			{
				throw new ArgumentException( "Path \'" + aName + "\' does not contain a root" );
			}
			//
			return drive;
		}

		public static string PopDir( string aDir )
		{
			string ret = DirectoryFromPath( aDir );
			ret = AddPathSeparator( ret );

			DirectoryInfo info = new DirectoryInfo( ret );
			ret = info.Parent.FullName;
			ret = AddPathSeparator( ret );

			return ret;
		}

		public static string MergePaths( string aBasePath, string aOtherPath )
		{
			#region Examples
			// "X:\Wibble\"                 +   "+Build\Generated"      => "X:\<epocroot>\EPOC32\Build\Generated";
			// "X:\Wibble\"                 +   "..\something\"         => "X:\something\"
			// "X:\Wibble\"                 +   "..something\"          => "X:\Wibble\something"
			// "X:\Wibble\"                 +   "\something\"           => "X:\something\"
			// "X:\Wibble\"                 +   ".\something\"          => "X:\wibble\something\"
			// "X:\Wibble\"                 +   "something\"            => "X:\wibble\something\"
			// "X:\Wibble\"                 +   "."                     => "X:\Wibble\"
			// "X:\Wibble\"                 +   "\"                     => "X:\Wibble\"
			// "X:\Wibble\"                 +   "a"                     => "X:\Wibble\a"
			// "X:\Wibble\Whatsit.txt"      +   "X:\Wibble\"            => "X:\Wibble\"
			// "X:\Wibble\Whatsit.txt"      +   "NextWibble.txt"        => "X:\Wibble\NextWibble.txt"
			#endregion
			string otherPath = aOtherPath;
			//
			string ret = aBasePath.Trim();
			ret = DirectoryFromPath(ret);
			ret = AddPathSeparator(ret);
			//
			int length = otherPath.Length;
			if	(length >= 1)
			{
				char firstChar = otherPath[0];
				if  ( firstChar == '.' )
				{
					bool needToPushRemainder = false;
					bool finishedCheckingForDirPops = false;
					//
					while( finishedCheckingForDirPops == false )
					{
						// Assume we only go around once
						finishedCheckingForDirPops = true;
						firstChar = otherPath[0];
						length = otherPath.Length;
						//
						if	( firstChar == '.' )
						{
							if	( length >= 2 )
							{
								char secondChar = otherPath[1];
								if	( secondChar == '.' )
								{
									// Keep popping directories from the base path (now stored in 'ret')
									// while there are "..\" sub-strings remaining at the start of the 
									// "other path"
									ret = PopDir(ret);

									// Tidy up the "otherPath" to remove the two dots which we just parsed
									int subStringPos = 2;
									if	(length >= 3 && otherPath[2] == '\\')
									{
										subStringPos = 3;
									}
									//
									needToPushRemainder = true;
									otherPath = otherPath.Substring( subStringPos );
									finishedCheckingForDirPops = false;
								}
							}
						}
					}

					if  ( needToPushRemainder )
					{
						ret += otherPath;
					}
				}
				else if ( firstChar == '\\' )
				{
					if	(length > 1)
					{
						// Root path
						ret = DriveFromPath( ret );
						ret += otherPath;
					}
				}
				else if ( firstChar == '+' && otherPath.Length > 0 )
				{
					// Need to add epoc root and then EPOC32, then the path without the +
					string actualPathPart = otherPath.Substring(1);
					if	( actualPathPart.Length > 0 && actualPathPart[0] == '\\' )
					{
						actualPathPart = actualPathPart.Substring(1);
					}

					string epocRootValue = SymEnvironment.EpocRoot;
					ret = DriveFromPath(ret);
					ret += AddPathSeparator( epocRootValue );
					ret += AddPathSeparator( SymEnvironment.Epoc32Path );
					ret += AddPathSeparator( actualPathPart );
				}
				else
				{
					if  ( otherPath[ length-1 ] == '\\' )
					{
						// Assume we already have a complete path
						ret = otherPath;
					}
					else
					{
						ret += otherPath;
					}
				}
			}

			/*
			const string nameAndExt = FileNameAndExtension(ret);
			const bool containsDot = (nameAndExt.find_last_of('.') != string::npos);
			if  (!FileExists(ret) && ret.length() && ret[ret.length()-1] != '\\' && containsDot)
				{
				cout << "WARNING - possible file merge error:" << endl;
				cout << "\tMerging: " << aBasePath << " with: " << aOtherPath << endl;
				cout << "\t\t -> " << ret << endl << endl;
				}
			*/
			return ret;
		}
	}
}
