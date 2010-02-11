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
    internal class SymSourceManager : SymSourceCollection
    {
        #region Constructors
        public SymSourceManager( SymbolPlugin aPlugin )
        {
            iPlugin = aPlugin;
        }
        #endregion

        #region API
        public IEnumerable<SymbolCollection> GetFixedCollectionEnumerator()
        {
            foreach ( SymSource source in this )
            {
                foreach ( SymbolCollection col in source )
                {
                    if ( col.IsFixed )
                    {
                        yield return col;
                    }
                }
            }
        }
        #endregion

        #region Properties
        public SymSourceAndCollection this[ CodeSegDefinition aCodeSeg ]
        {
            get
            {
                SymSourceAndCollection ret = null;
                //
                foreach ( SymSource source in this )
                {
                    SymbolCollection col = source[ aCodeSeg ];
                    if ( col != null )
                    {
                        ret = new SymSourceAndCollection( source, col );
                        break;
                    }
                }
                //
                return ret;
            }
        }
        #endregion

        #region Data members
        private readonly SymbolPlugin iPlugin;
        #endregion
    }
}
