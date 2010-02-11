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
using System.Text;
using System.Collections;
using SymBuildParsingLib.Token;
using SymbianTree;

namespace SymBuildParsingLib.Common.Objects
{
	public class SymDefineArgument : SymArgument
	{
		#region Constructors & destructor
		public SymDefineArgument( SymArgument aArgumentToCopyFrom )
		{
			AppendChildrenFrom( aArgumentToCopyFrom );
		}
		#endregion

		#region API
		#endregion

		#region Properties
		#endregion

		#region Data members
		#endregion
	}
	
	public class SymDefineDefinition
	{
		#region Enumerations
		#endregion

		#region Constructors & destructor
		public SymDefineDefinition()
		{
		}

		public SymDefineDefinition( string aName )
		{
			iName = aName;
		}
		#endregion

		#region API
		public void AddArgument( SymDefineArgument aArgument )
		{
			iArguments.Add( aArgument );
		}
		#endregion

		#region Properties
		public bool IsValid
		{
			get
			{
				bool valid = ( Name.Length > 0 );
				return valid;
			}
		}

		public string Name
		{
			get { return iName; }
			set { iName = value; }
		}

		public string CoalescedTokenValue
		{
			get
			{
				StringBuilder args = new StringBuilder();
				//
				int count = iArguments.Count;
				for( int i=0; i<count; i++ )
				{
					SymDefineArgument arg = (SymDefineArgument) iArguments[ i ];
					//
					args.Append( arg.CoalescedTokenValue );
					if	( i <count-1 )
					{
						args.Append( ", " );
					}
				}
				//
				if	( count > 0 )
				{
					args.Insert( 0, "(" );
					args.Append( ")" );
				}
				return Name + args.ToString();
			}
		}
		public SymTokenContainer Value
		{
			get { return iValue; }
			set { iValue = value; }
		}
		#endregion

		#region Properties - argument related
		public bool HasArguments
		{
			get { return ArgumentCount > 0; }
		}

		public int ArgumentCount
		{
			get { return iArguments.Count; }
		}

		public SymDefineArgument this[ int aIndex ]
		{
			get
			{
				SymDefineArgument arg = (SymDefineArgument) iArguments[ aIndex ];
				return arg;
			}
		}
		#endregion

		#region From System.Object
		public override int GetHashCode()
		{
			int ret = iName.GetHashCode();
			return ret;
		}
		#endregion

		#region Data members
		private string iName = string.Empty;
		private ArrayList iArguments = new ArrayList();
		private SymTokenContainer iValue = new SymTokenContainer();
		#endregion
	}
}
