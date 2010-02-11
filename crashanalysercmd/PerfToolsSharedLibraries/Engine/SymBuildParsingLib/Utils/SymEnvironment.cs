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

namespace SymBuildParsingLib.Utils
{
	public class SymEnvironment
	{
		#region API
		public static string EpocRoot
		{
			get
			{
				//string ret = System.Environment.GetEnvironmentVariable( KSymEpocRootEnvVarName );
				string ret = RootPath;
				return ret;
			}
		}

		public static string Epoc32IncludePath
		{
			get
			{
				string ret = iRootPath + KSymEpoc32IncludePath;
				return ret;
			}
		}

		public static string Epoc32Path
		{
			get
			{
				string ret = iRootPath + KSymEpoc32Path;
				return ret;
			}
		}

		public static string Epoc32DataPath
		{
			get { return iRootPath + KSymEpoc32DataPath; }
		}

		public static string RootPath
		{
			get { return iRootPath; }
			set { iRootPath = value; }
		}
		#endregion

		#region Internal constants
		private const string KSymEpocRootEnvVarName = "EPOCROOT";
		private const string KSymEpoc32Path = "\\EPOC32";
		private const string KSymEpoc32IncludePath = "\\EPOC32\\Include\\";
		private const string KSymEpoc32DataPath = "\\EPOC32\\Data\\";
		#endregion

		#region Data members
		private static string iRootPath = string.Empty;
		#endregion
	}
}
