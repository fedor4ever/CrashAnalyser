/*
* Copyright (c) 2009 Nokia Corporation and/or its subsidiary(-ies). 
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
using System.Xml;
using System.Collections;

namespace SymbianTree
{
	public class SymNodeAddAsSibling : SymNode
	{
		#region Constructors
		public SymNodeAddAsSibling()
		{
		}

		public SymNodeAddAsSibling( SymNode aParent )
			: base( aParent )
		{
		}

		public SymNodeAddAsSibling( object aData )
			: base( aData )
		{
		}

		public SymNodeAddAsSibling( object aData, SymNode aParent )
			: base( aData, aParent )
		{
		}
		#endregion

		#region From SymNode
		public override void Add( SymNode aNode )
		{
			AppendSibling( aNode );
		}
		#endregion
	}
}
