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
	public class SymDefineDirectory
	{
		#region Constructors & destructor
		public SymDefineDirectory()
		{
		}

		public SymDefineDirectory( SymDefineDirectory aCopy )
		{
		}
		#endregion

		#region API
		public void Add( SymDefineDefinition aDefinition )
		{
			object key = aDefinition.GetHashCode();
			iDefines.Add( key, aDefinition );
		}

		public bool IsDefined( string aName )
		{
			SymDefineDefinition ret = DefineDefinition( aName );
			return ( ret != null );
		}

		public SymDefineDefinition DefineDefinition( string aName )
		{
			SymDefineDefinition ret = null;
			//
			object key = new SymDefineDefinition( aName ).GetHashCode();
			object item = iDefines[ key ];
			if	( item != null )
			{
				ret = (SymDefineDefinition) item;
			}
			//
			return ret;
		}
		#endregion

		#region Properties
		#endregion

		#region Internal methods
		#endregion

		#region Data members
		private Hashtable iDefines = new Hashtable( 10 );
		#endregion
	}
}
