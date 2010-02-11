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

namespace SymBuildParsingLib.Common.Objects
{
	public class SymIncludeDefinition
	{
		#region Enumerations
		public enum TType
		{
			ETypeUndefined = 0,
			ETypeUser,
			ETypeSystem
		};
		#endregion

		#region Constructors & destructor
		public SymIncludeDefinition()
		{
		}

		public SymIncludeDefinition( string aLocation )
		{
			iLocation = aLocation;
		}

		public SymIncludeDefinition( TType aType )
		{
			iType = aType;
		}

		public SymIncludeDefinition( TType aType, string aLocation )
		{
			iType = aType;
			iLocation = aLocation;
		}
		#endregion

		#region API
		public void AdjustRelativeInclude( string aBasePath )
		{
			bool isRooted = Path.IsPathRooted( Location );
			//
			if	( isRooted == false )
			{
				Location = Utils.SymFileSystemUtils.MergePaths( aBasePath, Location );
			}
		}
		#endregion

		#region Properties
		public TType Type
		{
			get { return iType; }
			set { iType = value; }
		}

		public string Location
		{
			get { return iLocation; }
			set { iLocation = value; }
		}
		#endregion

		#region Data members
		private TType iType = TType.ETypeUndefined;
		private string iLocation = String.Empty;
		#endregion
	}
}
