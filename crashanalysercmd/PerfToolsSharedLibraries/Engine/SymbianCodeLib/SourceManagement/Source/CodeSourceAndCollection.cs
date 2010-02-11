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
using System.Collections.Generic;
using System.Text;
using SymbianUtils;
using SymbianStructuresLib.Debug.Code;
using SymbianStructuresLib.CodeSegments;
using SymbianCodeLib.DbgEnginePlugin;

namespace SymbianCodeLib.SourceManagement.Source
{
    public class CodeSourceAndCollection
    {
        #region Constructors
        internal CodeSourceAndCollection( CodeSource aSource, CodeCollection aCollection )
        {
            System.Diagnostics.Debug.Assert( aSource != null && aCollection != null );
            //
            iSource = aSource;
            iCollection = aCollection;
        }

        internal CodeSourceAndCollection( CodeSourceAndCollection aCopy )
        {
            iSource = aCopy.Source;
            iCollection = aCopy.Collection;
        }

        internal CodeSourceAndCollection( CodeSourceAndCollection aCopy, CodeCollection aCollection )
        {
            iSource = aCopy.Source;
            iCollection = aCollection;
        }
        #endregion
        
        #region Properties
        public CodeSource Source
        {
            get { return iSource; }
            internal set { iSource = value; }
        }

        public CodeCollection Collection
        {
            get { return iCollection; }
            internal set { iCollection = value; }
        }
        #endregion

        #region Data members
        private CodeSource iSource;
        private CodeCollection iCollection;
        #endregion
    }
}
