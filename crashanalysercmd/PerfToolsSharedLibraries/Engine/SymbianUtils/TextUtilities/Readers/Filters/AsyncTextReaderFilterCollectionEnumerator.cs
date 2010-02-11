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
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;

namespace SymbianUtils
{
	public class AsyncTextReaderFilterCollectionEnumerator : IEnumerator
	{
		#region Constructors
		public AsyncTextReaderFilterCollectionEnumerator( AsyncTextReaderFilterCollection aCollection )
		{
			iCollection = aCollection;
		}
		#endregion

		#region IEnumerator Members
		public void Reset()
		{
			iCurrentIndex = -1;
		}

		public object Current
		{
			get
			{
				return (AsyncTextReaderFilter) iCollection[ iCurrentIndex ];
			}
		}

		public bool MoveNext()
		{
			return ( ++iCurrentIndex < iCollection.Count );
		}
		#endregion

		#region Data members
		private readonly AsyncTextReaderFilterCollection iCollection;
		private int iCurrentIndex = -1;
		#endregion
	}
}
