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
using System.Collections.Generic;

namespace SymbolLib.Generics
{
	public class GenericSymbolCollectionList : IEnumerable< GenericSymbolCollection >
	{
		#region Construct & destruct
		public GenericSymbolCollectionList()
		{
		}
		#endregion

		#region API
		public void Add( GenericSymbolCollection aCollection )
		{
			iList.Add( aCollection );
		}

		public void Remove( GenericSymbolCollection aCollection )
		{
			iList.Remove( aCollection );
		}

		public void Reset()
		{
			iList.Clear();
		}
		#endregion

		#region Properties
		public int Count
		{
			get { return iList.Count; }
		}

		public GenericSymbolCollection this[ int aIndex ]
		{
			get { return (GenericSymbolCollection) iList[ aIndex ]; }
		}
		#endregion

        #region From IEnumerable<GenericSymbolCollection>
        IEnumerator<GenericSymbolCollection> IEnumerable<GenericSymbolCollection>.GetEnumerator()
        {
            foreach ( GenericSymbolCollection col in iList )
            {
                yield return col;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( GenericSymbolCollection col in iList )
            {
                yield return col;
            }
        }
		#endregion

		#region Data members
        private List<GenericSymbolCollection> iList = new List<GenericSymbolCollection>( 100 );
		#endregion
    }
}
