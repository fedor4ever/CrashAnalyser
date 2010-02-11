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
using CrashDebuggerLib.Structures.CodeSeg;
using CrashDebuggerLib.Structures.Common;
using CrashDebuggerLib.Structures.Library;

namespace CrashDebuggerLib.Structures.Process
{
    public class ProcessCodeSeg : CrashDebuggerAware
    {
        #region Constructors
        public ProcessCodeSeg( CrashDebuggerInfo aCrashDebugger )
            : this( aCrashDebugger, 0, 0 )
        {
        }

        public ProcessCodeSeg( CrashDebuggerInfo aCrashDebugger, uint aCodeSegAddress, uint aLibraryAddress )
            : base( aCrashDebugger )
        {
            iCodeSegAddress = aCodeSegAddress;
            iLibraryAddress = aLibraryAddress;
        }
        #endregion

        #region API
        #endregion

        #region Properties
        public uint CodeSegAddress
        {
            get { return iCodeSegAddress; }
            set
            {
                iCodeSegAddress = value;
                CodeSegEntry entry = CodeSeg;
            }
        }

        public CodeSegEntry CodeSeg
        {
            get { return CrashDebugger.CodeSegByAddress( CodeSegAddress ); }
        }

        public string FileName
        {
            get
            {
                string ret = string.Empty;
                if ( CodeSeg != null )
                {
                    ret = CodeSeg.FileName;
                }
                return ret;
            }
        }

        public uint LibraryAddress
        {
            get { return iLibraryAddress; }
            set { iLibraryAddress = value; }
        }

        public DLibrary Library
        {
            get { return CrashDebugger.LibraryByAddress( LibraryAddress ); }
        }

        public uint Size
        {
            get
            {
                uint ret = 0;
                //
                if ( CodeSeg != null )
                {
                    ret = CodeSeg.Size;
                }
                //
                return ret;
            }
            set
            {
                if ( CodeSeg != null )
                {
                    CodeSeg.Size = value;
                }
            }
        }

        public uint ProcessLocalRunAddress
        {
            get { return iProcessLocalRunAddress; }
            set { iProcessLocalRunAddress = value; }
        }

        public uint ProcessLocalRunAddressEnd
        {
            get
            {
                uint ret = ProcessLocalRunAddress + Size;
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            //
            ret.Append( ProcessLocalRunAddress.ToString( "x8" ) );
            if ( Size != 0 )
            {
                ret.Append( "-" );
                ret.Append( ProcessLocalRunAddressEnd.ToString( "x8" ) );
            }
            ret.Append( " " );
            if ( CodeSeg != null )
            {
                ret.Append( CodeSeg.FileName );
            }
            //
            return ret.ToString();
        }
        #endregion

        #region Data members
        private uint iCodeSegAddress = 0;
        private uint iLibraryAddress = 0;
        private uint iProcessLocalRunAddress = 0;
        #endregion
    }

    public class ProcessCodeSegCollection : IEnumerable<ProcessCodeSeg>
    {
        #region Constructors
        public ProcessCodeSegCollection()
        {
        }
        #endregion

        #region API
        public void Add( ProcessCodeSeg aCodeSeg )
        {
            if ( !Contains( aCodeSeg.CodeSegAddress ) )
            {
                iEntries.Add( aCodeSeg );
            }
        }

        public bool Contains( uint aAddress )
        {
            ProcessCodeSeg ret = this[ aAddress ];
            bool found = ( ret != null );
            return found;
        }
        #endregion

        #region Properties
        public ProcessCodeSeg this[ int aIndex ]
        {
            get { return iEntries[ aIndex ]; }
        }

        public ProcessCodeSeg this[ uint aAddress ]
        {
            get
            {
                ProcessCodeSeg ret = iEntries.Find( delegate( ProcessCodeSeg aEntry ) { return aEntry.CodeSegAddress == aAddress; } );
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

        #region From IEnumerable<ProcessCodeSegCollection>
        public IEnumerator<ProcessCodeSeg> GetEnumerator()
        {
            return new ProcessCodeSegEnumerator( this );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new ProcessCodeSegEnumerator( this );
        }
        #endregion

        #region Data members
        private List<ProcessCodeSeg> iEntries = new List<ProcessCodeSeg>();
        #endregion
    }

    #region Internal enumerator
    internal class ProcessCodeSegEnumerator : IEnumerator<ProcessCodeSeg>
    {
        #region Constructors
        public ProcessCodeSegEnumerator( ProcessCodeSegCollection aList )
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

        #region From IEnumerator<ProcessCodeSeg>
        ProcessCodeSeg IEnumerator<ProcessCodeSeg>.Current
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
        private readonly ProcessCodeSegCollection iList;
        private int iCurrentIndex = -1;
        #endregion
    }
    #endregion
}
