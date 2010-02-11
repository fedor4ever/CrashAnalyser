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
using System.Xml;
using SymBuildParsingLib.Token;
using SymbianTree;

namespace SymBuildParsingLib.Tree
{
	public class SymNodeToken : SymNodeAddAsChild
	{
		#region Constructors & destructor
		public SymNodeToken( SymToken aToken )
		{
			Data = aToken;
		}
		#endregion

		#region API
		public virtual int CountTokenByType( SymToken.TClass aClass )
		{
			int count = 0;
			//
			foreach( SymNode n in this )
			{
				if	( n is SymNodeToken )
				{
					SymToken t = ((SymNodeToken) n).Token;
					//
					if	( t.Class == aClass )
					{
						++count;
					}
				}
			}
			//
			return count;
		}
		#endregion

		#region Properties - data
		public SymToken Token
		{
			get { return (SymToken) Data; }
			set { Data = value; }
		}
		#endregion

		#region From System.Object
		public override string ToString()
		{
			return Token.ToString();
		}

		public override bool Equals( object aObject )
		{
			bool same = false;
			//
			if	( aObject is SymNodeToken )
			{
				SymNodeToken otherToken = (SymNodeToken) aObject;
				//
				same = otherToken.Token.Equals( Token );
			}
			//
			return same;
		}
		#endregion

		#region XML
		public override void ExternaliseAsXML( XmlWriter aSink )
		{
			if	( Token != null )
			{
				aSink.WriteAttributeString( "value", Token.Value );
			}
			else
			{
				aSink.WriteAttributeString( "value", "" );
			}
		}
		#endregion
	}
}
