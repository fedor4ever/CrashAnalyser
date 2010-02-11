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
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace SymbianUtils.RawItems
{
	internal class RawItemCollectionEnumerator : IEnumerator<RawItem>
	{
		#region Constructors
		public RawItemCollectionEnumerator( RawItemCollection aCollection )
		{
            iCollection = aCollection;
		}
		#endregion

        #region From IEnumeratorFrom <RawItem>
        RawItem IEnumerator<RawItem>.Current
        {
            get { return iCollection[ iCurrentIndex ]; }
        }
        #endregion

        #region IEnumerator Members
        object IEnumerator.Current
        {
            get
            {
                return iCollection[ iCurrentIndex ];
            }
        }

        bool IEnumerator.MoveNext()
        {
            return ( ++iCurrentIndex < iCollection.Count );
        }

        void IEnumerator.Reset()
        {
            iCurrentIndex = -1;
        }
        #endregion

        #region IDisposable Members
        void IDisposable.Dispose()
        {
        }
        #endregion

        #region Data members
        private readonly RawItemCollection iCollection;
        private int iCurrentIndex = -1;
        #endregion
    }
}
