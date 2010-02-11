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
using SymBuildParsingLib.Tree;
using SymBuildParsingLib.Utils;
using SymBuildParsingLib.Parser.BuildFile.Document;

namespace SymBuildParsingLib.Parser.BuildFile.Nodes
{
	public abstract class SymNodeBuildFileMMPEntity : SymNodeAddAsChild
	{
		#region Constructors & destructor
		public SymNodeBuildFileMMPEntity()
		{
		}	
		#endregion

		#region Internal enumerations
		[Flags]
		private enum TAttributes
		{
			EAttributeNone = 0,
			EAttributeTidy = 1,
			EAttributeBuildAsARM = 2
		}
		#endregion

		#region Properties
		public bool AttribsBuildAsARM
		{
			get
			{
				bool ret = ( iAttributes & TAttributes.EAttributeBuildAsARM ) == TAttributes.EAttributeBuildAsARM;
				return ret;
			}
			set
			{
				iAttributes &= ~TAttributes.EAttributeBuildAsARM;
			}
		}

		public bool AttribsTidy
		{
			get
			{
				bool ret = ( iAttributes & TAttributes.EAttributeTidy ) == TAttributes.EAttributeTidy;
				return ret;
			}
			set
			{
				iAttributes &= ~TAttributes.EAttributeTidy;
			}
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
					// If a relative path is specified with an .mmp file, the path will be considered relative to the directory containing the bld.inf file. 
					// For example, if in the prj_mmpfiles section, a certain Hello.mmp file is specified as a relative path:
					// 
					// PRJ_MMPFILES
					// ProjSpec\Hello.mmp
					// and if the bld.inf file is in \MyComp\, then the full path for the location of Hello.mmp will be \MyComp\ProjSpec\Hello.mmp.

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
		#endregion

		#region Data members
		private TAttributes iAttributes = TAttributes.EAttributeNone;
		private string iFileName = string.Empty;
		#endregion
	}

	public sealed class SymNodeBuildFileMMP : SymNodeBuildFileMMPEntity
	{
		#region Constructors & destructor
		public SymNodeBuildFileMMP()
		{
		}	
		#endregion
	}

	public sealed class SymNodeBuildFileMakefile : SymNodeBuildFileMMPEntity
	{
		#region Constructors & destructor
		public SymNodeBuildFileMakefile()
		{
		}	
		#endregion
	}

	public sealed class SymNodeBuildFileNMakefile : SymNodeBuildFileMMPEntity
	{
		#region Constructors & destructor
		public SymNodeBuildFileNMakefile()
		{
		}	
		#endregion
	}

	public sealed class SymNodeBuildFileGNUMakefile : SymNodeBuildFileMMPEntity
	{
		#region Constructors & destructor
		public SymNodeBuildFileGNUMakefile()
		{
		}
		#endregion
	}

}
