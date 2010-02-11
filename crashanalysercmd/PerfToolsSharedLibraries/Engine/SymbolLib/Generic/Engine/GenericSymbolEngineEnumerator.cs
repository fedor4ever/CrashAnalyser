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
using System.Collections.Generic;
using SymbianUtils;
using SymbianUtils.Range;

namespace SymbolLib.Generics
{
    internal class GenericSymbolEngineEnumerator : IEnumerator<GenericSymbolCollection>
    {
        #region Construct & destruct
        public GenericSymbolEngineEnumerator( GenericSymbolEngine aEngine )
        {
            iEngine = aEngine;
        }
        #endregion

        #region From IEnumerator
        public void Reset()
        {
            iCurrentIndex = -1;
        }

        public object Current
        {
            get
            {
                return iEngine[ iCurrentIndex ];
            }
        }

        public bool MoveNext()
        {
            return ( ++iCurrentIndex < iEngine.NumberOfCollections );
        }
        #endregion

        #region From IEnumerator<GenericSymbolCollection>
        GenericSymbolCollection IEnumerator<GenericSymbolCollection>.Current
        {
            get { return iEngine[ iCurrentIndex ]; }
        }
        #endregion

        #region From IDisposable
        public void Dispose()
        {
        }
        #endregion

        #region Data members
        private readonly GenericSymbolEngine iEngine;
        private int iCurrentIndex = -1;
        #endregion
    }
}
