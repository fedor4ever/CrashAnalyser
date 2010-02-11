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

namespace CrashDebuggerLib.Structures.KernelObjects
{
    public class DObjectCon : DBase, IEnumerable<DObject>
    {
        #region Constructors
        public DObjectCon( CrashDebuggerInfo aCrashDebugger, DObject.TObjectType aType )
            : base( aCrashDebugger )
        {
            iType = aType;
        }
        #endregion

        #region API
        internal void Add( DObject aObject )
        {
            if ( aObject.Type != Type )
            {
                throw new ArgumentException( "Object is not of the correct type" );
            }
            
            // Add it to the address-based index.
            iObjectsInOriginalOrder.Add( aObject.KernelAddress, aObject );

            try
            {
                // Must munge in the address, because name is not sufficiently unique. For example
                // there can be dead threads or processes (or anything really) with the same name
                // but since one is dead and the other is not, then they have different addresses.
                string sortedByName = string.Format( "{0}_{1:x8}", aObject, aObject.KernelAddress );
                iObjectsInSortedOrder.Add( sortedByName, aObject );
            }
            catch ( Exception )
            {
                // Keep the two synchronised...
                iObjectsInOriginalOrder.Remove( aObject.KernelAddress );
            }
        }

        public bool Contains( DObject aObject )
        {
            bool exists = iObjectsInOriginalOrder.ContainsKey( aObject.KernelAddress );
            return exists;
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return iObjectsInSortedOrder.Count; }
        }

        public int ExpectedCount
        {
            get { return iExpectedCount; }
            set { iExpectedCount = value; }
        }

        public int Index
        {
            get { return iIndex; }
            set { iIndex = value; }
        }

        public DObject this[ uint aAddress ]
        {
            get
            {
                DObject ret = null;
                iObjectsInOriginalOrder.TryGetValue( aAddress, out ret );
                return ret;
            }
        }

        public DObject.TObjectType Type
        {
            get { return iType; }
        }

        public string TypeDescription
        {
            get
            {
                string ret = DObject.AsTypeDescription( iType );
                return ret;
            }
        }
        #endregion

        #region Internal methods
        #endregion

        #region Internal constants
        #endregion

        #region From IEnumerable<DObject>
        public IEnumerator<DObject> GetEnumerator()
        {
            foreach ( KeyValuePair<string, DObject> pair in iObjectsInSortedOrder )
            {
                yield return pair.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach ( KeyValuePair<string, DObject> pair in iObjectsInSortedOrder )
            {
                yield return pair.Value;
            }
        }
        #endregion

        #region From System.Object
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.AppendFormat( "[{0:d4}] {1} objects", Count, TypeDescription );
            return ret.ToString();
        }
        #endregion

        #region Data members
        private readonly DObject.TObjectType iType;
        private int iIndex = 0;
        private int iExpectedCount = 0;
        private Dictionary<uint, DObject> iObjectsInOriginalOrder = new Dictionary<uint, DObject>();
        private SortedList<string, DObject> iObjectsInSortedOrder = new SortedList<string, DObject>();
        #endregion
    }
}
