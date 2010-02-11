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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SymbianStructuresLib.Debug.Symbols;
using SymbianStructuresLib.Debug.Symbols.Interfaces;
using SymbianStructuresLib.Debug.Common.Interfaces;
using SymbianStructuresLib.Debug.Common.FileName;
using SymbianSymbolLib.SourceManagement.Source;
using SymbianSymbolLib.SourceManagement.Provisioning;
using SymbianUtils;
using SymbianUtils.Tracer;
using SymbianUtils.FileTypes;
using SLPluginSymbol.Source;

namespace SLPluginSymbol.Data
{
	internal class SymbolCollectionSegment : DisposableObject, IEnumerable<string>
	{
		#region Constructors
        public SymbolCollectionSegment( IPlatformIdAllocator aIdAllocator, string aHostFileName )
		{
            iCollection = SymbolCollection.NewByHostFileName( aIdAllocator, aHostFileName );
		}
        #endregion

        #region API
        public SymbolCollection ExcavateSymbolCollection()
        {
            SymbolCollection ret = iCollection;
            iCollection = null;
            return ret;
        }

        public void AddLine( string aLine )
        {
            iLines.Add( aLine );
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iLines.Count; }
        }

        public SymbolCollection Collection
        {
            get { return iCollection; }
        }

        public PlatformFileName FileName
        {
            get { return iCollection.FileName; }
        }
        #endregion

        #region Internal constants
        #endregion

        #region Internal methods
        #endregion

        #region From IEnumerable<string>
        public IEnumerator<string> GetEnumerator()
        {
            foreach ( string line in iLines )
            {
                yield return line;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( string line in iLines )
            {
                yield return line;
            }
        }
        #endregion

        #region From DisposableObject
        protected override void CleanupManagedResources()
        {
            try
            {
                base.CleanupManagedResources();
            }
            finally
            {
                iCollection = null;
                iLines.Clear();
            }
        }
        #endregion

        #region Data members
        private SymbolCollection iCollection = null;
        private List<string> iLines = new List<string>();
        #endregion
    }
}
