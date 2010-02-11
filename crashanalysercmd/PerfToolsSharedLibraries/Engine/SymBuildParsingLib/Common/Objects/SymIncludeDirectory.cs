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
using System.Collections;
using SymBuildParsingLib.Utils;

namespace SymBuildParsingLib.Common.Objects
{
	public class SymIncludeDirectory
	{
		#region Constructors & destructor
		public SymIncludeDirectory()
		{
		}

		public SymIncludeDirectory( SymIncludeDirectory aCopy )
		{
		}
		#endregion

		#region API
		public void Add( SymIncludeDefinition aDefinition )
		{
			string location = aDefinition.Location;
			if	( location == String.Empty )
			{
				throw new ArgumentException( "Include location cannot be null when adding to include directory", aDefinition.ToString() );
			}
			
			if	( iIncludes[ location ] == null )
			{
				iIncludes.Add( location, aDefinition );
			}
		}

		public string ResolveFileName( SymIncludeDefinition aDefinition )
		{
			// First check against the specified type for an exact match...
			string location = aDefinition.Location;
			string ret = FindInSpecifiedIncludeEntries( location, aDefinition.Type );

			if  ( ret == string.Empty )
			{
				// Try the other remaining type
				SymIncludeDefinition.TType type = SymIncludeDefinition.TType.ETypeUser;

				if	( aDefinition.Type == SymIncludeDefinition.TType.ETypeUser )
				{
					type = SymIncludeDefinition.TType.ETypeSystem;
				}
				else
				{
					type = SymIncludeDefinition.TType.ETypeUser;
				}

				ret = FindInSpecifiedIncludeEntries( location, type );
			}
			//
			return ret;
		}
		#endregion

		#region Properties
		#endregion

		#region Internal methods
		private string FindInSpecifiedIncludeEntries( string aFileName, SymIncludeDefinition.TType aType )
		{
			string ret = string.Empty;
			//
			IDictionaryEnumerator enumerator = iIncludes.GetEnumerator();
			while ( enumerator.MoveNext() )
			{
				SymIncludeDefinition include = (SymIncludeDefinition) enumerator.Value;
				//
				if	( include.Type == aType )
				{
					string includePath = include.Location;
					string resolvedFileName = SymFileSystemUtils.MergePaths( includePath, aFileName );
					//
					if	( SymFileSystemUtils.FileExists( resolvedFileName ) )
					{
						ret = resolvedFileName;
						break;
					}
				}
			}
			//
			return ret;
		}
		#endregion

		#region Data members
		private Hashtable iIncludes = new Hashtable( 10 );
		#endregion
	}
}
