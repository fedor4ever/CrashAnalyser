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
using System.Collections;
using System.Collections.Specialized;

namespace SymBuildParsingLib.Utils
{
	public class SymFileNameCollection
	{
		#region Constructors & destructor
		public SymFileNameCollection()
		{
		}
		#endregion

		#region API
		public void Add( string aFileName )
		{
			bool exists = File.Exists( aFileName );
			if	( exists )
			{
				bool isPresent = IsPresent( aFileName );
				if	( isPresent == false )
				{
					iFiles.Add( aFileName.ToLower(), aFileName );
				}
			}

		}

		public void Remove( string aFileName )
		{
			bool isPresent = IsPresent( aFileName );
			if	( isPresent == false )
			{
				iFiles.Remove( aFileName.ToLower() );
			}
		}

		public bool IsPresent( string aFileName )
		{
			bool isPresent = iFiles.ContainsKey( aFileName.ToLower() );
			return isPresent;
		}
		#endregion

		#region Properties
		public int Count
		{
			get { return iFiles.Count; }
		}

		public string this[ int aIndex ]
		{
			get
			{
				String[] keys = new String[ iFiles.Count ];
				iFiles.Keys.CopyTo( keys, 0 );
				//
				string key = keys[ aIndex];
				string file = iFiles[ key ];
				//
				return file;
			}
		}
		#endregion

		#region Internal methods
		#endregion

		#region Data members
		private StringDictionary iFiles = new StringDictionary();
		#endregion
	}
}
