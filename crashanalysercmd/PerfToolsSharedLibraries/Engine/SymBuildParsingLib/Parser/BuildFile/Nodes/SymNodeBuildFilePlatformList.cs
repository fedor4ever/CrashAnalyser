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
using System.Collections.Specialized;
using SymBuildParsingLib.Tree;
using SymbianTree;

namespace SymBuildParsingLib.Parser.BuildFile.Nodes
{
	public sealed class SymNodeBuildFilePlatformList : SymNodeAddAsChild
	{
		#region Constructors & destructor
		public SymNodeBuildFilePlatformList()
		{
		}
		#endregion

		#region API
		public void Add( string aPlatform )
		{
			if	( iPlatforms.IndexOf( aPlatform ) < 0 )
			{
				iPlatforms.Add( aPlatform );
			}
		}

		public void Remove( string aPlatform )
		{
			if	( iPlatforms.IndexOf( aPlatform ) >= 0 )
			{
				iPlatforms.Remove( aPlatform );
			}
		}

		public bool IsPresent( string aPlatform )
		{
			return ( iPlatforms.IndexOf( aPlatform ) >= 0 );
		}

		public string At( int aIndex )
		{
			// NB. Cannot use indexer because it would hide SymNode::this[ int ]
			return iPlatforms[ aIndex ];
		}
		#endregion

		#region Properties
		public int Count
		{
			get { return iPlatforms.Count; }
		}
		#endregion

		#region Data members
		private StringCollection iPlatforms = new StringCollection();
		#endregion
	}
}
