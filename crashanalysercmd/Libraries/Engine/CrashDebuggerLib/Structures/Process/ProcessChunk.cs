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
using CrashDebuggerLib.Structures.Chunk;
using CrashDebuggerLib.Structures.Common;

namespace CrashDebuggerLib.Structures.Process
{
    public class ProcessChunk : CrashDebuggerAware
    {
        #region Constructors
        public ProcessChunk( CrashDebuggerInfo aCrashDebugger, uint aChunkAddress, int aAccessCount )
            : base( aCrashDebugger )
        {
            iChunkAddress = aChunkAddress;
            iAccessCount = aAccessCount;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint ChunkAddress
        {
            get { return iChunkAddress; }
            set { iChunkAddress = value; }
        }

        public Chunk.DChunk Chunk
        {
            get { return CrashDebugger.ChunkByAddress( ChunkAddress ); }
        }

        public int AccessCount
        {
            get { return iAccessCount; }
            set { iAccessCount = value; }
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

        #region Data members
        private uint iChunkAddress = 0;
        private int iAccessCount = 0;
        #endregion
    }

    public class ProcessChunkCollection : IEnumerable<ProcessChunk>
    {
        #region Constructors
        public ProcessChunkCollection()
        {
        }
        #endregion

        #region API
        public void Add( ProcessChunk aChunk )
        {
            iEntries.Add( aChunk );
        }
        #endregion

        #region Properties
        public ProcessChunk this[ int aIndex ]
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
        public IEnumerator<ProcessChunk> GetEnumerator()
        {
            return new ProcessChunkEnumerator( this );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new ProcessChunkEnumerator( this );
        }
        #endregion

        #region Data members
        private List<ProcessChunk> iEntries = new List<ProcessChunk>();
        #endregion
    }

    #region Internal enumerator
    internal class ProcessChunkEnumerator : IEnumerator<ProcessChunk>
    {
        #region Constructors
        public ProcessChunkEnumerator( ProcessChunkCollection aList )
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

        #region From IEnumerator<ProcessChunk>
        ProcessChunk IEnumerator<ProcessChunk>.Current
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
        private readonly ProcessChunkCollection iList;
        private int iCurrentIndex = -1;
        #endregion
    }
    #endregion
}
