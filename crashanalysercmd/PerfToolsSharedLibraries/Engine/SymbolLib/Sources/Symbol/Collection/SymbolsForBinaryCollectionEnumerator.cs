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
using System.Collections;
using System.Collections.Generic;
using SymbolLib.Sources.Symbol.File;

namespace SymbolLib.Sources.Symbol.Collection
{
	internal class SymbolsForBinaryCollectionEnumerator : IEnumerator<SymbolsForBinary>
	{
		#region Constructors & destructor
        public SymbolsForBinaryCollectionEnumerator( SymbolsForBinaryCollection aCollection )
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
                return iCollection[ iCurrentIndex ];
			}
		}

		public bool MoveNext()
		{
			return ( ++iCurrentIndex < iCollection.Count );
		}
		#endregion

        #region From IEnumerator<SymbolsForBinary>
        SymbolsForBinary IEnumerator<SymbolsForBinary>.Current
        {
            get { return iCollection[ iCurrentIndex ]; }
        }
        #endregion

        #region From IDisposable
        public void Dispose()
        {
        }
        #endregion

        #region Data members
        private readonly SymbolsForBinaryCollection iCollection;
		private int iCurrentIndex = -1;
		#endregion
	}
}
