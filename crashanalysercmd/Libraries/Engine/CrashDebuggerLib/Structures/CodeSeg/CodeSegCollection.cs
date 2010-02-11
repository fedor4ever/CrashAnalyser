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
using CrashDebuggerLib.Structures.Common;

namespace CrashDebuggerLib.Structures.CodeSeg
{
    public class CodeSegCollection : CrashDebuggerAware, IEnumerable<CodeSegEntry>
    {
        #region Constructors
        public CodeSegCollection( CrashDebuggerInfo aCrashDebugger )
            : base( aCrashDebugger )
        {
        }
        #endregion

        #region API
        public void Clear()
        {
            iEntries.Clear();
        }

        public void Add( CodeSegEntry aEntry )
        {
            iEntries.Add( aEntry );
        }
        #endregion

        #region Properties
        public CodeSegEntry this[ int aIndex ]
        {
            get { return iEntries[ aIndex ]; }
        }

        public CodeSegEntry this[ uint aAddress ]
        {
            get
            {
                CodeSegEntry ret = iEntries.Find( delegate( CodeSegEntry aEntry ) { return aEntry.KernelAddress == aAddress; } );
                return ret;
            }
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

        #region From IEnumerable<CodeSegEntry>
        public IEnumerator<CodeSegEntry> GetEnumerator()
        {
            return new CodeSegEnumerator( this );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new CodeSegEnumerator( this );
        }
        #endregion

        #region Data members
        private List<CodeSegEntry> iEntries = new List<CodeSegEntry>();
        #endregion
    }

    #region Internal enumerator
    internal class CodeSegEnumerator : IEnumerator<CodeSegEntry>
    {
        #region Constructors
        public CodeSegEnumerator( CodeSegCollection aList )
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

        #region From IEnumerator<CodeSegEntry>
        CodeSegEntry IEnumerator<CodeSegEntry>.Current
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
        private readonly CodeSegCollection iList;
        private int iCurrentIndex = -1;
        #endregion
    }
    #endregion
}
