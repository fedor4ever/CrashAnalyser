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
using SymbianStructuresLib.Debug.Symbols;
using SymbianStructuresLib.CodeSegments;
using SymbianSymbolLib.DbgEnginePlugin;

namespace SymbianSymbolLib.SourceManagement.Source
{
    public class SymSourceAndCollection
    {
        #region Constructors
        internal SymSourceAndCollection( SymSource aSource, SymbolCollection aCollection )
        {
            System.Diagnostics.Debug.Assert( aSource != null && aCollection != null );
            //
            iSource = aSource;
            iCollection = aCollection;
        }

        internal SymSourceAndCollection( SymSourceAndCollection aCopy )
        {
            iSource = aCopy.Source;
            iCollection = aCopy.Collection;
        }

        internal SymSourceAndCollection( SymSourceAndCollection aCopy, SymbolCollection aCollection )
        {
            iSource = aCopy.Source;
            iCollection = aCollection;
        }
        #endregion
        
        #region Properties
        public SymSource Source
        {
            get { return iSource; }
            internal set { iSource = value; }
        }

        public SymbolCollection Collection
        {
            get { return iCollection; }
            internal set { iCollection = value; }
        }
        #endregion

        #region Data members
        private SymSource iSource;
        private SymbolCollection iCollection;
        #endregion
    }
}
