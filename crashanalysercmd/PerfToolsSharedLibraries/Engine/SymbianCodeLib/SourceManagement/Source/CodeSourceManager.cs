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
using SymbianStructuresLib.CodeSegments;
using SymbianStructuresLib.Debug.Code;
using SymbianCodeLib.DbgEnginePlugin;

namespace SymbianCodeLib.SourceManagement.Source
{
    internal class CodeSourceManager : CodeSourceCollection
    {
        #region Delegates & events
        public delegate void SourceEventHandler( CodeSource aSource );
        public event SourceEventHandler SourceAdded = null;
        public event SourceEventHandler SourceRemoved = null;
        #endregion

        #region Constructors
        public CodeSourceManager( CodePlugin aPlugin )
        {
            iPlugin = aPlugin;
        }
        #endregion

        #region API
        public IEnumerable<CodeCollection> GetFixedCollectionEnumerator()
        {
            foreach ( CodeSource source in this )
            {
                foreach ( CodeCollection col in source )
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
        public CodeSourceAndCollection this[ CodeSegDefinition aCodeSeg ]
        {
            get
            {
                CodeSourceAndCollection ret = null;
                //
                foreach ( CodeSource source in this )
                {
                    CodeCollection col = source[ aCodeSeg ];
                    if ( col != null )
                    {
                        ret = new CodeSourceAndCollection( source, col );
                        break;
                    }
                }
                //
                return ret;
            }
        }
        #endregion

        #region From CodeSourceCollection
        protected override void OnAdded( CodeSource aSource )
        {
            base.OnAdded( aSource );
            if ( SourceAdded != null )
            {
                SourceAdded( aSource );
            }
        }

        protected override void OnRemoved( CodeSource aSource )
        {
            base.OnRemoved( aSource );
            if ( SourceRemoved != null )
            {
                SourceRemoved( aSource );
            }
        }
        #endregion

        #region Data members
        private readonly CodePlugin iPlugin;
        #endregion
    }
}
