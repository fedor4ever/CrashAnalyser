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
using CrashDebuggerLib.Structures.KernelObjects;

namespace CrashDebuggerLib.Structures.Chunk
{
    public class ChunkCollection : IEnumerable<DChunk>
    {
        #region Constructors
        public ChunkCollection()
        {
        }
        #endregion

        #region API
        public void Add( DChunk aChunk )
        {
            iEntries.Add( aChunk );
        }
        #endregion

        #region Properties
        public DChunk this[ int aIndex ]
        {
            get { return iEntries[ aIndex ]; }
        }

        public int Count
        {
            get { return iEntries.Count; }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        #region From IEnumerable<Chunk>
        public IEnumerator<DChunk> GetEnumerator()
        {
            return new ChunkEnumerator( this );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new ChunkEnumerator( this );
        }
        #endregion

        #region Data members
        private List<DChunk> iEntries = new List<DChunk>();
        #endregion
    }

    #region Internal enumerator
    internal class ChunkEnumerator : IEnumerator<DChunk>
    {
        #region Constructors
        public ChunkEnumerator( ChunkCollection aList )
        {
            iList = aList;
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
                return iList[ iCurrentIndex ];
            }
        }

        public bool MoveNext()
        {
            return ( ++iCurrentIndex < iList.Count );
        }
        #endregion

        #region From IEnumerator<Chunk>
        DChunk IEnumerator<DChunk>.Current
        {
            get { return iList[ iCurrentIndex ]; }
        }
        #endregion

        #region From IDisposable
        public void Dispose()
        {
        }
        #endregion

        #region Data members
        private readonly ChunkCollection iList;
        private int iCurrentIndex = -1;
        #endregion
    }
    #endregion
}
