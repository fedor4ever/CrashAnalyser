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
using System.Collections.Generic;
using SymbolLib.Generics;

namespace SymbolLib.Sources.Map.File
{
    internal class MapFileEnumerator : IEnumerator<GenericSymbol>
	{
		#region Constructors & destructor
        public MapFileEnumerator( MapFile aMapFile )
		{
            iMapFile = aMapFile;
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
                return iMapFile[ iCurrentIndex ];
			}
		}

		public bool MoveNext()
		{
            return ( ++iCurrentIndex < iMapFile.Count );
		}
		#endregion
 
        #region From IEnumerator<GenericSymbol>
        GenericSymbol IEnumerator<GenericSymbol>.Current
        {
            get { return iMapFile[ iCurrentIndex ]; }
        }
        #endregion

        #region From IDisposable
        public void Dispose()
        {
        }
        #endregion
        
        #region Data members
        private readonly MapFile iMapFile;
		private int iCurrentIndex = -1;
		#endregion
	}
}
