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

namespace SymBuildParsingLib.Token
{
	internal class SymTokenContainerEnumerator : IEnumerator
	{
		#region Constructors & destructor
		public SymTokenContainerEnumerator( SymTokenContainer aContainer )
		{
			iContainer = aContainer;
		}
		#endregion

		#region IEnumerator Members
		void IEnumerator.Reset()
		{
			iEnumeratorIndex = -1;
		}

		object IEnumerator.Current
		{
			get
			{
				return iContainer[ iEnumeratorIndex ];
			}
		}

		bool IEnumerator.MoveNext()
		{
			return ( ++iEnumeratorIndex < iContainer.Count );
		}
		#endregion

		#region Data members
		private int iEnumeratorIndex = -1;
		private readonly SymTokenContainer iContainer;
		#endregion
	}
}
